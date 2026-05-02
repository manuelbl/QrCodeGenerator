/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using Microsoft.Testing.Platform.Extensions.Messages;
using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class DataSegmentAlphanumericTest : DataSegmentTestBase
    {
        [Fact]
        public void AlphanumericEncoding()
        {
            for (int ch = '0'; ch <= '9'; ch += 1)
            {
                Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)ch));
                Assert.Equal((uint)(ch - '0'), DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)ch));
            }
            for (int ch = 'A'; ch <= 'Z'; ch += 1)
            {
                Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)ch));
                Assert.Equal((uint)(ch - 'A' + 10), DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)ch));
            }
        
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)' '));
            Assert.Equal(36u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)' '));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'$'));
            Assert.Equal(37u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'$'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'%'));
            Assert.Equal(38u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'%'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'*'));
            Assert.Equal(39u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'*'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'+'));
            Assert.Equal(40u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'+'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'-'));
            Assert.Equal(41u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'-'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'.'));
            Assert.Equal(42u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'.'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)'/'));
            Assert.Equal(43u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)'/'));
            Assert.True(DataSegmentAlphanumeric.IsAlphanumeric((byte)':'));
            Assert.Equal(44u, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)':'));
        
            var numChars = 0;
            for (var ch = 0; ch < 256; ch += 1)
            {
                if (DataSegmentAlphanumeric.IsAlphanumeric((byte)ch))
                {
                    numChars += 1;
                    Assert.NotEqual(0xffu, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)ch));
                }
                else
                {
                    Assert.Equal(0xffu, DataSegmentAlphanumeric.EncodeAlphanumericByte((byte)ch));
                }
            }
        
            Assert.Equal(45, numChars);
        }
    
        [Theory]
        [InlineData("A", new byte[] { 0b0010_1000 })]
        [InlineData("AZ", new byte[] { 0b0011_1100, 0b1010_0000 })]
        [InlineData("AZ:", new byte[] { 0b0011_1100, 0b1011_0110, 0b0000_0000 })]
        [InlineData("AZ%3", new byte[] { 0b0011_1100, 0b1011_1010, 0b1100_0100 })]
        public void EncodeAlphanumeric(string text, byte[] expectedData)
        {
            EncodeAndCompare(text, expectedData, data => new DataSegmentAlphanumeric(new ArraySegment<byte>(data)));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("7")]
        [InlineData("B3")]
        [InlineData("TH")]
        [InlineData("QR3")]
        [InlineData("7BV")]
        [InlineData("QTN3")]
        [InlineData("PO1FF")]
        [InlineData("WV8XH3")]
        [InlineData("3082201")]
        [InlineData("MQDBXPPL")]
        [InlineData("$%*+-./:")]
        public void EncodeDecodeAlphanumeric(string text)
        {
            EncodeDecode(text, t => new DataSegmentAlphanumeric(new ArraySegment<byte>(t)), DecodeAlphanumeric);
        }
    
        private static byte[] DecodeAlphanumeric(BitStream bitStream)
        {
            var bitLength = bitStream.Length;
            var numChars = bitLength / 11 * 2 + bitLength % 11 / 6;
            var result = new byte[numChars];
            var index = 0;
            
            for (var offset = 0; offset + 10 < bitLength; offset += 11)
            {
                // process groups of 2 characters
                var group = bitStream.ExtractBits(offset, 11);
                result[index] = DataSegmentAlphanumeric.DecodeAlphanumericByte(group / 45);
                result[index + 1] = DataSegmentAlphanumeric.DecodeAlphanumericByte(group % 45);
                index += 2;
            }

            if (numChars % 2 == 1)
            {
                result[numChars - 1] = DataSegmentAlphanumeric.DecodeAlphanumericByte(bitStream.ExtractBits(bitLength - 6, 6));
            }

            return result;
        }
    
        [Theory, CombinatorialData]
        public void BitLength([CombinatorialRange(0, 100)] int bitLength)
        {
            var byteCount = DataSegmentAlphanumeric.GetAlphanumericByteCount(bitLength);
            var bitLengthLower = DataSegmentAlphanumeric.GetAlphanumericBitLength(byteCount);
            var bitLengthUpper = DataSegmentAlphanumeric.GetAlphanumericBitLength(byteCount + 1);
            Assert.InRange(bitLengthLower, bitLengthLower, bitLengthUpper);
        }
    }
}
