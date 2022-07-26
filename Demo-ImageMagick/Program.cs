//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2022 suxiaobu9, Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using ImageMagick;
using Net.Codecrete.QrCodeGenerator;

string text = "Hello, world!",
    fileName = "hello-world-QR.png";

var qr = QrCode.EncodeText(text, QrCode.Ecc.Medium);

qr.SaveAsPng(fileName, 10, 4, MagickColors.Black, MagickColors.White);
