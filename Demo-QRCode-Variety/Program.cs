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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    internal class Program
    {
        // The main application program.
        internal static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Enable support for encodings like ShiftJIS

            DoBasicDemo();
            DoVarietyDemo();
            DoEncodingDemo();
            DoBinaryDemo();
        }

        #region Demo suite

        // Creates a single QR code, then writes it to an SVG file.
        private static void DoBasicDemo()
        {
            const string text = "Hello, world!"; // Payload text
            var errCorLvl = QrCode.Ecc.Low; // Minimal error correction level

            var qr = QrCode.EncodeText(text, errCorLvl); // Make the QR code symbol
            SaveAsSvg(qr, "hello-world-QR.svg", border: 4); // Save as SVG
        }


        // Creates a variety of QR codes that exercise different features of the library, and writes each one to file.
        private static void DoVarietyDemo()
        {
            // Numeric mode encoding (3.33 bits per digit)
            var qr = QrCode.EncodeText("314159265358979323846264338327950288419716939937510", QrCode.Ecc.Medium);
            SaveAsSvg(qr, "pi-digits-QR.svg");

            // Alphanumeric mode encoding (5.5 bits per character)
            qr = QrCode.EncodeText("DOLLAR-AMOUNT:$39.87 PERCENTAGE:100.00% OPERATIONS:+-*/", QrCode.Ecc.High);
            SaveAsSvg(qr, "alphanumeric-QR.svg", 4);

            // Moderately large QR code using longer text (from Lewis Carroll's Alice in Wonderland)
            qr = QrCode.EncodeText(
                "Alice was beginning to get very tired of sitting by her sister on the bank, "
                + "and of having nothing to do: once or twice she had peeped into the book her sister was reading, "
                + "but it had no pictures or conversations in it, 'and what is the use of a book,' thought Alice "
                + "'without pictures or conversations?' So she was considering in her own mind (as well as she could, "
                + "for the hot day made her feel very sleepy and stupid), whether the pleasure of making a "
                + "daisy-chain would be worth the trouble of getting up and picking the daisies, when suddenly "
                + "a White Rabbit with pink eyes ran close by her.", QrCode.Ecc.High);
            SaveAsSvg(qr, "alice-wonderland-QR.svg", 10);
        }

        private static void DoEncodingDemo()
        {
            // ShiftJIS encoding so Kanji characters are compactly encoded (13 bits per character)
            var qr = QrCode.EncodeTextAdvanced("こんにちは", QrCode.Ecc.Medium, eci: ECI.ShiftJIS); // Japanese "Hello"
            SaveAsSvg(qr, "kanji-QR.svg", 4);
        }



        private static void DoBinaryDemo()
        {
            // create binary data
            byte[] data = {
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00,
                0x01, 0x00, 0x80, 0x01, 0x00, 0xff, 0xff, 0xff,
                0x00, 0x00, 0x00, 0x21, 0xf9, 0x04, 0x01, 0x0a,
                0x00, 0x01, 0x00, 0x2c, 0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x4c,
                0x01, 0x00, 0x3b
            };
            var qr = QrCode.EncodeBinary(data, QrCode.Ecc.Medium);
            SaveAsSvg(qr, "binary.svg");
        }

        #endregion

        private static void SaveAsSvg(QrCode qrCode, string filname, int border = 3)
        {
            string svg = qrCode.ToSvgString(border); // Convert to SVG XML code
            File.WriteAllText(filname, svg, Encoding.UTF8); // Write image to file
        }

    }
}
