/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Data segment using alphanumeric mode.
    /// </summary>
    internal class DataSegmentAlphanumeric : DataSegment
    {
        internal DataSegmentAlphanumeric(ArraySegment<byte> bytes)
            : base(DataSegmentMode.Alphanumeric, GetAlphanumericBitLength(bytes.Count))
        {
            Debug.Assert(bytes.Array != null);
            DataBytes = bytes;
        }
        
        /// <summary>
        /// Indicates if the given byte can be encoded in the Alphanumeric mode.
        /// </summary>
        /// <param name="b">Byte to encode</param>
        /// <returns><c>true</c> if the byte can be encoded in Alphanumeric mode, <c>false</c> otherwise.</returns>
        internal static bool IsAlphanumeric(byte b)
        {
            return (b >= 'A' && b <= 'Z')
                   || (b >= '-' && b <= ':') // includes digits
                   || b == ' '
                   || b == '$'
                   || b == '%'
                   || b == '*'
                   || b == '+';
        }

        internal static int GetAlphanumericBitLength(int length)
        {
            return (length * 11 + 1) / 2;
        }

        internal static int GetAlphanumericByteCount(int bitLength)
        {
            return bitLength * 2 / 11;
        }

        internal override void WriteToBitStream(BitStream bitStream)
        {
            Debug.Assert(DataBytes.Array != null);
            for (var i = 0; i + 1 < DataBytes.Count; i += 2)
            {
                // 2 letters are encoded into 11 bits
                var letter1Byte = DataBytes.Array[DataBytes.Offset + i];
                var letter2Byte = DataBytes.Array[DataBytes.Offset + i + 1];
                Debug.Assert(IsAlphanumeric(letter1Byte));
                Debug.Assert(IsAlphanumeric(letter2Byte));
                
                bitStream.AppendBits(EncodeAlphanumericByte(letter1Byte) * 45 + EncodeAlphanumericByte(letter2Byte), 11);
            }

            if (DataBytes.Count % 2 == 1)
            {
                var letterByte = DataBytes.Array[DataBytes.Offset + DataBytes.Count - 1];
                Debug.Assert(IsAlphanumeric(letterByte));
                bitStream.AppendBits(EncodeAlphanumericByte(letterByte), 6);
            }
        }

        internal static uint EncodeAlphanumericByte(byte b)
        {
            if (b < 0x20 || b > 0x5A)
            {
                return 0xff;
            }
            return AlphanumericEncoding[b - 0x20];
        }

        internal static byte DecodeAlphanumericByte(uint value)
        {
            var index = Array.IndexOf(AlphanumericEncoding, (byte)value, 0);
            return (byte)(index + 0x20);
        }

        [SuppressMessage("csharpsquid", "S125")]
        private static readonly byte[] AlphanumericEncoding =
        {
            0x24,  // 20h SPACE
            0xFF,  // 21h !
            0xFF,  // 22h "
            0xFF,  // 23h #
            0x25,  // 24h $
            0x26,  // 25h %
            0xFF,  // 26h &
            0xFF,  // 27h '
            0xFF,  // 28h (
            0xFF,  // 29h )
            0x27,  // 2Ah *
            0x28,  // 2Bh +
            0xFF,  // 2Ch ,
            0x29,  // 2Dh -
            0x2A,  // 2Eh .
            0x2B,  // 2Fh /
            0x00,  // 30h 0
            0x01,  // 31h 1
            0x02,  // 32h 2
            0x03,  // 33h 3
            0x04,  // 34h 4
            0x05,  // 35h 5
            0x06,  // 36h 6
            0x07,  // 37h 7
            0x08,  // 38h 8
            0x09,  // 39h 9
            0x2C,  // 3Ah :
            0xFF,  // 3Bh ;
            0xFF,  // 3Ch <
            0xFF,  // 3Dh =
            0xFF,  // 3Eh >
            0xFF,  // 3Fh ?
            0xFF,  // 40h @
            0x0A,  // 41h A
            0x0B,  // 42h B
            0x0C,  // 43h C
            0x0D,  // 44h D
            0x0E,  // 45h E
            0x0F,  // 46h F
            0x10,  // 47h G
            0x11,  // 48h H
            0x12,  // 49h I
            0x13,  // 4Ah J
            0x14,  // 4Bh K
            0x15,  // 4Ch L
            0x16,  // 4Dh M
            0x17,  // 4Eh N
            0x18,  // 4Fh O
            0x19,  // 50h P
            0x1A,  // 51h Q
            0x1B,  // 52h R
            0x1C,  // 53h S
            0x1D,  // 54h T
            0x1E,  // 55h U
            0x1F,  // 56h V
            0x20,  // 57h W
            0x21,  // 58h X
            0x22,  // 59h Y
            0x23   // 5Ah Z
        };
    }
}

