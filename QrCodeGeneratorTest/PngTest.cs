/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 */

using System.IO;
using System.Threading.Tasks;
using VerifyTests;
using VerifyXunit;
using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class PngTest
    {
        static PngTest()
        {
            VerifyImageMagick.Initialize();
            VerifyImageMagick.RegisterComparers(threshold: 0.005);
        }

        private const string CodeText1 = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga.";
        private const string CodeText2 = "The quick brown fox";

        protected readonly VerifySettings Settings = new VerifySettings();

        public PngTest()
        {
            Settings.UseDirectory("ReferenceFiles");
        }

        [Fact]
        public Task PngImage()
        {
            var qrCode = EncodeText(CodeText1, Ecc.Medium);
            var pngData = qrCode.ToPngBitmap(5, 3);

            using (var stream = new MemoryStream(pngData))
            {
                return Verifier.Verify(stream, "png", Settings);
            }
        }


        [Fact]
        public Task ColorPngImage()
        {
            var qrCode = EncodeText(CodeText2, Ecc.Medium);
            var pngData = qrCode.ToPngBitmap(3, 7, 0x171c80, 0xccbc9b);

            using (var stream = new MemoryStream(pngData))
            {
                return Verifier.Verify(stream, "png", Settings);
            }
        }

    }
}
