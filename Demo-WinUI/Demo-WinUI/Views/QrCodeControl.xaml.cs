using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Net.Codecrete.QrCodeGenerator.Demo.Views;

public sealed partial class QrCodeControl : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(QrCodeControl),
        new PropertyMetadata(string.Empty, OnRenderPropertyChanged));

    public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register(
        nameof(BorderWidth),
        typeof(int),
        typeof(QrCodeControl),
        new PropertyMetadata(4, OnRenderPropertyChanged));

    public static readonly DependencyProperty ErrorCorrectionLevelProperty = DependencyProperty.Register(
        nameof(ErrorCorrectionLevel),
        typeof(QrCode.Ecc),
        typeof(QrCodeControl),
        new PropertyMetadata(QrCode.Ecc.Medium, OnRenderPropertyChanged));

    public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register(
        nameof(ForegroundColor),
        typeof(Color),
        typeof(QrCodeControl),
        new PropertyMetadata(Colors.Black, OnRenderPropertyChanged));

    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
        nameof(BackgroundColor),
        typeof(Color),
        typeof(QrCodeControl),
        new PropertyMetadata(Colors.White, OnRenderPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public int BorderWidth
    {
        get => (int)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    public QrCode.Ecc ErrorCorrectionLevel
    {
        get => (QrCode.Ecc)GetValue(ErrorCorrectionLevelProperty);
        set => SetValue(ErrorCorrectionLevelProperty, value);
    }

    public Color ForegroundColor
    {
        get => (Color)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    private static void OnRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((QrCodeControl)d).qrCodeCanvas.Invalidate();
    }

    public QrCodeControl()
    {
        InitializeComponent();
    }

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
        if (string.IsNullOrEmpty(Text))
            return;

        var code = QrCode.EncodeText(Text, ErrorCorrectionLevel);
        var scale = (float)(sender.ActualWidth / (code.Size + 2 * BorderWidth));
        args.DrawingSession.Transform = Matrix3x2.CreateScale(scale, scale);
        QrCodeDrawing.Draw(code, args.DrawingSession, BorderWidth, ForegroundColor, BackgroundColor);
    }
}
