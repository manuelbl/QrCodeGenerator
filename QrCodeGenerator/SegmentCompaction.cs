/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Functions for building a list of segments with the shortest bit stream.
    /// </summary>
    internal static class SegmentCompaction
    {
        #region Optimal Segments

        /// <summary>
        /// Builds optimal segments encoding the given byte array.
        /// <para>
        /// The optimal segments result in the shortest possible bit stream for the given version.
        /// The version only affects the count-indicator widths, which are the same for
        /// versions 1 to 9, 10 to 26 and 27 to 40. So the result is identical for all versions
        /// of the same group.
        /// </para>
        /// <para>
        /// The Kanji mode is only used if <paramref name="considerKanjiMode"/> is <c>true</c>.
        /// For the best compatibility, it should only be used if the text is encoded in Shift JIS.
        /// </para>
        /// </summary>
        /// <param name="bytes">Bytes to encode</param>
        /// <param name="version">QR code version</param>
        /// <param name="considerKanjiMode">If <c>true</c>, Kanji encoding is considered; if <c>false</c>, Kanji encoding is not used.</param>
        /// <returns>QR segments</returns>
        internal static List<DataSegment> BuildSegments(ArraySegment<byte> bytes, int version = 20,
            bool considerKanjiMode = false)
        {
            return BuildSegments(bytes, BuildBlocks(bytes, considerKanjiMode), version);
        }

        /// <summary>
        /// Builds optimal segments encoding the given byte array from its blocks.
        /// <para>
        /// The blocks do not depend on the version, so they can be built once with
        /// <see cref="BuildBlocks"/> and reused to build the segments for several versions.
        /// </para>
        /// </summary>
        /// <param name="bytes">Bytes to encode</param>
        /// <param name="blocks">The blocks of the bytes, as built by <see cref="BuildBlocks"/></param>
        /// <param name="version">QR code version</param>
        /// <returns>QR segments</returns>
        internal static List<DataSegment> BuildSegments(ArraySegment<byte> bytes, Block[] blocks, int version)
        {
            if (blocks.Length == 0)
            {
                return new List<DataSegment>();
            }

            // Since switching from one mode to another requires additional bits, the cost of
            // switching can be higher than the savings from using a more efficient mode.
            // A dynamic programme assigns each block the segment mode minimizing the total bit stream.
            var modes = AssignModes(blocks, version);

            // Consecutive blocks with the same mode form a single segment.
            var segments = new List<DataSegment>();
            var offset = 0;
            var length = 0;
            for (var i = 0; i < blocks.Length; i += 1)
            {
                length += blocks[i].Length;
                if (i + 1 == blocks.Length || modes[i + 1] != modes[i])
                {
                    segments.Add(DataSegment.MakeSegment(modes[i], bytes.MakeSlice(offset, length)));
                    offset += length;
                    length = 0;
                }
            }

            return segments;
        }

        // The modes a block can be encoded in, indexed by ModeIndex().
        private const int NumModes = 4;

        // Bits per byte for each mode (indexed by ModeIndex()), in sixths of a bit so that all values
        // are integers: numeric 10/3, alphanumeric 11/2, Kanji 13/2 (per byte), binary 8.
        private static readonly int[] ByteCosts = { 20, 33, 39, 48 };

        private static int ModeIndex(DataSegmentMode mode) => (int)mode - 1;

        private static DataSegmentMode ModeAt(int index) => (DataSegmentMode)(index + 1);

        /// <summary>
        /// Calculates a lower bound of the bit length of any segmentation of the blocks:
        /// the data bits if each block is encoded in its cheapest mode, without any headers.
        /// </summary>
        internal static int MinBitLength(Block[] blocks)
        {
            var cost = 0;
            foreach (var block in blocks)
            {
                cost += block.Length * ByteCosts[ModeIndex(block.Mode)];
            }
            return cost / 6;
        }

        private const int Infinity = int.MaxValue / 2;

        /// <summary>
        /// Assigns each block the mode of the segment it is encoded in such that the total bit stream is minimal.
        /// <para>
        /// For each block and each mode able to encode the block, the minimal cost of the bit stream up to and
        /// including the block is computed, either by continuing the segment of the previous block or by
        /// starting a new segment (which adds a header). Costs are tracked in sixths of a bit; a segment's
        /// length is the sum of its byte costs rounded up to whole bits, so rounding up when a segment ends
        /// yields the exact length.
        /// </para>
        /// </summary>
        /// <param name="blocks">The blocks, each with its cheapest mode.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>The segment mode for each block.</returns>
        private static DataSegmentMode[] AssignModes(Block[] blocks, int version)
        {
            var headerCosts = new int[NumModes];
            for (var m = 0; m < NumModes; m += 1)
            {
                // A block of length 0 costs exactly the segment header (mode and count indicator).
                headerCosts[m] = 6 * new Block { Mode = ModeAt(m), Length = 0 }.GetSegmentLength(version);
            }

            var blockCount = blocks.Length;
            var previousCosts = new int[NumModes]; // minimal cost up to the previous block, per mode
            var costs = new int[NumModes]; // minimal cost up to the current block, per mode
            var previousModes = new byte[blockCount * NumModes]; // mode of the previous block on the minimal path

            for (var i = 0; i < blockCount; i += 1)
            {
                (previousCosts, costs) = (costs, previousCosts);
                var block = blocks[i];
                for (var m = 0; m < NumModes; m += 1)
                {
                    if (!CanEncode(ModeAt(m), block.Mode))
                    {
                        costs[m] = Infinity;
                        continue;
                    }

                    var dataCost = block.Length * ByteCosts[m];
                    if (i == 0)
                    {
                        costs[m] = headerCosts[m] + dataCost;
                        continue;
                    }

                    // Continue the segment of the previous block (no header); on a tie, this is preferred.
                    var best = previousCosts[m] + dataCost;
                    var bestPrevious = m;

                    // Or start a new segment after a segment of another mode.
                    for (var p = 0; p < NumModes; p += 1)
                    {
                        var previousCost = previousCosts[p];
                        if (p == m || previousCost >= Infinity)
                        {
                            continue;
                        }

                        var cost = RoundUpToBits(previousCost) + headerCosts[m] + dataCost;
                        if (cost < best)
                        {
                            best = cost;
                            bestPrevious = p;
                        }
                    }

                    costs[m] = best;
                    previousModes[i * NumModes + m] = (byte)bestPrevious;
                }
            }

            // Pick the cheapest mode for the last block and walk the path back.
            var mode = 0;
            for (var m = 1; m < NumModes; m += 1)
            {
                if (RoundUpToBits(costs[m]) < RoundUpToBits(costs[mode]))
                {
                    mode = m;
                }
            }

            var modes = new DataSegmentMode[blockCount];
            for (var i = blockCount - 1; i >= 0; i -= 1)
            {
                modes[i] = ModeAt(mode);
                mode = previousModes[i * NumModes + mode];
            }

            return modes;
        }

        // Rounds a cost in sixths of a bit up to whole bits (still in sixths).
        private static int RoundUpToBits(int cost) => (cost + 5) / 6 * 6;

        /// <summary>
        /// Tests if a segment of the given mode can encode a block whose cheapest mode is <paramref name="blockMode"/>.
        /// </summary>
        private static bool CanEncode(DataSegmentMode segmentMode, DataSegmentMode blockMode)
        {
            return segmentMode == DataSegmentMode.Binary
                   || segmentMode == blockMode
                   || (segmentMode == DataSegmentMode.Alphanumeric && blockMode == DataSegmentMode.Numeric);
        }

        /// <summary>
        /// Builds blocks of bytes with the same encoding mode.
        /// <para>
        /// The algorithm first determines the best encoding mode for each byte.
        /// Then it creates blocks of consecutive bytes with the same encoding mode.
        /// </para>
        /// </summary>
        /// <param name="bytes">Bytes to process</param>
        /// <param name="useKanji">If <c>true</c>, Kanji encoding is considered; if <c>false</c>, Kanji encoding is not used.</param>
        /// <returns>List of blocks</returns>
        internal static Block[] BuildBlocks(ArraySegment<byte> bytes, bool useKanji)
        {
            if (bytes.Count == 0)
            {
                return Array.Empty<Block>();
            }

            var modes = CalcCompactionMode(bytes, useKanji);
            
            // create blocks
            var modeChanges = CountModeChanges(modes);
            var blocks = new Block[modeChanges];
            var blockCount = 0;
            var blockStartIndex = 0;
            var previousMode = modes[0];
            for (var i = 0; i < modes.Length; i += 1)
            {
                var currentMode = modes[i];
                if (currentMode == previousMode)
                {
                    continue;
                }

                blocks[blockCount] = new Block { Mode = (DataSegmentMode) previousMode, Length = i - blockStartIndex };
                blockCount += 1;
                previousMode = currentMode;
                blockStartIndex = i;
            }

            blocks[blockCount] = new Block { Mode = (DataSegmentMode) previousMode, Length = modes.Length - blockStartIndex };
            return blocks;
        }

        private static int CountModeChanges(byte[] modes)
        {
            var count = 1;
            var previousMode = modes[0];
            for (var i = 0; i < modes.Length; i += 1)
            {
                var currentMode = modes[i];
                if (currentMode != previousMode)
                {
                    count += 1;
                    previousMode = currentMode;
                }
            }
            return count;
        }

        /// <summary>
        /// Calculates the best encoding mode for each byte.
        /// <para>
        /// The best mode is the mode requiring the fewest bits to encode the byte.
        /// The priority thus is: Numeric mode, alphanumeric mode, Kanji mode, binary mode.
        /// </para>
        /// <para>
        /// The Kanji mode is slightly different as it can only be applied to pairs of bytes.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to encode.</param>
        /// <param name="useKanji">If <c>true</c>, Kanji encoding is considered; if <c>false</c>, Kanji encoding is not used.</param>
        /// <returns>An array of the best encoding mode for each byte.</returns>
        private static byte[] CalcCompactionMode(ArraySegment<byte> bytes, bool useKanji)
        {
            var len = bytes.Count;
            var modes = new byte[len];
            var index = 0;
            while (index < len)
            {
                var b1 = bytes.At(index);
                if (DataSegmentNumeric.IsNumeric(b1))
                {
                    modes[index] = (byte) DataSegmentMode.Numeric;
                }
                else if (DataSegmentAlphanumeric.IsAlphanumeric(b1))
                {
                    modes[index] = (byte) DataSegmentMode.Alphanumeric;
                }
                else if (useKanji && index < len - 1 && DataSegmentKanji.IsShiftJisDoubleByte(b1, bytes.At(index + 1)))
                {
                    modes[index] = (byte) DataSegmentMode.Kanji;
                    index += 1;
                    modes[index] = (byte) DataSegmentMode.Kanji;
                }
                else
                {
                    modes[index] = (byte) DataSegmentMode.Binary;
                }

                index += 1;
            }

            return modes;
        }

        #endregion

        #region Block

        /// <summary>
        /// Block of bytes with an associated encoding mode.
        /// </summary>
        internal struct Block
        {
            /// <summary>
            /// Encoding mode.
            /// </summary>
            internal DataSegmentMode Mode;

            /// <summary>
            /// Number of bytes in the block.
            /// </summary>
            internal int Length;

            /// <summary>
            /// Calculates the segment length for this block.
            /// </summary>
            /// <param name="version">QR code version.</param>
            /// <returns>Resulting segment length, in bits.</returns>
            internal int GetSegmentLength(int version)
            {
                // Duplicated code for performance
                switch (Mode)
                {
                    case DataSegmentMode.Binary:
                        return 12 + (version <= 9 ? 0 : 8) + Length * 8;
                    case DataSegmentMode.Numeric:
                        return 14 + (version + 7) / 17 * 2 + (Length * 10 + 2) / 3;
                    case DataSegmentMode.Alphanumeric:
                        return 13 + (version + 7) / 17 * 2 + (Length * 11 + 1) / 2;
                    case DataSegmentMode.Kanji:
                        return 12 + (version + 7) / 17 * 2 + Length * 13 / 2;
                    default:
                        Debug.Assert(false, "data segment mode not supported by this function");
                        return 0;
                }
            }

            public override string ToString()
            {
                return $"{Mode}: {Length} bytes";
            }
        }

        #endregion
    }
}
