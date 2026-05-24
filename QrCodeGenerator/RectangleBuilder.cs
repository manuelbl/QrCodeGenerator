/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Merges the dark modules of a QR code into a small set of rectangles.
    /// <para>
    /// This is a simple greedy algorithm that reduces the number of rectangles needed to
    /// draw a QR code (most rectangles cover more than a single module). The resulting
    /// rectangles are non-overlapping and their union is exactly the set of dark modules.
    /// </para>
    /// </summary>
    internal static class RectangleBuilder
    {
        // Merges the dark modules of the given matrix into rectangles.
        // The input matrix is not modified (a working copy is used internally).
        internal static IReadOnlyList<QrRectangle> Build(BitMatrix modules)
        {
            // Work on a copy as the algorithm is destructive (it clears processed modules).
            var working = modules.Copy();
            
            var size = working.Size;
            var rectangles = new List<QrRectangle>();

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if (working.Get(x, y))
                    {
                        rectangles.Add(ExtractLargestRectangle(working, x, y));
                    }
                }
            }

            return rectangles;
        }

        // Find and clear the largest rectangle with (x, y) as the top left corner.
        private static QrRectangle ExtractLargestRectangle(BitMatrix modules, int x, int y)
        {
            var size = modules.Size;

            var bestW = 1;
            var bestH = 1;
            var maxArea = 1;

            var xLimit = size;
            var iy = y;
            while (iy < size && modules.Get(x, iy))
            {
                var w = 0;
                while (x + w < xLimit && modules.Get(x + w, iy))
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

            ClearRectangle(modules, x, y, bestW, bestH);
            return new QrRectangle(x, y, bestW, bestH);
        }

        // Clear a rectangle of modules.
        private static void ClearRectangle(BitMatrix modules, int x, int y, int width, int height)
        {
            for (var iy = y; iy < y + height; iy++)
            {
                for (var ix = x; ix < x + width; ix++)
                {
                    modules.Set(ix, iy, false);
                }
            }
        }
    }
}
