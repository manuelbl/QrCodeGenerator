/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class DataSegmentByteTest : DataSegmentTestBase
{
    [Theory]
    [InlineData("a", new byte[] { 0x61 })]
    [InlineData("k/", new byte[] { 0x6b, 0x2f })]
    [InlineData("ĠźǄȻ", new byte[] { 0xc4, 0xa0, 0xc5, 0xba, 0xc7, 0x84, 0xc8, 0xbb })]
    [InlineData("Ξϴ", new byte[] { 0xce, 0x9e, 0xcf, 0xb4 })]
    [InlineData("أطعمة", new byte[] { 0xd8, 0xa3, 0xd8, 0xb7, 0xd8, 0xb9, 0xd9, 0x85, 0xd8, 0xa9 })]
    public void EncodeByteText(string text, byte[] expectedData)
    {
        EncodeAndCompare(text, expectedData, data => new DataSegmentByte(data));
    }
    
    [Theory]
    [InlineData("a")]
    [InlineData("7")]
    [InlineData("c3")]
    [InlineData("k/")]
    [InlineData("0@f")]
    [InlineData("7¾ô")]
    [InlineData("ĠźǄȻ")]
    [InlineData("Ξϴ")]
    [InlineData("أطعمة")]
    [InlineData("𝄢𝇇")]
    [InlineData("夢に体当たり、砕け散って9カ月経った今")]
    [InlineData("😀💂💆‍♀️⛑👆🏻")]
    public void EncodeDecodeByteText(string text)
    {
        EncodeDecode(text, t => new DataSegmentByte(t), DecodeByte);
    }
    
    private static byte[] DecodeByte(BitStream bitStream)
    {
        var bitLength = bitStream.Length;
        var numBytes = bitLength / 8;
        var result = new byte[numBytes];
        for (var i = 0; i < numBytes; i += 1)
        {
            result[i] = (byte)bitStream.ExtractBits(i * 8, 8);
        }
        return result;
    }

    [Theory, CombinatorialData]
    public void BitLength([CombinatorialRange(0, 100)] int bitLength)
    {
        var byteCount = DataSegmentByte.GetByteByteCount(bitLength);
        var bitLengthLower = DataSegmentByte.GetByteBitLength(byteCount);
        var bitLengthUpper = DataSegmentByte.GetByteBitLength(byteCount + 1);
        Assert.InRange(bitLengthLower, bitLengthLower, bitLengthUpper);
    }
}
