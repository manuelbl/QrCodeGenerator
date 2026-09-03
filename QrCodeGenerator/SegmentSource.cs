/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Data prepared for segment compaction.
    /// <para>
    /// The version-independent part of the compaction (the blocks of bytes with the same
    /// cheapest mode) is done once; the segments for a particular version are derived
    /// from it as often as needed, e.g. once per version group.
    /// </para>
    /// </summary>
    internal sealed class SegmentSource
    {
        private readonly ArraySegment<byte> _data;
        private readonly ECI _eci;
        private readonly SegmentCompaction.Block[] _blocks;

        /// <param name="data">The data to encode.</param>
        /// <param name="eci">The ECI designator to prepend as a segment, or <see cref="ECI.None"/> to prepend none.</param>
        /// <param name="considerKanjiMode">If <c>true</c>, Kanji encoding is considered.</param>
        internal SegmentSource(ArraySegment<byte> data, ECI eci, bool considerKanjiMode)
        {
            _data = data;
            _eci = eci;
            _blocks = SegmentCompaction.BuildBlocks(data, considerKanjiMode);
        }

        /// <summary>
        /// A lower bound of the bit length of the segments for any version (data bits only, no headers).
        /// </summary>
        internal int MinBitLength => SegmentCompaction.MinBitLength(_blocks);

        /// <summary>
        /// Builds the optimal segments for the given version, including the ECI segment if any.
        /// </summary>
        internal List<DataSegment> ToSegments(int version)
        {
            var segments = SegmentCompaction.BuildSegments(_data, _blocks, version);
            if (!Equals(_eci, ECI.None))
            {
                segments.Insert(0, new DataSegmentEci(_eci));
            }
            return segments;
        }
    }
}
