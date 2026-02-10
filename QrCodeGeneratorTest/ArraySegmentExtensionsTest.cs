using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class ArraySegmentExtensionsTest
{
    [Fact]
    public void At_GetsElement()
    {
        var data = new byte[] { 0x10, 0x39, 0xba };
        var segment = new ArraySegment<byte>(data);
        Assert.Equal(0x10, segment.At(0));
        Assert.Equal(0x39, segment.At(1));
        Assert.Equal(0xba, segment.At(2));
    }

    [Fact]
    public void At_ThrowsExceptionForInvalidIndex()
    {
        var data = new byte[] { 0x10, 0x39, 0xba };
        var segment = new ArraySegment<byte>(data);
        Assert.Throws<IndexOutOfRangeException>(() => segment.At(-1));
        Assert.Throws<IndexOutOfRangeException>(() => segment.At(3));
    }

    [Fact]
    public void Slice_Succeeds()
    {
        var data = new byte[] { 0x00, 0x00, 0x10, 0x39, 0xba, 0x00, 0x00 };
        var segment = new ArraySegment<byte>(data);
        var slice = segment.MakeSlice(2, 3);
        
        Assert.Equal(2, slice.Offset);
        Assert.Equal(3, slice.Count);
        Assert.Equal(0x10, slice.At(0));
        Assert.Equal(0x39, slice.At(1));
        Assert.Equal(0xba, slice.At(2));
    }

    [Fact]
    public void Slice_ThrowsExceptionForInvalidRange()
    {
        var data = new byte[] { 0x00, 0x00, 0x10, 0x39, 0xba, 0x00, 0x00 };
        var segment = new ArraySegment<byte>(data);

        Assert.Throws<ArgumentException>(() => segment.MakeSlice(0, 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => segment.MakeSlice(2, -1));
        Assert.Throws<ArgumentException>(() => segment.MakeSlice(8, 3));
    }
}