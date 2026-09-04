/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Builds the finished module matrix from the interleaved codewords: draws the
    /// fixed patterns, fills the payload into the free modules, then selects and
    /// applies the lowest-penalty mask pattern.
    /// </summary>
    internal static class MatrixEncoder
    {
        #region Caches

        private static readonly ConcurrentDictionary<(int, int), MaskPair> MaskPatternCache = new ConcurrentDictionary<(int, int), MaskPair>();

        private static readonly ConcurrentDictionary<int, ushort[]> PayloadTargetCache = new ConcurrentDictionary<int, ushort[]>();

        #endregion

        #region Encode

        /// <summary>
        /// Encodes the given interleaved codewords into a finished <see cref="QrCode"/>.
        /// </summary>
        /// <param name="codewords">The interleaved data and error correction codewords.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <param name="encodingInfo">Optional diagnostics sink, or <c>null</c>.</param>
        /// <returns>The finished QR code.</returns>
        internal static QrCode Encode(byte[] codewords, int version, int ecc, EncodingInfo encodingInfo)
        {
            var modules = FixedPatterns.CreateWithFixedPatterns(version);
            FillPayload(modules, codewords, version);
            var pattern = ApplyBestPattern(modules, version, ecc, encodingInfo);
            return new QrCode(modules, (QrCode.Ecc)ecc, pattern);
        }

        #endregion

        #region Payload

        /// <summary>
        /// Fills the payload bits into the modules the fixed patterns leave free.
        /// <para>
        /// Which module a codeword bit lands on depends on the version alone, so the walk itself
        /// is precomputed by <see cref="GetPayloadTargets"/> and this is a flat pass over that
        /// table: one codeword bit and one target per iteration, and no coordinate arithmetic.
        /// Nor is there a branch on the bit — a light module ORs in a zero and leaves its word
        /// unchanged — because a codeword bit is as good as random and would mispredict half the
        /// time.
        /// </para>
        /// <para>
        /// The QR code has room for a few bits more than the codewords occupy; those remainder
        /// bits are left light, which is why the pass stops at the codewords rather than at the
        /// end of the table.
        /// </para>
        /// </summary>
        /// <param name="modules">
        /// The module matrix with the fixed patterns already drawn. It must have the version's
        /// size, since the targets address its words directly, and its payload area must be clear.
        /// </param>
        /// <param name="codewords">The interleaved data and error correction codewords.</param>
        /// <param name="version">The QR code version.</param>
        internal static void FillPayload(BitMatrix modules, byte[] codewords, int version)
        {
            var targets = GetPayloadTargets(version);
            var bitCount = Math.Min(codewords.Length * 8, targets.Length);

            for (var i = 0; i < bitCount; i += 1)
            {
                var bit = (codewords[i >> 3] >> (7 - (i & 0x07))) & 1;
                modules.OrBit(targets[i], bit);
            }
        }

        /// <summary>
        /// Returns the modules the payload occupies, in the order the codeword bits fill them.
        /// <para>
        /// Each entry is a <see cref="BitMatrix.Address"/>, so a caller writes the bits with
        /// <see cref="BitMatrix.OrBit"/>. The table is as long as the payload area, one entry per
        /// module: a version whose codewords fill the area exactly but for the remainder bits uses
        /// all but the last few.
        /// </para>
        /// <para>
        /// The returned array is shared and cached. Callers must not mutate it.
        /// </para>
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The shared table of addresses.</returns>
        private static ushort[] GetPayloadTargets(int version)
        {
            return PayloadTargetCache.GetOrAdd(version, ComputePayloadTargets);
        }

        /// <summary>
        /// Walks the payload zigzag of a version and records the module it visits at each step.
        /// <para>
        /// The codewords are laid out in a zigzag of two-column strides, starting in the bottom
        /// right corner and skipping the reserved modules. The walk covers every column but the
        /// vertical timing pattern, which is reserved over its full height, so it visits the
        /// payload area exactly once and its population count is the table's length.
        /// </para>
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The table of addresses.</returns>
        [SuppressMessage("csharpsquid", "S127")]
        private static ushort[] ComputePayloadTargets(int version)
        {
            var payloadArea = FixedPatterns.GetPayloadAreaMap(version);
            var size = payloadArea.Size;
            var targets = new ushort[payloadArea.PopCount()];
            var count = 0;

            // right to left, in strides of two columns
            for (var h = size - 1; h > 0; h -= 2)
            {
                if (h == 6)
                {
                    h -= 1; // skip the vertical timing pattern
                }

                var upward = ((size - h - 1) & 2) == 0;

                for (var v = 0; v < size; v += 1)
                {
                    var y = upward ? size - v - 1 : v;

                    // alternate between the two columns of the stride
                    for (var x = h; x > h - 2; x -= 1)
                    {
                        if (payloadArea.Get(x, y))
                        {
                            targets[count] = payloadArea.Address(x, y);
                            count += 1;
                        }
                    }
                }
            }

            return targets;
        }

        #endregion

        #region Mask patterns

        /// <summary>
        /// Returns the <see cref="MaskPair"/> for the given mask pattern and version.
        /// It only affects the area where payload data goes.
        /// <para>
        /// The returned mask pair is shared and cached. Callers must not mutate it.
        /// </para>
        /// </summary>
        /// <param name="patternIndex">The data mask pattern index.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>A shared mask pair.</returns>
        private static MaskPair GetMaskPair(int patternIndex, int version)
        {
            return MaskPatternCache.GetOrAdd((patternIndex, version), CreateMaskPair);
        }

        private static MaskPair CreateMaskPair((int patternIndex, int version) key)
        {
            var rows = CreatePattern(key.patternIndex, key.version);
            rows.And(FixedPatterns.GetPayloadAreaMap(key.version));
            var columns = rows.Copy();
            columns.Transpose();
            return new MaskPair(rows, columns);
        }

        /// <summary>
        /// Creates a <see cref="BitMatrix"/> initialized with the pattern
        /// for the given pattern index and QR code version.
        /// </summary>
        /// <param name="patternIndex">The pattern index (0–7).</param>
        /// <param name="version">The QR code version (1–40).</param>
        /// <returns>A new bit matrix filled with the repeating pattern.</returns>
        private static BitMatrix CreatePattern(int patternIndex, int version)
        {
            var pattern = PatternFunctions[patternIndex];
            var size = QrCodeParameters.GetSize(version);
            var matrix = new BitMatrix(size);

            // Pre-compute the words of each of the 12 distinct pattern rows.
            for (var y = 0; y < 12; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    if (pattern(x, y))
                        matrix.Set(x, y, true);
                }
            }

            // Replicate the pattern vertically. All mask patterns repeat with a period of 12 rows,
            // so each row is a word-for-word copy of the row 12 above it.
            var bits = matrix.Raw;
            var srcIndex = 0;
            var destIndex = matrix.WordsPerRow * 12;
            while (destIndex < bits.Length)
            {
                bits[destIndex] = bits[srcIndex];
                srcIndex += 1;
                destIndex += 1;
            }

            return matrix;
        }

        // Ordered by mask-selection frequency (descending) so low-penalty patterns
        // set a tight lowestPenalty early, maximizing early-stop bailouts in
        // Penalty.Calculate(). See QrCodeGeneratorProfiling/README.md
        // "Mask Pattern Selection".
        private static readonly int[] PatternEvaluationOrder = { 2, 3, 7, 4, 6, 5, 0, 1 };

        private static readonly Func<int, int, bool>[] PatternFunctions =
        {
            (x, y) => (x + y) % 2 == 0,
            (x, y) => y % 2 == 0,
            (x, y) => x % 3 == 0,
            (x, y) => (x + y) % 3 == 0,
            (x, y) => (x / 3 + y / 2) % 2 == 0,
            (x, y) => x * y % 2 + x * y % 3 == 0,
            (x, y) => (x * y % 2 + x * y % 3) % 2 == 0,
            (x, y) => ((x + y) % 2 + x * y % 3) % 2 == 0
        };

        private static int ApplyBestPattern(BitMatrix modules, int version, int ecc, EncodingInfo encodingInfo = null)
        {
            var scoringMatrix = ScoringMatrix.From(modules);

            var bestPattern = -1;
            var lowestPenalty = int.MaxValue;

            foreach (var pattern in PatternEvaluationOrder)
            {
                DrawFormatInformation(scoringMatrix, ecc, pattern);
                var mask = GetMaskPair(pattern, version);
                // apply pattern
                scoringMatrix.Xor(mask);

                var penalty = encodingInfo == null
                    ? Penalty.Calculate(scoringMatrix, lowestPenalty)
                    : Penalty.CalculateFully(scoringMatrix, ref encodingInfo.Penalties[pattern]);

                // undo pattern
                scoringMatrix.Xor(mask);
                if (penalty < lowestPenalty)
                {
                    lowestPenalty = penalty;
                    bestPattern = pattern;
                }
            }

            if (encodingInfo != null && encodingInfo.ForcedDataMask >= 0)
            {
                bestPattern = encodingInfo.ForcedDataMask;
            }

            DrawFormatInformation(scoringMatrix, ecc, bestPattern);
            // Finalize in place: Rows aliases `modules`, which the caller turns into the QrCode.
            scoringMatrix.Finish(GetMaskPair(bestPattern, version));
            return bestPattern;
        }

        #endregion

        #region Format information

        internal static void DrawFormatInformation(ScoringMatrix modules, int ecc, int pattern)
        {
            DrawFormatBits(modules, QrCodeParameters.GetFormatInformationBits(ecc, pattern));
        }

        private static void DrawFormatBits(ScoringMatrix modules, int formatBits)
        {
            var size = modules.Size;

            for (var i = 0; i < 8; i += 1)
            {
                SetFormatBit(modules, size - 1 - i, 8, formatBits, i);
            }
            for (var i = 8; i < 15; i += 1)
            {
                SetFormatBit(modules, 8, size - 15 + i, formatBits, i);
            }
            for (var i = 0; i < 6; i += 1)
            {
                SetFormatBit(modules, 8, i, formatBits, i);
            }

            SetFormatBit(modules, 8, 7, formatBits, 6);
            SetFormatBit(modules, 8, 8, formatBits, 7);
            SetFormatBit(modules, 7, 8, formatBits, 8);

            for (var i = 9; i < 15; i += 1)
            {
                SetFormatBit(modules, 14 - i, 8, formatBits, i);
            }
        }

        private static void SetFormatBit(ScoringMatrix modules, int x, int y, int bits, int bitIndex)
        {
            modules.SetFormatBit(x, y, (bits & (1 << bitIndex)) != 0);
        }

        #endregion
    }
}
