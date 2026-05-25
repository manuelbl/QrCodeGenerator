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

using System.IO;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    internal class Program
    {
        // The main application program.
        internal static void Main()
        {
            // Enable support for special encodings like Shift-JIS
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            BasicQrCode();
            AlphanumericText();
            LongerText();
            KanjiText();
            Emojis();
            EmojisWithoutEci();
            BinaryData();
            GraphicsFormats();
        }

        // Creates a single QR code, then writes it to an SVG file.
        private static void BasicQrCode()
        {
            const string text = "Hello, world!"; // Payload text
            var errCorLvl = QrCode.Ecc.Low; // Minimal error correction level

            var qrCode = QrCode.EncodeText(text, errCorLvl); // Create the QR code symbol
            SaveAsSvg(qrCode, "hello-world-qr.svg"); // Save as SVG
        }


        // Creates QR code with digits and alphanumeric characters only.
        private static void AlphanumericText()
        {
            // For digits, a more compact representation will automatically be chosen (3.33 bits per digit)
            var qrCode = QrCode.EncodeText("27182818284590452353602874713526624977572470936999595749669676277240766",
                QrCode.Ecc.Medium);
            SaveAsSvg(qrCode, "digits-qr.svg");

            // For an alphanumeric subset of characters (not including lower-case letters),
            // a more compact representation will be automatically chosen (5.5 bits per character)
            qrCode = QrCode.EncodeText("THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG $ * 0123456789", QrCode.Ecc.High);
            SaveAsSvg(qrCode, "alphanumeric-qr.svg");
        }

        private static void LongerText()
        {
            // Moderately large QR code using longer text
            var qrCode = QrCode.EncodeText(
                "I was a Flower of the mountain yes when I put the rose in my hair like the Andalusian girls used " +
                "or shall I wear a red yes and how he kissed me under the Moorish wall and I thought well as well " +
                "him as another and then I asked him with my eyes to ask again yes and then he asked me would I " +
                "yes to say yes my mountain flower and first I put my arms around him yes and drew him down to me " +
                "so he could feel my breasts all perfume yes and his heart was going like mad and yes I said yes " +
                "I will Yes.", QrCode.Ecc.High);
            SaveAsSvg(qrCode, "joyce-qr.svg");
        }

        private static void KanjiText()
        {
            // Select Shift-JIS encoding so Kanji characters are compactly encoded (13 bits per character)
            var qrCode = QrCode.EncodeTextAdvanced("こんにちは", QrCode.Ecc.Medium, eci: ECI.ShiftJIS);
            SaveAsSvg(qrCode, "kanji-qr.svg");
        }

        private static void Emojis()
        {
            // The full Unicode character set is supported.
            // By default, the library uses UTF-8 encoding and indicates this with an ECI designator.
            var qrCode = QrCode.EncodeText("🎲 😇 🤒 🏌 ⏭ 🚍", QrCode.Ecc.Quartile);
            SaveAsSvg(qrCode, "emojis-qr.svg");
        }

        private static void EmojisWithoutEci()
        {
            // Suppress the ECI designator.
            // Most QR code readers will correctly guess the encoding.
            // Some readers always ignore the ECI designator.
            var qrCode = QrCode.EncodeTextAdvanced("🎲 😇 🤒 🏌 ⏭ 🚍", QrCode.Ecc.Quartile,
                encoding: Encoding.UTF8, eci: ECI.None);
            SaveAsSvg(qrCode, "emojis-qr.svg");
        }

        private static void BinaryData()
        {
            // Encode binary data. An ECI designator will be added to indicate it.
            // Exchanging binary data with QR codes usually works in closed systems only.
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

        private static void GraphicsFormats()
        {
            // Save the QR code in various graphics formats, directly supported by the library.
            // See the demo applications for further graphics format and displaying options.
            var qrCode = QrCode.EncodeText(
                "Ineluctable modality of the visible: at least that if no more, thought through my eyes. Signatures " +
                "of all things I am here to read, seapawn and searack, the nearing tide, that rusty boot. Snotgreen, " +
                "bluesilver, rust: colored signs. Limits of the diaphane.", QrCode.Ecc.Medium);

            File.WriteAllBytes("qr-code.png", qrCode.ToPngBitmap(border: 4));

            File.WriteAllBytes("qr-code.bmp", qrCode.ToBmpBitmap(border: 4));
        }

        private static void SaveAsSvg(QrCode qrCode, string filname)
        {
            string svg = qrCode.ToSvgString(4); // Convert to SVG XML code
            File.WriteAllText(filname, svg, Encoding.UTF8); // Write image to file
        }

    }
}
