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
using System.Windows.Input;

namespace Net.Codecrete.QrCodeGenerator.Analyzer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel = new MainWindowViewModel();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var bitmap = _viewModel.CreateClipboardBitmap();
        if (bitmap == null) return;
        var dataObject = new DataObject();
        dataObject.SetData(DataFormats.Bitmap, bitmap);
        Clipboard.SetDataObject(dataObject);
    }

    private void HighlightLabel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe
            && fe.Tag is string tag
            && Enum.TryParse(tag, out HighlightKind kind))
        {
            fe.CaptureMouse();
            _viewModel.Highlight = kind;
        }
    }

    private void HighlightLabel_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            fe.ReleaseMouseCapture();
        }
        _viewModel.Highlight = HighlightKind.None;
    }
}
