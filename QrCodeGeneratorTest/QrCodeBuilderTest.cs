/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class QrCodeBuilderTest
    {
        #region Test initialization
    
        static QrCodeBuilderTest()
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }
    
#endregion

        [Theory]
        [InlineData(10, 1, 1, 0, false, 0)]
        [InlineData(10, 1, 1, 1, false, 1)]
        [InlineData(10, 1, 1, 1, true, 3)]
        [InlineData(100, -1, 4, 0, true, 0)]
        [InlineData(100, -1, 5, 1, true, 1)]
        [InlineData(700, -1, 17, 1, true, 1)]
        [InlineData(1900, -1, 30, 1, true, 1)]
        public void FindVersionAndEcc(int textLength, int initialVersion, int resultingVersion, int initialEcc, bool improveEcc, int resultingEcc)
        {
            var segments = MakeAlphaNumSegments(textLength);

            var minVersion = initialVersion == -1 ? 1 : initialVersion;
            var maxVersion = initialVersion == -1 ? 40 : initialVersion;
            var (version, ecc) = QrCodeBuilder.FindVersionAndEcc(segments, initialEcc, minVersion, maxVersion, improveEcc);
            Assert.Equal(resultingVersion, version);
            Assert.Equal(resultingEcc, ecc);
        }

        [Theory, CombinatorialData]
        public void FindVersionBoundaries([CombinatorialRange(1, 40)] int version, [CombinatorialRange(0, 3)] int ecc)
        {
            var segments = MakeAlphaNumSegments(QrCodeCapacity.GetAlphanumericCapacity(version, ecc));
            var (resultingVersion, resultingEcc) = QrCodeBuilder.FindVersionAndEcc(segments, ecc);
            Assert.Equal(version, resultingVersion);
            Assert.Equal(ecc, resultingEcc);

            segments = MakeAlphaNumSegments(QrCodeCapacity.GetAlphanumericCapacity(version, ecc) + 1);
            Assert.Throws<DataTooLongException>(() => QrCodeBuilder.FindVersionAndEcc(segments, ecc, version, version, false));
        }
    
        [Theory]
        [InlineData(50, 1, 1)]
        [InlineData(6000, 1, 40)]
        public void DataTooLongException(int textLength, int minVersion, int maxVersion)
        {
            var segments = MakeAlphaNumSegments(textLength);
            var exception = Assert.Throws<DataTooLongException>(() => QrCodeBuilder.FindVersionAndEcc(segments, 1, minVersion, maxVersion));
            if (minVersion == maxVersion)
            {
                Assert.Contains($"version {maxVersion}", exception.Message);
            }
            Assert.Contains("level M", exception.Message);
        }

        [Theory, CombinatorialData]
        public void BuildCodewordsPadding([CombinatorialRange(1, 40)] int version, [CombinatorialRange(0, 3)] int ecc)
        {
            for (var i = -7; i < 0; i += 1)
            {
                var textLength = QrCodeCapacity.GetAlphanumericCapacity(version, ecc) + i;
                var segments = MakeAlphaNumSegments(textLength);
                var codewords = QrCodeBuilder.BuildCodewords(segments, version, ecc);
            
                var expectedCodewords = (DataSegment.GetBitLength(segments, version) + 7) / 8;
                var maxCodewords = QrCodeCapacity.GetNumDataCodewords(version, ecc);
                Assert.InRange(expectedCodewords, maxCodewords - 5, maxCodewords);
            
                var bitLength = DataSegment.GetBitLength(segments, version) + 4;
                if (bitLength <= maxCodewords * 8 - 8)
                {
                    Assert.Equal(0b1110_1100, codewords[(bitLength + 7) / 8]);
                }

                if (bitLength <= maxCodewords * 8 - 16)
                {
                    Assert.Equal(0b0001_0001, codewords[(bitLength + 7) / 8 + 1]);
                }

            }
        }

        [Theory, CombinatorialData]
        public void BuildCodewordsMaximum([CombinatorialRange(1, 40)] int version, [CombinatorialRange(0, 3)] int ecc)
        {
            var textLength = QrCodeCapacity.GetAlphanumericCapacity(version, ecc);
            var segments = MakeAlphaNumSegments(textLength);
            var codewords = QrCodeBuilder.BuildCodewords(segments, version, ecc);

            var expectedCodewords = QrCodeCapacity.GetNumDataCodewords(version, ecc);
            Assert.Equal(expectedCodewords, codewords.Length);
        }

        [Theory, CombinatorialData]
        public void AddErrorCorrection([CombinatorialRange(1, 40)] int version, [CombinatorialRange(0, 3)] int ecc)
        {
            var textLength = QrCodeCapacity.GetAlphanumericCapacity(version, ecc);
            var segments = MakeAlphaNumSegments(textLength);
            var codewords = QrCodeBuilder.BuildCodewords(segments, version, ecc);

            var result = QrCodeBuilder.AddErrorCorrection(codewords, version, ecc);

            Assert.Equal(QrCodeCapacity.GetTotalCodewords(version), result.Length);

            var extractedData = ExtractDataCodewords(result, version, ecc);
            Assert.Equal<byte[]>(codewords, extractedData);
        }
    
        [Theory]
        [InlineData(1, 21)]
        [InlineData(2, 25)]
        [InlineData(3, 29)]
        [InlineData(40, 177)]
        public void GetQrCodeSize(int version, int expectedSize)
        {
            var actualSize = QrCodeBuilder.GetSize(version);
            Assert.Equal(expectedSize, actualSize);
        }

        [Theory, CombinatorialData]
        public void DrawFixedPatterns([CombinatorialRange(1, 40)] int version)
        {
            var modules = QrCodeBuilder.CreateWithFixedPatterns(version);
            var protectedModules = QrCodeBuilder.CreateProtectedModules(version);

            AssertMirrored(modules);
            AssertMirrored(protectedModules);

            AssertFixedPatterns(modules);
        }

        [Theory, CombinatorialData]
        public void FillPayload([CombinatorialRange(1, 40)] int version)
        {
            // create the payload consisting of all dark modules
            var numCodewords = QrCodeBuilder.GetCodewordCapacity(version);
            var codewords = new byte[numCodewords];
            for (var i = 0; i < numCodewords; i += 1)
            {
                codewords[i] = 0xff;
            }

            var modules = QrCodeBuilder.CreateWithFixedPatterns(version);
            var dataMask = QrCodeBuilder.GetDataMask(version);
            var size = modules.Size;
            Assert.Equal(0, CountDarkDataModules(modules, dataMask));

            QrCodeBuilder.FillPayload(modules, codewords, version);

            var darkModules = CountDarkDataModules(modules, dataMask);
            Assert.Equal(codewords.Length * 8, darkModules);
            var lightModules = CountLightDataModules(modules, dataMask);
            Assert.InRange(lightModules, 0, 7);

            // assert that all light modules are near the bottom left corner
            if (lightModules > 0)
            {
                var count = 0;
                var versionInfoOffset = version < 7 ? 0 : 3;
                for (var x = 0; x < 2; x += 1)
                {
                    for (var y = size - 12 - versionInfoOffset; y < size - 8 - versionInfoOffset; y += 1)
                    {
                        if (!modules.Get(x, y) && dataMask.Get(x, y))
                        {
                            count += 1;
                        }
                    }
                }
                Assert.Equal(lightModules, count);
            }

            AssertFixedPatterns(modules);
        }

        private static void AssertFixedPatterns(BitMatrix modules)
        {
            var size = modules.Size;
            var version = (size - 17) / 4;

            AssertFinderPattern(modules, 0, 0);
            AssertFinderPattern(modules, size - 7, 0);
            AssertFinderPattern(modules, 0, size - 7);

            AssertTimingPatterns(modules);
            AssertAlignmentPatterns(modules, version);
        }

        private static void AssertFinderPattern(BitMatrix modules, int x, int y)
        {
            for (var i = 0; i < 7; i += 1)
            {
                Assert.True(modules.Get(x + i, y),     $"finder outer top ({x+i},{y})");
                Assert.True(modules.Get(x + i, y + 6), $"finder outer bottom ({x+i},{y+6})");
                Assert.True(modules.Get(x, y + i),     $"finder outer left ({x},{y+i})");
                Assert.True(modules.Get(x + 6, y + i), $"finder outer right ({x+6},{y+i})");
            }

            for (var i = 1; i < 6; i += 1)
            {
                Assert.False(modules.Get(x + i, y + 1), $"finder inner top ({x+i},{y+1})");
                Assert.False(modules.Get(x + i, y + 5), $"finder inner bottom ({x+i},{y+5})");
                Assert.False(modules.Get(x + 1, y + i), $"finder inner left ({x+1},{y+i})");
                Assert.False(modules.Get(x + 5, y + i), $"finder inner right ({x+5},{y+i})");
            }

            for (var i = 2; i < 5; i += 1)
                for (var j = 2; j < 5; j += 1)
                    Assert.True(modules.Get(x + i, y + j), $"finder center ({x+i},{y+j})");
        }

        private static void AssertTimingPatterns(BitMatrix modules)
        {
            var size = modules.Size;
            for (var i = 8; i < size - 8; i += 1)
            {
                if (i % 2 == 0)
                {
                    Assert.True(modules.Get(i, 6), $"timing row ({i},6)");
                    Assert.True(modules.Get(6, i), $"timing col (6,{i})");
                }
                else
                {
                    Assert.False(modules.Get(i, 6), $"timing row ({i},6)");
                    Assert.False(modules.Get(6, i), $"timing col (6,{i})");
                }
            }
        }

        private static void AssertAlignmentPatterns(BitMatrix modules, int version)
        {
            if (version == 1)
                return;

            var positions = QrCodeBuilder.GetAlignmentPatternPosition(version);
            var numPositions = positions.Length;

            for (var xi = 0; xi < numPositions; xi += 1)
            {
                for (var yi = 0; yi < numPositions; yi += 1)
                {
                    if ((xi == 0 && yi == 0) ||
                        (xi == numPositions - 1 && yi == 0) ||
                        (xi == 0 && yi == numPositions - 1))
                        continue;

                    AssertAlignmentPattern(modules, positions[xi], positions[yi]);
                }
            }
        }

        private static void AssertAlignmentPattern(BitMatrix modules, int cx, int cy)
        {
            for (var i = -2; i <= 2; i += 1)
            {
                Assert.True(modules.Get(cx + i, cy - 2), $"align outer top ({cx+i},{cy-2})");
                Assert.True(modules.Get(cx + i, cy + 2), $"align outer bottom ({cx+i},{cy+2})");
                Assert.True(modules.Get(cx - 2, cy + i), $"align outer left ({cx-2},{cy+i})");
                Assert.True(modules.Get(cx + 2, cy + i), $"align outer right ({cx+2},{cy+i})");
            }

            for (var i = -1; i <= 1; i += 1)
            {
                Assert.False(modules.Get(cx + i, cy - 1), $"align inner top ({cx+i},{cy-1})");
                Assert.False(modules.Get(cx + i, cy + 1), $"align inner bottom ({cx+i},{cy+1})");
                Assert.False(modules.Get(cx - 1, cy + i), $"align inner left ({cx-1},{cy+i})");
                Assert.False(modules.Get(cx + 1, cy + i), $"align inner right ({cx+1},{cy+i})");
            }

            Assert.True(modules.Get(cx, cy), $"align center ({cx},{cy})");
        }

        private static void AssertMirrored(BitMatrix matrix)
        {
            var size = matrix.Size;
            for (var y = 0; y < size; y += 1)
            {
                for (var x = y + 1; x < size; x += 1)
                {
                    if (!(y == 8 && x == size - 8))
                    {
                        Assert.True(matrix.Get(x, y) == matrix.Get(y, x), $"mirror ({x},{y})");
                    }
                }
            }
        }

        private static int CountDarkDataModules(BitMatrix modules, BitMatrix dataMask)
        {
            var sum = 0;
            var size = modules.Size;
            for (var x = 0; x < size; x += 1)
            {
                for (var y = 0; y < size; y += 1)
                {
                    if (modules.Get(x, y) && dataMask.Get(x, y))
                    {
                        sum += 1;
                    }
                }
            }
            return sum;
        }

        private static int CountLightDataModules(BitMatrix modules, BitMatrix dataMask)
        {
            var sum = 0;
            var size = modules.Size;
            for (var x = 0; x < size; x += 1)
            {
                for (var y = 0; y < size; y += 1)
                {
                    if (!modules.Get(x, y) && dataMask.Get(x, y))
                    {
                        sum += 1;
                    }
                }
            }
            return sum;
        }

        [Theory, CombinatorialData]
        public void GetVersionInformationBits([CombinatorialRange(7, 40, 1)] int version)
        {
            var bits = QrCodeBuilder.GetVersionInformationBits(version);

            // The upper 6 bits must equal the version number
            Assert.Equal(version, bits >> 12);

            // The full 18-bit value must be a valid Golay(18,6) codeword:
            // remainder of polynomial division by the generator 0x1F25 must be zero
            // (0x1F25 being the coefficients of the generator polynomial
            // G(x) = x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1).
            var remainder = bits;
            for (var i = 17; i >= 12; i--)
            {
                if ((remainder & (1 << i)) != 0)
                {
                    remainder ^= 0x1F25 << (i - 12);
                }
            }
            Assert.Equal(0, remainder);
        }

        [Theory, CombinatorialData]
        public void GetFormatInformationBits(
            [CombinatorialRange(0, 3, 1)] int ecc,
            [CombinatorialRange(0, 7, 1)] int pattern)
        {
            var bits = QrCodeBuilder.GetFormatInformationBits(ecc, pattern);

            // Undo the XOR mask to recover the raw BCH(15,5) codeword
            var raw = bits ^ 0b101010000010010;

            // The 5 MSB data bits: 2 bits for ECC level + 3 bits for data mask pattern
            var eccBits = QrCodeBuilder.GetErrorCorrectionLevelBits(ecc);
            Assert.Equal(eccBits, (raw >> 13) & 0x3);
            Assert.Equal(pattern, (raw >> 10) & 0x7);

            // The full 15-bit value must be a valid BCH(15,5) codeword:
            // the remainder of polynomial division by generator
            // x^10 + x^8 + x^5 + x^4 + x^2 + x + 1 = 0x537 must be zero
            var remainder = raw;
            for (var i = 14; i >= 10; i -= 1)
            {
                if ((remainder & (1 << i)) != 0)
                {
                    remainder ^= 0x537 << (i - 10);
                }
            }
            Assert.Equal(0, remainder);
        }

        private static byte[] ExtractDataCodewords(byte[] codewordsWithEcc, int version, int ecc)
        {
            var numDataCodewords = QrCodeCapacity.GetNumDataCodewords(version, ecc);
            var numBlocks = QrCodeBuilder.GetNumBlocks(version, ecc);
            var smallBlockDataLength = numDataCodewords / numBlocks;
            var numLargeBlocks = numDataCodewords % numBlocks;
            var numSmallBlocks = numBlocks - numLargeBlocks;
        
            var blockOffsets = new int[numBlocks];
            for (var block = 1; block < numBlocks; block += 1)
                blockOffsets[block] = blockOffsets[block - 1] + smallBlockDataLength + (block - 1 < numSmallBlocks ? 0 : 1);

            var data = new byte[numDataCodewords];
            var index = 0;
            for (var i = 0; i < smallBlockDataLength; i += 1)
            {
                for (var block = 0; block < numBlocks; block += 1)
                {
                    data[blockOffsets[block] + i] = codewordsWithEcc[index];
                    index += 1;
                }
            }

            for (var i = numSmallBlocks; i < numBlocks; i += 1)
            {
                data[blockOffsets[i] + smallBlockDataLength] = codewordsWithEcc[index];
                index += 1;
            }

            return data;
        }

        private static List<DataSegment> MakeAlphaNumSegments(int textLength)
        {
            var text = RandomData.MakeAlphanumericString(textLength, 73629273);
            return new List<DataSegment> {
                new DataSegmentAlphanumeric(new ArraySegment<byte>(Encoding.GetEncoding("ISO-8859-1").GetBytes(text)))
            };
        }
    }
}