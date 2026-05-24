/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class RectangleTest
    {
        [Theory]
        [InlineData("A", Ecc.Low)]
        [InlineData("A", Ecc.High)]
        [InlineData("Hello, world!", Ecc.Medium)]
        [InlineData("12345678901234567890", Ecc.Quartile)]
        [InlineData("https://github.com/manuelbl/QrCodeGenerator", Ecc.Medium)]
        [InlineData("At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis "
            + "praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias "
            + "excepturi sint occaecati cupiditate non provident.", Ecc.High)]
        public void Rectangles_CoverExactlyTheDarkModules(string text, Ecc ecl)
        {
            var qrCode = EncodeText(text, ecl);
            var size = qrCode.Size;
            var covered = new bool[size, size];

            foreach (var rect in qrCode.ToRectangles())
            {
                // Rectangle has a positive size and lies within the QR code bounds.
                Assert.True(rect.Width > 0);
                Assert.True(rect.Height > 0);
                Assert.True(rect.X >= 0 && rect.X + rect.Width <= size);
                Assert.True(rect.Y >= 0 && rect.Y + rect.Height <= size);

                for (var y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    for (var x = rect.X; x < rect.X + rect.Width; x++)
                    {
                        // Only dark modules are covered.
                        Assert.True(qrCode.GetModule(x, y), $"Rectangle covers light module at ({x},{y})");
                        // Rectangles are non-overlapping.
                        Assert.False(covered[y, x], $"Module ({x},{y}) covered by more than one rectangle");
                        covered[y, x] = true;
                    }
                }
            }

            // The union of the rectangles is exactly the set of dark modules.
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    Assert.Equal(qrCode.GetModule(x, y), covered[y, x]);
                }
            }
        }

        [Fact]
        public void Rectangles_AreMerged()
        {
            // The top-left finder pattern's top row is a single 7-wide rectangle,
            // proving adjacent modules are merged rather than emitted one by one.
            var qrCode = EncodeText("A", Ecc.Medium);
            Assert.Contains(new QrRectangle(0, 0, 7, 1), qrCode.ToRectangles());
        }

        [Fact]
        public void QrRectangle_Properties()
        {
            var rect = new QrRectangle(2, 3, 5, 7);
            Assert.Equal(2, rect.X);
            Assert.Equal(3, rect.Y);
            Assert.Equal(5, rect.Width);
            Assert.Equal(7, rect.Height);
        }

        [Fact]
        public void QrRectangle_Equality()
        {
            var a = new QrRectangle(1, 2, 3, 4);
            var b = new QrRectangle(1, 2, 3, 4);
            var c = new QrRectangle(1, 2, 3, 5);

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());

            Assert.False(a.Equals(c));
            Assert.True(a != c);
            Assert.False(a == c);
            Assert.False(a.Equals("not a rectangle"));
        }

        [Fact]
        public void QrRectangle_Deconstruct()
        {
            var (x, y, width, height) = new QrRectangle(2, 3, 5, 7);
            Assert.Equal(2, x);
            Assert.Equal(3, y);
            Assert.Equal(5, width);
            Assert.Equal(7, height);
        }

        [Fact]
        public void QrRectangle_ToString()
        {
            Assert.Equal("QrRectangle(X=2, Y=3, Width=5, Height=7)", new QrRectangle(2, 3, 5, 7).ToString());
        }
    }
}
