# QR Code Generator for .NET

Open-source library for generating QR codes from text strings and byte arrays.

The library is built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.) including .NET 6 on all platforms.

It started as a C# port of [Project Nayuki's](https://www.nayuki.io/page/qr-code-generator-library) Java version.
Version 3 is a complete rewrite of the library, and it is more standard-compliant
and about 10x faster than the original implementation.


## Features

Core features:

- Supports encoding all 40 versions (sizes) and all 4 error correction levels, as per the QR Code Model 2 standard
- Output formats: List of rectangles, outline polygons, raw modules/pixels of the QR symbol, SVG, XAML path, PNG and BMP files. For other raster bitmap formats, see [below](#raster-images--bitmaps).
- Computes optimal segment modes for smallest possible QR code.
- High speed: 10x faster than most comparable libraries
- Open source code under the permissive *MIT License*
- Built for .NET Standard 2.0 and therefore runs on most modern .NET platforms (.NET Core, .NET Framework, Mono etc.).
- Available as a [NuGet package](https://www.nuget.org/packages/Net.Codecrete.QrCodeGenerator/) (named *Net.Codecrete.QrCodeGenerator*)
- Example code for WinForms, WPF, ASP.NET, ImageSharp, SkiaSharp and many more

Advanced features:

- Specify minimum and maximum version number allowed
- Specify text encoding and use of ECI designators
- Kanji mode for compact encoding of Japanese text
- Create data segments manually
- Split long text into multiple linked QR codes (aka Structured Append)
- Opt-in encoding diagnostics: per-mask penalty breakdown and chosen segments


## Getting started

1. Create a new Visual Studio project for .NET 8 (or higher) (*File > New > Project...* / *Visual C# > .NET Core > Console App (.NET Core)*)

2. Add the library via NuGet:

   Either via *Project > Manage NuGet Packages...* / *Browse* / search for *qrcodegenerator* / *Install*
   
   Or by running a command in the Package Manager Console

```
Install-Package Net.Codecrete.QrCodeGenerator -Version 3.2.0
```
3. Add the code from the example below

4. Run it

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
            var qrCode = QrCode.EncodeText("Hello, world!", QrCode.Ecc.Medium);
            File.WriteAllText("hello-world-qr.svg", qrCode.ToSvgString(4), Encoding.UTF8);
        }
    }
}
```

That writes an SVG file with a border of 4 modules.


## API Documentation

See [API Documentation](https://codecrete.net/QrCodeGenerator/api/index.html)


## Creating a QR code

The static factory methods cover the common cases:

```csharp
QrCode.EncodeText("https://www.example.com", QrCode.Ecc.Medium);
QrCode.EncodeBinary(bytes, QrCode.Ecc.High);
QrCode.EncodeSegments(segments, QrCode.Ecc.Low);
```

The library picks the smallest version (size) the data fits into and then raises the error
correction level as far as that version allows, so the better error correction costs nothing.

`EncodeTextAdvanced()` takes the remaining options as named parameters:

```csharp
var qrCode = QrCode.EncodeTextAdvanced(
    "昨夜のコンサートは最高でした。",
    QrCode.Ecc.Quartile,
    minVersion: 5,
    maxVersion: 20,
    eci: ECI.ShiftJIS,
    kanjiStrategy: KanjiStrategy.Automatic);
```

`minVersion` and `maxVersion` restrict the size, e.g. to print a whole series of QR codes at
the same size. Pass `boostEcl: false` to get exactly the requested error correction level
instead of the boosted one. If the data does not fit into `maxVersion`, the method throws a
`DataTooLongException`.

Numeric and alphanumeric text is encoded compactly without any configuration. Digits take
3.33 bits each, and the alphanumeric character set (digits, uppercase letters, space and
`$%*+-./:`) takes 5.5 bits each, compared to 8 bits for arbitrary text:

```csharp
QrCode.EncodeText("27182818284590452353602874713526624977", QrCode.Ecc.Medium);
QrCode.EncodeText("THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG", QrCode.Ecc.High);
```

The library also mixes the modes within a single QR code and computes the split that yields the
fewest bits. `Invoice 4711 / 2024-06-01` becomes a 7-byte binary segment followed by an 18-byte
alphanumeric one, because switching modes after the lowercase part costs less than the bits it saves.


## Output formats

```csharp
byte[] png  = qrCode.ToPngBitmap(4, 10);                        // black on white
byte[] png2 = qrCode.ToPngBitmap(4, 10, 0x00335c, 0xf5f5f5);    // 0xRRGGBB colors
byte[] bmp  = qrCode.ToBmpBitmap(4, 10);

