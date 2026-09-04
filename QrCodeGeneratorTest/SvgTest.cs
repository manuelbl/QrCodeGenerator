/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class SvgTest
    {
        private const string CodeText = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga.";

        [Fact]
        public void SvgImage()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);
            var svg = qrCode.ToSvgString(0);

            Assert.StartsWith("<?xml", svg);
            Assert.Contains("\"0 0 69 69\"", svg); // view box

            File.WriteAllText("qrcode.svg", svg, Encoding.UTF8);
        }

        [Fact]
        public void SvgImageWithColor()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);
            var svg = qrCode.ToSvgString(0, "#FF0000", "#000000");

            Assert.Contains("#000000", svg);
            Assert.Contains("#FF0000", svg);

            Assert.DoesNotContain("#FFFFFF", svg);
        }

        [Fact]
        public void SvgImageWithoutBackground()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);
            var svg = qrCode.ToSvgString(0, "#000000", null);

            // no background rectangle: the light modules and the border stay transparent
            Assert.DoesNotContain("<rect", svg);
            Assert.Contains("<path d=\"", svg);
        }

        [Fact]
        public void SvgImageWithNullForeground()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);

            Assert.Throws<ArgumentNullException>(() => qrCode.ToSvgString(0, null));
        }

        [Fact]
        public void SvgImageUsesCrispEdges()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);

            // the module boundaries are not anti-aliased into gray fringes
            Assert.Contains("shape-rendering=\"crispEdges\"", qrCode.ToSvgString(0));
        }

        [Fact]
        public void SvgPath()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);
            var path = qrCode.ToGraphicsPath(3);

            // one closed sub-path per outline polygon, offset by the border;
            // the first polygon is the outer boundary of the top-left finder pattern
            Assert.StartsWith("M3,3h7v7h-7z ", path);
            Assert.EndsWith("z", path);
            Assert.Equal(qrCode.ToOutlines().Count, path.Split('z').Length - 1);
        }

        [Fact]
        public void SvgPath_IsThePathOfTheSvgImage()
        {
            var qrCode = EncodeText(CodeText, Ecc.Medium);

            Assert.Contains($"d=\"{qrCode.ToGraphicsPath(4)}\"", qrCode.ToSvgString(4));
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("en-GB")]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("nb-NO")]
        [InlineData("tr-TR")]
        [InlineData("zh-CN")]
        public void SvgLocale(string locale)
        {
            CultureInfo savedCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo savedCurrentUiCulture = CultureInfo.CurrentUICulture;

            CultureInfo culture = CultureInfo.CreateSpecificCulture(locale);

            try
            {
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                var qrCode = EncodeText("A", Ecc.Medium);
                var svg = qrCode.ToSvgString(0);
                Assert.Contains("d=\"M0,0h7v7h-7z", svg);
            }
            finally
            {
                CultureInfo.CurrentCulture = savedCurrentCulture;
                CultureInfo.CurrentUICulture = savedCurrentUiCulture;
            }
        }
    }
}
