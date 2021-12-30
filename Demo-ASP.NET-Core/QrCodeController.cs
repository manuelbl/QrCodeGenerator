//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    /// <summary>
    /// Controller for generating QR code as PNG or SVG images
    /// </summary>
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        private static readonly QrCode.Ecc[] errorCorrectionLevels = { QrCode.Ecc.Low, QrCode.Ecc.Medium, QrCode.Ecc.Quartile, QrCode.Ecc.High };

        /// <summary>
        /// Generates QR code as PNG image
        /// </summary>
        /// <param name="text">Text to encode in QR code</param>
        /// <param name="ecc">Error correction level (0: low ... 3: high)</param>
        /// <param name="borderWidth">Border width in multiples of a module (QR code pixel)</param>
        /// <returns>PNG image</returns>
        [HttpGet("qrcode/png")]
        [ResponseCache(Duration = 2592000)]
        public ActionResult<byte[]> GeneratePng([FromQuery(Name = "text")] string text,
            [FromQuery(Name = "ecc")] int? ecc, [FromQuery(Name = "border")] int? borderWidth)
        {
            ecc = Math.Clamp(ecc ?? 1, 0, 3);
            borderWidth = Math.Clamp(borderWidth ?? 3, 0, 999999);

            var qrCode = QrCode.EncodeText(text, errorCorrectionLevels[(int)ecc]);
            byte[] png = qrCode.ToPng(20, (int)borderWidth);
            return new FileContentResult(png, "image/png");
        }

        /// <summary>
        /// Generates QR code as SVG image
        /// </summary>
        /// <param name="text">Text to encode in QR code</param>
        /// <param name="ecc">Error correction level (0: low ... 3: high)</param>
        /// <param name="borderWidth">Border width in multiples of a module (QR code pixel)</param>
        /// <returns>SVG image</returns>
        [HttpGet("qrcode/svg")]
        [ResponseCache(Duration = 2592000)]
        public ActionResult<byte[]> GenerateSvg([FromQuery(Name = "text")] string text,
            [FromQuery(Name = "ecc")] int? ecc, [FromQuery(Name = "border")] int? borderWidth)
        {
            ecc = Math.Clamp(ecc ?? 1, 0, 3);
            borderWidth = Math.Clamp(borderWidth ?? 3, 0, 999999);

            var qrCode = QrCode.EncodeText(text, errorCorrectionLevels[(int)ecc]);
            byte[] svg = Encoding.UTF8.GetBytes(qrCode.ToSvgString((int)borderWidth));
            return new FileContentResult(svg, "image/svg+xml; charset=utf-8");
        }
    }
}
