//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Custom control for displaying a QR code
    /// </summary>
    public class QrCodeControl : Control
    {

        private string _textData;
        private byte[] _binaryData;
        private int _errorCorrection;
        private int _borderWidth;

        public QrCodeControl()
        {
            _textData = "Test";
            _errorCorrection = 2;
            _borderWidth = 3;
            ResizeRedraw = true;
        }

        public string TextData
        {
            get
            {
                if (_binaryData != null)
                {
                    return "binary data";
                }
                return _textData;
            }

            set
            {
                _textData = value;
                _binaryData = null;
                if (_textData == null)
                {
                    _textData = "";
                }
                Invalidate();
            }
        }

        public byte[] BinaryData
        {
            get { return _binaryData; }
            set
            {
                _binaryData = value;
                _textData = null;
                if (_binaryData == null)
                {
                    _binaryData = new byte[0];
                }
                Invalidate();
            }
        }

        public int ErrorCorrection
        {
            get { return _errorCorrection; }
            set
            {
                _errorCorrection = Math.Min(Math.Max(value, 0), 3);
                Invalidate();
            }
        }

        public int BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = Math.Max(value, 0);
                Invalidate();
            }
        }


        private static readonly QrCode.Ecc[] errorCorrectionLevels = { QrCode.Ecc.Low, QrCode.Ecc.Medium, QrCode.Ecc.Quartile, QrCode.Ecc.High };

        /// <summary>
        /// Creates the <c>QrCode</c> instance with the current settings.
        /// </summary>
        /// <returns></returns>
        private QrCode CreateQrCode()
        {
            QrCode qrCode;
            var ecc = errorCorrectionLevels[_errorCorrection];

            if (_binaryData != null)
            {
                qrCode = QrCode.EncodeBinary(_binaryData, ecc);
            }
            else
            {
                qrCode = QrCode.EncodeText(_textData, ecc);
            }

            return qrCode;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int size = Math.Min(Width, Height);
            var graphics = e.Graphics;

            var qrCode = CreateQrCode();
            graphics.TranslateTransform((Width - size) / 2, (Height - size) / 2);
            qrCode.Draw(graphics, scale: size / (float)(qrCode.Size + 2 * _borderWidth), border: _borderWidth,
                foreground: Color.Black, background: Color.White);
        }

        /// <summary>
        /// Copy the QR code to the clipboard.
        /// <para>
        /// The QR code is copied as a bitmap. It uses a scaling factor of 20 to
        /// prevent a blurry result from upscaling.
        /// </para>
        /// </summary>
        public void CopyToClipboard()
        {
            DataObject dataObject = new DataObject();
            var qrCode = CreateQrCode();
            dataObject.SetData(DataFormats.Bitmap, qrCode.ToBitmap(20, _borderWidth));
            Clipboard.SetDataObject(dataObject);
        }
    }
}