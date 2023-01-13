using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class QrCodeBitmapTest
{
    [Theory]
    [InlineData("https://some-weird-test-site.site?whatever=test&test=123&qrcodes=true", 0, "Qk1GAQAAAAAAAD4AAAAoAAAAIQAAACEAAAABAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP///wABGCx2gAAAAH1eebmAAAAARXqcGoAAAABFbeAjgAAAAEUxPQaAAAAAfZ54cIAAAAABHHRSgAAAAP81xHUAAAAARaA3AwAAAABerNA5gAAAAFFkeZqAAAAAPnniygAAAABkUj3mgAAAAE6V0/WAAAAAELqxOoAAAADG9cBaAAAAABVIFDaAAAAAP23B3YAAAAAMZHv2gAAAALK/5p4AAAAAlBC9o4AAAACSd3A9gAAAAK1cl9qAAAAAgldg3AAAAAAEKDQqgAAAAP+M63+AAAAAAVVVQAAAAAB9IH3fAAAAAEW7TtEAAAAARRAn0QAAAABF9dHRAAAAAH1a8V8AAAAAAddiwAAAAAA=")]
    public void ToBitmap_CreatesValidBitmap(string stubText, int stubEccOrdinal, string expectedBitmapBase64)
    {
        var stubEcc = stubEccOrdinal switch
        {
            0 => QrCode.Ecc.Low,
            1 => QrCode.Ecc.Medium,
            2 => QrCode.Ecc.Quartile,
            3 => QrCode.Ecc.High,
            _ => throw new ArgumentOutOfRangeException(nameof(stubEccOrdinal), stubEccOrdinal, string.Empty)
        };

        var qrCode = QrCode.EncodeText(stubText, stubEcc);

        var actualBitmap = qrCode.ToMonochromeBitmap();
        var actualBase64 = Convert.ToBase64String(actualBitmap);
        
        Assert.Equal(expectedBitmapBase64, actualBase64);
    }
}