string svg  = qrCode.ToSvgString(4);                            // small SVG file
string svg2 = qrCode.ToSvgString(4, "#00335c", "white");        // any CSS color
string svg3 = qrCode.ToSvgString(4, "#00335c", null);           // transparent background

string path = qrCode.ToGraphicsPath(4);                         // for SVG or XAML
```

`ToPngBitmap()` and `ToBmpBitmap()` take the border width in modules and the size of a module in
pixels. The SVG methods take the border width only, as SVG scales without loss. All borders
default to 0, but the standard asks for a quiet zone of 4 modules around the symbol, so 4 is the
value to use unless the surrounding layout already provides the margin.

The built-in PNG and BMP encoders are limited, e.g. with regard to the image size. For more
control over raster images, see [Raster Images / Bitmaps](#raster-images--bitmaps).

`ToSvgString()` and `ToGraphicsPath()` generate an optimized short graphics path so
the resulting graphics file is small.

For a graphics library that is not supported directly, ask for the dark modules as rectangles.
Adjacent modules are merged, so there are far fewer rectangles than modules:

```csharp
foreach (var (x, y, width, height) in qrCode.ToRectangles())
{
    graphics.FillRectangle(brush, x, y, width, height);
}
```

Anti-aliased rendering can show hairline seams between adjacent rectangles. To avoid them, fill
a single path built from the outline polygons instead. Polygons around groups of dark modules run
clockwise and polygons around holes counterclockwise, so the path fills correctly under both the
nonzero and the even-odd fill rule:

```csharp
foreach (var polygon in qrCode.ToOutlines())
{
    foreach (var (x, y) in polygon.Vertices)
    {
        ... add vertex to the path
    }
}
```

Or read the modules one by one. Coordinates outside the QR code are light, so the border needs
no special case:

```csharp
for (int y = -4; y < qrCode.Size + 4; y++)
{
    for (int x = -4; x < qrCode.Size + 4; x++)
    {
        bool dark = qrCode.GetModule(x, y);
    }
}
```

Rectangles, polygons and modules all use the same coordinate system: the top left module is at
(0, 0), one unit is one module, and no border is included.


## Long text: Structured Append

Text too long for a single QR code can be split across up to 16 linked QR codes. A scanner
reads them in any order and reassembles the text.

```csharp
var qrCodes = QrCode.EncodeTextInMultipleBalancedCodes(longText, QrCode.Ecc.Medium,
    minVersion: 10, maxVersion: 29);
```

It first determines the least number of codes required, then reduces the shared
version as far as it can, and finally spreads the payload evenly.
All codes use the same version.

It returns a single standalone QR code, without the Structured Append overhead, if the
text fits into one. If it does not fit into 16 codes, it throws a `DataTooLongException`.

Text encoding works the same as for a single QR code: Latin-1 if the text can be represented in
it, UTF-8 with an ECI designator in each code otherwise. UTF-8 text is split at character
boundaries and never in the middle of a multi-byte character. The standard does not require this,
but it keeps scanners happy.


## Character encoding

The QR code standard defines Latin-1 (ISO-8859-1) as the default text encoding, and an ECI
designator to announce anything else. By default, the library encodes the text in Latin-1 and
adds no ECI designator. If the text contains a character outside Latin-1, it switches to UTF-8
and adds the ECI designator for it. That is what scanners expect and needs no configuration:

```csharp
QrCode.EncodeText("Grüezi mitenand", QrCode.Ecc.Medium);   // Latin-1, no ECI
QrCode.EncodeText("🎲 😇 🤒 🏌 ⏭ 🚍", QrCode.Ecc.Medium);      // UTF-8 with ECI
```

To override it, pass an `eci` designator:

```csharp
// Force UTF-8 and announce it, even if the text would fit into Latin-1
QrCode.EncodeTextAdvanced(text, QrCode.Ecc.Medium, eci: ECI.UTF8);

