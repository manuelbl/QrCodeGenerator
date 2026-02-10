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
    /// A stream of bits for encoding the QR code payload.
    /// <para>
    /// Bit streams use a big endian order for both bytes and bits.
    /// </para>
    /// <para>
    /// Instances of this class must be allocated with sufficient
    /// capacity. They cannot grow.
    /// </para>
    /// </summary>
    internal class BitStream
    {
        private readonly byte[] _codewords;
        private readonly int _capacity; // in bytes

        /// <summary>
        /// Creates a new instance with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity, in bytes.</param>
        internal BitStream(int capacity)
        {
            Length = 0;
            _capacity = capacity;
            _codewords = new byte[capacity];
        }
        
        /// <summary>
        /// Length of the bit stream, in bits.
        /// </summary>
        internal int Length { get; private set; }

        /// <summary>
        /// Appends the specified value (with the specified number of bits).
        /// <para>
        /// The passed value must be in the range 0 &#x2264; value &lt; 2<sup>length</sup>.
        /// </para>
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <param name="length">The number of  bits to append.</param>
        /// <exception cref="ArgumentOutOfRangeException">Value or number of bits is out of range.</exception>
        internal void AppendBits(uint value, int length)
        {
            if (length <= 0 || length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length must be between 1 and 32");
            }

            if (length < 32 && value >> length != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value must be in the range 0 <= value < 2^length");
            }

            var newLength = Length + length;
            if (newLength > _capacity * 8)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "adding the specified length exceeds the capacity");
            }

            var valueMask = 1U << (length - 1);
            for (var i = Length; i < newLength; i += 1)
            {
                if ((value & valueMask) != 0)
                {
                    var codewordMask = (byte)(1U << (7 - (i & 7)));
                    _codewords[i >> 3] |= codewordMask;
                }
                valueMask >>= 1;
            }

            Length = newLength;
        }
        
        /// <summary>
        /// Extracts the specified number of bits at the specified index.
        /// <para>
        /// Requires 0 &lt; <em>len</em> &#x2264; 31.
        /// </para>
        /// </summary>
        /// <param name="index">The index of the first bit to extract.</param>
        /// <param name="length">The number of bits to extract.</param>
        /// <returns>The extracted bits as an unsigned integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index or length is out of range.</exception>
        internal uint ExtractBits(int index, int length)
        {
            if (length <= 0 || length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length must be between 1 and 32");
            }

            if (index < 0 || index + length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "'index' out of range");
            }

            var result = 0u;
            var valueMask = 1u << (length - 1);
            for (var i = index; i < index + length; i += 1)
            {
                var codewordMask = (byte)(1U << (7 - (i & 7)));
                if ((_codewords[i >> 3] & codewordMask) != 0)
                {
                    result |= valueMask;
                }
                valueMask >>= 1;
            }

            return result;
        }

        /// <summary>
        /// Gets the QR code bit stream as 8-bit codewords.
        /// <para>
        /// If the bit stream is not a multiple of 8 bits, the last codeword will be padded with zeros.
        /// </para>
        /// </summary>
        /// <returns>The codewords array.</returns>
        internal byte[] GetCodewords()
        {
            var resultLength = (Length + 7) >> 3;
            var result = new byte[resultLength];
            Array.Copy(_codewords, 0, result, 0, resultLength);
            return result;
        }

        /// <summary>
        /// Copies the QR code bit stream as 8-bit codewords to the specified array.
        /// <para>
        /// If the bit stream is not a multiple of 8 bits, the last codeword will be padded with zeros.
        /// </para>
        /// </summary>
        /// <param name="codewords">The destination array to copy the codewords to.</param>
        /// <param name="index">The starting index in the destination array.</param>
        internal void CopyCodewords(byte[] codewords, int index)
        {
            var resultLength = (Length + 7) >> 3;
            Array.Copy(_codewords, 0, codewords, index, resultLength);
        }
    }
}