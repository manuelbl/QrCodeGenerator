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
            this.qrCodeControl = new Net.Codecrete.QrCodeGenerator.QrCodeControl();
            this.qrCodeText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.errorCorrectionCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.copyButton = new System.Windows.Forms.Button();
            this.borderNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.borderNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // qrCodeControl
            // 
            this.qrCodeControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.qrCodeControl.BinaryData = null;
            this.qrCodeControl.BorderWidth = 3;
            this.qrCodeControl.ErrorCorrection = 2;
            this.qrCodeControl.Location = new System.Drawing.Point(32, 32);
            this.qrCodeControl.Name = "qrCodeControl";
            this.qrCodeControl.Size = new System.Drawing.Size(712, 344);
            this.qrCodeControl.TabIndex = 0;
            this.qrCodeControl.TextData = "Test";
            // 
            // qrCodeText
            // 
            this.qrCodeText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.qrCodeText.Location = new System.Drawing.Point(32, 420);
            this.qrCodeText.Name = "qrCodeText";
            this.qrCodeText.Size = new System.Drawing.Size(712, 39);
            this.qrCodeText.TabIndex = 1;
            this.qrCodeText.Text = "QR code text";
            this.qrCodeText.TextChanged += new System.EventHandler(this.QrCodeText_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 488);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "Error Correction:";
            // 
            // errorCorrectionCombo
            // 
            this.errorCorrectionCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.errorCorrectionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorCorrectionCombo.FormattingEnabled = true;
            this.errorCorrectionCombo.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "Quartile",
            "High"});
            this.errorCorrectionCombo.Location = new System.Drawing.Point(225, 485);
            this.errorCorrectionCombo.Name = "errorCorrectionCombo";
            this.errorCorrectionCombo.Size = new System.Drawing.Size(186, 40);
            this.errorCorrectionCombo.TabIndex = 3;
            this.errorCorrectionCombo.SelectedIndexChanged += new System.EventHandler(this.ErrorCorrectionCombo_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(496, 488);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 32);
            this.label2.TabIndex = 4;
            this.label2.Text = "Border:";
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.Location = new System.Drawing.Point(498, 568);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(246, 46);
            this.copyButton.TabIndex = 6;
            this.copyButton.Text = "Copy QR Code";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // borderNumericUpDown
            // 
            this.borderNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.borderNumericUpDown.Location = new System.Drawing.Point(592, 486);
            this.borderNumericUpDown.Name = "borderNumericUpDown";
            this.borderNumericUpDown.Size = new System.Drawing.Size(152, 39);
            this.borderNumericUpDown.TabIndex = 7;
            this.borderNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.borderNumericUpDown.ValueChanged += new System.EventHandler(this.BorderNumericUpDown_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 649);
            this.Controls.Add(this.borderNumericUpDown);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.errorCorrectionCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.qrCodeText);
            this.Controls.Add(this.qrCodeControl);
            this.Name = "Form1";
            this.Text = "QR Code";
            ((System.ComponentModel.ISupportInitialize)(this.borderNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
