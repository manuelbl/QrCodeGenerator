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
            blockCount = MergeBlocks<AlphanumericRule>(blocks, blockCount, version);
            blockCount = MergeBlocks<BinaryRule>(blocks, blockCount, version);

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
        /// Merges blocks according to the merge rule until no further merge is possible.
        /// </summary>
        /// <typeparam name="TRule">The merge rule.</typeparam>
        /// <param name="blocks">The array of blocks to process.</param>
        /// <param name="blockCount">The number of active blocks in the array.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>Number of remaining blocks in array</returns>
        private static int MergeBlocks<TRule>(Block[] blocks, int blockCount, int version)
            where TRule : struct, IMergeRule
        {
            // A pass can create new merge opportunities, so repeat until nothing changes anymore.
            var previousCount = -1;
            while (blockCount > 1 && blockCount != previousCount)
            {
                previousCount = blockCount;
                blockCount = MergePass<TRule>(blocks, blockCount, version);
            }

            return blockCount;
        }

        /// <summary>
        /// Runs a single merge pass, compacting the blocks in place.
        /// </summary>
        /// <typeparam name="TRule">The merge rule.</typeparam>
        /// <param name="blocks">The array of blocks to process.</param>
        /// <param name="blockCount">The number of active blocks in the array.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>Number of remaining blocks in array</returns>
        private static int MergePass<TRule>(Block[] blocks, int blockCount, int version)
            where TRule : struct, IMergeRule
        {
            var processedBlocks = 1; // number of processed blocks
            var sourceIndex = 1; // blocks from this index have yet to be processed
            while (sourceIndex < blockCount)
            {
                var consumed = TryMergeAt<TRule>(blocks, processedBlocks - 1, sourceIndex, blockCount, version);
                if (consumed > 0)
                {
                    sourceIndex += consumed;
                }
                else
                {
                    blocks[processedBlocks] = blocks[sourceIndex];
                    processedBlocks += 1;
                    sourceIndex += 1;
                }
            }

            return processedBlocks;
        }

        /// <summary>
        /// Tries to merge the block at <paramref name="targetIndex"/> with the 2 or 1 blocks
        /// starting at <paramref name="sourceIndex"/>, replacing the target block if successful.
        /// </summary>
        /// <typeparam name="TRule">The merge rule.</typeparam>
        /// <param name="blocks">The array of blocks to process.</param>
        /// <param name="targetIndex">The index of the last processed block.</param>
        /// <param name="sourceIndex">The index of the first unprocessed block.</param>
        /// <param name="blockCount">The number of active blocks in the array.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>Number of source blocks consumed (0 if the blocks have not been merged)</returns>
        private static int TryMergeAt<TRule>(Block[] blocks, int targetIndex, int sourceIndex, int blockCount,
            int version)
            where TRule : struct, IMergeRule
        {
            var rule = default(TRule);
            ref var target = ref blocks[targetIndex];
            var first = blocks[sourceIndex];

            // Case 1: merge 3 blocks (last processed one plus 2 unprocessed ones)
            // Test if the bit stream is shorter if all 3 blocks are merged (using the rule's merged mode).
            if (sourceIndex + 1 < blockCount)
            {
                var second = blocks[sourceIndex + 1];
                if (rule.CanMerge3(target.Mode, first.Mode, second.Mode))
                {
                    var separateLength = target.GetSegmentLength(version) + first.GetSegmentLength(version)
                                         + second.GetSegmentLength(version);
                    return TryReplaceWithMerged(ref target, first.Length + second.Length, separateLength,
                        rule.MergedMode, version) ? 2 : 0;
                }
            }

            // Case 2: merge 2 blocks (last processed one and the current unprocessed one)
            // Test if the bit stream is shorter if the 2 blocks are merged (using the rule's merged mode).
            if (rule.CanMerge2(target.Mode, first.Mode))
            {
                var separateLength = target.GetSegmentLength(version) + first.GetSegmentLength(version);
                return TryReplaceWithMerged(ref target, first.Length, separateLength, rule.MergedMode, version) ? 1 : 0;
            }

            return 0;
        }

        /// <summary>
        /// Replaces the target block with the merged block unless the merged block
        /// results in a longer bit stream.
        /// </summary>
        /// <param name="target">The block to replace, and the first block of the merged block.</param>
        /// <param name="addedLength">The number of payload bytes of the further blocks to merge.</param>
        /// <param name="separateLength">The bit stream length of the blocks if they are not merged.</param>
        /// <param name="mergedMode">The data segment mode of the merged block.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns><c>true</c> if the blocks have been merged</returns>
        private static bool TryReplaceWithMerged(ref Block target, int addedLength, int separateLength,
            DataSegmentMode mergedMode, int version)
        {
            var merged = new Block { Mode = mergedMode, Length = target.Length + addedLength };
            if (merged.GetSegmentLength(version) > separateLength)
            {
                return false;
            }

            target = merged;
            return true;
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

        #region Merge Rules

        /// <summary>
        /// Rule deciding which consecutive blocks are candidates for merging,
        /// and what the data segment mode of the merged block is.
        /// </summary>
        /// <remarks>
        /// The rule is implemented as a struct and used as a generic type argument. The JIT compiler
        /// therefore specializes the merge code per rule and inlines the checks (no delegate calls,
        /// no allocation).
        /// </remarks>
        private interface IMergeRule
        {
            /// <summary>
            /// Data segment mode of the merged block.
            /// </summary>
            DataSegmentMode MergedMode { get; }

            /// <summary>
            /// Tests if 3 consecutive blocks with the given modes are candidates for merging.
            /// </summary>
            bool CanMerge3(DataSegmentMode mode0, DataSegmentMode mode1, DataSegmentMode mode2);

            /// <summary>
            /// Tests if 2 consecutive blocks with the given modes are candidates for merging.
            /// </summary>
            bool CanMerge2(DataSegmentMode mode0, DataSegmentMode mode1);
        }

        /// <summary>
        /// Rule merging short numeric blocks with adjacent alphanumeric blocks.
        /// </summary>
        private readonly struct AlphanumericRule : IMergeRule
        {
            public DataSegmentMode MergedMode => DataSegmentMode.Alphanumeric;

            public bool CanMerge3(DataSegmentMode mode0, DataSegmentMode mode1, DataSegmentMode mode2)
                => mode0 == DataSegmentMode.Alphanumeric && mode1 == DataSegmentMode.Numeric && mode2 == mode0;

            public bool CanMerge2(DataSegmentMode mode0, DataSegmentMode mode1)
                => (mode0 == DataSegmentMode.Alphanumeric && mode1 == DataSegmentMode.Numeric)
                   || (mode0 == DataSegmentMode.Numeric && mode1 == DataSegmentMode.Alphanumeric);
        }

        /// <summary>
        /// Rule merging blocks of any mode into binary blocks.
        /// </summary>
        private readonly struct BinaryRule : IMergeRule
        {
            public DataSegmentMode MergedMode => DataSegmentMode.Binary;

            public bool CanMerge3(DataSegmentMode mode0, DataSegmentMode mode1, DataSegmentMode mode2)
                => mode1 != DataSegmentMode.Binary && mode2 == mode0;

            public bool CanMerge2(DataSegmentMode mode0, DataSegmentMode mode1)
                => (mode0 == DataSegmentMode.Binary) != (mode1 == DataSegmentMode.Binary);
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
