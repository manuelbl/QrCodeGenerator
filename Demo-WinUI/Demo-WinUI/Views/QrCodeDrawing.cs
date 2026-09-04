//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2022 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;

namespace Net.Codecrete.QrCodeGenerator.Demo.Views;

public static class QrCodeDrawing
{
    /// <summary>
    /// Draws the QR code in the specified drawing context.
    /// <para>
    /// The QR code is drawn with the top left corner of the border at (0, 0).
    /// Each module (QR code pixel) will be drawn with 1 unit wide and tall.
    /// If a different position or size is desired, the drawing context's transformation
    /// matrix can be setup accordingly.
    /// </para>
    /// </summary>
    /// <param name="qrCode">The QR code.</param>
    /// <param name="drawingSession">The drawing session.</param>
    /// <param name="scale">The scaling factor</param>
    /// <param name="borderWidth">The border width (in multiples of the QR code modules)</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public static void Draw(QrCode qrCode, CanvasDrawingSession drawingSession, float scale, float borderWidth, Color foreground, Color? background)
    {
        // turn off anti-aliasing to get crisp edges and avoid gray lines between modules
        drawingSession.Antialiasing = CanvasAntialiasing.Aliased;

        drawingSession.Transform = Matrix3x2.CreateTranslation(borderWidth, borderWidth) * Matrix3x2.CreateScale(scale);

        int size = qrCode.Size;

        // draw the background
        if (background != null)
        {
            drawingSession.FillRectangle(
                new Rect(-borderWidth, -borderWidth, size + 2 * borderWidth, size + 2 * borderWidth),
                (Color)background
            );
        }

        // create path from QR code outline
        var pathBuilder = new CanvasPathBuilder(drawingSession);
        foreach (var polygon in qrCode.ToOutlines())
        {
            var vertices = polygon.Vertices;
            pathBuilder.BeginFigure(vertices[0].X, vertices[0].Y);
            for (var i = 1; i < vertices.Count; i++) {
                pathBuilder.AddLine(vertices[i].X, vertices[i].Y);
            }
            pathBuilder.EndFigure(CanvasFigureLoop.Closed);
        }

        // draw path
        var geometry = CanvasGeometry.CreatePath(pathBuilder);
        drawingSession.FillGeometry(geometry, foreground);
    }

    /// <summary>
    /// Writes a PNG for the specified QR code.
    /// <para>
    /// To achieve a crisp bitmap without any anti-aliasing, the bitmap is sized such
    /// that each QR code module is multiple pixels tall and wide.
    /// The resulting size depends on the QR code size;
    /// it is (qr-code-size + 2 * border-width) pixels tall and wide.
    /// </para>
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <param name="qrCode">The QR code.</param>
    /// <param name="moduleSize">The size of each module (QR code pixel), in pixels</param>
    /// <param name="borderWidth">The width of the border around the QR code, in multiples of a single module (QR code pixel).</param>
    public static Task WriteAsPng(IRandomAccessStream stream, QrCode qrCode, int moduleSize, int borderWidth = 3)
    {
        return WriteAsPng(stream, qrCode, moduleSize, borderWidth, Colors.Black, Colors.White);
    }

    /// <inheritdoc cref="CreateBitmapAsPng(QrCode, int, int)"/>
    /// <param name="foreground">The forground color.</param>
    /// <param name="background">The background color.</param>
    /// <returns>PNG image</returns>
    public static async Task WriteAsPng(IRandomAccessStream stream, QrCode qrCode, int moduleSize, int borderWidth, Color foreground, Color background)
    {
        // create offscreen bitmap
        int size = moduleSize * (qrCode.Size + 2 * borderWidth);
        var device = CanvasDevice.GetSharedDevice();
        var offscreen = new CanvasRenderTarget(device, size, size, 96);

        using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
        {
            // draw QR code
            ds.Clear(background);
            Draw(qrCode, ds, moduleSize, borderWidth, foreground, null);
        }

        // create PNG image
        await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);
    }

}
