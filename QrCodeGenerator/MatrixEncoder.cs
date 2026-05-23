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

        private static readonly ConcurrentDictionary<(int, int), BitMatrix> DataMaskPatternCache = new ConcurrentDictionary<(int, int), BitMatrix>();
        private static readonly ConcurrentDictionary<(int, int), BitMatrix> DataMaskPatternTransposedCache = new ConcurrentDictionary<(int, int), BitMatrix>();

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

            Debug.Assert(bitIndex >= bitLength && bitIndex <= bitLength + 7);
        }

        #endregion

        #region Mask patterns

        /// <summary>
        /// Returns a <see cref="BitMatrix"/> for the given mask pattern.
        /// It only affects the area where payload data goes.
        /// <para>
        /// The returned matrix is shared and cached. Callers must not mutate it.
        /// </para>
        /// </summary>
        /// <param name="patternIndex">The data mask pattern index.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>A shared BitMatrix instance.</returns>
        private static BitMatrix GetDataMaskPattern(int patternIndex, int version)
        {
            return DataMaskPatternCache.GetOrAdd((patternIndex, version), CreateDataMaskPattern);
        }

        private static BitMatrix CreateDataMaskPattern((int patternIndex, int version) key)
        {
            var pattern = CreatePattern(key.patternIndex, key.version);
            pattern.And(FixedPatterns.GetPayloadAreaMap(key.version));
            return pattern;
        }

        /// <summary>
        /// Returns the transpose of <see cref="GetDataMaskPattern"/> for the given mask pattern.
        /// <para>
        /// The returned matrix is shared and cached. Callers must not mutate it.
        /// </para>
        /// </summary>
        /// <param name="patternIndex">The data mask pattern index.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>A shared BitMatrix instance.</returns>
        private static BitMatrix GetDataMaskPatternTransposed(int patternIndex, int version)
        {
            return DataMaskPatternTransposedCache.GetOrAdd((patternIndex, version), CreateDataMaskPatternTransposed);
        }

        private static BitMatrix CreateDataMaskPatternTransposed((int patternIndex, int version) key)
        {
            var pattern = GetDataMaskPattern(key.patternIndex, key.version).Copy();
            pattern.Transpose();
            return pattern;
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
        // Penalty.CalculatePenalty(). See QrCodeGeneratorProfiling/README.md
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
            var transposed = modules.Copy();
            transposed.Transpose();

            var bestPattern = -1;
            var lowestPenalty = int.MaxValue;

            foreach (var pattern in PatternEvaluationOrder)
            {
                DrawFormatInformation(modules, transposed, ecc, pattern);
                var mask = GetDataMaskPattern(pattern, version);
                var maskT = GetDataMaskPatternTransposed(pattern, version);
                // apply pattern
                modules.Xor(mask);
                transposed.Xor(maskT);

                var penalty = encodingInfo == null
                    ? Penalty.CalculatePenalty(modules, transposed, lowestPenalty)
                    : Penalty.CalculatePenaltyFully(modules, transposed, ref encodingInfo.Penalties[pattern]);

                // undo pattern
                modules.Xor(mask);
                transposed.Xor(maskT);
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

            DrawFormatInformation(modules, ecc, bestPattern);
            var bestMask = GetDataMaskPattern(bestPattern, version);
            modules.Xor(bestMask);
            return bestPattern;
        }

        #endregion

        #region Format information

        internal static void DrawFormatInformation(BitMatrix modules, int ecc, int pattern)
        {
            DrawFormatBits(modules, QrCodeParameters.GetFormatInformationBits(ecc, pattern));
        }

        internal static void DrawFormatInformation(BitMatrix modules, BitMatrix transposed, int ecc, int pattern)
        {
            DrawFormatBits(modules, transposed, QrCodeParameters.GetFormatInformationBits(ecc, pattern));
        }

        private static void DrawFormatBits(BitMatrix modules, int formatBits)
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

        private static void DrawFormatBits(BitMatrix modules, BitMatrix transposed, int formatBits)
        {
            var size = modules.Size;

            for (var i = 0; i < 8; i += 1)
            {
                SetFormatBit(modules, transposed, size - 1 - i, 8, formatBits, i);
            }
            for (var i = 8; i < 15; i += 1)
            {
                SetFormatBit(modules, transposed, 8, size - 15 + i, formatBits, i);
            }
            for (var i = 0; i < 6; i += 1)
            {
                SetFormatBit(modules, transposed, 8, i, formatBits, i);
            }

            SetFormatBit(modules, transposed, 8, 7, formatBits, 6);
            SetFormatBit(modules, transposed, 8, 8, formatBits, 7);
            SetFormatBit(modules, transposed, 7, 8, formatBits, 8);

            for (var i = 9; i < 15; i += 1)
            {
                SetFormatBit(modules, transposed, 14 - i, 8, formatBits, i);
            }
        }

        private static void SetFormatBit(BitMatrix modules, int x, int y, int bits, int bitIndex)
        {
            modules.Set(x, y, (bits & (1 << bitIndex)) != 0);
        }

        [SuppressMessage("csharpsquid", "S2234")]
        private static void SetFormatBit(BitMatrix modules, BitMatrix transposed, int x, int y, int bits, int bitIndex)
        {
            var value = (bits & (1 << bitIndex)) != 0;
            modules.Set(x, y, value);
            transposed.Set(y, x, value);
        }

        #endregion
    }
}
