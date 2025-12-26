/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class StructuredAppendTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SplitSimpleText(bool considerUtf8Boundaries)
        {
            var text = Encoding.UTF8.GetBytes("Hello, world!");
            var segments = QrSegment.MakeStructuredAppendSegments(text, numberOfCodes: 2, considerUtf8Boundaries: considerUtf8Boundaries);
            Assert.Equal(2, segments.Count);
            Assert.Equal(2, segments[0].Count);
            Assert.Equal(0, segments[0][0].NumChars);
            Assert.Equal(6, segments[0][1].NumChars);
            Assert.Equal(2, segments[1].Count);
            Assert.Equal(0, segments[1][0].NumChars);
            Assert.Equal(7, segments[1][1].NumChars);
        }

        [Fact]
        public void SplitAtCharBoundary()
        {
            var text = Encoding.UTF8.GetBytes("ab👍🏿🇺🇦𬿞");
            Assert.Equal(22, text.Length);
            var segments = QrSegment.MakeStructuredAppendSegments(text, numberOfCodes: 2, considerUtf8Boundaries: true);
            Assert.Equal(2, segments.Count);
            Assert.Equal(2, segments[0].Count);
            Assert.Equal(0, segments[0][0].NumChars);
            Assert.Equal(14, segments[0][1].NumChars);
            Assert.Equal(2, segments[1].Count);
            Assert.Equal(0, segments[1][0].NumChars);
            Assert.Equal(8, segments[1][1].NumChars);
        }

        [Fact]
        public void SplitInMiddleOfChar()
        {
            var text = Encoding.UTF8.GetBytes("ab👍🏿🇺🇦𬿞");
            Assert.Equal(22, text.Length);
            var segments = QrSegment.MakeStructuredAppendSegments(text, numberOfCodes: 2, considerUtf8Boundaries: false);
            Assert.Equal(2, segments.Count);
            Assert.Equal(2, segments[0].Count);
            Assert.Equal(0, segments[0][0].NumChars);
            Assert.Equal(11, segments[0][1].NumChars);
            Assert.Equal(2, segments[1].Count);
            Assert.Equal(0, segments[1][0].NumChars);
            Assert.Equal(11, segments[1][1].NumChars);
        }

        [Fact]
        public void ShortTextFitsInSingleQrCode()
        {
            var qrCodes = EncodeTextInMultipleCodes(MakeString(1264), Ecc.Medium, 29);
            Assert.Single(qrCodes);
        }


        [Fact]
        public void LongTextRequires2QrCodes()
        {
            var qrCodes = EncodeTextInMultipleCodes(MakeString(1444), Ecc.Medium, 29);
            Assert.Equal(2, qrCodes.Count);
        }

        [Fact]
        public void MaximumTextFor2QrCodes()
        {
            var qrCodes = EncodeTextInMultipleCodes(MakeString(2 * 1262), Ecc.Medium);
            Assert.Equal(2, qrCodes.Count);
        }

        [Theory]
        [ClassData(typeof(AllVersions))]
        public void TestSplitBoundaries(int version, Ecc ecl)
        {
            var maxChars = GetNumDataCodewords(version, ecl) - (version <= 9 ? 4 : 5);
            var qrCodes = EncodeTextInMultipleCodes(MakeString(maxChars * 3), ecl, version: version);
            Assert.Equal(3, qrCodes.Count);
            qrCodes = EncodeTextInMultipleCodes(MakeString(maxChars * 3 + 1), ecl, version: version);
            Assert.Equal(4, qrCodes.Count);
        }

        private static string MakeString(int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)(32 + (i % 95));
            }
            return new string(chars);
        }
    }

    public class AllVersions : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            for (int i = 1; i <= 40; i++)
            {
                yield return new object[] { i, Ecc.Low };
                yield return new object[] { i, Ecc.Medium };
                yield return new object[] { i, Ecc.Quartile };
                yield return new object[] { i, Ecc.High };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
