/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * Copyright (c) Project Nayuki (MIT License)
 * https://www.nayuki.io/page/qr-code-generator-library
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System.IO;
using System.Text;
using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class SvgTest
    {
        private static readonly string CODE_TEXT = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga.";

        [Fact]
        public void SvgImage()
        {
            var qrCode = EncodeText(CODE_TEXT, Ecc.Medium);
            string svg = qrCode.ToSvgString(0);

            Assert.StartsWith("<?xml", svg);
            Assert.Contains("\"0 0 69 69\"", svg); // view box

            File.WriteAllText("qrcode.svg", svg, Encoding.UTF8);
        }

        [Fact]
        public void SvgImageWithColor()
        {
            var qrCode = EncodeText(CODE_TEXT, Ecc.Medium);
            string svg = qrCode.ToSvgString(0, "#FF0000", "#000000");

            Assert.Contains("#000000", svg);
            Assert.Contains("#FF0000", svg);

            Assert.DoesNotContain("#FFFFFF", svg);
        }
    }
}
