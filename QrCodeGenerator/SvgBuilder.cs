/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Creates SVG images and SVG/XAML graphics paths from the outline of a QR code.
    /// <para>
    /// The dark modules are drawn as a single path of closed sub-paths, one per outline polygon.
    /// A single path has no seams between adjacent shapes, where anti-aliased rendering would
    /// otherwise show hairlines. The polygons wind holes the other way than groups, so the path
    /// needs no fill-rule attribute: both the nonzero rule (the SVG default) and the even-odd rule
    /// (the XAML default) fill it to the QR code.
    /// </para>
    /// </summary>
    internal static class SvgBuilder
    {
        // Creates a complete SVG document for the given QR code.
        internal static string ToSvgString(QrCode qrCode, int border, string foreground, string background)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            var dim = qrCode.Size + border * 2;
            var sb = new StringBuilder()
                .Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n")
                .Append("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n")
                .Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewBox=\"0 0 {dim} {dim}\" stroke=\"none\">\n")
                .Append($"\t<rect width=\"100%\" height=\"100%\" fill=\"{background}\"/>\n")
                .Append("\t<path d=\"");

            AppendPath(sb, qrCode.ToOutlines(), border);

            return sb
                .Append($"\" fill=\"{foreground}\"/>\n")
                .Append("</svg>\n")
                .ToString();
        }

        // Creates an SVG/XAML graphics path for the given QR code.
        internal static string ToGraphicsPath(QrCode qrCode, int border)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            var path = new StringBuilder();
            AppendPath(path, qrCode.ToOutlines(), border);
            return path.ToString();
        }

        // Append an SVG/XAML path for the QR code outline to the provided string builder.
        // The edges of a polygon are strictly axis-parallel, so each becomes a relative "h" or "v"
        // command; "z" draws the closing edge.
        private static void AppendPath(StringBuilder path, IReadOnlyList<QrPolygon> polygons, int border)
        {
            for (var i = 0; i < polygons.Count; i++)
            {
                var vertices = polygons[i].Vertices;

                // append path command (no leading space before the first polygon)
                if (i != 0)
                {
                    path.Append(' ');
                }

                // Different locales use different digits and minus signs.
                var first = vertices[0];
                path.Append(FormattableString.Invariant($"M{first.X + border},{first.Y + border}"));

                for (var j = 1; j < vertices.Count; j++)
                {
                    var from = vertices[j - 1];
                    var to = vertices[j];
                    if (to.Y == from.Y)
                    {
                        path.Append(FormattableString.Invariant($"h{to.X - from.X}"));
                    }
                    else
                    {
                        path.Append(FormattableString.Invariant($"v{to.Y - from.Y}"));
                    }
                }
                path.Append('z');
            }
        }
    }
}

