/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Text;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class DataSegmentTestBase
{
    #region Test initialization
    
    protected DataSegmentTestBase()
    {
    }
    
    static DataSegmentTestBase()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    
    #endregion
    
    #region Test Helper

    /// <summary>
    /// Calculates the number of bytes encoded by a segment of the given length.
    /// </summary>
    /// <param name="mode">The encoding mode.</param>
    /// <param name="bitLength">The length of the encoded data, in bits.</param>
    /// <returns>The number data bytes.</returns>
    internal static int CalcByteLength(DataSegmentMode mode, int bitLength)
    {
        switch (mode)
        {
            case DataSegmentMode.Numeric:
                return bitLength / 10  * 3 + (bitLength % 10 + 1) / 4;
            case DataSegmentMode.Alphanumeric:
                return bitLength / 11 * 2 + bitLength % 11 / 6;
            case DataSegmentMode.Kanji:
                return bitLength / 13 * 2;
            case DataSegmentMode.Binary: 
                return bitLength / 8;
            default:
                return 0;
        }
    }
    
    protected static void EncodeAndCompare(string text, byte[] expectedData, Func<byte[], DataSegment> encoder)
    {
        EncodeAndCompare(text, expectedData, encoder, ECI.UTF8.GetEncoding());   
    }

    protected static void EncodeAndCompare(string text, byte[] expectedData, Func<byte[], DataSegment> encoder,
        Encoding encoding)
    {
        var data = encoding.GetBytes(text);
        var segment = encoder(data);
        var bitStream = new BitStream(data.Length + 20);
        segment.WriteToBitStream(bitStream);
        var codewords = bitStream.GetCodewords();
        Assert.Equal(codewords, expectedData);
    }

    internal static void EncodeDecode(string text, Func<byte[], DataSegment> encoder, Func<BitStream, byte[]> decoder)
    {
        EncodeDecode(text, encoder, decoder, ECI.UTF8.GetEncoding());
    }
    
    internal static void EncodeDecode(string text, Func<byte[], DataSegment> encoder,
        Func<BitStream, byte[]> decoder, Encoding encoding)
    {
        var data = encoding.GetBytes(text);
        var segment = encoder(data);
        var bitStream = new BitStream(data.Length + 20);
        segment.WriteToBitStream(bitStream);
        Assert.Equal(segment.EncodedLength, bitStream.Length);
        var decodedData = decoder(bitStream);
        Assert.Equal(text, encoding.GetString(decodedData));
    }

    protected static byte[] CreateCodewords(DataSegment segment)
    {
        var bitStream = new BitStream(10);
        segment.WriteToBitStream(bitStream);
        return bitStream.GetCodewords();
    }

    #endregion
}