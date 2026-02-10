//
// Swiss QR Bill Generator for .NET
// Copyright (c) 2026 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Net.Codecrete.QrCodeGenerator.Demo.Models;

internal partial class QrCodeSettings : INotifyPropertyChanged
{
    private string _text = "Hello, World!";
    private int _borderWidth = 4;
    private QrCode.Ecc _eccLevel = QrCode.Ecc.Medium;

    public string Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public int BorderWidth
    {
        get => _borderWidth;
        set => SetField(ref _borderWidth, value);
    }

    public QrCode.Ecc EccLevel
    {
        get => _eccLevel;
        set => SetField(ref _eccLevel, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
