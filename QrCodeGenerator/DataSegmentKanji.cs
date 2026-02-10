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
    /// Data segment using Kanji mode.
    /// </summary>
    internal class DataSegmentKanji : DataSegment
    {
        internal DataSegmentKanji(ArraySegment<byte> bytes)
            : base(DataSegmentMode.Kanji, GetKanjiBitLength(bytes.Count))
        {
            Debug.Assert(bytes.Array != null);
            Debug.Assert(bytes.Count % 2 == 0);
            DataBytes = bytes;
        }
        
        /// <summary>
        /// Indicates if the given pair of bytes can be encoded in the Kanji mode.
        /// <para>
        /// Kanji mode can be used for Shift JIS double-byte codes,
        /// or for any other byte sequence in the range of the Shift JIS double-byte codes.
        /// </para>
        /// </summary>
        /// <param name="b1">Byte 1 to encode</param>
        /// <param name="b2">Byte 2 to encode</param>
        /// <returns><c>true</c> if the pair of bytes can be encoded in Kanji mode, <c>false</c> otherwise.</returns>
        internal static bool IsShiftJisDoubleByte(byte b1, byte b2)
        {
            if (b1 < 0x81 || (b1 > 0x9f && b1 < 0xe0) || b1 > 0xeb)
            {
                return false;
            }

            return b2 >= 0x40 && b2 <= 0xfc;
        }

        internal static int GetKanjiBitLength(int length)
        {
            return length * 13 / 2;
        }

        internal static int GetKanjiByteCount(int bitLength)
        {
            return bitLength / 13 * 2;
        }

        internal override void WriteToBitStream(BitStream bitStream)
        {
            var bytes = DataBytes;
            Debug.Assert(bytes.Array != null);
            
            for (var i = 0; i < bytes.Count; i += 2)
            {
                // Each double byte character is encoded into 13 bits
                var kanjiByte1 = bytes.Array[bytes.Offset + i];
                var kanjiByte2 = bytes.Array[bytes.Offset + i + 1];
                Debug.Assert(IsShiftJisDoubleByte(kanjiByte1, kanjiByte2));
                
                bitStream.AppendBits(EncodeShiftJisCode(kanjiByte1 * 256 + kanjiByte2), 13);
            }
        }

        /// <summary>
        /// Encode a double-byte Shift JIS code into a 13-bit integer.
        /// </summary>
        /// <param name="shiftJisCode">Shift JIS code</param>
        /// <returns>Encoded number</returns>
        internal static uint EncodeShiftJisCode(int shiftJisCode)
        {
            int code;
            if (shiftJisCode >= 0x8140 && shiftJisCode <= 0x9ffc)
            {
                code = shiftJisCode - 0x8140;
            }
            else if (shiftJisCode >= 0xe040 && shiftJisCode <= 0xebbf)
            {
                code = shiftJisCode - 0xc140;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(shiftJisCode), shiftJisCode, "Invalid Shift JIS code: only two-byte codes can be encoded in Kanji mode.");
            }
            return (uint)((code >> 8) * 0xc0 + (code & 0xff));
        }
    }
}

