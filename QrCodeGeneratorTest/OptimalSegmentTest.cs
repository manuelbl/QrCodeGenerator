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

using Xunit;

// ReSharper disable StringLiteralTypo

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class OptimalSegmentTest
    {
        private const string Text1 = "2342342340ABC234234jkl~~";

        private static readonly string[] Modules1 = {
            "XXXXXXX X  X X        XXXXXXX",
            "X     X X  XXXX  XX X X     X",
            "X XXX X X XXXXXX XX   X XXX X",
            "X XXX X  XX  X X X    X XXX X",
            "X XXX X   XXX XXXXX X X XXX X",
            "X     X XX XXX X X X  X     X",
            "XXXXXXX X X X X X X X XXXXXXX",
            "        XX   XXXXXXX         ",
            "  XXX X X X XX X     XXX  XXX",
            "XX X X XXX XX    XX X XX  XXX",
            "  XX XX XXX   XXXX X    X XXX",
            "XX  XX X  XX X   X XXX X XXXX",
            "XXX X XX    XX   XXX XX XX  X",
            "XXX XX X X XX X   XX    X  X ",
            "     XX X  X X  X     X  XXX ",
            "  XXXX XXX X    XXX XX  X XX ",
            "XX XX XX XXXXXXXX  XXX X  XXX",
            "XX   X XX XXXX XXXX  X X X X ",
            "X  X XXXX  XX  XXX  X X X  XX",
            "X  X    X   XX X     X   XXXX",
            "X X XXX X XXXXXXXXX XXXXXX X ",
            "        X  XX  X XXXX   X XX ",
            "XXXXXXX  X   X X  XXX X X  XX",
            "X     X   XX   X XX X   XXXX ",
            "X XXX X X  X X X  X XXXXX XXX",
            "X XXX X XX  XXX  X    X XX X ",
            "X XXX X XXX  XXXXXXX    X XX ",
            "X     X  XX XXXX  X   X  XX  ",
            "XXXXXXX    XX X X XXXXX  X X "
        };

        [Fact]
        public void OptimalSegmentCode()
        {
            var segments = QrSegmentAdvanced.MakeSegmentsOptimally(Text1, QrCode.Ecc.High);
            var qrCode = QrCode.EncodeSegments(segments, QrCode.Ecc.High);

            Assert.Same(QrCode.Ecc.High, qrCode.ErrorCorrectionLevel);
            Assert.Equal(29, qrCode.Size);
            Assert.Equal(2, qrCode.Mask);
            Assert.Equal(Modules1, TestHelper.ToStringArray(qrCode));
        }
    }
}
