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

            return FilterNotInFinderArea(rectangles, qr.Size);
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

            return FilterNotInFinderArea(rectangles, qr.Size);
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
            return FilterNotInFinderArea(rectangles, qr.Size);
        }

        internal static List<Rectangle> GetHorizontalFinderPatterns(QrCode qr)
        {
            return GetFinderPatterns(qr, false);
        }

        internal static List<Rectangle> GetVerticalFinderPatterns(QrCode qr)
        {
            return GetFinderPatterns(qr, true);
        }

        // Locates finder-like patterns (1:1:3:1:1, i.e. the 7 modules "1011101")
        // with at least 4 white modules on one side and at least 1 on the other.
        // Modules outside the QR code are treated as white (the quiet zone).
        // When 'vertical' is true, the scan runs along columns instead of rows.
        private static List<Rectangle> GetFinderPatterns(QrCode qr, bool vertical)
        {
            var rectangles = new List<Rectangle>();
            var size = qr.Size;

            // The 7-module core spans positions p..p+6; the side checks look 4 modules
            // beyond each end, so the scan ranges over every position where the core
            // can sit while its required margins fall within the quiet zone or matrix.
            for (var line = 0; line < size; line++)
            {
                for (var p = 0; p <= size - 7; p++)
                {
                    if (!IsFinderCore(qr, line, p, vertical))
                    {
                        continue;
                    }

                    var leftWhite = WhiteCount(qr, line, p - 1, -1, vertical);
                    var rightWhite = WhiteCount(qr, line, p + 7, 1, vertical);
                    if ((leftWhite >= 4 && rightWhite >= 1) || (leftWhite >= 1 && rightWhite >= 4))
                    {
                        rectangles.Add(vertical
                            ? new Rectangle(line, p, 1, 7)
                            : new Rectangle(p, line, 7, 1));
                    }
                }
            }

            return FilterNotInFinderArea(rectangles, size);
        }

        // Returns whether the 7 modules starting at 'p' match the core "1011101".
        private static bool IsFinderCore(QrCode qr, int line, int p, bool vertical)
        {
            return Module(qr, line, p, vertical)
                && !Module(qr, line, p + 1, vertical)
                && Module(qr, line, p + 2, vertical)
                && Module(qr, line, p + 3, vertical)
                && Module(qr, line, p + 4, vertical)
                && !Module(qr, line, p + 5, vertical)
                && Module(qr, line, p + 6, vertical);
        }

        // Counts consecutive white (and out-of-bounds) modules from 'start', stepping
        // by 'step', up to a maximum of 4 (the largest margin the rule cares about).
        private static int WhiteCount(QrCode qr, int line, int start, int step, bool vertical)
        {
            var count = 0;
            for (var i = 0; i < 4; i++)
            {
                if (Module(qr, line, start + i * step, vertical))
                {
                    break;
                }
                count++;
            }
            return count;
        }

        // Reads a module, treating coordinates outside the QR code as white (false).
        private static bool Module(QrCode qr, int line, int pos, bool vertical)
        {
            var x = vertical ? line : pos;
            var y = vertical ? pos : line;
            if (x < 0 || y < 0 || x >= qr.Size || y >= qr.Size)
            {
                return false;
            }
            return qr.GetModule(x, y);
        }

        private static List<Rectangle> FilterNotInFinderArea(List<Rectangle> rectangles, int size)
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
