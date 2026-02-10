/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using STH1123.ReedSolomon;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class ReedSolomonTest
    {
        [Theory]
        [InlineData(7, 19)]
        [InlineData(10, 16)]
        [InlineData(10, 34)]
        [InlineData(13, 13)]
        [InlineData(15, 55)]
        [InlineData(16, 27)]
        [InlineData(16, 28)]
        [InlineData(18, 14)]
        [InlineData(18, 15)]
        [InlineData(18, 16)]
        [InlineData(18, 17)]
        [InlineData(18, 31)]
        [InlineData(18, 32)]
        [InlineData(18, 68)]
        [InlineData(18, 69)]
        [InlineData(20, 16)]
        [InlineData(20, 17)]
        [InlineData(20, 78)]
        [InlineData(20, 80)]
        [InlineData(20, 81)]
        [InlineData(22, 18)]
        [InlineData(22, 19)]
        [InlineData(22, 22)]
        [InlineData(22, 36)]
        [InlineData(22, 37)]
        [InlineData(22, 38)]
        [InlineData(22, 39)]
        [InlineData(22, 87)]
        [InlineData(22, 88)]
        [InlineData(24, 19)]
        [InlineData(24, 20)]
        [InlineData(24, 21)]
        [InlineData(24, 40)]
        [InlineData(24, 41)]
        [InlineData(24, 42)]
        [InlineData(24, 43)]
        [InlineData(24, 92)]
        [InlineData(24, 93)]
        [InlineData(24, 97)]
        [InlineData(24, 98)]
        [InlineData(24, 99)]
        [InlineData(26, 20)]
        [InlineData(26, 21)]
        [InlineData(26, 22)]
        [InlineData(26, 24)]
        [InlineData(26, 41)]
        [InlineData(26, 42)]
        [InlineData(26, 43)]
        [InlineData(26, 44)]
        [InlineData(26, 45)]
        [InlineData(26, 106)]
        [InlineData(26, 107)]
        [InlineData(26, 108)]
        [InlineData(28, 22)]
        [InlineData(28, 23)]
        [InlineData(28, 45)]
        [InlineData(28, 46)]
        [InlineData(28, 47)]
        [InlineData(28, 48)]
        [InlineData(28, 107)]
        [InlineData(28, 108)]
        [InlineData(28, 111)]
        [InlineData(28, 112)]
        [InlineData(28, 113)]
        [InlineData(28, 114)]
        [InlineData(28, 115)]
        [InlineData(28, 116)]
        [InlineData(28, 117)]
        [InlineData(30, 23)]
        [InlineData(30, 24)]
        [InlineData(30, 25)]
        [InlineData(30, 50)]
        [InlineData(30, 51)]
        [InlineData(30, 115)]
        [InlineData(30, 116)]
        [InlineData(30, 117)]
        [InlineData(30, 118)]
        [InlineData(30, 119)]
        [InlineData(30, 120)]
        [InlineData(30, 121)]
        [InlineData(30, 122)]
        [InlineData(30, 123)]
        public void CompareWithAlternative(int degree, int dataLength)
        {
            var payloadData = RandomData.MakeRandomBytes(dataLength, 7392201);

            var reedSolomon = ReedSolomon.GeneratorForCapacity(degree);
            var eccData = reedSolomon.ComputeErrorCorrection(new ArraySegment<byte>(payloadData));

            var alternativeEcc = ComputeAlternative(degree, payloadData);
        
            Assert.Equal<byte[]>(alternativeEcc, eccData);
        }

        private static byte[] ComputeAlternative(int eccLength, byte[] data)
        {
            var field = new GenericGF(0b100011101, 256, 0);
            var encoder = new ReedSolomonEncoder(field);
        
            var buffer = new int[data.Length + eccLength];
            for (var i = 0; i < data.Length; i += 1)
            {
                buffer[i] = data[i];
            }
            encoder.Encode(buffer, eccLength);
        
            var remainder = new byte[eccLength];
            for (var i = 0; i < eccLength; i += 1)
            {
                remainder[i] = (byte) buffer[data.Length + i];
            }
        
            return remainder;
        }

        [Theory]
        [InlineData(-123)]
        [InlineData(0)]
        [InlineData(256)]
        [InlineData(300)]
        public void EncoderWithInvalidCapacity(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ReedSolomon.GeneratorForCapacity(capacity));
        }
    }
}