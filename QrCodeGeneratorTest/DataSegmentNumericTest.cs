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
    public class DataSegmentNumericTest : DataSegmentTestBase
    {
        [Fact]
        public void NumericEncoding()
        {
            for (var b = 0; b < 256; b += 1)
            {
                Assert.Equal(b >= '0' && b <= '9', DataSegmentNumeric.IsNumeric((byte)b));
            }
        }

        [Theory]
        [InlineData("1", new byte[] { 0b0001_0000 })]
        [InlineData("35", new byte[] { 0b0100_0110 })]
        [InlineData("999", new byte[] { 0b1111_1001, 0b1100_0000 })]
        [InlineData("987", new byte[] { 0b1111_0110, 0b1100_0000 })]
        [InlineData("9871", new byte[] { 0b1111_0110, 0b1100_0100 })]
        [InlineData("4032", new byte[] { 0b0110_0100, 0b1100_1000 })]
        [InlineData("40327", new byte[] { 0b0110_0100, 0b1100_1101, 0b1000_0000 })]
        public void EncodeNumeric(string text, byte[] expectedData)
        {
            EncodeAndCompare(text, expectedData, data => new DataSegmentNumeric(new ArraySegment<byte>(data)));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("7")]
        [InlineData("23")]
        [InlineData("51")]
        [InlineData("293")]
        [InlineData("700")]
        [InlineData("1730")]
        [InlineData("2293")]
        [InlineData("70906")]
        [InlineData("3082201")]
        [InlineData("00000000")]
        public void EncodeDecodeNumeric(string text)
        {
            EncodeDecode(text, t => new DataSegmentNumeric(new ArraySegment<byte>(t)), DecodeNumeric);
        }
    
        private static byte[] DecodeNumeric(BitStream bitStream)
        {
            var bitLength = bitStream.Length;
            var numDigits = bitLength / 10 * 3 + (bitLength % 10 + 1) / 4;
            var result = new byte[numDigits];
            var index = 0;
            
            for (var offset = 0; offset < bitLength; offset += 10)
            {
                // process groups of 3 digits
                var group = bitStream.ExtractBits(offset, Math.Min(10, bitLength - offset));
                var numDigitsInGroup = Math.Min(numDigits - index, 3);
                for (var i = 0; i < numDigitsInGroup; i += 1)
                {
                    result[index + numDigitsInGroup - i - 1] = (byte)(group % 10 + 48);
                    group /= 10;
                }
                index += numDigitsInGroup;
            }

            return result;
        }

        [Theory, CombinatorialData]
        public void BitLength([CombinatorialRange(0, 100)] int bitLength)
        {
            var byteCount = DataSegmentNumeric.GetNumericByteCount(bitLength);
            var bitLengthLower = DataSegmentNumeric.GetNumericBitLength(byteCount);
            var bitLengthUpper = DataSegmentNumeric.GetNumericBitLength(byteCount + 1);
            Assert.InRange(bitLengthLower, bitLengthLower, bitLengthUpper);
        }
    }
}
