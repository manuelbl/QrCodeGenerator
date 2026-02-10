/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Linq;
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
            var qrCode = EncodeSegments(testCase.Segments, testCase.RequestedEcc, testCase.MinVersion, testCase.MaxVersion, testCase.BoostEcl);
            Assert.Equal(testCase.ExpectedModules.Length, qrCode.Size);
            Assert.Equal(testCase.EffectiveVersion, qrCode.Version);
            Assert.Equal(testCase.EffectiveEcc, qrCode.ErrorCorrectionLevel);
            Assert.Equal(testCase.EffectiveMask, qrCode.Mask);
            var actualModules = TestHelper.ToStringArray(qrCode);
            if (!Enumerable.SequenceEqual(testCase.ExpectedModules, actualModules))
            {
                if (testCase.ExpectedModules.Length != actualModules.Length)
                {
                    Console.WriteLine($"Size differs: expected {testCase.ExpectedModules.Length}, actual {actualModules.Length}");
                }
                else
                {
                    for (var i = 0; i < actualModules.Length; i += 1)
                    {
                        var isOk = testCase.ExpectedModules[i] == actualModules[i] ? "OK " : "NOK";
                        Console.WriteLine($"{isOk} Actual:   {actualModules[i]}");
                        Console.WriteLine($"{isOk} Expected: {testCase.ExpectedModules[i]}");
                    }
                }
            }
            Assert.Equal(testCase.ExpectedModules, actualModules);
        }

        [Fact]
        public void Constants()
        {
            Assert.Equal(40, MaxVersion);
            Assert.Equal(1, MinVersion);
        }
    }
}
