//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2022 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Net.Codecrete.QrCodeGenerator.Demo;

/// <summary>
/// Control for displaying a QR code.
/// </summary>
public sealed partial class QrCodeControl : UserControl
{
    public QrCodeControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Text contained in QR code.
    /// </summary>
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(QrCodeControl),
        new PropertyMetadata("", (d, args) => { (d as QrCodeControl).qrCodeCanvas.Invalidate(); })
    );

    /// <summary>
    /// Width of border around QR code (in QR pixels)
    /// </summary>
    public int BorderWidth
    {
        get { return (int)GetValue(BorderWidthProperty); }
        set { SetValue(BorderWidthProperty, value); }
    }

    public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register(
        nameof(BorderWidth),
        typeof(int),
        typeof(QrCodeControl),
        new PropertyMetadata(3, (d, args) => { (d as QrCodeControl).qrCodeCanvas.Invalidate(); })
    );

    /// <summary>
    /// Error correction level
    /// </summary>
    public QrCode.Ecc ErrorCorrection
    {
        get { return (QrCode.Ecc)GetValue(ErrorCorrectionProperty); }
        set { SetValue(ErrorCorrectionProperty, value); }
    }

    public static readonly DependencyProperty ErrorCorrectionProperty = DependencyProperty.Register(
        nameof(ErrorCorrection),
        typeof(QrCode.Ecc),
        typeof(QrCodeControl),
        new PropertyMetadata(QrCode.Ecc.Medium, (d, args) => { (d as QrCodeControl).qrCodeCanvas.Invalidate(); })
    );

    /// <summary>
    /// QR code background color
    /// </summary>
    public Color QrCodeBackgroundColor
    {
        get { return (Color)GetValue(QrCodeBackgroundColorProperty); }
        set { SetValue(QrCodeBackgroundColorProperty, value); }
    }

    public static readonly DependencyProperty QrCodeBackgroundColorProperty = DependencyProperty.Register(
        nameof(QrCodeBackgroundColor),
        typeof(Color),
        typeof(QrCodeControl),
        new PropertyMetadata(Colors.White, (d, args) => { (d as QrCodeControl).qrCodeCanvas.Invalidate(); })
    );

    /// <summary>
    /// QR code pixel color
    /// </summary>
    public Color QrCodePixelColor
    {
        get { return (Color)GetValue(QrCodePixelColorProperty); }
        set { SetValue(QrCodePixelColorProperty, value); }
    }

    public static readonly DependencyProperty QrCodePixelColorProperty = DependencyProperty.Register(
        nameof(QrCodePixelColor),
        typeof(Color),
        typeof(QrCodeControl),
        new PropertyMetadata(Colors.Black, (d, args) => { (d as QrCodeControl).qrCodeCanvas.Invalidate(); })
    );

    protected override Size ArrangeOverride(Size finalSize)
    {
        var dim = Math.Min(finalSize.Width, finalSize.Height);
        qrCodeCanvas.Arrange(new Rect(new Point((finalSize.Width - dim) / 2, (finalSize.Height - dim) / 2), new Size(dim, dim)));
        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var dim = Math.Min(availableSize.Width, availableSize.Height);
        if (double.IsPositiveInfinity(dim))
            dim = 3000;
        return new Size(dim, dim);
    }

    private void QrCode_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var code = QrCode.EncodeText(Text, ErrorCorrection);
        var scale = (float)(sender.ActualWidth / (code.Size + 2 * BorderWidth));
        args.DrawingSession.Transform = Matrix3x2.CreateScale(scale, scale);
        QrCodeDrawing.Draw(code, args.DrawingSession, BorderWidth, QrCodePixelColor, QrCodeBackgroundColor);
    }
}
