# Sample code for ASP.NET Core

This example program shows how to create a QR codes in an ASP.NET core application.

The [`QrCodeController`](QrCodeController.cs) class receives the QR code text, border width and error correction level as query parameters and generates the QR code, either as an SVG or PNG.

For  PNG generation, the [SkiaSharp](https://github.com/mono/SkiaSharp) rasterization library is used.
