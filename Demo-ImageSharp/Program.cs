//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    internal class Program
    {
        // Create a QR code and save it as a PNG.
        internal static void Main()
        {
            HelloWorld();
            QrCodeWithImage();
        }


        internal static void HelloWorld()
        {
            var text = "Hello, world!";
            var filename = "hello-world-QR.png";

            var qr = QrCode.EncodeText(text, QrCode.Ecc.Medium); // Create the QR code symbol
            qr.SaveAsPng(filename, scale: 10, border: 4);

            Console.WriteLine($"The QR code has been saved as {Path.GetFullPath(filename)}");
        }

        internal static void QrCodeWithImage()
        {
            var text = "https://github.com/manuelbl/QrCodeGenerator";
            var filename = "qr-code-with-image.png";
            var logoFilename = "heart.png";
            const float logoWidth = 0.15f; // logo will have 15% the width of the QR code 

            var qr = QrCode.EncodeText(text, QrCode.Ecc.Medium);

            using (var bitmap = qr.ToBitmap(scale: 10, border: 4))
            using (var logo = Image.Load(logoFilename))
            {
                // resize logo
                var w = (int)Math.Round(bitmap.Width * logoWidth);
                logo.Mutate(logo => logo.Resize(w, 0));

                // draw logo in center
                var topLeft = new Point((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2);
                bitmap.Mutate(img => img.DrawImage(logo, topLeft, 1));

                // save as PNG
                bitmap.SaveAsPng(filename);
            }

            Console.WriteLine($"The QR code has been saved as {Path.GetFullPath(filename)}");
        }
    }
}
