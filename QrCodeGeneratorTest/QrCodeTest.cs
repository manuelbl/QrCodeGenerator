﻿/* 
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
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class QrCodeTest
    {
        [Theory]
        [ClassData(typeof(QrCodeDataProvider))]
        public void TestQrCode(QrCodeTestCase testCase)
        {
            var qrCode = EncodeSegments(testCase.Segments, testCase.RequestedEcc, testCase.MinVersion, testCase.MaxVersion, testCase.RequestedMask, testCase.BoostEcl);
            Assert.Equal(testCase.ExpectedModules.Length, qrCode.Size);
            Assert.Equal(testCase.EffectiveEcc, qrCode.ErrorCorrectionLevel);
            Assert.Equal(testCase.EffectiveMask, qrCode.Mask);
            Assert.Equal(testCase.ExpectedModules, TestHelper.ToStringArray(qrCode));
        }
    }
}
