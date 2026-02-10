/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class BitMatrixTest
{
    [Fact]
    public void Create_Works()
    {
        var bitMatrix = new BitMatrix(191);
        
        Assert.Equal(191, bitMatrix.Size);
        Assert.False(bitMatrix.Get(190, 190));
        bitMatrix.Set(190, 190, true);
        Assert.True(bitMatrix.Get(190, 190));
    }

    [Fact]
    public void GetSet_Works()
    {
        const int size = 131;
        var bitMatrix = new BitMatrix(size);

        for (var y = 0; y < size; y += 1)
        {
            for (var x = y % 2; x < size; x += 2)
            {
                bitMatrix.Set(x, y, true);
            }
        }

        for (var y = 0; y < size; y += 1)
        {
            for (var x = y; x < size; x += 1)
            {
                Assert.Equal((x + y) % 2 == 0, bitMatrix.Get(x, y));
            }
        }
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(5, 7, 1, 1)]
    [InlineData(10, 10, 30, 20)]
    [InlineData(0, 0, 64, 1)]
    [InlineData(0, 0, 65, 1)]
    [InlineData(63, 0, 2, 1)]
    [InlineData(63, 0, 1, 1)]
    [InlineData(64, 0, 64, 1)]
    [InlineData(10, 0, 200, 1)]
    [InlineData(0, 0, 256, 1)]
    [InlineData(0, 0, 256, 256)]
    [InlineData(50, 50, 150, 150)]
    [InlineData(127, 0, 2, 256)]
    [InlineData(255, 255, 1, 1)]
    [InlineData(0, 0, 1, 256)]
    public void FillRect_FillsExactArea(int x, int y, int width, int height)
    {
        const int size = 256;
        var bitMatrix = new BitMatrix(size);

        bitMatrix.FillRect(x, y, width, height);

        for (var yy = 0; yy < size; yy += 1)
        {
            for (var xx = 0; xx < size; xx += 1)
            {
                var expected = xx >= x && xx < x + width && yy >= y && yy < y + height;
                Assert.Equal(expected, bitMatrix.Get(xx, yy));
            }
        }
    }

    [Fact]
    public void FillRect_PreservesExistingBits()
    {
        const int size = 200;
        var bitMatrix = new BitMatrix(size);

        bitMatrix.Set(5, 5, true);
        bitMatrix.Set(150, 150, true);
        bitMatrix.Set(199, 199, true);

        bitMatrix.FillRect(20, 20, 80, 80);

        Assert.True(bitMatrix.Get(5, 5));
        Assert.True(bitMatrix.Get(150, 150));
        Assert.True(bitMatrix.Get(199, 199));
    }

    [Fact]
    public void FillRect_OrsIntoExistingBits()
    {
        const int size = 100;
        var bitMatrix = new BitMatrix(size);

        bitMatrix.Set(50, 50, true);
        bitMatrix.FillRect(40, 40, 30, 30);

        Assert.True(bitMatrix.Get(50, 50));
        Assert.Equal(30 * 30, bitMatrix.BitCount());
    }

    [Theory]
    [InlineData(0, 0, 0, 10)]
    [InlineData(0, 0, 10, 0)]
    [InlineData(0, 0, -1, 10)]
    [InlineData(0, 0, 10, -1)]
    public void FillRect_NoOpForEmpty(int x, int y, int width, int height)
    {
        const int size = 64;
        var bitMatrix = new BitMatrix(size);

        bitMatrix.FillRect(x, y, width, height);

        Assert.Equal(0, bitMatrix.BitCount());
    }

    [Fact]
    public void And_CombinesMatricesBitwise()
    {
        const int size = 131;
        var a = new BitMatrix(size);
        var b = new BitMatrix(size);

        a.FillRect(10, 10, 50, 50);
        b.FillRect(30, 30, 50, 50);

        a.And(b);

        for (var y = 0; y < size; y += 1)
        {
            for (var x = 0; x < size; x += 1)
            {
                var expected = x >= 30 && x < 60 && y >= 30 && y < 60;
                Assert.Equal(expected, a.Get(x, y));
            }
        }
    }

    [Fact]
    public void And_WithDisjointMatrices_YieldsEmpty()
    {
        const int size = 80;
        var a = new BitMatrix(size);
        var b = new BitMatrix(size);

        a.FillRect(0, 0, 30, 30);
        b.FillRect(40, 40, 30, 30);

        a.And(b);

        Assert.Equal(0, a.BitCount());
    }

    [Fact]
    public void And_WithSelf_LeavesMatrixUnchanged()
    {
        const int size = 200;
        var a = new BitMatrix(size);
        a.FillRect(5, 7, 91, 33);
        a.Set(199, 199, true);

        var expected = a.BitCount();
        a.And(a);

        Assert.Equal(expected, a.BitCount());
        Assert.True(a.Get(199, 199));
    }

    [Fact]
    public void And_DifferentSize_Throws()
    {
        var a = new BitMatrix(64);
        var b = new BitMatrix(65);

        Assert.Throws<ArgumentException>(() => a.And(b));
    }

    [Fact]
    public void Xor_TogglesBits()
    {
        const int size = 131;
        var a = new BitMatrix(size);
        var b = new BitMatrix(size);

        a.FillRect(10, 10, 50, 50);
        b.FillRect(30, 30, 50, 50);

        a.Xor(b);

        for (var y = 0; y < size; y += 1)
        {
            for (var x = 0; x < size; x += 1)
            {
                var inA = x >= 10 && x < 60 && y >= 10 && y < 60;
                var inB = x >= 30 && x < 80 && y >= 30 && y < 80;
                Assert.Equal(inA ^ inB, a.Get(x, y));
            }
        }
    }

    [Fact]
    public void Xor_WithSelf_YieldsEmpty()
    {
        const int size = 200;
        var a = new BitMatrix(size);
        a.FillRect(5, 7, 91, 33);
        a.Set(199, 199, true);

        a.Xor(a);

        Assert.Equal(0, a.BitCount());
    }

    [Fact]
    public void Xor_TwiceRestoresOriginal()
    {
        const int size = 100;
        var a = new BitMatrix(size);
        var b = new BitMatrix(size);

        a.FillRect(20, 20, 40, 40);
        b.FillRect(50, 50, 30, 30);
        var expected = a.BitCount();

        a.Xor(b);
        a.Xor(b);

        Assert.Equal(expected, a.BitCount());
        Assert.True(a.Get(20, 20));
        Assert.False(a.Get(80, 80));
    }

    [Fact]
    public void Xor_DifferentSize_Throws()
    {
        var a = new BitMatrix(64);
        var b = new BitMatrix(65);

        Assert.Throws<ArgumentException>(() => a.Xor(b));
    }

    [Theory, CombinatorialData]
    public void Transpose_MatchesNaiveOracle(
        [CombinatorialValues(0, 1, 2, 21, 41, 63, 64, 65, 81, 121, 127, 128, 129, 177, 192, 255, 256)] int size)
    {
        var a = BuildPattern(size, seed: 0xC0FFEE);
        var b = a.Copy();

        a.Transpose();
        NaiveTranspose(b);

        AssertEqual(a, b);
    }

    [Fact]
    public void Transpose_AsymmetricPattern()
    {
        var m = new BitMatrix(5);
        // pattern:
        // 1 0 0 0 0
        // 1 1 0 0 0
        // 1 0 1 0 0
        // 1 0 0 1 0
        // 1 0 0 0 1
        for (var y = 0; y < 5; y += 1)
        {
            m.Set(0, y, true);
            m.Set(y, y, true);
        }

        m.Transpose();

        // expected (rows become columns):
        // 1 1 1 1 1
        // 0 1 0 0 0
        // 0 0 1 0 0
        // 0 0 0 1 0
        // 0 0 0 0 1
        for (var y = 0; y < 5; y += 1)
        {
            for (var x = 0; x < 5; x += 1)
            {
                var expected = (y == 0) || (x == y);
                Assert.Equal(expected, m.Get(x, y));
            }
        }
    }

    [Fact]
    public void Transpose_Empty_NoOp()
    {
        var m = new BitMatrix(0);
        m.Transpose();
        Assert.Equal(0, m.Size);
    }

    [Fact]
    public void Transpose_SingleBit_Identity()
    {
        var m = new BitMatrix(1);
        m.Set(0, 0, true);

        m.Transpose();

        Assert.True(m.Get(0, 0));
        Assert.Equal(1, m.BitCount());
    }

    [Fact]
    public void Transpose_Twice_RestoresOriginal()
    {
        const int size = 177;
        var a = BuildPattern(size, seed: 0xBEEF);
        var original = a.Copy();

        a.Transpose();
        a.Transpose();

        AssertEqual(a, original);
    }

    private static BitMatrix BuildPattern(int size, uint seed)
    {
        var m = new BitMatrix(size);
        var state = seed;
        for (var y = 0; y < size; y += 1)
        {
            for (var x = 0; x < size; x += 1)
            {
                state = state * 1664525u + 1013904223u;
                if ((state & 1u) != 0)
                {
                    m.Set(x, y, true);
                }
            }
        }
        return m;
    }

    [SuppressMessage("csharpsquid", "S2234")]
    private static void NaiveTranspose(BitMatrix m)
    {
        var n = m.Size;
        for (var y = 0; y < n; y += 1)
        {
            for (var x = y + 1; x < n; x += 1)
            {
                var a = m.Get(x, y);
                var b = m.Get(y, x);
                m.Set(x, y, b);
                m.Set(y, x, a);
            }
        }
    }

    private static void AssertEqual(BitMatrix a, BitMatrix b)
    {
        Assert.Equal(a.Size, b.Size);
        for (var y = 0; y < a.Size; y += 1)
        {
            for (var x = 0; x < a.Size; x += 1)
            {
                Assert.Equal(b.Get(x, y), a.Get(x, y));
            }
        }
    }

    [Fact]
    public void FillRect_MultipleRectsCombine()
    {
        const int size = 256;
        var bitMatrix = new BitMatrix(size);

        bitMatrix.FillRect(0, 0, 100, 100);
        bitMatrix.FillRect(150, 150, 50, 50);

        Assert.Equal(100 * 100 + 50 * 50, bitMatrix.BitCount());
        Assert.True(bitMatrix.Get(0, 0));
        Assert.True(bitMatrix.Get(99, 99));
        Assert.False(bitMatrix.Get(100, 100));
        Assert.True(bitMatrix.Get(150, 150));
        Assert.True(bitMatrix.Get(199, 199));
        Assert.False(bitMatrix.Get(200, 200));
    }
}