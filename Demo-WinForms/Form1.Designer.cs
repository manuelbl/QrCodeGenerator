//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

namespace Net.Codecrete.QrCodeGenerator.Demo
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            qrCodeControl = new QrCodeControl();
            qrCodeText = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            errorCorrectionCombo = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            copyButton = new System.Windows.Forms.Button();
            borderNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)borderNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // qrCodeControl
            // 
            qrCodeControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            qrCodeControl.BinaryData = null;
            qrCodeControl.BorderWidth = 4;
            qrCodeControl.ErrorCorrection = 2;
            qrCodeControl.Location = new System.Drawing.Point(32, 32);
            qrCodeControl.Name = "qrCodeControl";
            qrCodeControl.Size = new System.Drawing.Size(712, 344);
            qrCodeControl.TabIndex = 0;
            qrCodeControl.TextData = "Test";
            // 
            // qrCodeText
            // 
            qrCodeText.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            qrCodeText.Location = new System.Drawing.Point(32, 420);
            qrCodeText.Name = "qrCodeText";
            qrCodeText.Size = new System.Drawing.Size(712, 39);
            qrCodeText.TabIndex = 1;
            qrCodeText.Text = "QR code text";
            qrCodeText.TextChanged += QrCodeText_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(32, 488);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(187, 32);
            label1.TabIndex = 2;
            label1.Text = "Error Correction:";
            // 
            // errorCorrectionCombo
            // 
            errorCorrectionCombo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            errorCorrectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            errorCorrectionCombo.FormattingEnabled = true;
            errorCorrectionCombo.Items.AddRange(new object[] { "Low", "Medium", "Quartile", "High" });
            errorCorrectionCombo.Location = new System.Drawing.Point(225, 485);
            errorCorrectionCombo.Name = "errorCorrectionCombo";
            errorCorrectionCombo.Size = new System.Drawing.Size(186, 40);
            errorCorrectionCombo.TabIndex = 3;
            errorCorrectionCombo.SelectedIndexChanged += ErrorCorrectionCombo_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(496, 488);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(90, 32);
            label2.TabIndex = 4;
            label2.Text = "Border:";
            // 
            // copyButton
            // 
            copyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            copyButton.Location = new System.Drawing.Point(498, 568);
            copyButton.Name = "copyButton";
            copyButton.Size = new System.Drawing.Size(246, 46);
            copyButton.TabIndex = 6;
            copyButton.Text = "Copy QR Code";
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += CopyButton_Click;
            // 
            // borderNumericUpDown
            // 
            borderNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            borderNumericUpDown.Location = new System.Drawing.Point(592, 486);
            borderNumericUpDown.Name = "borderNumericUpDown";
            borderNumericUpDown.Size = new System.Drawing.Size(152, 39);
            borderNumericUpDown.TabIndex = 7;
            borderNumericUpDown.Value = new decimal(new int[] { 4, 0, 0, 0 });
            borderNumericUpDown.ValueChanged += BorderNumericUpDown_ValueChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(774, 649);
            Controls.Add(borderNumericUpDown);
            Controls.Add(copyButton);
            Controls.Add(label2);
            Controls.Add(errorCorrectionCombo);
            Controls.Add(label1);
            Controls.Add(qrCodeText);
            Controls.Add(qrCodeControl);
            Name = "Form1";
            Text = "QR Code";
            ((System.ComponentModel.ISupportInitialize)borderNumericUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private QrCodeControl qrCodeControl;
        private System.Windows.Forms.TextBox qrCodeText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox errorCorrectionCombo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.NumericUpDown borderNumericUpDown;
    }
}
