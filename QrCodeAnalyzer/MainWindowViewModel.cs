//
// QR code generator library (.NET)
// https://github.com/manuelbl/QrCodeGenerator
//
// Copyright (c) 2021 Manuel Bleichenbacher
// Licensed under MIT License
// https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Net.Codecrete.QrCodeGenerator.Analyzer;

public enum HighlightKind { None, Blocks, HorizontalStreaks, VerticalStreaks }

public sealed record SegmentRow(string Mode, string Content);

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private string _text = "QR code text";
    private int _borderWidth = 4;
    private QrCode.Ecc _errorCorrection = QrCode.Ecc.Medium;
    private bool fixedEcc = false;
    private int _dataMaskPattern = -1;
    private PenaltyScore _penaltyScore;
    private ImageSource? _qrCodeImage;
    private int _selectedDataMaskPattern = -1;
    private QrCode? _currentQrCode;
    private HighlightKind _highlight = HighlightKind.None;
    private IReadOnlyList<SegmentRow> _dataSegments = [];
    private int _qrCodeVersion;
    private string _qrCodeSize = string.Empty;
    private QrCode.Ecc _selectedEcc;

    public MainWindowViewModel()
    {
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

    public PenaltyScore PenaltyInfo
    {
        get => _penaltyScore;
        private set
        {
            _penaltyScore = value;
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

    public int QrCodeVersion
    {
        get => _qrCodeVersion;
        private set
        {
            if (_qrCodeVersion == value) return;
            _qrCodeVersion = value;
            OnPropertyChanged();
        }
    }

    public string QrCodeSize
    {
        get => _qrCodeSize;
        private set
        {
            if (_qrCodeSize == value) return;
            _qrCodeSize = value;
            OnPropertyChanged();
        }
    }

    public QrCode.Ecc SelectedEcc
    {
        get => _selectedEcc;
        private set
        {
            if (_selectedEcc == value) return;
            _selectedEcc = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<SegmentRow> DataSegments
    {
        get => _dataSegments;
        private set
        {
            _dataSegments = value;
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
        EncodingInfo encodingInfo = new()
        {
            ForcedDataMask = _dataMaskPattern
        };
        var qrCode = QrCode.EncodeTextAdvanced(_text, _errorCorrection, boostEcl: !fixedEcc, encodingInfo: encodingInfo);
        _currentQrCode = qrCode;
        SelectedDataMaskPattern = qrCode.Mask;
        PenaltyInfo = encodingInfo.Penalties[qrCode.Mask];
        QrCodeVersion = qrCode.Version;
        QrCodeSize = $"{qrCode.Size}×{qrCode.Size}";
        SelectedEcc = qrCode.ErrorCorrectionLevel;
        DataSegments = encodingInfo.DataSegments?.Select(BuildSegmentRow).ToArray()
            ?? (IReadOnlyList<SegmentRow>)[];
        RenderQrCodeImage();
    }

    private static SegmentRow BuildSegmentRow(DataSegment segment)
    {
        return segment.Mode switch
        {
            DataSegmentMode.Numeric => new SegmentRow("Numeric", DecodeAscii(segment.DataBytes)),
            DataSegmentMode.Alphanumeric => new SegmentRow("Alphanumeric", DecodeAscii(segment.DataBytes)),
            DataSegmentMode.Binary => new SegmentRow("Byte", FormatByteContent(segment.DataBytes)),
            DataSegmentMode.Kanji => new SegmentRow("Kanji", FormatHex(segment.DataBytes)),
            DataSegmentMode.ECI => new SegmentRow("ECI", "TODO"),
            DataSegmentMode.StructuredAppend => new SegmentRow("Structured Append", "TODO"),
            _ => new SegmentRow(segment.Mode.ToString(), FormatHex(segment.DataBytes)),
        };
    }

    private static string DecodeAscii(ArraySegment<byte> data)
    {
        return data.Array == null ? string.Empty : Encoding.ASCII.GetString(data.Array, data.Offset, data.Count);
    }

    private static readonly UTF8Encoding StrictUtf8 = new(false, throwOnInvalidBytes: true);

    private static string FormatByteContent(ArraySegment<byte> data)
    {
        if (data.Array == null || data.Count == 0) return string.Empty;
        try
        {
            var text = StrictUtf8.GetString(data.Array, data.Offset, data.Count);
            foreach (var c in text)
            {
                if (c < 0x20 && c != '\t' && c != '\r' && c != '\n')
                {
                    return FormatHex(data);
                }
            }
            return text;
        }
        catch (DecoderFallbackException)
        {
            return FormatHex(data);
        }
    }

    private static string FormatHex(ArraySegment<byte> data)
    {
        if (data.Array == null || data.Count == 0) return string.Empty;
        var sb = new StringBuilder(data.Count * 3);
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(data.Array[data.Offset + i].ToString("X2"));
        }
        return sb.ToString();
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
