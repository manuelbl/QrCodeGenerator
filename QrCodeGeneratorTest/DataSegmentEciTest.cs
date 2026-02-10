/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class DataSegmentEciTest : DataSegmentTestBase
{
    [Theory]
    [InlineData(0, new byte[] { 0b0000_0000 })]
    [InlineData(127, new byte[] { 0b0111_1111 })]
    [InlineData(128, new byte[] { 0b1000_0000, 0b1000_0000 })]
    [InlineData(16383, new byte[] { 0b1011_1111, 0b1111_1111 })]
    [InlineData(16384, new byte[] { 0b1100_0000, 0b0100_0000, 0b0000_0000 })]
    [InlineData(999999, new byte[] { 0b1100_1111, 0b0100_0010, 0b0011_1111 })]
    public void EncodeECI(int eciValue, byte[] expectedData)
    {
        var segment = new DataSegmentEci(ECI.FromValue(eciValue));
        var codewords = CreateCodewords(segment);
        Assert.Equal(codewords, expectedData);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(16383)]
    [InlineData(16384)]
    [InlineData(999999)]
    public void EncodeDecodeECI(int eciValue)
    {
        var segment = new DataSegmentEci(ECI.FromValue(eciValue));
        var bitStream = new BitStream(10);
        segment.WriteToBitStream(bitStream);
        var eci = DecodeEciDesignator(bitStream);
        Assert.Equal(eci.Value, eciValue);
    }

    [Fact]
    public void InvalidECIValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DataSegmentEci(ECI.FromValue(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DataSegmentEci(ECI.FromValue(1000000)));
    }

    [Fact]
    public void DecodeInvalidECI()
    {
        var bitStream = new BitStream(10);
        bitStream.AppendBits(17, 6);
        var exception = Assert.Throws<ArgumentException>(() => DecodeEciDesignator(bitStream));
        Assert.Contains("Bit array of ECI designator must be 8, 16 or 24 bits long", exception.Message);

        bitStream = new BitStream(10);
        bitStream.AppendBits(3, 2);
        bitStream.AppendBits(0, 6);
        exception = Assert.Throws<ArgumentException>(() => DecodeEciDesignator(bitStream));
        Assert.Contains("Invalid length of ECI designator bit array", exception.Message);
    }
    
    private static ECI DecodeEciDesignator(BitStream bitStream)
    {
        var length = bitStream.Length;
        if (length != 8 && length != 16 && length != 24)
        {
            throw new ArgumentException("Bit array of ECI designator must be 8, 16 or 24 bits long", nameof(bitStream));
        }
            
        var bits12 = bitStream.ExtractBits(0, 2);
        if (((bits12 == 0 || bits12 == 1) && length != 8)
            || (bits12 == 2 && length != 16)
            || (bits12 == 3 && length != 24))
        {
            throw new ArgumentException("Invalid length of ECI designator bit array", nameof(bitStream));
        }

        int eci;
        if (bits12 == 0 || bits12 == 1)
        {
            eci = (int)bitStream.ExtractBits(1, 7);
        }
        else if (bits12 == 2)
        {
            eci = (int)bitStream.ExtractBits(2, 14);
        }
        else
        {
            eci = (int)bitStream.ExtractBits(4, 20);
        }

        return ECI.FromValue(eci);
    }
}
