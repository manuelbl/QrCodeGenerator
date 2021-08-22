using System.Drawing;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class BitmapColorTest
    {

        [Fact]
        public void BitmapColorImageTest()
        {
            var qrCode = QrCode.EncodeText("The quick brown fox jumps over the lazy dog", QrCode.Ecc.High);
            var fg = Color.Red;
            var bg = Color.Black;
            using (var bitmap = qrCode.ToBitmap(3, 0, fg, bg))
            {
                var containsFg = false;
                var containsBg = false;
                //Start from bottom right
                for (int i = bitmap.Height - 1; i >= 0 && !(containsFg && containsBg); i--)
                {
                    for (int j = bitmap.Width - 1; j >= 0 && !(containsFg && containsBg); j--)
                    {
                        var pixel = bitmap.GetPixel(j, i).ToArgb();

                        if (pixel == fg.ToArgb())
                            containsFg = true;
                        else if (pixel == bg.ToArgb())
                            containsBg = true;

                    }
                }
                Assert.True(containsFg);
                Assert.True(containsBg);
            }
        }
    }
}
