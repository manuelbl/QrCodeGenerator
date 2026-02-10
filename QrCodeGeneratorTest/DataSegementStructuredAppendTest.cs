/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class DataSegmentStructuredAppendTest : DataSegmentTestBase
{
    [Theory]
    [InlineData(1, 13, 0xea, new byte[] { 0x0c, 0xea })]
    [InlineData(16, 16, 0x1c, new byte[] { 0xff, 0x1c })]
    public void EncodeStructuredAppend(int position, int total, byte parity, byte[] expectedData)
    {
        var segment = new DataSegmentStructuredAppend(position, total, parity);
        var codewords = CreateCodewords(segment);
        Assert.Equal(codewords, expectedData);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(17, 7)]
    [InlineData(1, 0)]
    [InlineData(1, 17)]
    [InlineData(5, 2)]
    public void Constructor_RejectsInvalidValues(int position, int total)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DataSegmentStructuredAppend(position, total, 0x3f));
    }
}
