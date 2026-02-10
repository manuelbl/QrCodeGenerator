/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Functions for building a list of segments with the shortest bit stream.
    /// </summary>
    internal static class SegmentCompaction
    {
        #region Optimal Segments

        private const int Numeric = (int) DataSegmentMode.Numeric;
        private const int Alphanumeric = (int) DataSegmentMode.Alphanumeric;
        private const int Byte = (int) DataSegmentMode.Binary;

        /// <summary>
        /// Builds optimal segments encoding the given byte array.
        /// <para>
        /// The optimal segments result in the shortest possible bit stream.
        /// </para>
        /// <para>
        /// In edge cases, the optimal result can slightly vary depending
        /// on the QR code version. However, the length difference is minimal.
        /// If the version is unknown, version 10 or higher is recommended. 
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
            // The algorithm first determines the best encoding mode for each byte
            // and builds blocks of bytes with the same encoding mode.
            var blocks = BuildBlocks(bytes, considerKanjiMode);

            // Since switching from one mode to another and back requires additional bits,
            // the additional cost of switching can be higher than the savings
            // from using a more efficient mode. If this is the case, two or three blocks are merged.
            // In the first step, short numeric blocks are merged with alphanumeric blocks.
            // In the second step, all types of blocks are merged into byte blocks.
            MergeBlocks(blocks, version, DataSegmentMode.Alphanumeric,
                (mode0, mode1, mode2) => mode0 == Alphanumeric
                                         && mode1 == Numeric && mode2 == mode0,
                (mode0, mode1) => (mode0 == Alphanumeric && mode1 == Numeric)
                                  || (mode0 == Numeric && mode1 == Alphanumeric)
            );
            MergeBlocks(blocks, version, DataSegmentMode.Binary,
                (mode0, mode1, mode2) => mode1 != Byte && mode2 == mode0,
                (mode0, mode1) => (mode0 == Byte && mode1 != Byte)
                                  || (mode0 != Byte && mode1 == Byte)
            );

            var offset = 0;
            return blocks.ConvertAll(block =>
            {
                var blockBytes = bytes.MakeSlice(offset, block.Length);
                offset += block.Length;
                return DataSegment.MakeSegment(block.Mode, blockBytes);
            });
        }

        /// <summary>
        /// Merges blocks if the length can be reduced.
        /// </summary>
        /// <param name="blocks">The list of blocks to process.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="mergedMode">The data segment mode of the merged block.</param>
        /// <param name="merge3Condition">Condition for testing 3 consecutive blocks.</param>
        /// <param name="merge2Condition">Condition for testing 2 consecutive blocks.</param>
        [SuppressMessage("csharpsquid", "S3776")]
        private static void MergeBlocks(List<Block> blocks, int version, DataSegmentMode mergedMode,
            Func<int, int, int, bool> merge3Condition,
            Func<int, int, bool> merge2Condition)
        {
            var previousCount = -1;
            while (blocks.Count > 1 && previousCount != blocks.Count)
            {
                previousCount = blocks.Count;

                // Work back to front to reduce the amount of copying in `blocks`
                var index = blocks.Count - 1;
                while (index > 0)
                {
                    var mode0 = (int) blocks[index].Mode;
                    var mode1 = (int) blocks[index - 1].Mode;
                    var mode2 = index >= 2 ? (int) blocks[index - 2].Mode : 0;

                    // Case 1: merge 3 blocks
                    // Test if the bit stream is shorter if all 3 blocks are merged (using the specified merged mode).
                    if (mode2 != 0 && merge3Condition(mode0, mode1, mode2))
                    {
                        var mergedPayloadLength =
                            blocks[index - 2].Length + blocks[index - 1].Length + blocks[index].Length;
                        var mergedBlock = new Block { Mode = mergedMode, Length = mergedPayloadLength };
                        var mergedLength = mergedBlock.GetSegmentLength(version);
                        var separateLength = blocks[index - 2].GetSegmentLength(version)
                                             + blocks[index - 1].GetSegmentLength(version)
                                             + blocks[index].GetSegmentLength(version);
                        if (mergedLength <= separateLength)
                        {
                            blocks[index - 2] = mergedBlock;
                            blocks.RemoveRange(index - 1, 2);
                            index -= 1;
                        }
                    }

                    // Case 2: merge 2 blocks
                    // Test if the bit stream is shorter if the 2 blocks are merged (using the specified merged mode).
                    else if (merge2Condition(mode0, mode1))
                    {
                        var mergedBlock = new Block
                            { Mode = mergedMode, Length = blocks[index - 1].Length + blocks[index].Length };
                        var mergedLength = mergedBlock.GetSegmentLength(version);
                        var separateLength = blocks[index - 1].GetSegmentLength(version) +
                                             blocks[index].GetSegmentLength(version);
                        if (mergedLength <= separateLength)
                        {
                            blocks[index - 1] = mergedBlock;
                            blocks.RemoveAt(index);
                        }
                    }

                    index -= 1;
                }
            }
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
        private static List<Block> BuildBlocks(ArraySegment<byte> bytes, bool useKanji)
        {
            if (bytes.Count == 0)
            {
                return new List<Block>();
            }

            var modes = CalcCompactionMode(bytes, useKanji);

            // create blocks
            var blockList = new List<Block>();
            var blockStartIndex = 0;
            var previousMode = modes[0];
            for (var i = 0; i < modes.Length; i += 1)
            {
                var currentMode = modes[i];
                if (currentMode == previousMode)
                {
                    continue;
                }

                blockList.Add(new Block { Mode = previousMode, Length = i - blockStartIndex });
                previousMode = currentMode;
                blockStartIndex = i;
            }

            blockList.Add(new Block { Mode = previousMode, Length = modes.Length - blockStartIndex });

            return blockList;
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
        private static DataSegmentMode[] CalcCompactionMode(ArraySegment<byte> bytes, bool useKanji)
        {
            var len = bytes.Count;
            var modes = new DataSegmentMode[len];
            var index = 0;
            while (index < len)
            {
                var b1 = bytes.At(index);
                if (DataSegmentNumeric.IsNumeric(b1))
                {
                    modes[index] = DataSegmentMode.Numeric;
                }
                else if (DataSegmentAlphanumeric.IsAlphanumeric(b1))
                {
                    modes[index] = DataSegmentMode.Alphanumeric;
                }
                else if (useKanji && index < len - 1 && DataSegmentKanji.IsShiftJisDoubleByte(b1, bytes.At(index + 1)))
                {
                    modes[index] = DataSegmentMode.Kanji;
                    index += 1;
                    modes[index] = DataSegmentMode.Kanji;
                }
                else
                {
                    modes[index] = DataSegmentMode.Binary;
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
        private struct Block
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
                return DataSegment.GetBitLength(Mode, Length, version);
            }

            public override string ToString()
            {
                return $"{Mode}: {Length} bytes";
            }
        }

        #endregion
    }
}