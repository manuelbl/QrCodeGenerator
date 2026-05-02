/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class BitStreamTest
    {
        [Fact]
        public void AppendExtractBits()
        {
            var stream = new BitStream(10);
            stream.AppendBits(18, 6);
            stream.AppendBits(30, 5);
            stream.AppendBits(15, 5);
            stream.AppendBits(3, 2);
            stream.AppendBits(2196229387, 32);

            Assert.Equal(6 + 5 + 5 + 2 + 32, stream.Length);

            Assert.Equal(18u, stream.ExtractBits(0, 6));
            Assert.Equal(30u, stream.ExtractBits(6, 5));
            Assert.Equal(15u, stream.ExtractBits(11, 5));
            Assert.Equal(3u, stream.ExtractBits(16, 2));
            Assert.Equal(2196229387u, stream.ExtractBits(18, 32));
        }

        [Fact]
        public void AppendExtractBits_WithReallocation()
        {
            var stream = new BitStream(200);
            for (var i = 0; i < 57; i += 1)
            {
                stream.AppendBits((uint)(358234 + i), 19);
            }

            for (var i = 0; i < 57; i += 1)
            {
                Assert.Equal((uint)(358234 + i), stream.ExtractBits(i * 19, 19));
            }
        }
    
        [Fact]
        public void AppendBits_TooLongValue()
        {
            var stream = new BitStream(10);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                stream.AppendBits(128, 4);
            });
        }

        [Fact]
        public void AppendBits_LengthOutOfRange()
        {
            var stream = new BitStream(10);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                stream.AppendBits(128, 33);
            });
        }

        [Fact]
        public void AppendBits_ExceedsCapacity()
        {
            var stream = new BitStream(1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                stream.AppendBits(128, 9);
            });
        }

        [Fact]
        public void ExtractBits_LengthOutOfRange()
        {
            var stream = new BitStream(20);
            stream.AppendBits(123456789, 32);
            stream.AppendBits(123456789, 32);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                stream.ExtractBits(0, 33);
            });
        }

        [Fact]
        public void ExtractBits_IndexOutOfRange()
        {
            var stream = new BitStream(10);
            stream.AppendBits(23456, 15);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                stream.ExtractBits(10, 6);
            });
        }
    
        [Fact]
        public void GetCodewords()
        {
            var stream = new BitStream(10);
            stream.AppendBits(0x41, 8);
            stream.AppendBits(0x7, 4);
            stream.AppendBits(0xa, 4);
            stream.AppendBits(0x20, 8);
            stream.AppendBits(0x38, 8);
        
            var codewords = stream.GetCodewords();
            Assert.Equal(4, codewords.Length);
            Assert.Equal(0x41, codewords[0]);
            Assert.Equal(0x7a, codewords[1]);
            Assert.Equal(0x20, codewords[2]);
            Assert.Equal(0x38, codewords[3]);
       }

        [Fact]
        public void GetCodewords_WithPadding()
        {
            var stream = new BitStream(10);
            stream.AppendBits(0x39, 8);
            stream.AppendBits(0x21, 6);
        
            var codewords = stream.GetCodewords();
            Assert.Equal(2, codewords.Length);
            Assert.Equal(0x39, codewords[0]);
            Assert.Equal(0x84, codewords[1]);
        }

        [Fact]
        public void CopyCodewords()
        {
            var stream = new BitStream(10);
            stream.AppendBits(0x41, 8);
            stream.AppendBits(0xa7, 8);
            stream.AppendBits(0x20, 8);
            stream.AppendBits(0x38, 8);
        
            var codewords = new byte[4];
            stream.CopyCodewords(codewords, 0);
            Assert.Equal(0x41, codewords[0]);
            Assert.Equal(0xa7, codewords[1]);
            Assert.Equal(0x20, codewords[2]);
            Assert.Equal(0x38, codewords[3]);
        }
    }
}