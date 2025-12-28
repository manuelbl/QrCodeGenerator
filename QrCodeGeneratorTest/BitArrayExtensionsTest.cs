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

using System;
using System.Collections;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class BitArrayExtensionsTest
    {
        [Fact]
        public void AppendInt1()
        {
            var ba = new BitArray(0);
            ba.AppendBits(18, 6);

            Assert.Equal(6, ba.Length);

            Assert.False(ba[0]);
            Assert.True(ba[1]);
            Assert.False(ba[2]);
            Assert.False(ba[3]);
            Assert.True(ba[4]);
            Assert.False(ba[5]);
        }

        [Fact]
        public void AppendInt2()
        {
            var ba = new BitArray(0);
            ba.AppendBits(18, 6);

            ba.AppendBits(3, 2);

            Assert.Equal(8, ba.Length);

            Assert.False(ba[0]);
            Assert.True(ba[1]);
            Assert.False(ba[2]);
            Assert.False(ba[3]);
            Assert.True(ba[4]);
            Assert.False(ba[5]);
            Assert.True(ba[6]);
            Assert.True(ba[7]);
        }

        [Fact]
        public void AppendExtraBits()
        {
            var ba = new BitArray(0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ba.AppendBits(128, 4);
            });
        }

        [Fact]
        public void ExtractBits()
        {
            var ba = new BitArray(0);
            ba.AppendBits(18, 6);
            ba.AppendBits(0b11001010, 8);

            Assert.Equal(18u, ba.ExtractBits(0, 6));
            Assert.Equal(0b11001010u, ba.ExtractBits(6, 8));
        }
    }
}
