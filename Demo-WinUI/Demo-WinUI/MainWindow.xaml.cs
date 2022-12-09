//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2022 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.UI.Xaml;

namespace Net.Codecrete.QrCodeGenerator.Demo;

/// <summary>
/// Main window
/// </summary>
public sealed partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
        RootElement.Loaded += RootElement_Loaded;
    }

    public MainViewModel ViewModel { get; } = new MainViewModel();

    private void RootElement_Loaded(object sender, RoutedEventArgs e)
    {
        // The combo box needs help to set the initial value
        ErrorCorrectionCombo.SelectedValue = ViewModel.ErrorCorrection;
    }
}
