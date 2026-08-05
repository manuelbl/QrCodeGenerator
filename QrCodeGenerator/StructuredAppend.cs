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
        /// <summary>
        /// The version used to compact the balanced segments once and reuse the result.
        /// <para>
        /// The compaction depends only marginally on the version (see
        /// <see cref="SegmentCompaction.BuildSegments"/>). Version 20 lies in the middle
        /// count-indicator band (10&#x2013;26), so it is exact for that band and a close
        /// approximation for the others.
        /// </para>
        /// </summary>
        private const int CompactionVersion = 20;


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
            Trace.Assert(!eci.Equals(ECI.Automatic));

            var qrCodes = Split(data, version, ecl, eci, isUtf8);
            AddStructuredAppendHeaders(qrCodes, data, eci);
            return qrCodes;
        }

        /// <summary>
        /// Builds evenly balanced structured-append segments from the specified text.
        /// <para>
        /// See <see cref="BuildBalancedSegments"/> for the balancing algorithm.
        /// </para>
        /// </summary>
        internal static (List<List<DataSegment>> QrCodes, int Version) BuildBalancedFromText(
            string text, int minVersion, int maxVersion, QrCode.Ecc ecl)
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

            return BuildBalancedSegments(data, minVersion, maxVersion, ecl, eci, isUtf8);
        }

        /// <summary>
        /// Builds evenly balanced structured-append segments from the specified data.
        /// <para>
        /// The result uses the least possible number of QR codes (computed at
        /// <paramref name="maxVersion"/>), the smallest shared version at least
        /// <paramref name="minVersion"/> that still fits that number of codes, and
        /// distributes the payload as evenly as possible by minimizing the fill of
        /// the fullest code. All returned codes use the same version.
        /// </para>
        /// </summary>
        internal static (List<List<DataSegment>> QrCodes, int Version) BuildBalancedSegments(
            byte[] data, int minVersion, int maxVersion, QrCode.Ecc ecl, ECI eci, bool isUtf8)
        {
            Trace.Assert(!eci.Equals(ECI.Automatic));

            // The segment compaction depends only marginally on the version (it merely affects the
            // count-indicator widths). Computing it once at a representative version and reusing the
            // result avoids recomputing this non-trivial step for every fit check below.
            var segments = SegmentCompaction.BuildSegments(new ArraySegment<byte>(data), CompactionVersion);

            // Step 1: least number of QR codes, achieved at the largest (max) version.
            var maxCapacity = FullCapacity(maxVersion, ecl, eci);
            var numQrCodes = Split(segments, maxVersion, maxCapacity, isUtf8).Count;
            if (numQrCodes > 16 || !FitsInCodes(segments, maxVersion, maxCapacity, isUtf8, numQrCodes))
            {
                throw new DataTooLongException("The text is too long to fit into 16 QR codes");
            }

            // Step 2: smallest shared version (>= minVersion) that still fits into that number of codes.
            var version = maxVersion;
            for (var v = minVersion; v <= maxVersion; v += 1)
            {
                if (FitsInCodes(segments, v, FullCapacity(v, ecl, eci), isUtf8, numQrCodes))
                {
                    version = v;
                    break;
                }
            }

            // Step 3: balance by finding the smallest per-code capacity that still fits into
            // the number of codes. This minimizes the fill of the fullest code.
            var lo = 1;
            var hi = FullCapacity(version, ecl, eci);
            while (lo < hi)
            {
                var mid = lo + (hi - lo) / 2;
                if (FitsInCodes(segments, version, mid, isUtf8, numQrCodes))
                {
                    hi = mid;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            var qrCodes = Split(segments, version, lo, isUtf8);
            AddStructuredAppendHeaders(qrCodes, data, eci);
            return (qrCodes, version);
        }

        /// <summary>
        /// Determines whether the data can be greedily split into at most <paramref name="maxCodes"/>
        /// codes at the given version such that every code stays within <paramref name="cap"/> bits.
        /// <para>
        /// The per-code check is required because the greedy split may leave a trailing chunk that
        /// exceeds the capacity; counting codes alone is not sufficient.
        /// </para>
        /// </summary>
        private static bool FitsInCodes(List<DataSegment> segments, int version, int cap, bool isUtf8, int maxCodes)
        {
            var qrCodes = Split(segments, version, cap, isUtf8);
            return qrCodes.Count <= maxCodes
                   && qrCodes.All(code => DataSegment.GetBitLength(code, version) <= cap);
        }

        private static void AddStructuredAppendHeaders(List<List<DataSegment>> qrCodes, byte[] data, ECI eci)
        {
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
        }

        /// <summary>
        /// Gets the per-code payload capacity in bits, i.e. the data capacity for the
        /// given version and error correction level minus the structured-append and
        /// (optional) ECI header overhead that every code carries.
        /// </summary>
        private static int FullCapacity(int version, QrCode.Ecc ecl, ECI eci)
        {
            var structuredAppendLength = new DataSegmentStructuredAppend(1, 10, 0).GetTotalLength(40);
            var eciLength = !eci.Equals(ECI.None) ? new DataSegmentEci(eci).GetTotalLength(40) : 0;
            return QrCodeParameters.GetCodewordDataCapacity(version, (int)ecl) * 8
                   - structuredAppendLength - eciLength;
        }

        private static List<List<DataSegment>> Split(byte[] data, int version, QrCode.Ecc ecl, ECI eci, bool isUtf8)
        {
            var segments = SegmentCompaction.BuildSegments(new ArraySegment<byte>(data), version);
            var result = Split(segments, version, FullCapacity(version, ecl, eci), isUtf8);

            return result.Count <= 16
                ? result
                : throw new DataTooLongException("The text is too long to fit into 16 QR codes");
        }

        /// <summary>
        /// Greedily distributes the given (already compacted) segments across QR codes, filling each
        /// code up to <paramref name="dataCapacity"/> bits and splitting segments at character
        /// boundaries where needed.
        /// <para>
        /// The segments are only read, never modified, so the same list may be reused across many
        /// calls with different versions and capacities.
        /// </para>
        /// </summary>
        private static List<List<DataSegment>> Split(List<DataSegment> segments, int version, int dataCapacity, bool isUtf8)
        {
            var result = new List<List<DataSegment>>();

            var current = new List<DataSegment>();
            var bitLength = 0;

            foreach (var wholeSegment in segments)
            {
                var segment = wholeSegment;
                while (true)
                {
                    var segmentLength = segment.GetTotalLength(version);
                    if (bitLength + segmentLength <= dataCapacity)
                    {
                        current.Add(segment);
                        bitLength += segmentLength;
                        break;
                    }

                    // segment does not fully fit into the current QR code: split off what fits
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

                    // If nothing more fits into a code that already holds data, close it and retry
                    // the (unchanged) segment in a fresh code.
                    if (numBytes <= 0 && current.Count > 0)
                    {
                        result.Add(current);
                        current = new List<DataSegment>();
                        bitLength = 0;
                        continue;
                    }

                    // Guarantee progress even when the capacity is too small for a single character
                    // (a degenerate case that the callers reject via the per-code capacity check).
                    if (numBytes <= 0)
                    {
                        numBytes = 1;
                    }

                    var splitSegments = SplitSegment(segment, numBytes);
                    current.Add(splitSegments.Item1);
                    result.Add(current);
                    current = new List<DataSegment>();
                    bitLength = 0;

                    // continue splitting the remainder, which may fill further codes
                    segment = splitSegments.Item2;
                }
            }

            // add remainder
            result.Add(current);
            return result;
        }

        private static Tuple<DataSegment, DataSegment> SplitSegment(DataSegment segment, int numBytes)
        {
            return new Tuple<DataSegment, DataSegment>(
                DataSegment.MakeSegment(segment.Mode, segment.DataBytes.MakeSlice(0, numBytes)),
                DataSegment.MakeSegment(segment.Mode, segment.DataBytes.MakeSlice(numBytes, segment.DataLength - numBytes))
            );
        }
        
        /// <summary>
        /// Calculates the parity of the data, i.e. the XOR of all its bytes.
        /// <para>
        /// Every QR code of the sequence carries it, so that a scanner can tell whether the codes
        /// it has collected belong together (see ISO/IEC 18004, section 8.4.1).
        /// </para>
        /// </summary>
        private static byte CalculateParity(byte[] data)
        {
            return data.Aggregate<byte, byte>(0, (current, value) => (byte)(current ^ value));
        }

    }
}