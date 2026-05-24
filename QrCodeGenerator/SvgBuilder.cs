/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Creates SVG images and SVG/XAML graphics paths from a list of QR code rectangles.
    /// </summary>
    internal static class SvgBuilder
    {
        // Creates a complete SVG document for the given rectangles.
        internal static string ToSvgString(IReadOnlyList<QrRectangle> rectangles, int size, int border,
            string foreground, string background)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            var dim = size + border * 2;
            var sb = new StringBuilder()
                .Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n")
                .Append("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n")
                .Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewBox=\"0 0 {dim} {dim}\" stroke=\"none\">\n")
                .Append($"\t<rect width=\"100%\" height=\"100%\" fill=\"{background}\"/>\n")
                .Append("\t<path d=\"");

            AppendPath(sb, rectangles, border);

            return sb
                .Append($"\" fill=\"{foreground}\"/>\n")
                .Append("</svg>\n")
                .ToString();
        }

        // Creates an SVG/XAML graphics path for the given rectangles.
        internal static string ToGraphicsPath(IReadOnlyList<QrRectangle> rectangles, int border)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            var path = new StringBuilder();
            AppendPath(path, rectangles, border);
            return path.ToString();
        }

        // Append an SVG/XAML path for the QR code rectangles to the provided string builder.
        private static void AppendPath(StringBuilder path, IReadOnlyList<QrRectangle> rectangles, int border)
        {
            for (var i = 0; i < rectangles.Count; i++)
            {
                var rect = rectangles[i];

                // append path command (no leading space before the first rectangle)
                if (i != 0)
                {
                    path.Append(' ');
                }

                // Different locales use different minus signs.
                FormattableString pathElement =
                    $"M{rect.X + border},{rect.Y + border}h{rect.Width}v{rect.Height}h{-rect.Width}z";
                path.Append(pathElement.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
