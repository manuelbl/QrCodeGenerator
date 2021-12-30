# QR Code Generator for .NET

Open-source library for generating QR codes from text strings and byte arrays.

The library is built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.) including .NET 6 on all platforms.

It is mostly a translation of Project Nayuki's Java version of the QR code generator. The project provides implementations for
many more programming languages, and the [Project Nayuki web site](https://www.nayuki.io/page/qr-code-generator-library) has additional information about the implementation.


## Features

Core features:

 * Supports encoding all 40 versions (sizes) and all 4 error correction levels, as per the QR Code Model 2 standard
 * Output formats: Raw modules/pixels of the QR symbol, SVG XML string. For raster bitmaps, additional code is provided. See [below](#raster-images--bitmaps).
 * Encodes numeric and special-alphanumeric text in less space than general text
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



## Getting started

1. Create a new Visual Studio project for .NET Core 3.1 (or higher) (*File > New > Project...* / *Visual C# > .NET Core > Console App (.NET Core)*)

2. Add the library via NuGet:

   Either via *Project > Manage NuGet Packages...* / *Browse* / search for *qrcodegenerator* / *Install*
   
   Or by running a command in the Package Manager Console

```
Install-Package Net.Codecrete.QrCodeGenerator -Version 2.0.1
```
3. Add the code from the example below

4. Run it


## API Documention

See [API Documentation](https://codecrete.net/QrCodeGenerator/api/index.html)


## Examples

**Simple operation**

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

**Manual operation**

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

Starting with .NET 6, *System.Drawing* is only supported on Windows operating system and thus cannot be used for multi-platform libraries like this one. Therefore, `ToBitmap()` has been removed and three options are now offered in the form of method extensions.

To use it:

- Select one of the imaging libraries below
- Add the NuGet dependencies to your project
- Copy the appropriate `QrCodeBitmapExtensions.cs` file to your project

| Imaging library | Recommendation | NuGet dependencies | Extension file |
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

## Examples

Several example projects are provided:

- [Demo-QRCode-Variety](Demo-QRCode-Variety): Demonstrates how QR codes with different encodings, error correction and masks can be generated. All QR codes are saved as SVG files.

- [Demo-WindowsPresentationFoundation](Demo-WindowsPresentationFoundation): Demonstrates how QR codes can be used in WPF applications (incl. copying to the clipboard).

- [Demo-WinForms](Demo-WinForms): Demonstrates how QR codes can be used in Windows Forms applications (incl. copying to the clipboard).

- [Demo-VCard](Demo-VCard): Demonstrates how contact data (similar to business cards) can be saved in a QR Code using the VCard standard.

- [Demo-System-Drawing](Demo-System-Drawing): Demonstrates how a QR code can be saved a PNG file, using the *System.Drawing* classes, which have become a Windows only technology starting with .NET 6.

- [Demo-SkiaSharp](Demo-SkiaSharp): Demonstrates how a QR code can be saved a PNG file, using the SkiaSharp multi-platform raster image library.

- [Demo-ImageSharp](Demo-ImageSharp): Demonstrates how a QR code can be saved a PNG file, using the ImageSharp raster image library. Additionally, a QR code with an image in the center is created.
