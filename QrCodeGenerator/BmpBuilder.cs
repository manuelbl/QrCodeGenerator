/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    internal class BmpBuilder
    {
        internal BmpBuilder(int size, bool[,] modules)
        {
            _size = size;
            _modules = modules;
        }

        private readonly int _size;

        // The modules of this QR code (false = light, true = dark).
        // Immutable after constructor finishes.
        private readonly bool[,] _modules;

        [SuppressMessage("csharpsquid", "S3776")]
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
    }
}
