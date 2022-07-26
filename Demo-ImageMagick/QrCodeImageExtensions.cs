//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2022 suxiaobu9, Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using ImageMagick;

namespace Net.Codecrete.QrCodeGenerator;

public static class QrCodeImageExtensions
{
    /// <summary>
    /// Creates a image of this QR code.
    /// <para>
    /// The <paramref name="scale"/> parameter specifies the scale of the image, which is
    /// equivalent to the width and height of each QR code module. Additionally, the number
    /// of modules to add as a border to all four sides can be specified.
    /// </para>
    /// <para>
    /// For example, <c>ToBitmap(scale: 10, border: 4)</c> means to pad the QR code with 4 white
    /// border modules on all four sides, and use 10&#xD7;10 pixels to represent each module.
    /// </para>
    /// </summary>
    /// <param name="scale">The width and height, in pixels, of each module.</param>
    /// <param name="border">The number of border modules to add to each of the four sides.</param>
    /// <param name="background">The background color.</param>
    /// <param name="foreground">The foreground color.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static MagickImage ToImage(this QrCode qrCode, int scale, int border, MagickColor foreground, MagickColor background)
    {
        if (scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), " Value out of range");
        }

        if (border < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(border), " Value out of range");
        }

        var size = qrCode.Size;
        var dim = (size + border * 2) * scale;

        if (dim > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), " Scale or border too large");
        }

        var image = new MagickImage(background, dim, dim)
        {
            Format = MagickFormat.Png,
        };

        var drawables = new Drawables();
        drawables.FillColor(foreground);

        for (var x = 0; x < size; x++)
        {
            var pointerX = (x + border) * scale;

            for (var y = 0; y < size; y++)
            {
                if (qrCode.GetModule(x, y))
                {
                    var pointerY = (y + border) * scale;
                    drawables.Rectangle(pointerX, pointerY, pointerX + scale - 1, pointerY + scale - 1);
                }
            }
        }
        drawables.Draw(image);
        return image;
    }

    /// <summary>
    /// Creates a PNG image of this QR code and returns it as a byte array.
    /// <para>
    /// The <paramref name="scale"/> parameter specifies the scale of the image, which is
    /// equivalent to the width and height of each QR code module. Additionally, the number
    /// of modules to add as a border to all four sides can be specified.
    /// </para>
    /// <para>
    /// For example, <c>ToPng(scale: 10, border: 4)</c> means to pad the QR code with 4 white
    /// border modules on all four sides, and use 10&#xD7;10 pixels to represent each module.
    /// </para>
    /// </summary>
    /// <param name="scale">The width and height, in pixels, of each module.</param>
    /// <param name="border">The number of border modules to add to each of the four sides.</param>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    /// <returns></returns>
    public static byte[] ToPng(this QrCode qrCode, int scale, int border, MagickColor foreground, MagickColor background)
    {
        using var image = qrCode.ToImage(scale, border, foreground, background);
        return image.ToByteArray();
    }

    /// <summary>
    /// Saves this QR code as a PNG file.
    /// <para>
    /// The <paramref name="scale"/> parameter specifies the scale of the image, which is
    /// equivalent to the width and height of each QR code module. Additionally, the number
    /// of modules to add as a border to all four sides can be specified.
    /// </para>
    /// <para>
    /// For example, <c>SaveAsPng("qrcode.png", scale: 10, border: 4)</c> means to pad the QR code with 4 white
    /// border modules on all four sides, and use 10&#xD7;10 pixels to represent each module.
    /// </para>
    /// </summary>
    /// <param name="scale">The width and height, in pixels, of each module.</param>
    /// <param name="border">The number of border modules to add to each of the four sides.</param>
    /// <param name="foreground">The foreground color.</param>
    /// <param name="background">The background color.</param>
    public static void SaveAsPng(this QrCode qrCode, string fileName, int scale, int border, MagickColor foreground, MagickColor background)
    {
        using var image = qrCode.ToImage(scale, border, foreground, background);
        image.Write(fileName);
    }
}