// Greek text in ISO-8859-7 (see the note on encoding providers below)
QrCode.EncodeTextAdvanced(text, QrCode.Ecc.Medium, eci: ECI.LatinGreek);
```

`ECI` has a member for each designator of the standard, both under its ISO name and under its
Latin number where it has one, e.g. `ECI.Iso8859_15` and `ECI.Latin9` are the same designator.

`ECI.None` suppresses the designator. The encoding is then no longer implied by the designator,
so it becomes a required parameter. Use this for readers that ignore or mishandle ECI
designators. Most of them guess UTF-8 correctly:

```csharp
QrCode.EncodeTextAdvanced(text, QrCode.Ecc.Medium, eci: ECI.None, encoding: Encoding.UTF8);
```

The `encoding` parameter also overrides the encoding that goes with a designator, for the rare
case that they must differ.

Latin-1, US-ASCII and the UTF encodings are available on every .NET platform. Shift-JIS, the
other ISO-8859 parts and the Windows code pages are not, and must be registered first on .NET Core
and .NET 5 or higher:

```csharp
// requires the NuGet package System.Text.Encoding.CodePages
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

Without it, the library throws an `ECIException` naming the ECI value it could not resolve.
On .NET Framework 4.x, all encodings are available without registration.

When the library derives the encoding from the ECI designator, a character the encoding cannot
represent is an error rather than a silent replacement by a question mark. The encoder throws an
`EncoderFallbackException`. An `Encoding` instance you pass yourself keeps whatever fallback
behavior it was configured with.

### Kanji mode

Kanji mode packs two Shift-JIS characters into 13 bits instead of 16, saving about 19%. The
library applies it automatically to text encoded in Shift-JIS:

```csharp
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var qrCode = QrCode.EncodeTextAdvanced("昨夜のコンサートは最高でした。",
    QrCode.Ecc.Medium, eci: ECI.ShiftJIS);
```

Those 15 characters fit into a version 2 QR code with Kanji mode and need a version 3 one without.
Encoded in UTF-8 instead, they need version 4, so the Shift-JIS designator is worth setting for
Japanese text even before Kanji mode enters the picture.

Kanji mode works on byte pairs and does not actually check that they are Japanese text, so it can
compress arbitrary data that happens to contain suitable byte values. `KanjiStrategy.Enabled`
uses it whenever it saves bits, `KanjiStrategy.Disabled` never does. Be careful with `Enabled`:
many scanners assume that Kanji mode implies Shift-JIS and will incorrectly decode the data as Japanese text.

### Binary data

`EncodeBinary()` prefixes the data with ECI designator 899, which marks it as binary rather than
text. Pass `omitEci: true` to leave it out:

```csharp
byte[] data = File.ReadAllBytes("payload.bin");
var qrCode = QrCode.EncodeBinary(data, QrCode.Ecc.Medium);
```

Scanners differ widely in how they hand binary data to the application, so this usually works in
closed systems only.


## Manual data segments

`DataSegment` builds the segments explicitly, for payloads the automatic mode selection cannot
express, such as a segment structure prescribed by an industry specification:

```csharp
var segments = new List<DataSegment>
{
    DataSegment.MakeECISegment(ECI.Latin2),
    DataSegment.MakeSegment(DataSegmentMode.Alphanumeric,
        new ArraySegment<byte>(Encoding.ASCII.GetBytes("AC-42"))),
    DataSegment.MakeSegment(DataSegmentMode.Binary,
        new ArraySegment<byte>(payload))
};
var qrCode = QrCode.EncodeSegments(segments, QrCode.Ecc.Medium);
```

A segment refers to the byte array it was created from instead of copying it. Do not modify the
array until the QR code has been created.

`DataSegment.FromText()` and `DataSegment.FromBinaryData()` return the optimal segments for a
text or a byte array, for inspection or as a starting point for a longer segment list.


## Diagnostics

`EncodingInfo` reports the segments the library chose and the penalty score of each of the eight
mask patterns:

```csharp
var info = new EncodingInfo();
var qrCode = QrCode.EncodeTextAdvanced(text, QrCode.Ecc.Medium, encodingInfo: info);

foreach (var segment in info.DataSegments)
{
    Console.WriteLine($"{segment.Mode}: {segment.DataBytes.Count} bytes");
}
for (int mask = 0; mask < 8; mask++)
{
    Console.WriteLine($"mask {mask}: {info.Penalties[mask].Total}");
}
```

`EncodingInfo` collides with `System.Text.EncodingInfo`. In a file that has both `using`
directives, qualify it or add an alias such as
`using EncodingInfo = Net.Codecrete.QrCodeGenerator.EncodingInfo;`.

`PenaltyScore` breaks the total down into its six components: horizontal and vertical streaks of
same-colored modules, 2x2 blocks, horizontal and vertical finder-like patterns, and the balance
between dark and light modules. Collecting the diagnostics is slower than a plain call, because
the library has to score every mask in full instead of abandoning a mask as soon as it is beaten.

