//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System;
using System.Windows;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _text = "QR code text";
        private int _borderWidth = 3;
        private QrCode.Ecc _errorCorrection = QrCode.Ecc.Medium;
        private readonly Tuple<string, QrCode.Ecc>[] _errorCorrectionLevels =
        {
            new Tuple<string, QrCode.Ecc>("Low", QrCode.Ecc.Low),
            new Tuple<string, QrCode.Ecc>("Medium", QrCode.Ecc.Medium),
            new Tuple<string, QrCode.Ecc>("Quartile", QrCode.Ecc.Quartile),
            new Tuple<string, QrCode.Ecc>("High", QrCode.Ecc.High)
        };

        public MainWindow()
        {
            InitializeComponent();
            UpdateQrCode();
        }

        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                UpdateQrCode();
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                UpdateQrCode();
            }
        }

        public Tuple<string, QrCode.Ecc>[] ErrorCorrectionLevels
        {
            get { return _errorCorrectionLevels; }
        }

        public QrCode.Ecc ErrorCorrection
        {
            get { return _errorCorrection; }
            set
            {
                _errorCorrection = value;
                UpdateQrCode();
            }
        }

        private void UpdateQrCode()
        {
            var qrCode = QrCode.EncodeText(_text, ErrorCorrection);
            QrCodeImage.Source = QrCodeDrawing.CreateDrawing(qrCode, 192, BorderWidth);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            // put the QR code on the clipboard as a bitmap
            var qrCode = QrCode.EncodeText(_text, ErrorCorrection);
            var bitmap = QrCodeDrawing.CreateBitmapImage(qrCode, 20, BorderWidth);
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Bitmap, bitmap);
            Clipboard.SetDataObject(dataObject);
        }
    }
}
