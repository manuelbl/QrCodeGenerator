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
    /// Data segment using numeric mode.
    /// </summary>
    internal class DataSegmentNumeric : DataSegment
    {
        internal DataSegmentNumeric(ArraySegment<byte> bytes)
            : base(DataSegmentMode.Numeric, GetNumericBitLength(bytes.Count))
        {
            Trace.Assert(bytes.Array != null);
            DataBytes = bytes;
        }
        
        /// <summary>
        /// Indicates if the given byte can be encoded in the Numeric mode.
        /// </summary>
        /// <param name="b">Byte to encode</param>
        /// <returns><c>true</c> if the byte can be encoded in Numeric mode, <c>false</c> otherwise.</returns>
        internal static bool IsNumeric(byte b)
        {
            return b >= '0' && b <= '9';
        }

        internal static int GetNumericBitLength(int length)
        {
            return (length * 10 + 2) / 3;
        }

        internal static int GetNumericByteCount(int bitLength)
        {
            return bitLength * 3 / 10;
        }
        
        internal override void WriteToBitStream(BitStream bitStream)
        {
            var bytes = DataBytes;
            Trace.Assert(bytes.Array != null);
            var i = 0;
            while (i < bytes.Count)
            {
                // 3 characters are encoded into 10 bits
                var n = Math.Min(bytes.Count - i, 3);
                var value = 0u;
                for (var j = i; j < i + n; j += 1)
                {
                    var digitByte = bytes.Array[bytes.Offset + j];
                    Debug.Assert(IsNumeric(digitByte));
                    value = value * 10 + digitByte - 48;
                }
                bitStream.AppendBits(value, n * 3 + 1);
                i += n;
            }
        }
    }
}

