using System.Collections.Generic;
using System.Linq;

namespace Net.Codecrete.QrCodeGenerator.Analyzer
{
    internal static class Penalty
    {
        internal static List<Rectangle> GetBlocks(QrCode qr)
        {
            var rectangles = new List<Rectangle>();
            var size = qr.Size;
            for (var x = 0; x < size - 1; x++)
            {
                for (var y = 0; y < size - 1; y++)
                {
                    var color = qr.GetModule(x, y);
                    if (qr.GetModule(x + 1, y) == color &&
                        qr.GetModule(x, y + 1) == color &&
                        qr.GetModule(x + 1, y + 1) == color)
                    {
                        rectangles.Add(new Rectangle(x, y, 2, 2));
                    }
                }
            }

            return FilterOutsideFinderArea(rectangles, qr.Size);
        }

        internal static List<Rectangle> GetHorizontalStreaks(QrCode qr)
        {
            var rectangles = new List<Rectangle>();
            var size = qr.Size;
            for (var y = 0; y < size; y++)
            {
                var color = qr.GetModule(0, y);
                var count = 1;
                for (var x = 1; x < size; x++)
                {
                    if (qr.GetModule(x, y) == color)
                    {
                        count++;
                    }
                    else
                    {
                        if (count >= 5)
                        {
                            rectangles.Add(new Rectangle(x - count, y, count, 1));
                        }
                        count = 1;
                        color = !color;
                    }
                }
                if (count >= 5)
                {
                    rectangles.Add(new Rectangle(size - count, y, count, 1));
                }
            }

            return FilterOutsideFinderArea(rectangles, qr.Size);
        }


        internal static List<Rectangle> GetVerticalStreaks(QrCode qr)
        {
            var rectangles = new List<Rectangle>();
            var size = qr.Size;
            for (var x = 0; x < size; x++)
            {
                var color = qr.GetModule(x, 0);
                var count = 1;
                for (var y = 1; y < size; y++)
                {
                    if (qr.GetModule(x, y) == color)
                    {
                        count++;
                    }
                    else
                    {
                        if (count >= 5)
                        {
                            rectangles.Add(new Rectangle(x, y - count, 1, count));
                        }
                        count = 1;
                        color = !color;
                    }
                }
                if (count >= 5)
                {
                    rectangles.Add(new Rectangle(x, size - count, 1, count));
                }
            }
            return FilterOutsideFinderArea(rectangles, qr.Size);
        }

        private static List<Rectangle> FilterOutsideFinderArea(List<Rectangle> rectangles, int size)
        {
            return [.. rectangles.Where(r => !IsInFinderArea(r.X, r.Y, r.Width, r.Height, size))];
        }

        private static bool IsInFinderArea(int x, int y, int w, int h, int size)
        {
            return (x >= 0 && y >= 0 && x + w <= 8 && y + h <= 8)
                || (x >= size - 8 && y >= 0 && x + w <= size && y + h <= 8)
                || (x >= 0 && y >= size - 8 && x + w <= 8 && y + h <= size);
        }
    }
}
