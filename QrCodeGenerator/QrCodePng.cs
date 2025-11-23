using System;

namespace Net.Codecrete.QrCodeGenerator;

internal sealed class QrCodePng
{
    private readonly QrCode _qr;
    private readonly int _scale;
    private readonly int _border;
    private readonly int _realSize;
    private readonly int _scaledSize;

    public QrCodePng(QrCode qr, int scale, int border)
    {
        _qr = qr;
        _scale = scale;
        _border = border;
        _realSize = qr.Size + border * 2;
        _scaledSize = _realSize * scale;
    }

    public byte[] GetBytes()
    {
        using var png = new PngBuilder();

        var data = Draw();

        png.WriteHeader(_scaledSize, _scaledSize, 1, 0);
        png.WriteData(data);
        png.WriteEnd();

        return png.GetBytes();
    }

    private byte[] Draw()
    {
        int bytesPerLine = (_scaledSize + 7) / 8 + 1;
        var data = new byte[bytesPerLine * _scaledSize];

        for (int y = 0; y < _realSize; y++)
        {
            int offset = y * bytesPerLine * _scale;

            for (int x = 0; x < _realSize; x++)
            {
                if (_qr.GetModule(x - _border, y - _border))
                    continue;

                int pos = x * _scale;
                int end = pos + _scale;

                for (; pos < end; pos++)
                {
                    int index = offset + pos / 8 + 1;
                    data[index] |= (byte)(0x80 >> pos % 8);
                }
            }

            for (var i = 1; i < _scale; i++)
                Array.Copy(data, offset, data, offset + i * bytesPerLine, bytesPerLine);
        }

        return data;
    }
}  
