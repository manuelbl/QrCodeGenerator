/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

namespace Net.Codecrete.QrCodeGenerator.Test
{
    internal static class TestHelper
    {
        internal static string[] ToStringArray(QrCode qrCode)
        {
            var size = qrCode.Size;
            var result = new string[size];

            for (var y = 0; y < size; y += 1)
            {
                var row = new char[size];
                for (var x = 0; x < size; x += 1)
                {
                    row[x] = qrCode.GetModule(x, y) ? 'X' : ' ';
                }
                result[y] = new string(row);
            }

            return result;
        }
    }
}
