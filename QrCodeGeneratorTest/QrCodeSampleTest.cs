/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.IO;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class QrCodeSampleTest
    {
        [Fact]
        public void Sample1()
        {
            // should result in a QR code with an ECI for UTF-8
            var qrCode = QrCode.EncodeText("Hello, world! € éèà", QrCode.Ecc.Medium);
            Assert.Equal(2, qrCode.Version);
            Assert.Equal(QrCode.Ecc.Medium, qrCode.ErrorCorrectionLevel);
            var svg = qrCode.ToSvgString(4);
            File.WriteAllText("sample1.svg", svg);
        }

        [Fact]
        public void Sample2()
        {
            // should result in a Latin-1 text
            var qrCode = QrCode.EncodeText(
                "Eine wunderbare Heiterkeit hat meine ganze Seele eingenommen, gleich den süßen Frühlingsmorgen, die ich mit ganzem Herzen genieße.",
                QrCode.Ecc.Medium);
            Assert.Equal(8, qrCode.Version);
            Assert.Equal(QrCode.Ecc.Medium, qrCode.ErrorCorrectionLevel);
            var svg = qrCode.ToSvgString(4);
            File.WriteAllText("sample2.svg", svg);
        }
    }
}