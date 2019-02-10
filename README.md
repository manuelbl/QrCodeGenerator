# QR Code Generator for .NET

Open-source library for generating QR codes from text strings and byte arrays.

The library is built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.).

It is mostly a translation of Project Nayuki's Java version of the QR code generator. The project provides implementations for
many more programming languages, and the [Project Nayuki web site](https://www.nayuki.io/page/qr-code-generator-library) has additional information about the implementation.


## Features

Core features:

 * Supports encoding all 40 versions (sizes) and all 4 error correction levels, as per the QR Code Model 2 standard
 * Output formats: Raw modules/pixels of the QR symbol, SVG XML string, raster bitmap
 * Encodes numeric and special-alphanumeric text in less space than general text
 * Open source code under the permissive *MIT License*
 * Significantly shorter code but more documentation compared to competing libraries
 * available as a [NuGet package](https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/) (named *Net.Codecrete.QrCodeGenerator*)

Manual parameters:

 * You can specify the minimum and maximum *version number* allowed, and the library will automatically choose the smallest version in the range that fits the data.
 * You can specify the *mask pattern* manually, otherwise library will automatically evaluate all 8 masks and select the optimal one.
 * You can specify an *error correction level*, or optionally allow the library to boost it if it doesn't increase the version number.
 * You can create a list of *data segments* manually and add *ECI segments*.

Optional advanced features:

 * Encodes Japanese Unicode text in *Kanji mode* to save a lot of space compared to UTF-8 bytes
 * Computes *optimal segment mode* switching for text with mixed numeric/alphanumeric/general/kanji parts



## Getting started

1. Create a new Visual Studio project for .NET Core 2.x (*File > New > Project...* / *Visual C# > .NET Core > Console App (.NET Core)*)

2. Add the library via NuGet:

   Either via *Project > Manage NuGet Packages...* / *Browse* / search for *qrcodegenerator* / *Install*
   
   Or by running a command in the Package Manager Console

```
Install-Package Net.Codecrete.QrCodeGenerator -Version 1.4.1	
```
3. Add the code from the example below

4. Run it


## API Documention

See DocFX [API Documentation](https://codecrete.net/QrCodeGenerator/api/index.html)


## Examples

**Simple operation**

```cslang
using Net.Codecrete.QrCodeGenerator;

namespace Examples
{
    class SimpleOperation
    {
        static void Main(string[] args)
        {
            var qr = QrCode.EncodeText("Hello, world!", QrCode.Ecc.Medium);
            using (var bitmap = qr.ToBitmap(4, 10))
            {
                bitmap.Save("qr-code.png", ImageFormat.Png);
            }
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
        static void Main(string[] args)
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

### Raster Images

For generating raster images, the *System.Drawing* library is used. On Linux and macOS, it depends on the native shared library *GDIPlus*, which must be separatley installed.

**macOS**:

```
brew install mono-libgdiplus
```

**Linux**

```
sudo apt-get install libgdiplus
```

For troubleshooting, check [Mono's GDIPlusInit page](https://www.mono-project.com/docs/gui/problemgdiplusinit/).
