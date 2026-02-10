/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Data segment using byte mode.
    /// </summary>
    internal class DataSegmentByte : DataSegment
    {
        internal DataSegmentByte(ArraySegment<byte> bytes)
            : base(DataSegmentMode.Binary, GetByteBitLength(bytes.Count))
        {
            Debug.Assert(bytes.Array != null);
            DataBytes = bytes;
        }

        internal static int GetByteBitLength(int length)
        {
            return length * 8;
        }
        
        internal static int GetByteByteCount(int bitLength)
        {
            return bitLength / 8;
        }

        internal override void WriteToBitStream(BitStream bitStream)
        {
            foreach (var b in DataBytes)
            {
                bitStream.AppendBits(b, 8);
            }
        }
    }
}

