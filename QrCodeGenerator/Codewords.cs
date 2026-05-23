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
    /// Turns data segments into the codeword stream the matrix is filled with:
    /// first the data codewords (terminator + padding), then the Reed-Solomon error
    /// correction codewords, interleaved per specification.
    /// </summary>
    internal static class Codewords
    {
        /// <summary>
        /// Builds the data codewords for the given data segments.
        /// <para>
        /// The result includes the terminator and the padding for the
        /// given QR code version and error correction level.
        /// </para>
        /// </summary>
        /// <param name="dataSegments">The data segments to encode.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The data codewords.</returns>
        internal static byte[] BuildData(List<DataSegment> dataSegments, int version, int ecc)
        {
            var capacity = QrCodeParameters.GetCodewordDataCapacity(version, ecc);
            var bitStream = DataSegment.CreateBitStream(dataSegments, version, capacity);
            var bitstreamLength = bitStream.Length;

            // TODO: avoid allocation of another array
            var result = new byte[capacity];
            bitStream.CopyCodewords(result, 0);

            // add padding
            for (var index = (bitstreamLength + 7) / 8; index < capacity; index += 2)
            {
                result[index] = 0b1110_1100;
            }
            for (var index = (bitstreamLength + 15) / 8; index < capacity; index += 2)
            {
                result[index] = 0b0001_0001;
            }

            return result;
        }

        /// <summary>
        /// Adds the error correction to the given codewords and returns the combined result.
        /// <para>
        /// The result is transposed and interleaved as per specification.
        /// </para>
        /// </summary>
        /// <param name="codewords">The data codewords.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>The combined result of data and error correction codewords.</returns>
        internal static byte[] AddErrorCorrection(byte[] codewords, int version, int ecc)
        {
            var numDataCodewords = codewords.Length;
            var numBlocks = QrCodeParameters.GetNumBlocks(version, ecc);
            var smallBlockDataLength = numDataCodewords / numBlocks;
            var eccBlockLength = (QrCodeParameters.GetCodewordCapacity(version) - numDataCodewords) / numBlocks;
            var numLargeBlocks = numDataCodewords % numBlocks;
            var numSmallBlocks = numBlocks - numLargeBlocks;

            var result = new byte[QrCodeParameters.GetCodewordCapacity(version)];
            var dataOffset = 0;
            var reedSolomon = ReedSolomon.GeneratorForCapacity(eccBlockLength);

            // split into blocks and process each block separately
            for (var block = 0; block < numBlocks; block += 1)
            {
                var dataLength = block < numSmallBlocks ? smallBlockDataLength : smallBlockDataLength + 1;

                // compute the error correction codewords
                var eccCodewords = reedSolomon.ComputeErrorCorrection(new ArraySegment<byte>(codewords, dataOffset, dataLength));

                // copy the data and error correction codewords to the transposed result
                for (var i = 0; i < smallBlockDataLength; i += 1)
                    result[i * numBlocks + block] = codewords[dataOffset + i];
                if (block >= numSmallBlocks)
                    result[numBlocks * smallBlockDataLength + block - numSmallBlocks] = codewords[dataOffset + dataLength - 1];
                for (var i = 0; i < eccBlockLength; i += 1)
                    result[numDataCodewords + i * numBlocks + block] = eccCodewords[i];

                dataOffset += dataLength;
            }

            return result;
        }
    }
}
