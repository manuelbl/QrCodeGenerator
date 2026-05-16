//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Net.Codecrete.QrCodeGenerator.Analyzer;

public enum HighlightKind { None, Blocks, HorizontalStreaks, VerticalStreaks }

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly QrCodeBuilder.DebugInfo _debugInfo = new QrCodeBuilder.DebugInfo();

    private string _text = "QR code text";
    private int _borderWidth = 4;
    private QrCode.Ecc _errorCorrection = QrCode.Ecc.Medium;
    private bool fixedEcc = false;
    private int _dataMaskPattern = -1;
    private QrCodeBuilder.PenaltyInfo _penaltyDetails;
    private ImageSource? _qrCodeImage;
    private int _selectedDataMaskPattern = -1;
    private QrCode? _currentQrCode;
    private HighlightKind _highlight = HighlightKind.None;

    public MainWindowViewModel()
    {
        QrCodeBuilder.DebugAccess = _debugInfo;
        ErrorCorrectionLevels =
        [
            new Tuple<string, QrCode.Ecc>("Low", QrCode.Ecc.Low),
            new Tuple<string, QrCode.Ecc>("Medium", QrCode.Ecc.Medium),
            new Tuple<string, QrCode.Ecc>("Quartile", QrCode.Ecc.Quartile),
            new Tuple<string, QrCode.Ecc>("High", QrCode.Ecc.High)
        ];
        DataMasks =
        [
            new Tuple<string, int>("Auto", -1),
            new Tuple<string, int>("Mask 0", 0),
            new Tuple<string, int>("Mask 1", 1),
            new Tuple<string, int>("Mask 2", 2),
            new Tuple<string, int>("Mask 3", 3),
            new Tuple<string, int>("Mask 4", 4),
            new Tuple<string, int>("Mask 5", 5),
            new Tuple<string, int>("Mask 6", 6),
            new Tuple<string, int>("Mask 7", 7)
        ];
        UpdateQrCode();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public int BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (_borderWidth == value) return;
            _borderWidth = value;
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public Tuple<string, QrCode.Ecc>[] ErrorCorrectionLevels { get; }

    public Tuple<string, int>[] DataMasks { get; }

    public QrCode.Ecc ErrorCorrection
    {
        get => _errorCorrection;
        set
        {
            if (_errorCorrection == value) return;
            _errorCorrection = value;
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public bool FixedEcc
    {
        get => fixedEcc;
        set
        {
            if (fixedEcc == value) return;
            fixedEcc = value;
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public int DataMaskPattern
    {
        get => _dataMaskPattern;
        set
        {
            if (_dataMaskPattern == value) return;
            _dataMaskPattern = Math.Clamp(value, -1, 7);
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public int SelectedDataMaskPattern
    {
        get => _selectedDataMaskPattern;
        private set
        {
            if (_selectedDataMaskPattern == value) return;
            _selectedDataMaskPattern = value;
            OnPropertyChanged();
        }
    }

    public QrCodeBuilder.PenaltyInfo PenaltyDetails
    {
        get => _penaltyDetails;
        private set
        {
            _penaltyDetails = value;
            OnPropertyChanged();
        }
    }

    public ImageSource? QrCodeImage
    {
        get => _qrCodeImage;
        private set
        {
            _qrCodeImage = value;
            OnPropertyChanged();
        }
    }

    public HighlightKind Highlight
    {
        get => _highlight;
        set
        {
            if (_highlight == value) return;
            _highlight = value;
            RenderQrCodeImage();
            OnPropertyChanged();
        }
    }

    public BitmapSource CreateClipboardBitmap()
    {
        var qrCode = QrCode.EncodeText(_text, _errorCorrection);
        return QrCodeDrawing.CreateBitmapImage(qrCode, 20, _borderWidth);
    }

    private void UpdateQrCode()
    {
        _debugInfo.ForcedDataMask = _dataMaskPattern;
        var qrCode = QrCode.EncodeTextAdvanced(_text, _errorCorrection, boostEcl: !fixedEcc);
        _currentQrCode = qrCode;
        SelectedDataMaskPattern = qrCode.Mask;
        PenaltyDetails = _debugInfo.Penalties[qrCode.Mask];
        RenderQrCodeImage();
    }

    private void RenderQrCodeImage()
    {
        if (_currentQrCode == null) return;
        var highlights = _highlight switch
        {
            HighlightKind.Blocks => Penalty.GetBlocks(_currentQrCode),
            HighlightKind.HorizontalStreaks => Penalty.GetHorizontalStreaks(_currentQrCode),
            HighlightKind.VerticalStreaks => Penalty.GetVerticalStreaks(_currentQrCode),
            _ => null
        };
        QrCodeImage = QrCodeDrawing.CreateDrawing(_currentQrCode, 192, _borderWidth, highlights);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
