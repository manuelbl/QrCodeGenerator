/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// The per-mode rules of a <see cref="DataSegmentMode"/>: the mode indicator, the
    /// count-indicator widths, and — for the data modes (numeric, alphanumeric, Kanji,
    /// binary) — the bit-length and byte-count formulas and the segment factory.
    /// <para>
    /// One immutable instance exists per mode, looked up by mode value via
    /// <see cref="For"/>. The public <see cref="DataSegmentMode"/> enum stays a plain
    /// enum; the behavior keyed off it lives here instead of in switch statements
    /// scattered across <see cref="DataSegment"/>. Each data mode's length formula stays
    /// next to its writer in the segment subtype; this descriptor only wires to those
    /// statics.
    /// </para>
    /// <para>
    /// The data-mode operations (<see cref="EncodedBitLength"/>, <see cref="ByteCount"/>,
    /// <see cref="Create"/>) are <c>null</c> for the non-data modes (ECI, structured
    /// append), which carry no payload count indicator either.
    /// </para>
    /// </summary>
    internal sealed class DataSegmentModeInfo
    {
        private DataSegmentModeInfo(
            int modeIndicator,
            int[] countIndicatorWidths,
            Func<int, int> encodedBitLength = null,
            Func<int, int> byteCount = null,
            Func<ArraySegment<byte>, DataSegment> create = null)
        {
            ModeIndicator = modeIndicator;
            _countIndicatorWidths = countIndicatorWidths;
            EncodedBitLength = encodedBitLength;
            ByteCount = byteCount;
            Create = create;
        }

        /// <summary>The 4-bit mode indicator symbol.</summary>
        internal int ModeIndicator { get; }

        // The character count indicator width (in bits) for each version group
        // (1–9, 10–26, 27–40), or null if the mode has no count indicator.
        private readonly int[] _countIndicatorWidths;

        /// <summary>
        /// Computes the encoded payload length (in bits, without header) for the given
        /// number of bytes. <c>null</c> for non-data modes.
        /// </summary>
        internal Func<int, int> EncodedBitLength { get; }

        /// <summary>
        /// Computes the number of bytes that fit in the given encoded bit length.
        /// <c>null</c> for non-data modes.
        /// </summary>
        internal Func<int, int> ByteCount { get; }

        /// <summary>
        /// Creates a data segment of this mode for the given bytes. <c>null</c> for
        /// non-data modes.
        /// </summary>
        internal Func<ArraySegment<byte>, DataSegment> Create { get; }

        /// <summary>Indicates whether a character count indicator is written for this mode.</summary>
        internal bool HasCountIndicator => _countIndicatorWidths != null;

        /// <summary>
        /// Gets the character count indicator length (in bits) for the given version,
        /// or 0 if this mode has no count indicator.
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The count indicator length, in bits.</returns>
        internal int GetCountIndicatorLength(int version)
        {
            // Version groups are 1–9, 10–26, 27–40, mapping to indices 0, 1, 2.
            return HasCountIndicator ? _countIndicatorWidths[(version + 7) / 17] : 0;
        }

        /// <summary>
        /// Gets the header length (mode indicator and count indicator) in bits.
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The header length, in bits.</returns>
        internal int GetHeaderLength(int version)
        {
            return 4 + GetCountIndicatorLength(version);
        }

        /// <summary>
        /// Gets the descriptor for the given data segment mode.
        /// </summary>
        /// <param name="mode">The data segment mode.</param>
        /// <returns>The descriptor.</returns>
        internal static DataSegmentModeInfo For(DataSegmentMode mode)
        {
            return ByMode[(int)mode];
        }

        // Indexed by (int)DataSegmentMode (1–6); index 0 is unused.
        private static readonly DataSegmentModeInfo[] ByMode = BuildTable();

        private static DataSegmentModeInfo[] BuildTable()
        {
            var table = new DataSegmentModeInfo[7];
            // Count indicator widths and mode indicators per ISO/IEC 18004 (Tables 2 and 3).
            table[(int)DataSegmentMode.Numeric] = new DataSegmentModeInfo(
                1, new[] { 10, 12, 14 },
                DataSegmentNumeric.GetNumericBitLength, DataSegmentNumeric.GetNumericByteCount,
                bytes => new DataSegmentNumeric(bytes));
            table[(int)DataSegmentMode.Alphanumeric] = new DataSegmentModeInfo(
                2, new[] { 9, 11, 13 },
                DataSegmentAlphanumeric.GetAlphanumericBitLength, DataSegmentAlphanumeric.GetAlphanumericByteCount,
                bytes => new DataSegmentAlphanumeric(bytes));
            table[(int)DataSegmentMode.Kanji] = new DataSegmentModeInfo(
                8, new[] { 8, 10, 12 },
                DataSegmentKanji.GetKanjiBitLength, DataSegmentKanji.GetKanjiByteCount,
                bytes => new DataSegmentKanji(bytes));
            table[(int)DataSegmentMode.Binary] = new DataSegmentModeInfo(
                4, new[] { 8, 16, 16 },
                DataSegmentByte.GetByteBitLength, DataSegmentByte.GetByteByteCount,
                bytes => new DataSegmentByte(bytes));
            table[(int)DataSegmentMode.ECI] = new DataSegmentModeInfo(
                7, null);
            table[(int)DataSegmentMode.StructuredAppend] = new DataSegmentModeInfo(
                3, null);
            return table;
        }
    }
}
