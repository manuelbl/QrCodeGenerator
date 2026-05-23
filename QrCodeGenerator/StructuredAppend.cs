/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    internal static class StructuredAppend
    {
        internal static List<List<DataSegment>> BuildFromText(string text, int version, QrCode.Ecc ecl)
        {
            Objects.RequireNonNull(text, nameof(text));
            
            byte[] data;
            ECI eci;
            bool isUtf8;
            
            // try ISO-8859-1 encoding
            try
            {
                data = ECI.Latin1.GetEncoding().GetBytes(text);
                eci = ECI.None;
                isUtf8 = false;
            }
            catch (EncoderFallbackException)
            {
                // Cannot encode as ISO-8859-1 without loss, use UTF-8
                data = Encoding.UTF8.GetBytes(text);
                eci = ECI.UTF8;
                isUtf8 = true;
            }

            return BuildSegments(data, version, ecl, eci, isUtf8);
        }
        
        internal static List<List<DataSegment>> BuildSegments(byte[] data, int version, QrCode.Ecc ecl, ECI eci, bool isUtf8)
        {
            Debug.Assert(!eci.Equals(ECI.Automatic));

            var qrCodes = Split(data, version, ecl, eci, isUtf8);
            var parity = CalculateParity(data);
            var eciSegment = !Equals(eci, ECI.None) ? new DataSegmentEci(eci) : null;

            var numQrCodes = qrCodes.Count;
            for (var i = 0; i < numQrCodes; i += 1)
            {
                var segments = qrCodes[i];
                segments.Insert(0, new DataSegmentStructuredAppend(i + 1, numQrCodes, parity));
                if (eciSegment != null)
                {
                    segments.Insert(1, eciSegment);
                }
            }

            return qrCodes;
        }
        
        private static List<List<DataSegment>> Split(byte[] data, int version, QrCode.Ecc ecl, ECI eci, bool isUtf8)
        {
            var structuredAppendLength = new DataSegmentStructuredAppend(1, 10, 0).GetTotalLength(40);
            var eciLength = !eci.Equals(ECI.None) ? new DataSegmentEci(eci).GetTotalLength(40) : 0;
            var dataCapacity = QrCodeParameters.GetCodewordDataCapacity(version, (int)ecl) * 8
                               - structuredAppendLength - eciLength;

            var result = Split(data, version, dataCapacity, isUtf8);

            if (result.Count > 16)
            {
                throw new DataTooLongException("The text is too long to fit into 16 QR codes");
            }

            return result;
        }

        private static List<List<DataSegment>> Split(byte[] data, int version, int dataCapacity, bool isUtf8)
        {
            var result = new List<List<DataSegment>>();
            
            var segments = SegmentCompaction.BuildSegments(new ArraySegment<byte>(data), version);
            var qrCodeStartIndex = 0;
            var bitLength = 0;
            for (var index = 0; index < segments.Count; index += 1)
            {
                var segment = segments[index];
                var segmentLength = segment.GetTotalLength(version);
                if (bitLength + segmentLength <= dataCapacity)
                {
                    bitLength += segmentLength;
                    continue;
                }
                
                // segment does not fully fit into QR code anymore: split and start a new one
                var remainingBits = dataCapacity - bitLength;
                var numBytes = DataSegment.GetByteCount(segment.Mode,
                    remainingBits - DataSegment.GetHeaderLength(segment.Mode, version));
                
                // find character boundary
                if (isUtf8)
                {
                    var dataBytes = segment.DataBytes;
                    while (numBytes > 0 && (dataBytes.At(numBytes) & 0xC0) == 0x80)
                    {
                        numBytes -= 1;
                    }
                }

                if (numBytes > 0)
                {
                    var splitSegments = SplitSegment(segment, numBytes);
                    segments[index] = splitSegments.Item1;
                    result.Add(segments.GetRange(qrCodeStartIndex, index - qrCodeStartIndex + 1));
                    segments[index] = splitSegments.Item2;
                }
                else
                {
                    result.Add(segments.GetRange(qrCodeStartIndex, index - qrCodeStartIndex));
                }
                
                qrCodeStartIndex = index;
                bitLength = 0;
            }
            
            // add remainder
            result.Add(segments.GetRange(qrCodeStartIndex, segments.Count - qrCodeStartIndex));
            return result;
        }

        private static Tuple<DataSegment, DataSegment> SplitSegment(DataSegment segment, int numBytes)
        {
            return new Tuple<DataSegment, DataSegment>(
                DataSegment.MakeSegment(segment.Mode, segment.DataBytes.MakeSlice(0, numBytes)),
                DataSegment.MakeSegment(segment.Mode, segment.DataBytes.MakeSlice(numBytes, segment.DataLength - numBytes))
            );
        }
        
        private static byte CalculateParity(byte[] data)
        {
            return data.Aggregate<byte, byte>(0, (current, value) => (byte)(current ^ (byte)(value >> 8)));
        }

    }
}