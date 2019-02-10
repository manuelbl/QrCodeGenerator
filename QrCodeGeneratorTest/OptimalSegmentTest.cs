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

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class OptimalSegmentTest
    {
        private const string Text1 = "2342342340ABC234234jkl~~";

        private static readonly string[] Modules1 = {
            "XXXXXXX XXXX XXXX   X XXXXXXX",
            "X     X       X    XX X     X",
            "X XXX X  X XXX  XXX X X XXX X",
            "X XXX X XXXXX  X  XX  X XXX X",
            "X XXX X  X XX    XX   X XXX X",
            "X     X  X     X  X   X     X",
            "XXXXXXX X X X X X X X XXXXXXX",
            "         X XX XXX            ",
            "  X XXX XX  XXX X   XX   X  X",
            "   X    XX   X     XX X XXXXX",
            "    XXX          X XXXX X    ",
            "    X     X X     X XX  X XXX",
            "XX X  XXXXX XXXXXXXXX   XXXX ",
            "  X X    X   XX  X     X X X ",
            "  XXXXX  XXX XXX    XX   X  X",
            "XXXXX   XX  XX  X  XXX X XXX ",
            "XXX   XXX  XXX     X  XX     ",
            "        X X    XX  X X  X  X ",
            "X X XXXX XXXX X  X   X  X X  ",
            " X X X XX  X   X XXX X XX XXX",
            "X  X XX  X XXX   XX XXXXXXX X",
            "        X    X X    X   XXXX ",
            "XXXXXXX   X  XX X XXX X X X  ",
            "X     X X X XX X   XX   X XX ",
            "X XXX X XXXX XX X X XXXXX    ",
            "X XXX X  X X  X   XX  XX   X ",
            "X XXX X X    X   XXXXXX X   X",
            "X     X  XXX  XX X X  XXX X  ",
            "XXXXXXX  XXXX  X  XX     XX X"
        };

        [Fact]
        private void OptimalSegmentCode()
        {
            var segments = QrSegmentAdvanced.MakeSegmentsOptimally(Text1, QrCode.Ecc.High);
            var qrCode = QrCode.EncodeSegments(segments, QrCode.Ecc.High);

            Assert.Same(QrCode.Ecc.High, qrCode.ErrorCorrectionLevel);
            Assert.Equal(29, qrCode.Size);
            Assert.Equal(0, qrCode.Mask);
            Assert.Equal(Modules1, TestHelper.ToStringArray(qrCode));
        }
    }
}
