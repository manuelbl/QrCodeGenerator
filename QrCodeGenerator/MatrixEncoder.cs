/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        [SuppressMessage("csharpsquid", "S3776")]
        [SuppressMessage("csharpsquid", "S127")]
        internal static void FillPayload(BitMatrix modules, byte[] codewords, int version)
        {
            var payloadArea = FixedPatterns.GetPayloadAreaMap(version);

            // zigzag up and down with a 2-wide stride, starting in the right bottom corner
            var size = modules.Size;
            var bitLength = codewords.Length * 8;
            var bitIndex = 0;

            // go from right to left in strides of 2
            for (var h = size - 1; h > 0; h -= 2)
            {
                if (h == 6)
                {
                    // skip timing pattern
                    h -= 1;
                }
                var upward = ((size - h - 1) & 2) == 0;

                // go up or down
                for (var v = 0; v < size; v += 1)
                {
                    // determine vertical direction
                    var y = upward ? size - v - 1 : v;

                    // alternate between the 2 columns
                    for (var x = h; x > h - 2; x -= 1)
                    {
                        if (!payloadArea.Get(x, y))
                        {
                            // skip modules not intended for payload
                            continue;
                        }
                        if (bitIndex < bitLength
                            && (codewords[bitIndex >> 3] & (0x80 >> (bitIndex & 0x07))) != 0)
                        {
                            modules.Set(x, y, true);
                        }
                        bitIndex += 1;
                    }
                }
            }

            Trace.Assert(bitIndex >= bitLength && bitIndex <= bitLength + 7);
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

            // Pre-compute the 4 ulongs for each of the 12 distinct pattern rows.
            for (var y = 0; y < 12; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    if (pattern(x, y))
                        matrix.Set(x, y, true);
                }
            }

            // Replicate the pattern vertically.
            var bits = matrix.Raw;
            var srcIndex = 0;
            var destIndex = 4 * 12;
            while (destIndex < size * 4)
            {
                bits[destIndex] = bits[srcIndex];
                srcIndex += 1;
                destIndex += 1;
            }

            return BitMatrix.FromBits(bits);
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
