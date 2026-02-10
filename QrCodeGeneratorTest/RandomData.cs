/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * 
 */

using System;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    internal static class RandomData
    {
        private const string AlphanumericCharset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

        internal static string MakeAlphanumericString(int length, int seed)
        {
            var random = new Random(seed);
            var charsetLength = AlphanumericCharset.Length;
            var chars = new char[length];
            for (var i = 0; i < length; i += 1)
            {
                chars[i] = AlphanumericCharset[random.Next(charsetLength)];
            }
            return new string(chars);
        }


        internal static string MakeString(int length, int seed)
        {
            // create batches of characters from similar alphabets
            var random = new Random(seed);
            var result = new StringBuilder(length);

            var n = 0;
            while (n < length)
            {
                var alphabet = random.Next(4);
                var batchSize = Math.Min(length - n, random.Next(1, alphabet == 0 ? 20 : 3));
                for (var i = 0; i < batchSize; i += 1)
                {
                    switch (alphabet)
                    {
                        case 0: // Alphanumeric
                            result.Append(AlphanumericCharset[random.Next(AlphanumericCharset.Length)]);
                            break;
                        case 1: // Basic Latin
                            result.Append((char)random.Next(0x20, 0x7e));
                            break;
                        case 2: // Latin-1 Supplement (subset)
                            result.Append((char)random.Next(0xc0, 0xcf));
                            break;
                        case 3: // Emoticons
                            var codePoint = random.Next(0x1f600, 0x1f608);
                            result.Append(char.ConvertFromUtf32(codePoint));
                            break;
                    }
                }
                n += batchSize;

            }

            return result.ToString();
        }
        
        internal static byte[] MakeRandomBytes(int length, int seed)
        {
            var random = new Random(seed);
            var result = new byte[length];
            random.NextBytes(result);
            return result;
        }
    }
}
