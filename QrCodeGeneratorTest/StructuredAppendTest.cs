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
            var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
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
            var text = RandomData.MakeString(34500, seed: 9117);
            var data = Encoding.UTF8.GetBytes(text);
            var segments = StructuredAppend.BuildSegments(data, 40, QrCode.Ecc.Low, ECI.UTF8, true);
            Assert.Equal(16, segments.Count);
            Assert.Equal(text, DataSegment.GetText(segments.SelectMany(it => it)));
            // every code must actually fit and build at version 40 (guards against overfilling splits)
            foreach (var code in segments)
            {
                QrCode.EncodeSegments(code, QrCode.Ecc.Low, minVersion: 40, maxVersion: 40);
            }
        }

        [Fact]
        public void StructuredAppendHeadersCarryDataParity()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 1001);
            var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            var expectedParity = data.Aggregate<byte, byte>(0, (current, value) => (byte)(current ^ value));

            var qrCodes = StructuredAppend.BuildSegments(data, 29, QrCode.Ecc.Medium, ECI.Latin1, false);

            Assert.NotEqual(0, expectedParity);
            Assert.All(qrCodes, segments => Assert.Equal(expectedParity, segments[0].StructuredAppendParity));
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

        [Fact]
        public void BalancedCodes_SpreadPayloadEvenly_AtFixedVersion()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 4711);
            var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            var (qrCodes, version) = StructuredAppend.BuildBalancedSegments(data, 29, 29, QrCode.Ecc.Medium, ECI.Latin1, false);

            Assert.Equal(29, version);
            Assert.Equal(2, qrCodes.Count);
            Assert.Equal(text, DataSegment.GetText(qrCodes.SelectMany(it => it)));

            // Evenly distributed: unlike greedy filling (first code full, last code nearly empty),
            // both codes carry a similar amount of payload.
            var payloads = qrCodes.Select(code => code.Sum(segment => segment.DataLength)).ToList();
            Assert.True(payloads.Min() >= payloads.Max() * 4 / 10,
                $"payloads not balanced: {string.Join(",", payloads)}");
        }

        [Fact]
        public void BalancedCodes_ReduceVersion()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 4711);
            var data = Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
            var (qrCodes, version) = StructuredAppend.BuildBalancedSegments(data, 10, 29, QrCode.Ecc.Medium, ECI.Latin1, false);

            // Two codes are needed at version 29, but a smaller shared version fits them evenly.
            Assert.Equal(2, qrCodes.Count);
            Assert.True(version < 29, $"expected a reduced version, got {version}");
            Assert.Equal(text, DataSegment.GetText(qrCodes.SelectMany(it => it)));
        }

        [Fact]
        public void BalancedCodes_Utf8RoundTrip()
        {
            var text = RandomData.MakeString(3003, seed: 2003);
            var data = Encoding.UTF8.GetBytes(text);
            var (qrCodes, _) = StructuredAppend.BuildBalancedSegments(data, 10, 31, QrCode.Ecc.Medium, ECI.UTF8, true);

            Assert.True(qrCodes.Count >= 2);
            Assert.Equal(text, DataSegment.GetText(qrCodes.SelectMany(it => it)));
        }

        [Fact]
        public void CreateMultipleBalancedQrCodes_UseSameVersion()
        {
            var text = RandomData.MakeAlphanumericString(3000, seed: 4711);
            var qrCodes = QrCode.EncodeTextInMultipleBalancedCodes(text, QrCode.Ecc.Medium, minVersion: 10, maxVersion: 29);

            Assert.Equal(2, qrCodes.Count);
            // every code is identical in version and ECC level
            Assert.All(qrCodes, qr => Assert.Equal(qrCodes[0].Version, qr.Version));
            Assert.All(qrCodes, qr => Assert.Equal(QrCode.Ecc.Medium, qr.ErrorCorrectionLevel));
        }

        [Fact]
        public void CreateSingleBalancedQrCode_Collapses()
        {
            var text = RandomData.MakeAlphanumericString(200, seed: 55);
            var qrCodes = QrCode.EncodeTextInMultipleBalancedCodes(text, QrCode.Ecc.Medium, minVersion: 10, maxVersion: 29);

            Assert.Single(qrCodes);
        }

        [Fact]
        public void RejectTooLongBalanced()
        {
            var text = RandomData.MakeString(10017, seed: 7543);
            var exception = Assert.Throws<DataTooLongException>(() =>
                QrCode.EncodeTextInMultipleBalancedCodes(text, QrCode.Ecc.High, minVersion: 19, maxVersion: 19));
            Assert.Equal("The text is too long to fit into 16 QR codes", exception.Message);
        }
    }
}
