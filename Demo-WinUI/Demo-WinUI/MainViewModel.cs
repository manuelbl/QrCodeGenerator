//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2022 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace Net.Codecrete.QrCodeGenerator.Demo;

/// <summary>
/// View model for MainWindow
/// </summary>
public partial class MainViewModel : ObservableObject
{
    /// <summary>
    /// QR code text
    /// </summary>
    [ObservableProperty]
    string text = "Hello, world!";

    /// <summary>
    /// QR code error correction level
    /// </summary>
    [ObservableProperty]
    QrCode.Ecc errorCorrection = QrCode.Ecc.Medium;

    /// <summary>
    /// Width of border around QR code (in QR code pixels)
    /// </summary>
    [ObservableProperty]
    int borderWidth = 3;

    private readonly Tuple<string, QrCode.Ecc>[] errorCorrectionLevels_ =
    {
        new Tuple<string, QrCode.Ecc>("Low", QrCode.Ecc.Low),
        new Tuple<string, QrCode.Ecc>("Medium", QrCode.Ecc.Medium),
        new Tuple<string, QrCode.Ecc>("Quartile", QrCode.Ecc.Quartile),
        new Tuple<string, QrCode.Ecc>("High", QrCode.Ecc.High)
    };

    /// <summary>
    /// List of error correction levels
    /// </summary>
    public Tuple<string, QrCode.Ecc>[] ErrorCorrectionLevels => errorCorrectionLevels_;

    /// <summary>
    /// Copy the QR code to the clipboard (as a PNG image).
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    async Task CopyToClipboard()
    {
        var qrCode = QrCode.EncodeText(Text, ErrorCorrection);

        // Don't close stream; it won't work anymore
        var stream = new InMemoryRandomAccessStream();
        await QrCodeDrawing.WriteAsPng(stream, qrCode, 20, BorderWidth);

        var dataPackage = new DataPackage();
        dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
        Clipboard.SetContent(dataPackage);
    }
}
