/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Calculates the penalty for a QR code to determine the optimal mask pattern.
    /// <para>
    /// See "7.8.3 Evaluation of data masking results" in the QR code specification
    /// (ISO/IEC 18004:2024(en) for details.
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
        internal static int CalculatePenalty(BitMatrix modules, BitMatrix transposed, int lowestPenaltySoFar)
        {
            // Ordered by mean penalty contribution (descending) for early-stop
            // effectiveness; see QrCodeGeneratorProfiling/README.md "Penalty Contribution".
            var sum = Calc2By2Blocks(modules);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcSameColor(transposed);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcSameColor(modules);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcFinderPattern(modules);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            sum += CalcFinderPattern(transposed);
            if (sum >= lowestPenaltySoFar)
            {
                return sum;
            }
            return sum + CalcColorBalance(modules);
        }

        internal static int CalcSameColor(BitMatrix modules)
        {
            // Penalty for adjacent modules in a row in the same color.
            // Penalty points: N1 + i, where i is the amount by which the number
            // of adjacent modules of the same color exceeds 5, and N1 is 3.
            //
            // For each row, with T[i] = bits[i] ^ bits[i+1]:
            //   fiveWindow[i] = ~T[i] & ~T[i+1] & ~T[i+2] & ~T[i+3]
            //                  (5 consecutive equal bits at positions i..i+4)
            //   run5Start[i]  = fiveWindow[i] AND (i == 0 OR T[i-1] == 1)
            // A run of length L >= 5 contributes (L - 2) = (L - 4) + 2, where
            // (L - 4) equals the number of 5-windows the run contains.
            // Per row: penalty = popcount(fiveWindow) + 2 * popcount(run5Start).
            // The fiveWindow mask is clipped to positions [0, size-5] so the
            // padding zeros past the row's last column don't create false windows.

            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 5)
            {
                return 0;
            }

            var edgeMask = BuildEdgeMask(size - 4);
            var fiveWindowCount = 0;
            var run5StartCount = 0;

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = 4 * y;
                var w0 = raw[rowOffset];
                var w1 = raw[rowOffset + 1];
                var w2 = raw[rowOffset + 2];
                var w3 = raw[rowOffset + 3];

                var t0 = w0 ^ ((w0 >> 1) | (w1 << 63));
                var t1 = w1 ^ ((w1 >> 1) | (w2 << 63));
                var t2 = w2 ^ ((w2 >> 1) | (w3 << 63));
                var t3 = w3 ^ (w3 >> 1);

                // tz[i] = ~T[i] & ~T[i+1] (3 consecutive equal bits at i..i+2)
                var tz0 = ~(t0 | ((t0 >> 1) | (t1 << 63)));
                var tz1 = ~(t1 | ((t1 >> 1) | (t2 << 63)));
                var tz2 = ~(t2 | ((t2 >> 1) | (t3 << 63)));
                var tz3 = ~(t3 | (t3 >> 1));

                // fw[i] = tz[i] & tz[i+2] (5 consecutive equal bits at i..i+4)
                var fw0 = tz0 & ((tz0 >> 2) | (tz1 << 62)) & edgeMask[0];
                var fw1 = tz1 & ((tz1 >> 2) | (tz2 << 62)) & edgeMask[1];
                var fw2 = tz2 & ((tz2 >> 2) | (tz3 << 62)) & edgeMask[2];
                var fw3 = tz3 & (tz3 >> 2) & edgeMask[3];

                // boundary[i] = T[i-1] (with bit 0 of word 0 forced to mark row start)
                var rs0 = fw0 & ((t0 << 1) | 1ul);
                var rs1 = fw1 & ((t1 << 1) | (t0 >> 63));
                var rs2 = fw2 & ((t2 << 1) | (t1 >> 63));
                var rs3 = fw3 & ((t3 << 1) | (t2 >> 63));

                fiveWindowCount += BitMatrix.PopCount(fw0) + BitMatrix.PopCount(fw1)
                                 + BitMatrix.PopCount(fw2) + BitMatrix.PopCount(fw3);
                run5StartCount += BitMatrix.PopCount(rs0) + BitMatrix.PopCount(rs1)
                                + BitMatrix.PopCount(rs2) + BitMatrix.PopCount(rs3);
            }

            // each finder pattern contributes 2 streaks of 5, 2 streaks of 7 and 1 streak of 8
            return fiveWindowCount + 2 * run5StartCount - 3 * (2 * 3 + 2 * 5 + 6);
        }

        internal static int Calc2By2Blocks(BitMatrix modules)
        {
            // Penalty for 2 by 2 blocks in the same color.
            // Penalty points: N2, where N2 is 3.
            //
            // For each consecutive row pair (A, B), bit x in `monochrome` is set iff
            // the 2x2 block starting at (x, y) is monochrome:
            //   monochrome = ~((A ^ (A>>1)) | (B ^ (B>>1)) | (A ^ B))
            // Bits at x >= size-1 are masked off via a precomputed edge mask.
            var raw = modules.Raw;
            var size = modules.Size;
            if (size < 2)
            {
                return 0;
            }

            var edgeMask = BuildEdgeMask(size - 1);

            var count = 0;
            for (var y = 0; y < size - 1; y += 1)
            {
                var aOffset = 4 * y;
                var bOffset = aOffset + 4;
                for (var w = 0; w < 4; w += 1)
                {
                    var a = raw[aOffset + w];
                    var b = raw[bOffset + w];
                    var aNext = w < 3 ? raw[aOffset + w + 1] : 0ul;
                    var bNext = w < 3 ? raw[bOffset + w + 1] : 0ul;
                    var aShift = (a >> 1) | (aNext << 63);
                    var bShift = (b >> 1) | (bNext << 63);
                    var monochrome = ~((a ^ aShift) | (b ^ bShift) | (a ^ b)) & edgeMask[w];
                    count += BitMatrix.PopCount(monochrome);
                }
            }

            // subtract the 3 x 4 blocks contributed by the finder patterns
            return (count - 4 * 3) * 3;
        }

        private static ulong[] BuildEdgeMask(int validBits)
        {
            var validWord = validBits >> 6;
            var validBit = validBits & 0x3F;
            var partialMask = (1ul << validBit) - 1;
            var mask = new ulong[4];
            for (var w = 0; w < 4; w += 1)
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

        internal static int CalcFinderPattern(BitMatrix modules)
        {
            // Count occurrences of the bit pattern "1011101" (1:1:3:1:1) in a row,
            // with at least 4 zero bits on one side and at least 1 zero bit on the
            // other side. Bits beyond the row are treated as zero.
            // A 15-bit window slides one column at a time.

            var raw = modules.Raw;
            var size = modules.Size;
            var count = 0;

            const ulong patternBits = 0x5D0;      // "1011101" at bits 4..10
            const ulong patternMask1 = 0x0FFF;    // bits 0..11 (4 zeros left + finder pattern + 1 zero right)
            const ulong patternMask2 = 0x7FF8;    // bits 3..14 (1 zero left + finder pattern + 4 zeros right)

            for (var y = 0; y < size; y += 1)
            {
                var rowOffset = 4 * y;
                var window = (raw[rowOffset] & 0x3ff) << 5;
                for (var c = 10; c < size + 4; c += 1)
                {
                    var bit = (raw[rowOffset + (c >> 6)] >> (c & 0x3F)) & 1ul;
                    window = (window >> 1) | (bit << 14);
                    if ((window & patternMask1) == patternBits || (window & patternMask2) == patternBits)
                    {
                        count += 1;
                    }
                }
            }

            // The mandatory finder pattern leads to 9 matches.
            // Subtract them.
            return (count - 9) * 40;
        }

        internal static int CalcColorBalance(BitMatrix modules)
        {
            // Penalty for the proportion of dark modules in the entire symbol.
            // Penalty points: N4 * k, where k is the rating of the deviation of the proportion of
            // the dark modules in the symbol from 50%, in steps of 5%, and N4 is 10.
            var darkModules = modules.PopCount();

            var size = modules.Size;
            var totalNumber = size * size;
            var deviation = Math.Abs(darkModules - totalNumber / 2);
            var step = totalNumber / 20;
            // The integer division rounds down such that a proportion between 45% and 55%
            // does not lead to any penalty points (as per specification).
            var deviationSteps = deviation / step;
            return 10 * deviationSteps;
        }

    }
}
