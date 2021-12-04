//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System;
using System.Windows.Forms;

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            errorCorrectionCombo.SelectedIndex = 1;

            qrCodeControl.TextData = qrCodeText.Text;
            qrCodeControl.ErrorCorrection = errorCorrectionCombo.SelectedIndex;
            qrCodeControl.BorderWidth = (int)borderNumericUpDown.Value;
        }

        private void QrCodeText_TextChanged(object sender, EventArgs e)
        {
            qrCodeControl.TextData = qrCodeText.Text;
        }

        private void ErrorCorrectionCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            qrCodeControl.ErrorCorrection = errorCorrectionCombo.SelectedIndex;
        }

        private void BorderNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            qrCodeControl.BorderWidth = (int)borderNumericUpDown.Value;
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            qrCodeControl.CopyToClipboard();
        }
    }
}
