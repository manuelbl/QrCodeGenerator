using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class QrCodeBitmapTest
{
    private static QrCode.Ecc EccFromOrdinal(int val)
    {
        return val switch
        {
            0 => QrCode.Ecc.Low,
            1 => QrCode.Ecc.Medium,
            2 => QrCode.Ecc.Quartile,
            3 => QrCode.Ecc.High,
            _ => throw new ArgumentOutOfRangeException(nameof(val), val, string.Empty)
        };
    }

    [Theory]
    [InlineData("https://some-weird-test-site.site?whatever=test&test=123&qrcodes=true", 1, 0,
        "Qk1mAQAAAAAAAD4AAAAoAAAAJQAAACUAAAABAAEAAAAAAAAAAADEDgAAxA4AAAAAAAAAAAAAAAAAAP///wABNbl4gAAAAH2YIHtIAAAARZKOUbgAAABF18iIsAAAAEULZ/AQAAAAfZzqZwAAAAABYtBVaAAAAP8KPrdoAAAAHVwj8EAAAADmh5vWmAAAANCnMkWYAAAAR+1mSzgAAAAtnd3kiAAAABPpnc5YAAAAiXY2DBgAAABv8FrGaAAAAHiVuNCYAAAAAywgdMgAAADQcKpM+AAAAOrRjotoAAAAVXNn/YAAAACSHPJaSAAAALhO2q4YAAAAxmIcBzgAAABsKCF5gAAAAN9hmn3IAAAA4MVQolgAAAArPWKDKAAAAHQXyPAwAAAA/wOSb/gAAAABVVVUAAAAAH0JTdXwAAAARRH2pRAAAABFccYFEAAAAEX0A8UQAAAAfaGzFfAAAAABEwzkAAAAAA==")]
    [InlineData("https://some-weird-test-site.site?whatever=test&test=123&qrcodes=true", 2, 3,
        "Qk22AQAAAAAAAD4AAAAoAAAALwAAAC8AAAABAAEAAAAAAAAAAADEDgAAxA4AAAAAAAAAAAAAAAAAAP///wD///////4AAP///////gAA///////+AADgO6v/YF4AAO+lxCohTgAA6L2ATReuAADouUKYTT4AAOigXCdAXgAA77/XlU5OAADgP97Ayo4AAP/iVozOrgAA5zlqO0D+AAD4+wMZZW4AAPilMcmmbgAA4c22rPiOAAD1l98/df4AAPnKd3lDfgAA8Z0JkavOAAD8dAKZIK4AAOGKaI1l3gAA6mgrWDluAADyLHIZrm4AAOt5c5RwLgAA6jxX4lfOAADsRaV5aW4AAOy55Z0tbgAA9XL2RLCOAAD5M02aHV4AAPPi/21fTgAA/rYvuSHuAADhczd3qq4AAPcUukk33gAA7UCOCQ1eAADgHCpZLO4AAPVGm7CyTgAA9qZa7lS+AAD//LTZL/4AAOAqqqqoDgAA76jKfwvuAADopX0Q+i4AAOi25Ci6LgAA6K8FphouAADvv3xO2+4AAOA6ZIx4DgAA///////+AAD///////4AAP///////gAA")]
    public void ToMonochromeBitmap_CreatesValidBitmap(
        string stubText,
        int stubEccOrdinal,
        int stubBorder,
        string expectedBitmapBase64)
    {
        var stubEcc = EccFromOrdinal(stubEccOrdinal);

        var qrCode = QrCode.EncodeText(stubText, stubEcc);

        var actualBitmap = qrCode.ToMonochromeBitmap(stubBorder);
        var actualBase64 = Convert.ToBase64String(actualBitmap);

        Assert.Equal(expectedBitmapBase64, actualBase64);
    }
}