/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
            var blockCount = blocks.Length;

            // Since switching from one mode to another and back requires additional bits,
            // the additional cost of switching can be higher than the savings
            // from using a more efficient mode. If this is the case, two or three blocks are merged.
            // In the first step, short numeric blocks are merged with alphanumeric blocks.
            // In the second step, all types of blocks are merged into byte blocks.
            blockCount = MergeBlocks(blocks, blockCount, version, DataSegmentMode.Alphanumeric,
                (mode0, mode1, mode2) => mode0 == DataSegmentMode.Alphanumeric
                                         && mode1 == DataSegmentMode.Numeric && mode2 == mode0,
                (mode0, mode1) => (mode0 == DataSegmentMode.Alphanumeric && mode1 == DataSegmentMode.Numeric)
                                  || (mode0 == DataSegmentMode.Numeric && mode1 == DataSegmentMode.Alphanumeric)
            );
            blockCount = MergeBlocks(blocks, blockCount, version, DataSegmentMode.Binary,
                (mode0, mode1, mode2) => mode1 != DataSegmentMode.Binary && mode2 == mode0,
                (mode0, mode1) => (mode0 == DataSegmentMode.Binary && mode1 != DataSegmentMode.Binary)
                                  || (mode0 != DataSegmentMode.Binary && mode1 == DataSegmentMode.Binary)
            );

            var segments = new List<DataSegment>(blockCount);
            var offset = 0;
            for (var i = 0; i < blockCount; i += 1)
            {
                var block = blocks[i];
                var blockBytes = bytes.MakeSlice(offset, block.Length);
                offset += block.Length;
                segments.Add(DataSegment.MakeSegment(block.Mode, blockBytes));
            }

            return segments;
        }

        /// <summary>
        /// Merges blocks if the length can be reduced.
        /// </summary>
        /// <param name="blocks">The array of blocks to process.</param>
        /// <param name="blockCount">The number of active blocks in the array.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="mergedMode">The data segment mode of the merged block.</param>
        /// <param name="merge3Condition">Condition for testing 3 consecutive blocks.</param>
        /// <param name="merge2Condition">Condition for testing 2 consecutive blocks.</param>
        /// <returns>Number of remaining blocks in array</returns>
        [SuppressMessage("csharpsquid", "S3776")]
        private static int MergeBlocks(Block[] blocks, int blockCount, int version, DataSegmentMode mergedMode,
            Func<DataSegmentMode, DataSegmentMode, DataSegmentMode, bool> merge3Condition,
            Func<DataSegmentMode, DataSegmentMode, bool> merge2Condition)
        {
            var previousCount = -1;
            while (blockCount > 1 && previousCount != blockCount)
            {
                previousCount = blockCount;
                
                var targetIndex = 1;
                var sourceIndex = 1;
                while (sourceIndex < blockCount)
                {
                    var merged = false;
                    var mode0 = blocks[targetIndex - 1].Mode;
                    var mode1 = blocks[sourceIndex].Mode;

                    // Case 1: merge 3 blocks (last processed one plus 2 unprocessed ones)
                    // Test if the bit stream is shorter if all 3 blocks are merged (using the specified merged mode).
                    if (sourceIndex + 1 < blockCount && merge3Condition(mode0, mode1, blocks[sourceIndex + 1].Mode))
                    {
                        var mergedPayloadLength =
                            blocks[targetIndex - 1].Length + blocks[sourceIndex].Length + blocks[sourceIndex + 1].Length;
                        var mergedBlock = new Block { Mode = mergedMode, Length = mergedPayloadLength };
                        var mergedLength = mergedBlock.GetSegmentLength(version);
                        var separateLength = blocks[targetIndex - 1].GetSegmentLength(version)
                                             + blocks[sourceIndex].GetSegmentLength(version)
                                             + blocks[sourceIndex + 1].GetSegmentLength(version);
                        if (mergedLength <= separateLength)
                        {
                            blocks[targetIndex - 1] = mergedBlock;
                            sourceIndex += 2;
                            merged = true;
                        }
                    }

                    // Case 2: merge 2 blocks (last processed one and the current unprocessed one)
                    // Test if the bit stream is shorter if the 2 blocks are merged (using the specified merged mode).
                    else if (merge2Condition(mode0, mode1))
                    {
                        var mergedPayloadLength = blocks[targetIndex - 1].Length + blocks[sourceIndex].Length;
                        var mergedBlock = new Block { Mode = mergedMode, Length = mergedPayloadLength };
                        var mergedLength = mergedBlock.GetSegmentLength(version);
                        var separateLength = blocks[targetIndex - 1].GetSegmentLength(version) +
                                             blocks[sourceIndex].GetSegmentLength(version);
                        if (mergedLength <= separateLength)
                        {
                            blocks[targetIndex - 1] = mergedBlock;
                            sourceIndex += 1;
                            merged = true;
                        }
                    }
                    
                    if (!merged)
                    {
                        blocks[targetIndex] = blocks[sourceIndex];
                        targetIndex += 1;
                        sourceIndex += 1;
                    }
                }
                
                blockCount = targetIndex;
            }
            
            return blockCount;
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
        private static Block[] BuildBlocks(ArraySegment<byte> bytes, bool useKanji)
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
                }

                Debug.Assert(false);
                return 0;
            }

            public override string ToString()
            {
                return $"{Mode}: {Length} bytes";
            }
        }

        #endregion
    }
}