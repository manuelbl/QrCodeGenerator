/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    internal static class QrCodeBuilder
    {
        #region Caches

        private static readonly ConcurrentDictionary<(int, int), BitMatrix> DataMaskPatternCache = new ConcurrentDictionary<(int, int), BitMatrix>();
        private static readonly ConcurrentDictionary<(int, int), BitMatrix> DataMaskPatternTransposedCache = new ConcurrentDictionary<(int, int), BitMatrix>();

        #endregion

        #region Build

        internal static QrCode Build(List<DataSegment> dataSegments, int ecc, int minVersion = 1, int maxVersion = 40, bool boostEcc = true, EncodingInfo encodingInfo = null)
        {
            if (encodingInfo != null)
            {
                encodingInfo.DataSegments = dataSegments;
            }

            var result = FindVersionAndEcc(dataSegments, ecc, minVersion, maxVersion, boostEcc);
            var version = result.Item1;
            ecc = result.Item2;

            var codewords = BuildCodewords(dataSegments, version, ecc);
            codewords = AddErrorCorrection(codewords, version, ecc);
            return Build(codewords, ecc, version, encodingInfo);
        }

        private static QrCode Build(byte[] codewords, int ecc, int version, EncodingInfo encodingInfo)
        {
            var modules = FixedPatterns.CreateWithFixedPatterns(version);
            FillPayload(modules, codewords, version);
            var pattern = ApplyBestPattern(modules, version, ecc, encodingInfo);
            return new QrCode(modules, (QrCode.Ecc)ecc, pattern);
        }

        #endregion

        #region Version and ECC level
        
        /// <summary>
        /// Finds the best version and error correction level for the given data segments.
        /// <para>
        /// If <paramref name="improveEcc"/> is <c>true</c>, <paramref name="ecc"/> will be considered
        /// the minimal error correction level and the error correction level will be increased
        /// if it is possible without increasing the version.
        /// </para>
        /// <para>
        /// If <paramref name="minVersion"/> and <paramref name="maxVersion"/> specify a range,
        /// the smallest version that fits the data will be selected.
        /// </para>
        /// </summary>
        /// <param name="dataSegments">The data segments to encode.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <param name="minVersion">The minimal QR code version.</param>
        /// <param name="maxVersion">The maximal QR code version, or -1 to automatically select the version.</param>
        /// <param name="improveEcc">If <c>true</c>, increase the error correction level if possible.</param>
        /// <returns>A tuple containing the error correction level and version.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the version is out of range.</exception>
        /// <exception cref="DataTooLongException">Thrown if the data does not fit into the QR code of the given version.</exception>
        internal static Tuple<int, int> FindVersionAndEcc(List<DataSegment> dataSegments, int ecc,
            int minVersion = 1, int maxVersion = 40, bool improveEcc = true)
        {
            var versionResult = FindSmallestVersion(dataSegments, ecc, minVersion, maxVersion);
            var version = versionResult.Item1;
            var bitLength = versionResult.Item2;

            if (improveEcc)
            {
                while (ecc < 3 && Fits(bitLength, version, ecc + 1))
                {
                    ecc += 1;
                }
            }
            
            return new Tuple<int, int>(version, ecc);
        }

        private static Tuple<int, int> FindSmallestVersion(List<DataSegment> dataSegments, int ecc, int minVersion, int maxVersion)
        {
            var bitLength = 0;
            int version;
            // find the smallest version that fits the data
            for (version = minVersion; version <= maxVersion; version += 1)
            {
                if (version == minVersion || version == 1 || version == 10 || version == 27)
                {
                    // update the bit length as it changes at these versions
                    bitLength = DataSegment.GetBitLength(dataSegments, version);
                }

                if (Fits(bitLength, version, ecc))
                {
                    break;
                }
                
                if (version == maxVersion)
                {
                    if (version < 40)
                    {
                        throw new DataTooLongException(
                            $"Data is too long to fit into a QR code with version {version} and error correction level {"LMQH"[ecc]}.");
                    }
                    
                    throw new DataTooLongException(
                        $"Data is too long to fit into a QR code with error correction level {"LMQH"[ecc]}");
                }
            }
            
            return new Tuple<int, int>(version, bitLength);
        }
        
        #endregion
        
        #region Codewords with ECC

        /// <summary>
        /// Builds the data codewords for the given data segments.
        /// <para>
        /// The result includes the terminator and the padding for the
        /// given QR code version and error correction level.
        /// </para>
        /// </summary>
        /// <param name="dataSegments">The data segments to encode.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The data codewords.</returns>
        internal static byte[] BuildCodewords(List<DataSegment> dataSegments, int version, int ecc)
        {
            var capacity = GetCodewordDataCapacity(version, ecc);
            var bitStream = DataSegment.CreateBitStream(dataSegments, version, capacity);
            var bitstreamLength = bitStream.Length;
            
            // TODO: avoid allocation of another array
            var result = new byte[capacity];
            bitStream.CopyCodewords(result, 0);
            
            // add padding
            for (var index = (bitstreamLength + 7) / 8; index < capacity; index += 2)
            {
                result[index] = 0b1110_1100;
            }
            for (var index = (bitstreamLength + 15) / 8; index < capacity; index += 2)
            {
                result[index] = 0b0001_0001;
            }
            
            return result;
        }

        /// <summary>
        /// Adds the error correction to the given codewords and returns the combined result.
        /// <para>
        /// The result is transposed and interleaved as per specification.
        /// </para>
        /// </summary>
        /// <param name="codewords">The data codewords.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The combined result of data and error correction codewords.</returns>
        internal static byte[] AddErrorCorrection(byte[] codewords, int version, int ecc)
        {
            var numDataCodewords = codewords.Length;
            var numBlocks = GetNumBlocks(version, ecc);
            var smallBlockDataLength = numDataCodewords / numBlocks;
            var eccBlockLength = (GetCodewordCapacity(version) - numDataCodewords) / numBlocks;
            var numLargeBlocks = numDataCodewords % numBlocks;
            var numSmallBlocks = numBlocks - numLargeBlocks;
            
            var result = new byte[GetCodewordCapacity(version)];
            var dataOffset = 0;
            var reedSolomon = ReedSolomon.GeneratorForCapacity(eccBlockLength);
            
            // split into blocks and process each block separately
            for (var block = 0; block < numBlocks; block += 1)
            {
                var dataLength = block < numSmallBlocks ? smallBlockDataLength : smallBlockDataLength + 1;
                
                // compute the error correction codewords
                var eccCodewords = reedSolomon.ComputeErrorCorrection(new ArraySegment<byte>(codewords, dataOffset, dataLength));
                
                // copy the data and error correction codewords to the transposed result
                for (var i = 0; i < smallBlockDataLength; i += 1)
                    result[i * numBlocks + block] = codewords[dataOffset + i];
                if (block >= numSmallBlocks)
                    result[numBlocks * smallBlockDataLength + block - numSmallBlocks] = codewords[dataOffset + dataLength - 1];
                for (var i = 0; i < eccBlockLength; i += 1)
                    result[numDataCodewords + i * numBlocks + block] = eccCodewords[i];
                
                dataOffset += dataLength;
            }
            
            return result;
        }
        
        #endregion
        
        #region Format information

        internal static void DrawFormatInformation(BitMatrix modules, int ecc, int pattern)
        {
            DrawFormatBits(modules, GetFormatInformationBits(ecc, pattern));
        }

        internal static void DrawFormatInformation(BitMatrix modules, BitMatrix transposed, int ecc, int pattern)
        {
            DrawFormatBits(modules, transposed, GetFormatInformationBits(ecc, pattern));
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

        #region Payload and mask patterns

        [SuppressMessage("csharpsquid", "S3776")]
        [SuppressMessage("csharpsquid", "S127")]
        internal static void FillPayload(BitMatrix modules, byte[] codewords, int version)
        {
            var dataMask = FixedPatterns.GetDataMask(version);

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
                        if (!dataMask.Get(x, y))
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
            pattern.And(FixedPatterns.GetDataMask(key.version));
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
            var size = GetSize(version);
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

                int penalty;
                if (encodingInfo != null) {
                    penalty = Penalty.CalculatePenaltyFully(modules, transposed, ref encodingInfo.Penalties[pattern]);
                }
                else
                {
                    penalty = Penalty.CalculatePenalty(modules, transposed, lowestPenalty);
                }

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
        
        #region Capacity

        /// <summary>
        /// Tests if the given number of data bits will fit into a QR code of the given version and error correction level.
        /// </summary>
        /// <param name="bitLength">The number of data bits.</param>
        /// <param name="version">The version of the QR code.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>True if the data fits, false otherwise.</returns>
        private static bool Fits(int bitLength, int version, int ecc)
        {
            return bitLength <= 8 * GetCodewordDataCapacity(version, ecc);
        }

        /// <summary>
        /// Gets the number of data codewords for a QR code of the given version and error correction level.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The number of codewords.</returns>
        internal static int GetCodewordDataCapacity(int version, int ecc)
        {
            return CodewordDataCapacity[ecc, version - 1];
        }
        
        // See table 7 "Number of symbol characters and input data capacity for QR Code"
        // in QR code specification (ISO/IEC 18004:2015(E))
        private static readonly int[,] CodewordDataCapacity =
        {
            // ECC L
            {
                19, 34, 55, 80, 108, 136, 156, 194, 232, 274,
                324, 370, 428, 461, 523, 589, 647, 721, 795, 861,
                932, 1006, 1094, 1174, 1276, 1370, 1468, 1531, 1631, 1735,
                1843, 1955, 2071, 2191, 2306, 2434, 2566, 2702, 2812, 2956
            },
            // ECC M 
            {
                16, 28, 44, 64, 86, 108, 124, 154, 182, 216,
                254, 290, 334, 365, 415, 453, 507, 563, 627, 669,
                714, 782, 860, 914, 1000, 1062, 1128, 1193, 1267, 1373,
                1455, 1541, 1631, 1725, 1812, 1914, 1992, 2102, 2216, 2334
            },
            // ECC Q
            {
                13, 22, 34, 48, 62, 76, 88, 110, 132, 154,
                180, 206, 244, 261, 295, 325, 367, 397, 445, 485,
                512, 568, 614, 664, 718, 754, 808, 871, 911, 985,
                1033, 1115, 1171, 1231, 1286, 1354, 1426, 1502, 1582, 1666
            },
            // ECC H
            {
                9, 16, 26, 36, 46, 60, 66, 86, 100, 122,
                140, 158, 180, 197, 223, 253, 283, 313, 341, 385,
                406, 442, 464, 514, 538, 596, 628, 661, 701, 745,
                793, 845, 901, 961, 986, 1054, 1096, 1142, 1222, 1276
            }
        };

        /// <summary>
        /// Gets the number of blocks for a QR code of the given version and error correction level.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The number of blocks.</returns>
        internal static int GetNumBlocks(int version, int ecc)
        {
            return NumBlocks[ecc, version - 1];
        }
        
        // See table 9 "Error correction characteristics for QR Code"
        // in QR code specification (ISO/IEC 18004:2015(E))
        private static readonly int[,] NumBlocks =
        {
            // ECC L
            {
                1, 1, 1, 1, 1, 2, 2, 2, 2, 4,
                4, 4, 4, 4, 6, 6, 6, 6, 7, 8,
                8, 9, 9, 10, 12, 12, 12, 13, 14, 15,
                16, 17, 18, 19, 19, 20, 21, 22, 24, 25
            },
            // ECC M
            {
                1, 1, 1, 2, 2, 4, 4, 4, 5, 5,
                5, 8, 9, 9, 10, 10, 11, 13, 14, 16,
                17, 17, 18, 20, 21, 23, 25, 26, 28, 29,
                31, 33, 35, 37, 38, 40, 43, 45, 47, 49
            },
            // ECC Q
            {
                1, 1, 2, 2, 4, 4, 6, 6, 8, 8,
                8, 10, 12, 16, 12, 17, 16, 18, 21, 20,
                23, 23, 25, 27, 29, 34, 34, 35, 38, 40,
                43, 45, 48, 51, 53, 56, 59, 62, 65, 68
            },
            // ECC H
            {
                1, 1, 2, 4, 4, 4, 5, 6, 8, 8,
                11, 11, 16, 16, 18, 16, 19, 21, 25, 25,
                25, 34, 30, 32, 35, 37, 40, 42, 45, 48,
                51, 54, 57, 60, 63, 66, 70, 74, 77, 81
            }
        };

        /// <summary>
        /// Gets the side length (in modules) of a QR code of the given version.
        /// </summary>
        /// <param name="version">The version (1–40).</param>
        /// <returns>The number of modules per side.</returns>
        internal static int GetSize(int version)
        {
            return 17 + version * 4;
        }

        /// <summary>
        /// Gets the total number of codewords (data and error correction) for a QR code of the given version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>The number of codewords.</returns>
        internal static int GetCodewordCapacity(int version)
        {
            return CodewordCapacity[version - 1];
        }
        
        // See table 9 "Error correction characteristics for QR Code"
        // in QR code specification (ISO/IEC 18004:2015(E))
        private static readonly int[] CodewordCapacity =
        {
            26, 44, 70, 100, 134, 172, 196, 242, 292, 346,
            404, 466, 532, 581, 655, 733, 815, 901, 991, 1085,
            1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185,
            2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706
        };
        
        #endregion
        
        #region Precomputed data
        
        internal static int[] GetAlignmentPatternPosition(int version)
        {
            return AlignmentPatternPosition[version - 1];
        }
        
        // See table E.1 "Row/column coordinates of center module of alignment patterns"
        // in QR code specification (ISO/IEC 18004:2015(E))
        private static readonly int[][]AlignmentPatternPosition =
        {
            new [] { 0 },
            new [] { 6, 18 },
            new [] { 6, 22 },
            new [] { 6, 26 },
            new [] { 6, 30 },
            new [] { 6, 34 },
            new [] { 6, 22, 38 },
            new [] { 6, 24, 42 },
            new [] { 6, 26, 46 },
            new [] { 6, 28, 50 },
            new [] { 6, 30, 54 },
            new [] { 6, 32, 58 },
            new [] { 6, 34, 62 },
            new [] { 6, 26, 46, 66 },
            new [] { 6, 26, 48, 70 },
            new [] { 6, 26, 50, 74 },
            new [] { 6, 30, 54, 78 },
            new [] { 6, 30, 56, 82 },
            new [] { 6, 30, 58, 86 },
            new [] { 6, 34, 62, 90 },
            new [] { 6, 28, 50, 72, 94 },
            new [] { 6, 26, 50, 74, 98 },
            new [] { 6, 30, 54, 78, 102 },
            new [] { 6, 28, 54, 80, 106 },
            new [] { 6, 32, 58, 84, 110 },
            new [] { 6, 30, 58, 86, 114 },
            new [] { 6, 34, 62, 90, 118 },
            new [] { 6, 26, 50, 74, 98, 122 },
            new [] { 6, 30, 54, 78, 102, 126 },
            new [] { 6, 26, 52, 78, 104, 130 },
            new [] { 6, 30, 56, 82, 108, 134 },
            new [] { 6, 34, 60, 86, 112, 138 },
            new [] { 6, 30, 58, 86, 114, 142 },
            new [] { 6, 34, 62, 90, 118, 146 },
            new [] { 6, 30, 54, 78, 102, 126, 150 },
            new [] { 6, 24, 50, 76, 102, 128, 154 },
            new [] { 6, 28, 54, 80, 106, 132, 158 },
            new [] { 6, 32, 58, 84, 110, 136, 162 },
            new [] { 6, 26, 54, 82, 110, 138, 166 },
            new [] { 6, 30, 58, 86, 114, 142, 170 }
        };

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

        internal static int GetErrorCorrectionLevelBits(int level)
        {
            // See table 12 – Error correction level indicators for QR Code symbols
            // L - 01
            // M - 00
            // Q - 11
            // H - 10
            return level ^ 1;
        }
        
        internal static int GetFormatInformationBits(int ecc, int pattern)
        {
            return FormatInformationBits[(ecc << 3) + pattern];
        }
        
        private static readonly int[] FormatInformationBits = {
            // ECC Low
            0x77c4, 0x72f3, 0x7daa, 0x789d, 0x662f, 0x6318, 0x6c41, 0x6976,
            // ECC Medium
            0x5412, 0x5125, 0x5e7c, 0x5b4b, 0x45f9, 0x40ce, 0x4f97, 0x4aa0,
            // ECC Quartile
            0x355f, 0x3068, 0x3f31, 0x3a06, 0x24b4, 0x2183, 0x2eda, 0x2bed,
            // ECC High
            0x1689, 0x13be, 0x1ce7, 0x19d0, 0x0762, 0x0255, 0x0d0c, 0x083b
        };

        internal static int GetVersionInformationBits(int version)
        {
            return VersionInformationBits[version - 7];
        }

        private static readonly int[] VersionInformationBits = {
            // Version 7 to 40
            0x07c94, 0x085bc, 0x09a99, 0x0a4d3,
            0x0bbf6, 0x0c762, 0x0d847, 0x0e60d, 0x0f928, 0x10b78, 0x1145d, 0x12a17, 0x13532, 0x149a6,
            0x15683, 0x168c9, 0x177ec, 0x18ec4, 0x191e1, 0x1afab, 0x1b08e, 0x1cc1a, 0x1d33f, 0x1ed75,
            0x1f250, 0x209d5, 0x216f0, 0x228ba, 0x2379f, 0x24b0b, 0x2542e, 0x26a64, 0x27541, 0x28c69
        };

        #endregion
    }
}