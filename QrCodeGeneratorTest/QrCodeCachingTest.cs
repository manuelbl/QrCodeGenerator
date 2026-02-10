/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class QrCodeCachingTest
{
    [Theory, CombinatorialData]
    public void RepeatedGenerationProducesIdenticalQrCode(
        [CombinatorialValues(1, 10, 25, 40)] int version,
        [CombinatorialValues(QrCode.Ecc.Low, QrCode.Ecc.High)] QrCode.Ecc ecc)
    {
        var text = RandomData.MakeAlphanumericString(QrCodeCapacity.GetAlphanumericCapacity(version, (int)ecc), 0xC0FFEE);

        var first = QrCode.EncodeText(text, ecc);
        var second = QrCode.EncodeText(text, ecc);

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(first.Size, second.Size);
        Assert.Equal(first.Mask, second.Mask);
        Assert.Equal(first.ErrorCorrectionLevel, second.ErrorCorrectionLevel);

        for (var y = 0; y < first.Size; y += 1)
        {
            for (var x = 0; x < first.Size; x += 1)
            {
                Assert.True(first.GetModule(x, y) == second.GetModule(x, y), $"module ({x},{y})");
            }
        }
    }
}
