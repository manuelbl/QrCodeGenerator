# QR Code Generator for .NET

Open-source library for generating QR codes from text strings and byte arrays.

The library is built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.) including .NET 6 on all platforms.

It started as a C# port of [Project Nayuki's](https://www.nayuki.io/page/qr-code-generator-library) Java version.
Version 3 is a complete rewrite of the library, and it is more standard compliant
and about 10x faster than the original implementation.


## Features

Core features:

- Supports encoding all 40 versions (sizes) and all 4 error correction levels, as per the QR Code Model 2 standard
- Output formats: Raw modules/pixels of the QR symbol, SVG, XAML path, PNG and BMP files. For other raster bitmap formats, see [below](#raster-images--bitmaps).
- Computes optimal segment modes for smallest possible QR code.
- High speed: 10x faster than comparable libraries
- Open source code under the permissive *MIT License*
- Built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.).
- Available as a [NuGet package](https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/) (named *Net.Codecrete.QrCodeGenerator*)
- Example code for WinForms, WPF, ASP.NET, ImageSharp, SkiaSharp and many more

Advanced features:

- Specify minimum and maximum version number allowed
- Specify text encoding and use of ECI designators
- Create data segments manually
- Split long text into multiple linked QR codes (aka Structured Append)


## Getting started

1. Create a new Visual Studio project for .NET 8 (or higher) (*File > New > Project...* / *Visual C# > .NET Core > Console App (.NET Core)*)

2. Add the library via NuGet:

   Either via *Project > Manage NuGet Packages...* / *Browse* / search for *qrcodegenerator* / *Install*
   
   Or by running a command in the Package Manager Console

```
Install-Package Net.Codecrete.QrCodeGenerator -Version 3.0.0
```
3. Add the code from the example below

4. Run it


## API Documentation

See [API Documentation](https://codecrete.net/QrCodeGenerator/api/index.html)


## Code Examples

**Simple operation**

```csharp
using System.IO;
using System.Text;
using Net.Codecrete.QrCodeGenerator;

namespace Examples
{
    class SimpleOperation
    {
        static void Main()
        {
            var qr = QrCode.EncodeText("Hello, world!", QrCode.Ecc.Medium);
            string svg = qr.ToSvgString(4);
            File.WriteAllText("hello-world-qr.svg", svg, Encoding.UTF8);
        }
    }
}
```

**Manual operation**

```csharp
using Net.Codecrete.QrCodeGenerator;

namespace Examples
{
    class ManualOperation
    {
        static void Main()
        {
            var qrCode = QrCode.EncodeTextAdvanced("3141592653589793238462643383",
                QrCode.Ecc.High, eci: ECI.Latin9, minVersion: 5, maxVersion: 5);
            foreach (var rect in qrCode.ToRectangles())
            {
                ... paint rectangle rect.X, rect.Y, rect.Width, rect.Height
            }
        }
    }
}
```


## Requirements

QR Code Generator for .NET requires a .NET implementation compatible with .NET Standard 2.0 or higher, i.e. any of:

- .NET Core 2.0 or higher
- .NET Framework 4.6.1 or higher
- Mono 5.4 or higher
- Universal Windows Platform 10.0.16299 or higher
- Xamarin

### Raster Images / Bitmaps

Starting with .NET 6, *System.Drawing* is only supported on Windows operating system and thus cannot be used for multi-platform libraries like this one. Therefore, `ToBitmap()` has been removed.

Two raster bitmap formats are supported with the need for additional libraries:

- *PNG*: See `QrCode.ToPngBitmap()`
- *BMP*: See `QrCode.ToBmpBitmap()`

These methods are limited, e.g. with regards to the size of the generated image.
For more advanced and more efficient ways to generate different raster image formats:

- Select one of the imaging libraries below
- Add the NuGet dependencies to your project
- Copy the appropriate `QrCodeBitmapExtensions.cs` file to your project

| Imaging library | Recommendation | NuGet dependencies | Extension file |
| ------- | -------------- | ------------------ | -------------- |
| **System.Drawing** | For Windows only projects | `System.Drawing.Common` | [QrCodeBitmapExtensions.cs](Demo-System-Drawing/QrCodeBitmapExtensions.cs) |
| **SkiaSharp** | For macOS, Linux, iOS, Android and multi-platform projects | `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` (for Linux only) | [QrCodeBitmapExtensions.cs](Demo-SkiaSharp/QrCodeBitmapExtensions.cs) |
| **ImageSharp** | Alternative for multi-platform projects. Might require a commercial license. | `SixLabors.ImageSharp.Drawing` | [QrCodeBitmapExtensions.cs](Demo-ImageSharp/QrCodeBitmapExtensions.cs) |

Using these extension methods, generating PNG images is straight-forward:

```csharp
using Net.Codecrete.QrCodeGenerator;

namespace Examples
{
    class PngImage
    {
        static void Main()
        {
            var qr = QrCode.EncodeText("Hello, world!", QrCode.Ecc.Medium);
            qr.SaveAsPng("hello-world-qr.png", 10, 3);
        }
    }
}
```

## Demo Projects

Several example projects demonstrate how to generate QR code with different frameworks and libraries:

- [Basic-Example](Basic-Example): Demonstrates the basic use of the libraries (different texts, error correction level). All QR codes are saved as either SVG, PNG or BMP files.

- [Demo-WinUI](Demo-WinUI): Demonstrates how QR codes can be used in WinUI 3 applications and/or using [Win2D](https://github.com/microsoft/Win2D) (incl. copying to the clipboard).

- [Demo-WindowsPresentationFoundation](Demo-WindowsPresentationFoundation): Demonstrates how QR codes can be used in WPF applications (incl. copying to the clipboard).

- [Demo-WinForms](Demo-WinForms): Demonstrates how QR codes can be used in Windows Forms applications (incl. copying to the clipboard).

- [Demo-ASP.NET-Core](Demo-ASP.NET-Core): Demonstrates how to create QR codes in a web application implemented using ASP.NET Core.

- [Demo-VCard](Demo-VCard): Demonstrates how contact data (similar to business cards) can be saved in a QR Code using the VCard standard.

- [Demo-System-Drawing](Demo-System-Drawing): Demonstrates how a QR code can be saved a PNG file, using the *System.Drawing* classes, which have become a Windows only technology starting with .NET 6.

- [Demo-SkiaSharp](Demo-SkiaSharp): Demonstrates how a QR code can be saved a PNG file, using the SkiaSharp multi-platform raster image library.

- [Demo-ImageSharp](Demo-ImageSharp): Demonstrates how a QR code can be saved a PNG file, using the ImageSharp raster image library. Additionally, a QR code with an image in the center is created.

- [Demo-ImageMagick](Demo-ImageMagick): Demonstrates how a QR code can be saved a PNG file, using the Magick.NET image manipulation library (based on ImageMagick).


## Upgrade from version 2.x to version 3.x

If your code uses `QrCode.EncodeText()` for generating QR codes, recompiling the code should be sufficient.

The generated QR code will not be an exact 1-to-1 match. Version 3 optimizes the
data segments and thus can achieve a smaller QR code or a higher error correction
level for the same text. Furthermore, if the text cannot be encoded in Latin-1,
it will be encoded in UTF-8 together with an ECI designator indicating it.
This is more standard-compliant than the previous version. Further differences
can arise from a different data mask selection. In most cases, the differences
will be irrelevant.

If your code uses `QrSegment.MakeSegments()` or other advanced methods,
you might first want to look at `QrCode.EncodeTextAdvanced()`. If this is not
sufficient for your use case, it is still possible to create the data segments
manually. The class is now called `DataSegment` instead of `QrSegment`.

The new library version no longer allows to select a specific data masking pattern
as it can lead to QR codes that a very difficult to scan and violate the standard.
For backward compatibility, the parameter is still present in one of the methods.
But it is ignored.
