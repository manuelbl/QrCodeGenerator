/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Reed-Solomon error detection and correction encoder for QR codes.
    /// <para>
    /// Instances of this class generate the error correction codewords
    /// for QR code data blocks using Reed-Solomon error correction codes over
    /// the Galois field GF(256).
    /// </para>
    /// <para>
    /// The implementation uses a generator polynomial approach with configurable
    /// error correction capacity.
    /// </para>
    /// <para>
    /// Instances of this class are immutable and thread-safe.
    /// They are created by the factory method <see cref="GeneratorForCapacity"/>,
    /// which uses a cache to reuse instances for the same ECC count.
    /// </para>
    /// </summary>
    internal sealed class ReedSolomon
    {
        #region Fields

        /// <summary>
        /// The generator polynomial for the Reed-Solomon code.
        /// </summary>
        private readonly int[] _generatorPolynomial;

        /// <summary>
        /// The error correction capacity (number of error correction codewords).
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// Cache of instances for different ECC counts.
        /// </summary>
        private static readonly ConcurrentDictionary<int, ReedSolomon> GeneratorCache = new ConcurrentDictionary<int, ReedSolomon>();

        #endregion
        
        #region Constructors

        /// <summary>
        /// Returns an instance of <see cref="ReedSolomon"/> for the specified number of error correction codewords.
        /// <para>
        /// If possible, an existing instance from the cache is returned
        /// to reuse the already computed generator polynomial.
        /// </para>
        /// </summary>
        /// <param name="capacity">The error correction capacity (number of error correction codewords).</param>
        /// <returns>A <see cref="ReedSolomon"/> instance.</returns>
        internal static ReedSolomon GeneratorForCapacity(int capacity)
        {
            if (capacity < 1 || capacity > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    "Error correction capacity must be between 1 and 255");
            }

            return GeneratorCache.GetOrAdd(capacity, CreateForCapacity);
        }

        private static ReedSolomon CreateForCapacity(int capacity)
        {
            var poly = ComputeGeneratorPolynomial(capacity);
            return new ReedSolomon(capacity, poly);
        }

        private ReedSolomon(int capacity, int[] generatorPolynomial)
        {
            _capacity = capacity;
            _generatorPolynomial = generatorPolynomial;
        }

        #endregion

        #region Main Methods

        /// <summary>
        /// Computes the error correction codewords for the specified data.
        /// </summary>
        /// <param name="data">The data codewords.</param>
        /// <returns>The error correction codewords.</returns>
        internal byte[] ComputeErrorCorrection(ArraySegment<byte> data)
        {
            // Initialize the remainder with zeros
            var remainder = new byte[_capacity];

            // Perform polynomial division
            foreach (var cw in data) {
                var coefficient = (byte)(cw ^ remainder[0]);

                // Multiply and add
                for (var j = 0; j < _capacity - 1; j += 1)
                {
                    remainder[j] = (byte)(remainder[j + 1] ^ GfMultiply(coefficient, _generatorPolynomial[j]));
                }
                remainder[_capacity - 1] = GfMultiply(coefficient, _generatorPolynomial[_capacity - 1]);
            }

            return remainder;
        }

        #endregion

        #region Private Methods - Generator Polynomial

        /// <summary>
        /// Computes the generator polynomial for Reed-Solomon encoding.
        /// </summary>
        /// <remarks>
        /// The generator polynomial is the product of (x - α^i) for i = 0 to eccCount-1,
        /// where α is the primitive element (2) in GF(256).
        /// </remarks>
        /// <param name="eccCount">The number of error correction codewords.</param>
        /// <returns>The coefficients of the generator polynomial.</returns>
        private static int[] ComputeGeneratorPolynomial(int eccCount)
        {
            // Start with polynomial: 1
            var poly = new int[eccCount + 1];
            poly[0] = 1;

            // Multiply by (x - α^i) for each i
            for (var i = 0; i < eccCount; i += 1)
            {
                var alphaI = GfPow(2, i);

                // Multiply current polynomial by (x + α^i)
                for (var j = eccCount; j > 0; j--)
                {
                    poly[j] = poly[j - 1] ^ GfMultiply(poly[j], alphaI);
                }
                poly[0] = GfMultiply(poly[0], alphaI);
            }

            // Return coefficients from highest to lowest power, excluding the leading 1
            var result = new int[eccCount];
            for (var i = 0; i < eccCount; i += 1)
            {
                result[i] = poly[eccCount - 1 - i];
            }
            return result;
        }

        #endregion
        
        #region Private Methods - Galois Field Operations

        /// <summary>
        /// Multiplies two elements in GF(256) using the QR code polynomial.
        /// </summary>
        /// <remarks>
        /// Uses the irreducible polynomial x^8 + x^4 + x^3 + x^2 + 1 (0x11D).
        /// </remarks>
        /// <param name="a">First operand (0-255).</param>
        /// <param name="b">Second operand (0-255).</param>
        /// <returns>The product a × b in GF(256).</returns>
        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        private static byte GfMultiply(int a, int b)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }

            // Use logarithm tables for efficiency
            return GaloisFieldExp[(GaloisFieldLog[a] + GaloisFieldLog[b]) % 255];
        }

        /// <summary>
        /// Computes the power of an element in GF(256).
        /// </summary>
        /// <param name="base">The base element.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The result of base^exponent in GF(256).</returns>
        private static int GfPow(int @base, int exponent)
        {
            if (exponent == 0)
            {
                return 1;
            }

            if (@base == 0)
            {
                return 0;
            }

            return GaloisFieldExp[GaloisFieldLog[@base] * exponent % 255];
        }

        #endregion
        
        #region Galois Field Tables

        /// <summary>
        /// Logarithm table for GF(256) with generator α = 2.
        /// GF_LOG[i] = k where α^k = i.
        /// </summary>
        private static readonly byte[] GaloisFieldLog =
        {
            0,   0,   1,  25,   2,  50,  26, 198,   3, 223,  51, 238,  27, 104, 199,  75,
            4, 100, 224,  14,  52, 141, 239, 129,  28, 193, 105, 248, 200,   8,  76, 113,
            5, 138, 101,  47, 225,  36,  15,  33,  53, 147, 142, 218, 240,  18, 130,  69,
           29, 181, 194, 125, 106,  39, 249, 185, 201, 154,   9, 120,  77, 228, 114, 166,
            6, 191, 139,  98, 102, 221,  48, 253, 226, 152,  37, 179,  16, 145,  34, 136,
           54, 208, 148, 206, 143, 150, 219, 189, 241, 210,  19,  92, 131,  56,  70,  64,
           30,  66, 182, 163, 195,  72, 126, 110, 107,  58,  40,  84, 250, 133, 186,  61,
          202,  94, 155, 159,  10,  21, 121,  43,  78, 212, 229, 172, 115, 243, 167,  87,
            7, 112, 192, 247, 140, 128,  99,  13, 103,  74, 222, 237,  49, 197, 254,  24,
          227, 165, 153, 119,  38, 184, 180, 124,  17,  68, 146, 217,  35,  32, 137,  46,
           55,  63, 209,  91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190,  97,
          242,  86, 211, 171,  20,  42,  93, 158, 132,  60,  57,  83,  71, 109,  65, 162,
           31,  45,  67, 216, 183, 123, 164, 118, 196,  23,  73, 236, 127,  12, 111, 246,
          108, 161,  59,  82,  41, 157,  85, 170, 251,  96, 134, 177, 187, 204,  62,  90,
          203,  89,  95, 176, 156, 169, 160,  81,  11, 245,  22, 235, 122, 117,  44, 215,
           79, 174, 213, 233, 230, 231, 173, 232, 116, 214, 244, 234, 168,  80,  88, 175
        };

        /// <summary>
        /// Exponential table for GF(256) with generator α = 2.
        /// GF_EXP[k] = α^k.
        /// </summary>
        private static readonly byte[] GaloisFieldExp =
        {
            1,   2,   4,   8,  16,  32,  64, 128,  29,  58, 116, 232, 205, 135,  19,  38,
           76, 152,  45,  90, 180, 117, 234, 201, 143,   3,   6,  12,  24,  48,  96, 192,
          157,  39,  78, 156,  37,  74, 148,  53, 106, 212, 181, 119, 238, 193, 159,  35,
           70, 140,   5,  10,  20,  40,  80, 160,  93, 186, 105, 210, 185, 111, 222, 161,
           95, 190,  97, 194, 153,  47,  94, 188, 101, 202, 137,  15,  30,  60, 120, 240,
          253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163,  91, 182, 113, 226,
          217, 175,  67, 134,  17,  34,  68, 136,  13,  26,  52, 104, 208, 189, 103, 206,
          129,  31,  62, 124, 248, 237, 199, 147,  59, 118, 236, 197, 151,  51, 102, 204,
          133,  23,  46,  92, 184, 109, 218, 169,  79, 158,  33,  66, 132,  21,  42,  84,
          168,  77, 154,  41,  82, 164,  85, 170,  73, 146,  57, 114, 228, 213, 183, 115,
          230, 209, 191,  99, 198, 145,  63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
          227, 219, 171,  75, 150,  49,  98, 196, 149,  55, 110, 220, 165,  87, 174,  65,
          130,  25,  50, 100, 200, 141,   7,  14,  28,  56, 112, 224, 221, 167,  83, 166,
           81, 162,  89, 178, 121, 242, 249, 239, 195, 155,  43,  86, 172,  69, 138,   9,
           18,  36,  72, 144,  61, 122, 244, 245, 247, 243, 251, 235, 203, 139,  11,  22,
           44,  88, 176, 125, 250, 233, 207, 131,  27,  54, 108, 216, 173,  71, 142,   1
        };

        #endregion
    }
}

