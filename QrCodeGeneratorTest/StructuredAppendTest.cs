/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 */

using System.Linq;
using System.Text;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class StructuredAppendTest
    {
        [Fact]
        public void EncodeAlphanumericStringInMultipleCodes()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 1001);
            var data = Encoding.Latin1.GetBytes(text);
            var segments = StructuredAppend.BuildSegments(data, 29, QrCode.Ecc.Medium, ECI.Latin1, false);
            Assert.Equal(2, segments.Count);
            Assert.Equal(text, DataSegment.GetText(segments.SelectMany(it => it)));
        }

        [Fact]
        public void EncodeStringInMultipleCodes()
        {
            var text = RandomData.MakeString(3003, seed: 2003);
            var data = Encoding.UTF8.GetBytes(text);
            var segments = StructuredAppend.BuildSegments(data, 31, QrCode.Ecc.Medium, ECI.UTF8, true);
            Assert.Equal(3, segments.Count);
            Assert.Equal(text, DataSegment.GetText(segments.SelectMany(it => it)));
        }

        [Fact]
        public void EncodeVeryLongStringInMultipleCodes()
        {
            var text = RandomData.MakeString(35663, seed: 9117);
            var data = Encoding.UTF8.GetBytes(text);
            var segments = StructuredAppend.BuildSegments(data, 40, QrCode.Ecc.Low, ECI.UTF8, true);
            Assert.Equal(16, segments.Count);
            Assert.Equal(text, DataSegment.GetText(segments.SelectMany(it => it)));
        }

        [Fact]
        public void RejectTooLongString()
        {
            var text = RandomData.MakeString(10017, seed: 7543);
            var data = Encoding.UTF8.GetBytes(text);
            var exception = Assert.Throws<DataTooLongException>(() => StructuredAppend.BuildSegments(data, 19, QrCode.Ecc.High, ECI.UTF8, true));
            Assert.Equal("The text is too long to fit into 16 QR codes", exception.Message);
        }

        [Fact]
        public void CreateMultipleQrCodes()
        {
            var text = RandomData.MakeString(2117, seed: 8172);
            var qrCodes = QrCode.EncodeTextInMultipleCodes(text, QrCode.Ecc.Medium, version: 33);
            Assert.Equal(2, qrCodes.Count);
        }

        [Fact]
        public void CreateSingleQrCode()
        {
            var text = RandomData.MakeAlphanumericString(2632, seed: 3200);
            var qrCodes = QrCode.EncodeTextInMultipleCodes(text, QrCode.Ecc.Medium, version: 35);
            Assert.Single(qrCodes);
        }
    }
}
