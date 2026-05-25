//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System.Text;
using System.Windows;

namespace Net.Codecrete.QrCodeGenerator.Analyzer
{
    /// <summary>
    /// Interaction logic for AnalyzerApp.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Required so non-default code pages (CP437, Windows-125x, Shift-JIS, Big5, ...) resolve.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            base.OnStartup(e);
        }
    }
}
