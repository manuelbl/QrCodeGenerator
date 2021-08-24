# QR Code Generator for .NET

Open-source library for generating QR codes from text strings and byte arrays.


## .NET API Documention

* [QrCode](xref:Net.Codecrete.QrCodeGenerator.QrCode): Creates and represents QR codes

* [QrSegment](xref:Net.Codecrete.QrCodeGenerator.QrSegment): Represents a segment of character/binary/control data in a QR code symbol

* [QrSegmentAdvanced](xref:Net.Codecrete.QrCodeGenerator.QrSegmentAdvanced): Advanced methods for encoding QR codes using Kanji mode or using multiple segments with different encodings.

* [All types and classes](xref:Net.Codecrete.QrCodeGenerator)

Additional information on [GitHub project page](https://github.com/manuelbl/QrCodeGenerator)


## Features

Core features:

 * Supports encoding all 40 versions (sizes) and all 4 error correction levels, as per the QR Code Model 2 standard
 * Output formats: Raw modules/pixels of the QR symbol and SVG XML string
 * Encodes numeric and special alphanumeric text in less space than general text
 * Open source code under the permissive *MIT License*
 * Significantly shorter code but more documentation compared to competing libraries
 * Available as a [NuGet package](https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/) (named *Net.Codecrete.QrCodeGenerator*)

Manual parameters:

 * You can specify the minimum and maximum *version number* allowed, and the library will automatically choose the smallest version in the range that fits the data.
 * You can specify the *mask pattern* manually, otherwise library will automatically evaluate all 8 masks and select the optimal one.
 * You can specify an *error correction level*, or optionally allow the library to boost it if it doesn't increase the version number.
 * You can create a list of *data segments* manually and add *ECI segments*.

Optional advanced features:

 * Encodes Japanese Unicode text in *Kanji mode* to save a lot of space compared to UTF-8 bytes
 * Computes *optimal segment mode* switching for text with mixed numeric/alphanumeric/general/kanji parts


## Examples

Simple operation:

```cslang
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

Manual operation:

```cslang
using Net.Codecrete.QrCodeGenerator;

namespace Examples
{
    class ManualOperation
    {
        static void Main()
        {
            var segments = QrCode.MakeSegments("3141592653589793238462643383");
            var qr = QrCode.EncodeSegments(segments, QrCode.Ecc.High, 5, 5, 2, false);
            for (int y = 0; y < qr.Size; y++)
            {
                for (int x = 0; x < qr.Size; x++)
                {
                    ... paint qr.GetModule(x,y) ...
                }
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

The previous version of this library depended on *System.Drawing*, which - starting with .NET 6 - will only be supported on Windows operation system. Therefore, `ToBitmap()` has been removed and three options are now offered in the form of method extensions.

In order to use it:

- Select one of the libraries
- Add the NuGet dependencies to your project
- Copy the appropriate `QrCodeBitmapExtensions.cs` file to your project

| Library | Recommendation | NuGet dependencies | Extension file |
| ------- | -------------- | ------------------ | -------------- |
| **System.Drawing** | For Windows only projects | `System.Drawing.Common` | [QrCodeBitmapExtensions.cs](Demo-System-Drawing/QrCodeBitmapExtensions.cs) |
| **SkiaSharp** | For macOS, Linux, iOS, Android and multi-platform projects | `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` (for Linux only) | [QrCodeBitmapExtensions.cs](Demo-SkiaSharp/QrCodeBitmapExtensions.cs) |
| **ImageSharp** | Currently in beta state | `SixLabors.ImageSharp.Drawing` | [QrCodeBitmapExtensions.cs](Demo-ImageSharp/QrCodeBitmapExtensions.cs) |

Using these extension methods, generating PNG images is straight-forward:

```cslang
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