Setting `info.ForcedDataMask` to a value between 0 and 7 before the call pins that mask instead of
letting the library choose. This is meant for analysis. A hand-picked mask can produce a QR code
that is hard to scan.

[QrCodeAnalyzer](QrCodeAnalyzer), a Windows UI application in this repository, is built on these
diagnostics. It shows the chosen segments, the score of every mask and the resulting QR code, and
it lets you force a mask to see what the others would look like.


## Requirements

QR Code Generator for .NET requires a .NET implementation compatible with .NET Standard 2.0 or higher, i.e. any of:

- .NET Core 2.0 or higher
- .NET Framework 4.6.1 or higher
- Mono 5.4 or higher
- Universal Windows Platform 10.0.16299 or higher
- Xamarin

## Raster Images / Bitmaps

Starting with .NET 6, *System.Drawing* is only supported on the Windows operating system and thus cannot be used for multi-platform libraries like this one.

Two raster bitmap formats are supported without the need for additional libraries:

- *PNG*: See `QrCode.ToPngBitmap()`
- *BMP*: See `QrCode.ToBmpBitmap()`

These methods are limited, e.g. with regard to the size of the generated image.
For more advanced and more efficient ways to generate different raster image formats:

- Select one of the imaging libraries below
- Add the NuGet dependencies to your project
- Copy the appropriate `QrCodeBitmapExtensions.cs` file to your project

| Imaging library | Recommendation | NuGet dependencies | Extension file |
| ------- | -------------- | ------------------ | -------------- |
| **System.Drawing** | For Windows only projects | `System.Drawing.Common` | [QrCodeBitmapExtensions.cs](Demo-System-Drawing/QrCodeBitmapExtensions.cs) |
| **SkiaSharp** | For macOS, Linux, iOS, Android and multi-platform projects | `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` (for Linux only) | [QrCodeBitmapExtensions.cs](Demo-SkiaSharp/QrCodeBitmapExtensions.cs) |
| **ImageSharp** | Alternative for multi-platform projects. Might require a commercial license. | `SixLabors.ImageSharp.Drawing` | [QrCodeBitmapExtensions.cs](Demo-ImageSharp/QrCodeBitmapExtensions.cs) |

Using these extension methods, generating PNG images is straightforward:

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

## Performance

QrCodeGenerator is optimized for speed and compares favorably with other libraries.
See [Profiling](QrCodeGeneratorProfiling/README.md) for the benchmarks and the details.


## Demo Projects

Several example projects demonstrate how to generate QR code with different frameworks and libraries:

- [Basic-Example](Basic-Example): Demonstrates the basic use of the libraries (different texts, error correction level). All QR codes are saved as either SVG, PNG or BMP files.

- [Demo-WinUI](Demo-WinUI): Demonstrates how QR codes can be used in WinUI 3 applications and/or using [Win2D](https://github.com/microsoft/Win2D) (incl. copying to the clipboard).

- [Demo-WindowsPresentationFoundation](Demo-WindowsPresentationFoundation): Demonstrates how QR codes can be used in WPF applications (incl. copying to the clipboard).

- [Demo-WinForms](Demo-WinForms): Demonstrates how QR codes can be used in Windows Forms applications (incl. copying to the clipboard).

- [Demo-ASP.NET-Core](Demo-ASP.NET-Core): Demonstrates how to create QR codes in a web application implemented using ASP.NET Core.

- [Demo-VCard](Demo-VCard): Demonstrates how contact data (similar to business cards) can be saved in a QR Code using the VCard standard.

- [Demo-System-Drawing](Demo-System-Drawing): Demonstrates how a QR code can be saved as a PNG file, using the *System.Drawing* classes, which have become a Windows-only technology starting with .NET 6.

- [Demo-SkiaSharp](Demo-SkiaSharp): Demonstrates how a QR code can be saved as a PNG file, using the SkiaSharp multi-platform raster image library.

- [Demo-ImageSharp](Demo-ImageSharp): Demonstrates how a QR code can be saved as a PNG file, using the ImageSharp raster image library. Additionally, a QR code with an image in the center is created.

- [Demo-ImageMagick](Demo-ImageMagick): Demonstrates how a QR code can be saved as a PNG file, using the Magick.NET image manipulation library (based on ImageMagick).


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
as it can lead to QR codes that are very difficult to scan and violate the standard.
For backward compatibility, the parameter is still present in one of the methods.
But it is ignored.


## License

MIT License. See [LICENSE](LICENSE).
