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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Net.Codecrete.QrCodeGenerator.Analyzer;

public enum HighlightKind { None, Blocks, HorizontalStreaks, VerticalStreaks, HorizontalFinderPatterns, VerticalFinderPatterns }

public sealed record SegmentRow(string Mode, string Content);

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private const int BorderWidth = 4;

    private string _text = "QR code text";
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
    private ECI _eci = ECI.Automatic;
    private string? _errorMessage;

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
        EciOptions =
        [
            // Auto and None pinned at top, then the common encodings, then the rest by ECI value.
            new Tuple<string, ECI>("Auto", ECI.Automatic),
            new Tuple<string, ECI>("None", ECI.None),
            new Tuple<string, ECI>("UTF-8", ECI.UTF8),
            new Tuple<string, ECI>("Latin-1 / ISO 8859-1", ECI.Latin1),
            new Tuple<string, ECI>("Windows-1252", ECI.Windows1252),
            new Tuple<string, ECI>("Shift-JIS", ECI.ShiftJIS),
            new Tuple<string, ECI>("ASCII", ECI.US_ASCII),
            new Tuple<string, ECI>("Code page 437", ECI.CodePage437),
            new Tuple<string, ECI>("Latin-2 / ISO 8859-2", ECI.Latin2),
            new Tuple<string, ECI>("Latin-3 / ISO 8859-3", ECI.Latin3),
            new Tuple<string, ECI>("Latin-4 / ISO 8859-4", ECI.Latin4),
            new Tuple<string, ECI>("Cyrillic / ISO 8859-5", ECI.LatinCyrillic),
            new Tuple<string, ECI>("Arabic / ISO 8859-6", ECI.LatinArabic),
            new Tuple<string, ECI>("Greek / ISO 8859-7", ECI.LatinGreek),
            new Tuple<string, ECI>("Hebrew / ISO 8859-8", ECI.LatinHebrew),
            new Tuple<string, ECI>("Latin-5 / ISO 8859-9", ECI.Latin5),
            new Tuple<string, ECI>("Latin-6 / ISO 8859-10", ECI.Latin6),
            new Tuple<string, ECI>("Thai / ISO 8859-11", ECI.LatinThai),
            new Tuple<string, ECI>("Latin-7 / ISO 8859-13", ECI.Latin7),
            new Tuple<string, ECI>("Latin-8 / ISO 8859-14", ECI.Latin8),
            new Tuple<string, ECI>("Latin-9 / ISO 8859-15", ECI.Latin9),
            new Tuple<string, ECI>("Latin-10 / ISO 8859-16", ECI.Latin10),
            new Tuple<string, ECI>("Windows-1250", ECI.Windows1250),
            new Tuple<string, ECI>("Windows-1251", ECI.Windows1251),
            new Tuple<string, ECI>("Windows-1256", ECI.Windows1256),
            new Tuple<string, ECI>("UTF-16BE", ECI.UTF16BE),
            new Tuple<string, ECI>("Big5", ECI.Big5),
            new Tuple<string, ECI>("GB2312", ECI.GB2312),
            new Tuple<string, ECI>("KS X 1001", ECI.KS_X1001),
            new Tuple<string, ECI>("GB18030", ECI.GB18030),
            new Tuple<string, ECI>("UTF-16LE", ECI.UTF16LE),
            new Tuple<string, ECI>("UTF-32BE", ECI.UTF32BE),
            new Tuple<string, ECI>("UTF-32LE", ECI.UTF32LE)
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

    public ECI Eci
    {
        get => _eci;
        set
        {
            if (Equals(_eci, value)) return;
            _eci = value;
            UpdateQrCode();
            OnPropertyChanged();
        }
    }

    public Tuple<string, QrCode.Ecc>[] ErrorCorrectionLevels { get; }

    public Tuple<string, int>[] DataMasks { get; }

    public Tuple<string, ECI>[] EciOptions { get; }

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

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InfoVisibility));
            OnPropertyChanged(nameof(ErrorVisibility));
        }
    }

    public Visibility InfoVisibility => _errorMessage == null ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ErrorVisibility => _errorMessage == null ? Visibility.Collapsed : Visibility.Visible;

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

    public BitmapSource? CreateClipboardBitmap()
    {
        // Render from the already-built QR code so the copy matches the preview (ECI, mask, ECC).
        return _currentQrCode == null ? null : QrCodeDrawing.CreateBitmapImage(_currentQrCode, 20, BorderWidth);
    }

    private void UpdateQrCode()
    {
        try
        {
            EncodingInfo encodingInfo = new()
            {
                ForcedDataMask = _dataMaskPattern
            };
            var (eci, encoding) = ResolveEci();
            var qrCode = QrCode.EncodeTextAdvanced(_text, _errorCorrection, boostEcl: !fixedEcc,
                eci: eci, encoding: encoding, encodingInfo: encodingInfo);
            _currentQrCode = qrCode;
            SelectedDataMaskPattern = qrCode.Mask;
            PenaltyInfo = encodingInfo.Penalties[qrCode.Mask];
            QrCodeVersion = qrCode.Version;
            QrCodeSize = $"{qrCode.Size}×{qrCode.Size}";
            SelectedEcc = qrCode.ErrorCorrectionLevel;
            DataSegments = encodingInfo.DataSegments?.Select(BuildSegmentRow).ToArray()
                ?? (IReadOnlyList<SegmentRow>)[];
            ErrorMessage = null;
            RenderQrCodeImage();
        }
        catch (Exception ex) when (ex is EncoderFallbackException or DataTooLongException or ECIException)
        {
            // Invalid combination of text and selected encoding: blank the preview and show the reason.
            _currentQrCode = null;
            QrCodeImage = null;
            DataSegments = [];
            ErrorMessage = FormatError(ex);
        }
    }

    private (ECI Eci, Encoding? Encoding) ResolveEci()
    {
        // ECI.None requires an explicit encoding; use UTF-8 so any text encodes (with no ECI marker).
        if (Equals(_eci, ECI.None))
        {
            return (ECI.None, ECI.UTF8.GetEncoding());
        }
        // Automatic and specific ECIs: let the library derive the encoding.
        return (_eci, null);
    }

    private string FormatError(Exception ex)
    {
        if (ex is DataTooLongException)
        {
            return "Text is too long to fit in a QR code.";
        }
        if (ex is ECIException)
        {
            return $"Encoding for {EciLabel()} is not available on this platform.";
        }
        return $"Text cannot be encoded in {EciLabel()}.";
    }

    private string EciLabel()
    {
        return EciOptions.FirstOrDefault(o => Equals(o.Item2, _eci))?.Item1 ?? _eci.Value.ToString();
    }

    private static SegmentRow BuildSegmentRow(DataSegment segment)
    {
        return segment.Mode switch
        {
            DataSegmentMode.Numeric => new SegmentRow("Numeric", DecodeAscii(segment.DataBytes)),
            DataSegmentMode.Alphanumeric => new SegmentRow("Alphanumeric", DecodeAscii(segment.DataBytes)),
            DataSegmentMode.Binary => new SegmentRow("Byte", FormatByteContent(segment.DataBytes)),
            DataSegmentMode.Kanji => new SegmentRow("Kanji", FormatHex(segment.DataBytes)),
            DataSegmentMode.ECI => new SegmentRow("ECI", FormatEci(segment.EciDesignator)),
            DataSegmentMode.StructuredAppend => new SegmentRow("Structured Append", FormatStructuredAppend(segment)),
            _ => new SegmentRow(segment.Mode.ToString(), FormatHex(segment.DataBytes)),
        };
    }

    private static string FormatEci(ECI eci)
    {
        try
        {
            return $"{eci.Value} ({eci.GetEncoding().WebName})";
        }
        catch (ECIException)
        {
            return eci.Value.ToString();
        }
    }

    private static string FormatStructuredAppend(DataSegment segment)
    {
        return $"Part {segment.StructuredAppendPosition} of {segment.StructuredAppendTotal}, "
            + $"parity 0x{segment.StructuredAppendParity:X2}";
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
            HighlightKind.HorizontalFinderPatterns => Penalty.GetHorizontalFinderPatterns(_currentQrCode),
            HighlightKind.VerticalFinderPatterns => Penalty.GetVerticalFinderPatterns(_currentQrCode),
            _ => null
        };
        QrCodeImage = QrCodeDrawing.CreateDrawing(_currentQrCode, 192, BorderWidth, highlights);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
