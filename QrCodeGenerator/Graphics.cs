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

using System;
using System.Globalization;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    internal class Graphics
    {
        internal Graphics(int size, bool[,] modules)
        {
            _size = size;
            _modules = modules;
        }

        private readonly int _size;

        // The modules of this QR code (false = light, true = dark).
        // Immutable after constructor finishes.
        private readonly bool[,] _modules;

        internal string ToSvgString(int border, string foreground, string background)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            var dim = _size + border * 2;
            var sb = new StringBuilder()
                .Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n")
                .Append("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\n")
                .Append($"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewBox=\"0 0 {dim} {dim}\" stroke=\"none\">\n")
                .Append($"\t<rect width=\"100%\" height=\"100%\" fill=\"{background}\"/>\n")
                .Append("\t<path d=\"");

            // Work on copy as it is destructive
            var modules = CopyModules();
            CreatePath(sb, modules, border);

            return sb
                .Append($"\" fill=\"{foreground}\"/>\n")
                .Append("</svg>\n")
                .ToString();
        }

        internal string ToGraphicsPath(int border)
        {
            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), "Border must be non-negative");
            }

            // Work on copy as it is destructive
            var modules = CopyModules();
            var path = new StringBuilder();
            CreatePath(path, modules, border);
            return path.ToString();
        }

        internal byte[] ToBmpBitmap(int border, int scale, int foreground, int background)
        {
            if (scale < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");
            }

            if (border < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(border), border, "Border must be non-negative.");
            }

            var dim = (_size + 2 * border) * scale;

            if (dim > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale or border too large.");
            }

            // NOTE: Works for Size > 0
            // Modules to bytes
            // x >> 3 == x / 8
            var bytesToWrite = ((dim - 1) >> 3) + 1;

            // NOTE: Align to 4 bytes
            // This is a Bitmap requirement
            // (size + (align - 1)) & ~(align - 1)
            var aligned = (bytesToWrite + 3) & ~3;
            var fileSize = 62 + dim * aligned;

            var buf = new byte[fileSize];

            // NOTE: BMP file header
            buf[0] = (byte)'B';
            buf[1] = (byte)'M';

            buf[2] = (byte)fileSize;
            buf[3] = (byte)(fileSize >> 8);
            buf[4] = (byte)(fileSize >> 16);
            buf[5] = (byte)(fileSize >> 24);

            // NOTE: Offset to bitmap data
            buf[10] = 62;

            // NOTE: BMP info header
            buf[14] = 40;

            // NOTE: Image width
            buf[18] = (byte)dim;
            buf[19] = (byte)(dim >> 8);
            buf[20] = (byte)(dim >> 16);
            buf[21] = (byte)(dim >> 24);

            // NOTE: Image height
            buf[22] = buf[18];
            buf[23] = buf[19];
            buf[24] = buf[20];
            buf[25] = buf[21];

            // NOTE: Number of color planes (usually 1)
            // Must be non-zero
            buf[26] = 1;

            // NOTE: Number of bits per pixel (1 bpp)
            buf[28] = 1;

            // NOTE: Horizontal resolution (pixels/meter)
            // 3780 ppm (96 dpi)
            buf[38] = 196;
            buf[39] = 14;

            // NOTE: Vertical resolution (pixels/meter)
            // 3780 ppm (96 dpi)
            buf[42] = buf[38];
            buf[43] = buf[39];

            // NOTE: Color table
            // Alpha isn't useful here
            // Foreground - Dark
            buf[54] = (byte)foreground; // blue
            buf[55] = (byte)(foreground >> 8); // green
            buf[56] = (byte)(foreground >> 16); // red

            // Background - Light
            buf[58] = (byte)background; // blue
            buf[59] = (byte)(background >> 8); // green
            buf[60] = (byte)(background >> 16); // red

            var scaledBorder = border * scale;

            int i;
            int y;
            byte px;

            if (border > 0)
            {
                var scaledSize = _size * scale;

                for (i = 0; i < aligned; ++i)
                {
                    px = 255;

                    if (i == bytesToWrite - 1)
                    {
                        px = (byte)(255 << ((bytesToWrite << 3) - dim));
                    }
                    else if (i >= bytesToWrite)
                    {
                        px = 0;
                    }

                    for (y = 0; y < scaledBorder; ++y)
                    {
                        buf[62 + i + y * aligned] = px;
                        buf[62 + i + (y + scaledSize + scaledBorder) * aligned] = px;
                    }
                }
            }

            for (y = 0; y < _size; ++y)
            {
                int j;
                var yOffset = y * scale + scaledBorder;

                for (i = 0; i < aligned; ++i)
                {
                    px = 0;

                    for (j = 0; j < 8; ++j)
                    {
                        var x = ((i << 3) + j) / scale;

                        if (x >= dim)
                        {
                            continue;
                        }

                        if (x < border || x >= _size + border)
                        {
                            px |= (byte)(1 << (7 - j));
                            continue;
                        }

                        px |= (byte)(_modules[(_size - y - 1), x - border] ? 0 : 1 << (7 - j));
                    }

                    buf[62 + i + yOffset * aligned] = px;
                }

                // NOTE: Copy rows when scaling
                for (i = 1; i <= scale - 1; ++i)
                {
                    for (j = 0; j < aligned; ++j)
                    {
                        buf[62 + j + (yOffset + i) * aligned] = buf[62 + j + yOffset * aligned];
                    }
                }
            }

            return buf;
        }

        // Append a SVG/XAML path for the QR code to the provided string builder
        private static void CreatePath(StringBuilder path, bool[,] modules, int border)
        {
            // Simple algorithm to reduce the number of rectangles for drawing the QR code
            // and reduce SVG/XAML size.
            var size = modules.GetLength(0);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if (modules[y, x])
                    {
                        DrawLargestRectangle(path, modules, x, y, border);
                    }
                }
            }
        }

        // Find, draw and clear largest rectangle with (x, y) as the top left corner
        private static void DrawLargestRectangle(StringBuilder path, bool[,] modules, int x, int y, int border)
        {
            var size = modules.GetLength(0);

            var bestW = 1;
            var bestH = 1;
            var maxArea = 1;

            var xLimit = size;
            var iy = y;
            while (iy < size && modules[iy, x])
            {
                var w = 0;
                while (x + w < xLimit && modules[iy, x + w])
                {
                    w++;
                }

                var area = w * (iy - y + 1);
                if (area > maxArea)
                {
                    maxArea = area;
                    bestW = w;
                    bestH = iy - y + 1;
                }
                xLimit = x + w;
                iy++;
            }

            // append path command
            if (x != 0 || y != 0)
            {
                path.Append(' ');
            }

            // Different locales use different minus signs.
            FormattableString pathElement = $"M{x + border},{y + border}h{bestW}v{bestH}h{-bestW}z";
            path.Append(pathElement.ToString(CultureInfo.InvariantCulture));

            // clear processed modules
            ClearRectangle(modules, x, y, bestW, bestH);
        }

        // Clear a rectangle of modules
        private static void ClearRectangle(bool[,] modules, int x, int y, int width, int height)
        {
            for (var iy = y; iy < y + height; iy++)
            {
                for (var ix = x; ix < x + width; ix++)
                {
                    modules[iy, ix] = false;
                }
            }
        }

        // Create a copy of the modules (in row-major order)
        private bool[,] CopyModules()
        {
            var modules = new bool[_size, _size];
            for (var y = 0; y < _size; y++)
            {
                for (var x = 0; x < _size; x++)
                {
                    modules[y, x] = _modules[y, x];
                }
            }

            return modules;
        }
    }
}
