/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;
#if NET6_0_OR_GREATER
using System.Numerics;
#endif

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Square matrix of binary pixels.
    /// <para>
    /// The bits are stored in a 64-bit unsigned integer array, in row-major order
    /// (y-coordinates specify the row, x-coordinates specify the column). A row always starts at
    /// a word boundary so the penalty rules can scan it word by word. In each row, the bits at
    /// column positions outside the logical size are always 0.
    /// </para>
    /// <para>
    /// A row holds its modules in one, two or three words, one per 64 columns, which is what
    /// <see cref="UsedWordsPerRow"/> reports. These are the library's <b>three row layouts</b>,
    /// and each one covers a range of QR code versions:
    /// </para>
    /// <list type="table">
    ///   <listheader><term>Words of modules</term><description>Sizes / versions / stride</description></listheader>
    ///   <item><term>1</term><description>sizes 1–64, versions 1–11, stride 1</description></item>
    ///   <item><term>2</term><description>sizes 65–128, versions 12–27, stride 2</description></item>
    ///   <item><term>3</term><description>sizes 129–192, versions 28–40, stride 4</description></item>
    /// </list>
    /// <para>
    /// The layout follows from the size alone, so two matrices of the same size always agree on it
    /// and an <see cref="And"/> or <see cref="Xor"/> can never mix two of them. It exists for
    /// <see cref="Penalty"/>, which has an implementation of every rule per layout and is where
    /// encoding a QR code spends most of its time: the narrower the row, the fewer words a rule
    /// scans, and versions 1 to 11 are most QR codes.
    /// </para>
    /// <para>
    /// The <em>stride</em> from one row to the next in <see cref="Raw"/>, which
    /// <see cref="WordsPerRow"/> reports, is that word count rounded up to a power of two, so a row
    /// index is a shift rather than a multiplication. It differs from the word count only for a
    /// three-word row, which is allocated a fourth, always-zero padding word: <see cref="Invert"/>
    /// clears it, <see cref="FillRect"/> cannot reach it, and <see cref="Transpose"/> never writes
    /// it. Operations over the whole matrix therefore ignore the distinction and run flat over
    /// <see cref="Raw"/>.
    /// </para>
    /// <para>
    /// The maximum supported size is <see cref="MaxSize"/> × <see cref="MaxSize"/> bits, the
    /// largest with three words of modules per row. Every QR code version fits: version 40, the
    /// largest, is 177 modules wide.
    /// </para>
    /// </summary>
    internal readonly struct BitMatrix
    {
        /// <summary>
        /// The greatest number of 64-bit words of modules a row can hold, that of the widest layout.
        /// </summary>
        internal const int MaxUsedWordsPerRow = 3;

        /// <summary>
        /// The greatest stride from one row to the next, that of the widest layout.
        /// <para>
        /// A three-word row is allocated a fourth word so the stride stays a power of two and a row
        /// index stays a shift. That word is always zero.
        /// </para>
        /// </summary>
        internal const int MaxWordsPerRow = 4;

        /// <summary>
        /// The maximum supported size, given by the <see cref="MaxUsedWordsPerRow"/> words per row.
        /// </summary>
        internal const int MaxSize = 64 * MaxUsedWordsPerRow;

        /// <summary>
        /// Gets the size of the matrix (number of bits in each dimension).
        /// </summary>
        internal int Size { get; }

        /// <summary>
        /// Gets the base-2 logarithm of the stride from one row to the next: 0, 1 or 2.
        /// </summary>
        internal int RowShift { get; }

        /// <summary>
        /// Gets the number of 64-bit words of each row that hold modules: 1, 2 or 3.
        /// <para>
        /// This is the matrix's row layout, one word per 64 columns. An algorithm reading the bits
        /// of a row scans that many words and dispatches on this to the implementation specialized
        /// for it.
        /// </para>
        /// </summary>
        internal int UsedWordsPerRow { get; }

        /// <summary>
        /// Gets the number of 64-bit words each row occupies, i.e. the stride from one row to the
        /// next in <see cref="Raw"/>: 1, 2 or 4.
        /// <para>
        /// This is <see cref="UsedWordsPerRow"/> rounded up to a power of two. The two differ only
        /// for a three-word row, whose fourth word is padding.
        /// </para>
        /// </summary>
        internal int WordsPerRow => 1 << RowShift;

        /// <summary>
        /// Initializes a new instance with the specified size.
        /// <para>
        /// Initially, all bits are cleared (<c>false</c>).
        /// </para>
        /// </summary>
        /// <param name="size">The size (number of bits in each dimension).</param>
        /// <exception cref="ArgumentException">Thrown if the size is negative or greater than <see cref="MaxSize"/>.</exception>
        internal BitMatrix(int size)
        {
            if (size < 0 || size > MaxSize)
            {
                throw new ArgumentException($"The size must be between 0 and {MaxSize}", nameof(size));
            }

            // one word of modules per 64 columns, in a stride rounded up to a power of two
            var shift = size <= 64 ? 0 : (size <= 128 ? 1 : 2);
            RowShift = shift;
            UsedWordsPerRow = size <= 64 ? 1 : (size <= 128 ? 2 : 3);
            Size = size;
            Raw = new ulong[size << shift];
        }

        private BitMatrix(ulong[] bits, int size, int rowShift, int usedWordsPerRow)
        {
            Raw = bits;
            Size = size;
            RowShift = rowShift;
            UsedWordsPerRow = usedWordsPerRow;
        }

        /// <summary>
        /// Gets the bit at the specified coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns><c>true</c> if the bit is set, <c>false</c> if the bit is cleared.</returns>
        internal bool Get(int x, int y)
        {
            var bitMask = 1ul << (x & 0x3f);
            var index = (y << RowShift) + (x >> 6);
            return (Raw[index] & bitMask) != 0;
        }

        /// <summary>
        /// Sets the bit at the specified coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="bit"><c>true</c> if the bit should be set, <c>false</c> if the bit should be cleared.</param>
        internal void Set(int x, int y, bool bit)
        {
            var bitMask = 1ul << (x & 0x3f);
            var index = (y << RowShift) + (x >> 6);
            if (bit)
            {
                Raw[index] |= bitMask;
            }
            else
            {
                Raw[index] &= ~bitMask;
            }
        }

        /// <summary>
        /// Sets all bits in the specified rectangular area.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left corner.</param>
        /// <param name="y">The y-coordinate of the top-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        internal void FillRect(int x, int y, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var startWord = x >> 6;
            var endX = x + width - 1;
            var endWord = endX >> 6;
            var startBit = x & 0x3f;
            var endBit = endX & 0x3f;

            var startMask = ulong.MaxValue << startBit;
            var endMask = ulong.MaxValue >> (63 - endBit);

            var wordsPerRow = WordsPerRow;
            var rowBase = y << RowShift;
            var rowEnd = rowBase + wordsPerRow * height;

            if (startWord == endWord)
            {
                var mask = startMask & endMask;
                for (var idx = rowBase + startWord; idx < rowEnd; idx += wordsPerRow)
                {
                    Raw[idx] |= mask;
                }
            }
            else
            {
                for (var row = rowBase; row < rowEnd; row += wordsPerRow)
                {
                    Raw[row + startWord] |= startMask;
                    for (var w = startWord + 1; w < endWord; w += 1)
                    {
                        Raw[row + w] = ulong.MaxValue;
                    }
                    Raw[row + endWord] |= endMask;
                }
            }
        }

        /// <summary>
        /// Inverts all bits in this matrix in place.
        /// </summary>
        internal void Invert()
        {
            var size = Size;
            var lastBit = size - 1;
            var lastWord = lastBit >> 6;
            var lastMask = ulong.MaxValue >> (63 - (lastBit & 0x3F));
            var wordsPerRow = WordsPerRow;

            for (var y = 0; y < size; y += 1)
            {
                var rowBase = y << RowShift;
                for (var w = 0; w < lastWord; w += 1)
                {
                    Raw[rowBase + w] = ~Raw[rowBase + w];
                }
                Raw[rowBase + lastWord] = ~Raw[rowBase + lastWord] & lastMask;
                for (var w = lastWord + 1; w < wordsPerRow; w += 1)
                {
                    Raw[rowBase + w] = 0;
                }
            }
        }

        /// <summary>
        /// Transposes this matrix in place (reflects bits across the main diagonal).
        /// <para>
        /// The matrix is processed as a grid of 64 × 64 bit blocks. Each block is transposed with a
        /// sequence of delta swaps, and blocks off the diagonal are exchanged pairwise. A matrix
        /// with a single word of modules per row is a single block.
        /// </para>
        /// </summary>
        [SuppressMessage("csharpsquid", "S2234")]
        internal void Transpose()
        {
            var size = Size;
            if (size <= 1)
            {
                return;
            }

            var nBlocks = (size + 63) >> 6;
            var blockA = new ulong[64];
            var blockB = new ulong[64];

            for (var br = 0; br < nBlocks; br += 1)
            {
                GatherBlock(blockA, br, br);
                Transpose64X64(blockA);
                ScatterBlock(blockA, br, br);

                for (var bc = br + 1; bc < nBlocks; bc += 1)
                {
                    GatherBlock(blockA, br, bc);
                    GatherBlock(blockB, bc, br);
                    Transpose64X64(blockA);
                    Transpose64X64(blockB);
                    ScatterBlock(blockA, bc, br);
                    ScatterBlock(blockB, br, bc);
                }
            }
        }

        private void GatherBlock(ulong[] dest, int br, int bc)
        {
            var rowStart = br << 6;
            var rows = Math.Min(Size - rowStart, 64);
            for (var i = 0; i < rows; i += 1)
            {
                dest[i] = Raw[((rowStart + i) << RowShift) + bc];
            }
            for (var i = rows; i < 64; i += 1)
            {
                dest[i] = 0;
            }
        }

        private void ScatterBlock(ulong[] src, int br, int bc)
        {
            var rowStart = br << 6;
            var rows = Math.Min(Size - rowStart, 64);
            for (var i = 0; i < rows; i += 1)
            {
                Raw[((rowStart + i) << RowShift) + bc] = src[i];
            }
        }

        private static void Transpose64X64(ulong[] a)
        {
            DeltaSwap(a, 32, 0x00000000FFFFFFFFul);
            DeltaSwap(a, 16, 0x0000FFFF0000FFFFul);
            DeltaSwap(a,  8, 0x00FF00FF00FF00FFul);
            DeltaSwap(a,  4, 0x0F0F0F0F0F0F0F0Ful);
            DeltaSwap(a,  2, 0x3333333333333333ul);
            DeltaSwap(a,  1, 0x5555555555555555ul);
        }

        private static void DeltaSwap(ulong[] a, int j, ulong m)
        {
            for (var k = 0; k < 64; k = (k + j + 1) & ~j)
            {
                var t = ((a[k] >> j) ^ a[k + j]) & m;
                a[k + j] ^= t;
                a[k] ^= t << j;
            }
        }

        /// <summary>
        /// Bitwise ANDs the specified matrix into this one in place.
        /// </summary>
        /// <param name="other">The matrix to AND with.</param>
        /// <exception cref="ArgumentException">Thrown if the matrices have different sizes.</exception>
        internal void And(BitMatrix other)
        {
            if (other.Size != Size)
            {
                throw new ArgumentException("The matrices must have the same size", nameof(other));
            }

            for (var i = 0; i < Raw.Length; i += 1)
            {
                Raw[i] &= other.Raw[i];
            }
        }

        /// <summary>
        /// Bitwise XORs the specified matrix into this one in place.
        /// </summary>
        /// <param name="other">The matrix to XOR with.</param>
        /// <exception cref="ArgumentException">Thrown if the matrices have different sizes.</exception>
        internal void Xor(BitMatrix other)
        {
            if (other.Size != Size)
            {
                throw new ArgumentException("The matrices must have the same size", nameof(other));
            }

            for (var i = 0; i < Raw.Length; i += 1)
            {
                Raw[i] ^= other.Raw[i];
            }
        }

        /// <summary>
        /// Creates a copy of this bit matrix.
        /// </summary>
        /// <returns>A new <see cref="BitMatrix"/> with the same contents.</returns>
        internal BitMatrix Copy()
        {
            return new BitMatrix((ulong[])Raw.Clone(), Size, RowShift, UsedWordsPerRow);
        }

        /// <summary>
        /// Provides access to the underlying raw data.
        /// <para>
        /// The bits are saved in an array of 64-bit integers, <see cref="WordsPerRow"/> integers
        /// per row, of which the first <see cref="UsedWordsPerRow"/> hold modules.
        /// The array is not copied: modifying it modifies this matrix.
        /// </para>
        /// </summary>
        internal ulong[] Raw { get; }

        /// <summary>
        /// Gets this bit matrix as a 2-dimensional boolean array.
        /// <para>
        /// The resulting array is in row-major order.
        /// </para>
        /// </summary>
        /// <returns>A <c>bool</c> array.</returns>
        internal bool[,] ToBoolArray()
        {
            var array = new bool[Size, Size];
            for (var y = 0; y < Size; y += 1)
            {
                for (var x = 0; x < Size; x += 1)
                {
                    array[y, x] = Get(x, y);
                }
            }
            
            return array;
        }

        /// <summary>
        /// Returns the number of bits set in this matrix
        /// (aka population count).
        /// </summary>
        /// <returns>The number of bits.</returns>
        internal int PopCount()
        {
            var sum = 0;
            foreach (var v in Raw)
            {
                sum += PopCount(v);
            }
            return sum;
        }

        /// <summary>
        /// Returns the number of bits set in this value
        /// (aka population count).
        /// </summary>
        /// <returns>The value.</returns>
        internal static int PopCount(ulong i)
        {
#if NET6_0_OR_GREATER
            // Test for X64. ARM has no scalar popcount instruction,
            // and the SIMD instruction CNT involves too much overhead
            // going from the general purpose registers to SIMD and back.
            // The JIT treats Popcnt.X64.IsSupported as a compile-time constant
            // and eliminates the dead branch entirely.
            if (System.Runtime.Intrinsics.X86.Popcnt.X64.IsSupported)
                return BitOperations.PopCount(i);
#endif
            i = i - ((i >> 1) & 0x5555555555555555UL);
            i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }
    }
}
