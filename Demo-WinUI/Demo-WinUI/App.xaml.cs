//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2022 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using WinRT.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Net.Codecrete.QrCodeGenerator.Demo;

/// <summary>
/// QR Code application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow
        {
            Title = "QR Code"
        };

        // Set initial windows size.
        Resize(500, 600);

        m_window.Activate();
    }

    private Window m_window;

    private void Resize(int x, int y)
    {
        var scalingFactor = GetDpiScalingFactor();
        GetAppWindow().Resize(new SizeInt32((int)(x * scalingFactor), (int)(y * scalingFactor)));
    }

    private AppWindow GetAppWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(m_window);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }

    private double GetDpiScalingFactor()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(m_window);
        var dpi = PInvoke.GetDpiForWindow((HWND)hWnd);
        return (float)dpi / 96;
    }
}
