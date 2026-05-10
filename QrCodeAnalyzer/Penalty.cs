using System.Collections.Generic;

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

            return rectangles;
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
            return rectangles;
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
            return rectangles;
        }
    }
}
