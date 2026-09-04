/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Concurrent;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Calculates the penalty for a QR code to determine the optimal mask pattern.
    /// <para>
    /// See "7.8.3 Evaluation of data masking results" in the QR code specification
    /// (ISO/IEC 18004:2024(en) for details.
    /// </para>
    /// <para>
    /// Each rule works on whole 64-bit words rather than on single modules, so a rule costs a
    /// handful of instructions per word instead of one per module. The rules that scan columns run
    /// the row algorithm over the transpose the <see cref="ScoringMatrix"/> carries.
    /// </para>
    /// <para>
    /// Every rule that scans rows comes in three forms, one per <see cref="BitMatrix"/> row layout,
    /// each scanning the one, two or three words of modules that layout holds with its loop over
    /// them unrolled. See <see cref="BitMatrix"/> for which versions take which layout. All three
    /// compute the same score, and a matrix's layout follows from its size, so which one runs never
    /// changes an outcome — only how many words are scanned.
    /// </para>
    /// <para>
    /// Every rule subtracts the contribution of the three finder patterns, which is the same for
    /// every mask pattern. That keeps the numbers small and comparable, and it is what lets
    /// <see cref="Calculate"/> stop early. A rule therefore scores non-negatively only for a matrix
    /// that carries the finder patterns; scored against anything else, an individual rule can
    /// return a negative number.
    /// </para>
    /// </summary>
    internal static class Penalty
    {

        // Computes the total penalty score, with optional early stopping.
        //
        // For real QR codes, every CalcXxx() returns a non-negative contribution
        // The running sum is therefore monotonic non-decreasing, so once it
        // reaches lowestPenaltySoFar this candidate cannot beat the current best.
        //
        // Returns the exact penalty when it is below lowestPenaltySoFar, otherwise
        // some value >= lowestPenaltySoFar (the partial sum at the point of bailout).
        // Either way, comparing the result against lowestPenaltySoFar with strict
        // less-than yields the correct mask-selection decision.
        internal static int Calculate(ScoringMatrix matrix, int lowestPenaltySoFar)
        {
            // Ordered by mean penalty contribution (descending) for early-stop
            // effectiveness; see QrCodeGeneratorProfiling/README.md "Penalty Contribution".
            // Rules that scan columns read the transpose (Columns) and reuse the row algorithm.
            var sum = Calc2By2Blocks(matrix.Rows);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcSameColor(matrix.Columns);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcSameColor(matrix.Rows);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcFinderPattern(matrix.Rows);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcFinderPattern(matrix.Columns);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            return sum + CalcColorBalance(matrix.Rows);
        }

        // Calculates the total penalty score, fully evaluating all contributions and collecting the details in penaltyInfo.
        internal static int CalculateFully(ScoringMatrix matrix, ref PenaltyScore penaltyInfo)
        {
            penaltyInfo.Blocks = Calc2By2Blocks(matrix.Rows);
            penaltyInfo.VerticalStreaks = CalcSameColor(matrix.Columns);
            penaltyInfo.HorizontalStreaks = CalcSameColor(matrix.Rows);
            penaltyInfo.HorizontalFinderPatterns = CalcFinderPattern(matrix.Rows);
            penaltyInfo.VerticalFinderPatterns = CalcFinderPattern(matrix.Columns);
            penaltyInfo.ColorBalance = CalcColorBalance(matrix.Rows);

            penaltyInfo.Total = penaltyInfo.Blocks
                + penaltyInfo.VerticalStreaks + penaltyInfo.HorizontalStreaks
                + penaltyInfo.HorizontalFinderPatterns + penaltyInfo.VerticalFinderPatterns
                + penaltyInfo.ColorBalance;
            return penaltyInfo.Total;
        }

        #region Streaks

        // Penalty for adjacent modules in a row in the same color.
        // Penalty points: N1 + i, where i is the amount by which the number
        // of adjacent modules of the same color exceeds 5, and N1 is 3.
        internal static int CalcSameColor(BitMatrix modules)
        {
            switch (modules.UsedWordsPerRow)
            {
                case 1: return CalcSameColorOneWord(modules);
                case 2: return CalcSameColorTwoWords(modules);
                default: return CalcSameColorThreeWords(modules);
            }
        }

        // Each finder pattern contributes 2 streaks of 5, 2 streaks of 7 and 1 streak of 8.
        private const int BaseSameColor = 3 * (2 * 3 + 2 * 5 + 6);

        private static int CalcSameColorOneWord(BitMatrix modules)
        {
            // For each row, with T[i] = bits[i] ^ bits[i+1] (set where two adjacent modules differ):
            //   tz[i]         = ~T[i] & ~T[i+1]     (3 consecutive equal bits at positions i..i+2)
            //   fiveWindow[i] = tz[i] & tz[i+2]     (5 consecutive equal bits at positions i..i+4)
            //   run5Start[i]  = fiveWindow[i] AND (i == 0 OR T[i-1] == 1)
            // A run of length L >= 5 contains (L - 4) five-windows and starts exactly once, and
            // contributes (L - 2) = (L - 4) + 2 points, so the row scores
            //   popcount(fiveWindow) + 2 * popcount(run5Start).
            // The fiveWindow mask is clipped to positions [0, size-5] so the padding zeros past
            // the row's last column don't create false windows.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 5)
            {
                return 0;
            }

            var edgeMask = (1ul << (size - 4)) - 1;
            var fiveWindowCount = 0;
            var run5StartCount = 0;

            for (var y = 0; y < size; y += 1)
            {
                var w = raw[y];

                var t = w ^ (w >> 1);
                var tz = ~(t | (t >> 1));
                var fw = tz & (tz >> 2) & edgeMask;
                // a window starts a run if the module before it differs; bit 0 is forced, as the
                // start of the row always starts a run
                var rs = fw & ((t << 1) | 1ul);

                fiveWindowCount += BitMatrix.PopCount(fw);
                run5StartCount += BitMatrix.PopCount(rs);
            }

            return fiveWindowCount + 2 * run5StartCount - BaseSameColor;
        }

        private static int CalcSameColorTwoWords(BitMatrix modules)
        {
            // The recurrence of CalcSameColorOneWord across two words; see CalcSameColorThreeWords
            // for how the carries and the last word work, which is the same here.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 5)
            {
                return 0;
            }

            var edgeMask = GetEdgeMask(size - 4);
            var fiveWindowCount = 0;
            var run5StartCount = 0;

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = 2 * y;
                var w0 = raw[rowOffset];
                var w1 = raw[rowOffset + 1];

                var t0 = w0 ^ ((w0 >> 1) | (w1 << 63));
                var t1 = w1 ^ (w1 >> 1);

                var tz0 = ~(t0 | ((t0 >> 1) | (t1 << 63)));
                var tz1 = ~(t1 | (t1 >> 1));

                var fw0 = tz0 & ((tz0 >> 2) | (tz1 << 62)) & edgeMask[0];
                var fw1 = tz1 & (tz1 >> 2) & edgeMask[1];

                var rs0 = fw0 & ((t0 << 1) | 1ul);
                var rs1 = fw1 & ((t1 << 1) | (t0 >> 63));

                fiveWindowCount += BitMatrix.PopCount(fw0) + BitMatrix.PopCount(fw1);
                run5StartCount += BitMatrix.PopCount(rs0) + BitMatrix.PopCount(rs1);
            }

            return fiveWindowCount + 2 * run5StartCount - BaseSameColor;
        }

        private static int CalcSameColorThreeWords(BitMatrix modules)
        {
            // The recurrence of CalcSameColorOneWord, plumbed across the three words of modules a
            // row holds: every shift carries the bits it drops into the neighbouring word. The
            // shifts out of the last word bring in zeros, which the edge mask clears again: at the
            // largest size of this layout the mask keeps bits 0 to 59 of that word, and the shifts
            // reach only bits 62 and 63.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 5)
            {
                return 0;
            }

            var edgeMask = GetEdgeMask(size - 4);
            var fiveWindowCount = 0;
            var run5StartCount = 0;

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = BitMatrix.MaxWordsPerRow * y;
                var w0 = raw[rowOffset];
                var w1 = raw[rowOffset + 1];
                var w2 = raw[rowOffset + 2];

                var t0 = w0 ^ ((w0 >> 1) | (w1 << 63));
                var t1 = w1 ^ ((w1 >> 1) | (w2 << 63));
                var t2 = w2 ^ (w2 >> 1);

                // tz[i] = ~T[i] & ~T[i+1] (3 consecutive equal bits at i..i+2)
                var tz0 = ~(t0 | ((t0 >> 1) | (t1 << 63)));
                var tz1 = ~(t1 | ((t1 >> 1) | (t2 << 63)));
                var tz2 = ~(t2 | (t2 >> 1));

                // fw[i] = tz[i] & tz[i+2] (5 consecutive equal bits at i..i+4)
                var fw0 = tz0 & ((tz0 >> 2) | (tz1 << 62)) & edgeMask[0];
                var fw1 = tz1 & ((tz1 >> 2) | (tz2 << 62)) & edgeMask[1];
                var fw2 = tz2 & (tz2 >> 2) & edgeMask[2];

                // a window starts a run if the module before it differs; bit 0 of word 0 is forced,
                // as the start of the row always starts a run
                var rs0 = fw0 & ((t0 << 1) | 1ul);
                var rs1 = fw1 & ((t1 << 1) | (t0 >> 63));
                var rs2 = fw2 & ((t2 << 1) | (t1 >> 63));

                fiveWindowCount += BitMatrix.PopCount(fw0) + BitMatrix.PopCount(fw1) + BitMatrix.PopCount(fw2);
                run5StartCount += BitMatrix.PopCount(rs0) + BitMatrix.PopCount(rs1) + BitMatrix.PopCount(rs2);
            }

            return fiveWindowCount + 2 * run5StartCount - BaseSameColor;
        }

        #endregion

        #region 2 by 2 blocks

        // Penalty for 2 by 2 blocks in the same color.
        // Penalty points: N2, where N2 is 3. Overlapping blocks count separately,
        // as the specification prescribes.
        internal static int Calc2By2Blocks(BitMatrix modules)
        {
            switch (modules.UsedWordsPerRow)
            {
                case 1: return Calc2By2BlocksOneWord(modules);
                case 2: return Calc2By2BlocksTwoWords(modules);
                default: return Calc2By2BlocksThreeWords(modules);
            }
        }

        // Each finder pattern contributes 4 blocks, from the 3x3 dark modules at its center.
        private const int Base2By2Blocks = 4 * 3;

        private static int Calc2By2BlocksOneWord(BitMatrix modules)
        {
            // For each consecutive row pair (A, B), bit x in `monochrome` is set iff
            // the 2x2 block starting at (x, y) is monochrome:
            //   monochrome = ~((A ^ (A>>1)) | (B ^ (B>>1)) | (A ^ B))
            // Bits at x >= size-1 are cleared by the edge mask.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 2)
            {
                return 0;
            }

            var edgeMask = (1ul << (size - 1)) - 1;
            var count = 0;

            // each row is the lower row of one pair and the upper row of the next, so it is read once
            var b = raw[0];
            for (var y = 0; y < size - 1; y += 1)
            {
                var a = b;
                b = raw[y + 1];
                var monochrome = ~((a ^ (a >> 1)) | (b ^ (b >> 1)) | (a ^ b)) & edgeMask;
                count += BitMatrix.PopCount(monochrome);
            }

            return (count - Base2By2Blocks) * 3;
        }

        private static int Calc2By2BlocksTwoWords(BitMatrix modules)
        {
            // The identity of Calc2By2BlocksOneWord, applied to each of the two words of modules a
            // row holds, with every shift carrying the bits it drops into the neighbouring word.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 2)
            {
                return 0;
            }

            var edgeMask = GetEdgeMask(size - 1);
            var count = 0;

            for (var y = 0; y < size - 1; y += 1)
            {
                var aOffset = 2 * y;
                var bOffset = aOffset + 2;

                var a0 = raw[aOffset];
                var a1 = raw[aOffset + 1];
                var b0 = raw[bOffset];
                var b1 = raw[bOffset + 1];

                var mono0 = ~((a0 ^ ((a0 >> 1) | (a1 << 63))) | (b0 ^ ((b0 >> 1) | (b1 << 63))) | (a0 ^ b0)) & edgeMask[0];
                var mono1 = ~((a1 ^ (a1 >> 1)) | (b1 ^ (b1 >> 1)) | (a1 ^ b1)) & edgeMask[1];

                count += BitMatrix.PopCount(mono0) + BitMatrix.PopCount(mono1);
            }

            return (count - Base2By2Blocks) * 3;
        }

        private static int Calc2By2BlocksThreeWords(BitMatrix modules)
        {
            // The identity of Calc2By2BlocksOneWord, applied to each of the three words of modules
            // a row holds, with every shift carrying the bits it drops into the neighbouring word.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 2)
            {
                return 0;
            }

            var edgeMask = GetEdgeMask(size - 1);
            var count = 0;

            for (var y = 0; y < size - 1; y += 1)
            {
                var aOffset = BitMatrix.MaxWordsPerRow * y;
                var bOffset = aOffset + BitMatrix.MaxWordsPerRow;

                var a0 = raw[aOffset];
                var a1 = raw[aOffset + 1];
                var a2 = raw[aOffset + 2];
                var b0 = raw[bOffset];
                var b1 = raw[bOffset + 1];
                var b2 = raw[bOffset + 2];

                var mono0 = ~((a0 ^ ((a0 >> 1) | (a1 << 63))) | (b0 ^ ((b0 >> 1) | (b1 << 63))) | (a0 ^ b0)) & edgeMask[0];
                var mono1 = ~((a1 ^ ((a1 >> 1) | (a2 << 63))) | (b1 ^ ((b1 >> 1) | (b2 << 63))) | (a1 ^ b1)) & edgeMask[1];
                var mono2 = ~((a2 ^ (a2 >> 1)) | (b2 ^ (b2 >> 1)) | (a2 ^ b2)) & edgeMask[2];

                count += BitMatrix.PopCount(mono0) + BitMatrix.PopCount(mono1) + BitMatrix.PopCount(mono2);
            }

            return (count - Base2By2Blocks) * 3;
        }

        #endregion

        #region Finder-like patterns

        // Penalty for patterns resembling a finder pattern.
        // Penalty points: N3, where N3 is 40. The pattern is the 1:1:3:1:1 ratio of the finder
        // pattern, i.e. the module sequence "1011101", with at least 4 light modules on one side
        // and at least 1 on the other. Modules beyond the edge of the symbol count as light.
        internal static int CalcFinderPattern(BitMatrix modules)
        {
            switch (modules.UsedWordsPerRow)
            {
                case 1: return CalcFinderPatternOneWord(modules);
                case 2: return CalcFinderPatternTwoWords(modules);
                default: return CalcFinderPatternThreeWords(modules);
            }
        }

        // The three mandatory finder patterns match 9 times in total.
        private const int BaseFinderPattern = 9;

        private static int CalcFinderPatternOneWord(BitMatrix modules)
        {
            var raw = modules.Raw;
            var size = modules.Size;
            var count = 0;

            for (var y = 0; y < size; y += 1)
            {
                count += MatchesInWord(0ul, raw[y], 0ul);
            }

            return (count - BaseFinderPattern) * 40;
        }

        private static int CalcFinderPatternTwoWords(BitMatrix modules)
        {
            var raw = modules.Raw;
            var size = modules.Size;
            var count = 0;

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = 2 * y;
                var w0 = raw[rowOffset];
                var w1 = raw[rowOffset + 1];

                count += MatchesInWord(0ul, w0, w1) + MatchesInWord(w0, w1, 0ul);
            }

            return (count - BaseFinderPattern) * 40;
        }

        private static int CalcFinderPatternThreeWords(BitMatrix modules)
        {
            var raw = modules.Raw;
            var size = modules.Size;
            var count = 0;

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = BitMatrix.MaxWordsPerRow * y;
                var w0 = raw[rowOffset];
                var w1 = raw[rowOffset + 1];
                var w2 = raw[rowOffset + 2];

                // the word past the last one is the always-zero padding word, and light is what the
                // rule wants beyond the edge of the symbol anyway
                count += MatchesInWord(0ul, w0, w1)
                    + MatchesInWord(w0, w1, w2)
                    + MatchesInWord(w1, w2, 0ul);
            }

            return (count - BaseFinderPattern) * 40;
        }

        // Counts the matches beginning in one word of a row, given the words on either side of it.
        //
        // A whole word is matched at once rather than column by column. Bit s of `pattern` is set
        // where the module sequence "1011101" begins at column s: each shift lines up the module at
        // a fixed offset from s, carrying in the bits it needs from the neighbouring word. Where
        // there is no neighbour the shift brings in zeros, which is exactly what the rule asks for,
        // as the modules beyond the edge of the symbol count as light. The pattern's own dark
        // modules keep s within [0, size-7], so no edge mask is needed either.
        //
        // The row layouts differ only in how many times this is called and what they pass for the
        // neighbours, so all of them match by the same identity.
        private static int MatchesInWord(ulong previous, ulong w, ulong next)
        {
            // bit s of rN is the module at column s+N, bit s of lN the module at column s-N
            var r1 = (w >> 1) | (next << 63);
            var r2 = (w >> 2) | (next << 62);
            var r3 = (w >> 3) | (next << 61);
            var r4 = (w >> 4) | (next << 60);
            var r5 = (w >> 5) | (next << 59);
            var r6 = (w >> 6) | (next << 58);
            var r7 = (w >> 7) | (next << 57);
            var r8 = (w >> 8) | (next << 56);
            var r9 = (w >> 9) | (next << 55);
            var r10 = (w >> 10) | (next << 54);
            var l1 = (w << 1) | (previous >> 63);
            var l2 = (w << 2) | (previous >> 62);
            var l3 = (w << 3) | (previous >> 61);
            var l4 = (w << 4) | (previous >> 60);

            var pattern = w & r2 & r3 & r4 & r6 & ~(r1 | r5);
            // both variants of the rule want one light module on either side of the pattern ...
            var enclosed = pattern & ~(l1 | r7);
            // ... and three more on one side or the other, which is where the four come from. A
            // pattern with room on both sides still counts once: ~(l & r) is ~l | ~r.
            return BitMatrix.PopCount(enclosed & ~((l2 | l3 | l4) & (r8 | r9 | r10)));
        }

        #endregion

        #region Color balance

        internal static int CalcColorBalance(BitMatrix modules)
        {
            // Penalty for the proportion of dark modules in the entire symbol.
            // Penalty points: N4 * k, where k is the rating of the deviation of the proportion of
            // the dark modules in the symbol from 50%, in steps of 5%, and N4 is 10.
            var darkModules = modules.PopCount();

            var size = modules.Size;
            var totalNumber = size * size;
            // The deviation in percent is |darkModules / totalNumber - 1/2| * 100, and a step is
            // 5% of it. Scaled by 2 * totalNumber, the whole expression stays exact in integers:
            // the numerator is |2 * darkModules - totalNumber| * 10, the denominator is
            // totalNumber. The integer division rounds down such that a proportion between 45%
            // and 55% does not lead to any penalty points (as per specification).
            var deviationSteps = Math.Abs(2 * darkModules - totalNumber) * 10 / totalNumber;
            return 10 * deviationSteps;
        }

        #endregion

        // The edge mask depends on the matrix size alone, and the rules ask for one on every
        // scan of a wide matrix — up to three per mask pattern evaluated. The cached instances
        // are shared and must not be mutated.
        private static readonly ConcurrentDictionary<int, ulong[]> EdgeMaskCache
            = new ConcurrentDictionary<int, ulong[]>();

        private static readonly Func<int, ulong[]> BuildEdgeMaskFunc = BuildEdgeMask;

        private static ulong[] GetEdgeMask(int validBits)
        {
            return EdgeMaskCache.GetOrAdd(validBits, BuildEdgeMaskFunc);
        }

        // Builds a per-word mask keeping the lowest validBits bits of a wide row and clearing the
        // rest, so the padding past the last column cannot produce a spurious match.
        private static ulong[] BuildEdgeMask(int validBits)
        {
            var validWord = validBits >> 6;
            var validBit = validBits & 0x3F;
            var partialMask = (1ul << validBit) - 1;

            var mask = new ulong[BitMatrix.MaxUsedWordsPerRow];
            for (var w = 0; w < mask.Length; w += 1)
            {
                if (w < validWord)
                {
                    mask[w] = ulong.MaxValue;
                }
                else if (w == validWord)
                {
                    mask[w] = partialMask;
                }
            }
            return mask;
        }
    }
}
