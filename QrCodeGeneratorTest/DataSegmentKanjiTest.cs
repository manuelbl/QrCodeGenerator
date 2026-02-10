/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class DataSegmentKanjiTest : DataSegmentTestBase
{
    [Theory]
    [InlineData(0x8147, true)]
    [InlineData(0x8167, true)]
    [InlineData(0x81BD, true)]
    [InlineData(0x81FC, true)]
    [InlineData(0x824F, true)]
    [InlineData(0x8286, true)]
    [InlineData(0x82E2, true)]
    [InlineData(0x838A, true)]
    [InlineData(0x8457, true)]
    [InlineData(0x84BE, true)]
    [InlineData(0x889F, true)]
    [InlineData(0x88EB, true)]
    [InlineData(0x8E90, true)]
    [InlineData(0x8FFC, true)]
    [InlineData(0x9040, true)]
    [InlineData(0x924F, true)]
    [InlineData(0x965D, true)]
    [InlineData(0x98CA, true)]
    [InlineData(0x9BD4, true)]
    [InlineData(0x9FFC, true)]
    [InlineData(0xE040, true)]
    [InlineData(0xE094, true)]
    [InlineData(0xE0E7, true)]
    [InlineData(0xE1D8, true)]
    [InlineData(0xE2D3, true)]
    [InlineData(0xE4FC, true)]
    [InlineData(0xEAA4, true)]
    [InlineData(0x4E, false)]
    [InlineData(0xA8, false)]
    [InlineData(0x813F, false)]
    [InlineData(0x81FD, false)]
    [InlineData(0x8047, false)]
    [InlineData(0xA043, false)]
    [InlineData(0xDF88, false)]
    [InlineData(0xEC71, false)]
    public void KanjiEncoding(int shiftJisCode, bool isValid)
    {
        var byte1 = (byte)(shiftJisCode >> 8);
        var byte2 = (byte)(shiftJisCode & 0xFF);
        Assert.Equal(isValid, DataSegmentKanji.IsShiftJisDoubleByte(byte1, byte2));
        if (isValid)
        {
            Assert.InRange(DataSegmentKanji.EncodeShiftJisCode(shiftJisCode), 0u, (1u << 13) - 1);
        }
    }

    [Theory]
    [InlineData(0x4E)]
    [InlineData(0xA8)]
    [InlineData(0x813F)]
    [InlineData(0x8047)]
    [InlineData(0x9FFD)]
    [InlineData(0xA043)]
    [InlineData(0xDF88)]
    [InlineData(0xEC71)]
    public void EncodeInvalidShiftJisCode(int shiftJisCode)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DataSegmentKanji.EncodeShiftJisCode(shiftJisCode));
    }

    [Theory]
    [InlineData("ヒ", new byte[] { 0b0000_1101, 0b1000_1000 })]
    [InlineData("鵈", new byte[] { 0b1111_0101, 0b1110_0000 })]
    [InlineData("苜悉", new byte[] { 0b1101_0100, 0b1110_0010, 0b1000_1110, 0b1100_0000 })]
    public void EncodeKanjiText(string text, byte[] expectedData)
    {
        EncodeAndCompare(text, expectedData, data => new DataSegmentKanji(data), ECI.ShiftJIS.GetEncoding());
    }
    
    [Theory]
    [InlineData("新購ラつ教")]
    [InlineData("なれ南恐文ぽういょ対捕ルタ年尾アケ和正言")]
    [InlineData("、（５ぺ殻適斑")]
    [InlineData("滌漾鼕堯")]
    public void EncodeDecodeKanji(string text)
    {
        EncodeDecode(text, t => new DataSegmentKanji(t), DecodeKanji, ECI.ShiftJIS.GetEncoding());
    }
    
    private static byte[] DecodeKanji(BitStream bitStream)
    {
        var bitLength = bitStream.Length;
        var numBytes = bitLength / 13 * 2;
        var result = new byte[numBytes];
        var index = 0;
            
        for (var offset = 0; offset + 12 < bitLength; offset += 13)
        {
            var encoded = bitStream.ExtractBits(offset, 13);
            // Decode the 13-bit encoded value back to Shift JIS code
            var shiftJisCode = DecodeShiftJisCode(encoded);
            result[index] = (byte)(shiftJisCode >> 8);
            result[index + 1] = (byte)(shiftJisCode & 0xff);
            index += 2;
        }

        return result;
    }

    /// <summary>
    /// Decode a 13-bit integer to a double-byte Shift JIS code.
    /// </summary>
    /// <param name="encoded">Encoded value (13 bits)</param>
    /// <returns>Shift JIS code</returns>
    private static int DecodeShiftJisCode(uint encoded)
    {
        var code = (encoded / 0xc0) << 8 | (encoded % 0xc0);
        var highByte = code >> 8;
        return (int)(highByte < 0x1F ? code + 0x8140 : code + 0xc140);
    }
    
    [Theory, CombinatorialData]
    public void BitLength([CombinatorialRange(0, 100)] int bitLength)
    {
        var byteCount = DataSegmentKanji.GetKanjiByteCount(bitLength);
        var bitLengthLower = DataSegmentKanji.GetKanjiBitLength(byteCount);
        var bitLengthUpper = DataSegmentKanji.GetKanjiBitLength(byteCount + 2);
        Assert.InRange(bitLengthLower, bitLengthLower, bitLengthUpper);
    }
}
