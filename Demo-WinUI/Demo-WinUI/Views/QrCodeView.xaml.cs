//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2026 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.UI.Xaml.Controls;
using Net.Codecrete.QrCodeGenerator.Demo.Models;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace Net.Codecrete.QrCodeGenerator.Demo.Views;

/// <summary>
/// Main view displaying QR code and related controls.
/// </summary>
public sealed partial class QrCodeView : Page
{
    private readonly QrCodeSettings settings = new();

    public QrCodeView()
    {
        InitializeComponent();

        ErrorCorrectionCombo.Loaded += ErrorCorrectionCombo_Loaded;
    }

    private void ErrorCorrectionCombo_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Workaround for limitations in ComboBox's two way binding when using complex types as items source.
        ErrorCorrectionCombo.Loaded -= ErrorCorrectionCombo_Loaded;
        ErrorCorrectionCombo.SelectedIndex = Array.FindIndex(ErrorCorrectionLevels, e => e.Item2 == settings.EccLevel);
    }

    private readonly Tuple<string, QrCode.Ecc>[] errorCorrectionLevels_ =
    [
        new Tuple<string, QrCode.Ecc>("Low", QrCode.Ecc.Low),
        new Tuple<string, QrCode.Ecc>("Medium", QrCode.Ecc.Medium),
        new Tuple<string, QrCode.Ecc>("Quartile", QrCode.Ecc.Quartile),
        new Tuple<string, QrCode.Ecc>("High", QrCode.Ecc.High)
    ];

    public Tuple<string, QrCode.Ecc>[] ErrorCorrectionLevels => errorCorrectionLevels_;

    private async void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await CopyQrCode();
    }

    /// <summary>
    /// Copy the QR code to the clipboard (as a PNG image).
    /// </summary>
    /// <returns></returns>
    private async Task CopyQrCode()
    {
        var qrCode = QrCode.EncodeText(settings.Text, settings.EccLevel);

        // Don't close stream; it won't work anymore
        var stream = new InMemoryRandomAccessStream();
        await QrCodeDrawing.WriteAsPng(stream, qrCode, 20, settings.BorderWidth);

        var dataPackage = new DataPackage();
        dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
        Clipboard.SetContent(dataPackage);
    }
}
