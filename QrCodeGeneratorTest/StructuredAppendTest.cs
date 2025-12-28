/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 */

using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class StructuredAppendTest
    {
        [Fact]
        public void EncodeAlphanumericStringInMultipleCodes()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 1001);
            var segments = QrSegmentAdvanced.MakeSegmentsForMultipleCodes(text, QrCode.Ecc.Medium, version: 29);
            Assert.Equal(2, segments.Count);
            Assert.Equal(text, QrSegment.GetJoinedText(segments));
        }

        [Fact]
        public void EncodeStringInMultipleCodes()
        {
            var text = RandomData.MakeString(3003, seed: 2003);
            var segments = QrSegmentAdvanced.MakeSegmentsForMultipleCodes(text, QrCode.Ecc.Medium, version: 31);
            Assert.Equal(3, segments.Count);
            Assert.Equal(text, QrSegment.GetJoinedText(segments));
        }

        [Fact]
        public void EncodeVeryLongStringInMultipleCodes()
        {
            var text = RandomData.MakeString(35006, seed: 9117);
            var segments = QrSegmentAdvanced.MakeSegmentsForMultipleCodes(text, QrCode.Ecc.Low, version: 40);
            Assert.Equal(16, segments.Count);
            Assert.Equal(text, QrSegment.GetJoinedText(segments));
        }

        [Fact]
        public void RejectTooLongString()
        {
            var text = RandomData.MakeString(10017, seed: 7543);
            var exception = Assert.Throws<DataTooLongException>(() => QrSegmentAdvanced.MakeSegmentsForMultipleCodes(text, QrCode.Ecc.High, version: 19));
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
