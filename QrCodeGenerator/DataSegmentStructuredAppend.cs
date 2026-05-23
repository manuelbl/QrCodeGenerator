using System;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Data segment for the structured append header.
    /// </summary>
    internal class DataSegmentStructuredAppend : DataSegment
    {
        private const int StructuredAppendBitLength = 16;
        
        /// <summary>
        /// The position of the QR code within the message.
        /// <para>
        /// Valid positions are between 1 and 16.
        /// </para>
        /// </summary>
        internal readonly int Position;

        /// <summary>
        /// The total number of QR code used for the message.
        /// <para>
        /// Valid numbers are between 1 and 16.
        /// </para>
        /// </summary>
        internal readonly int Total;
        
        /// <summary>
        /// The parity of the encoded data.
        /// </summary>
        internal readonly byte Parity;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="position">The position of the QR code within the message (1 to 16).</param>
        /// <param name="total">The total number of QR code used for the message (1 to 16).</param>
        /// <param name="parity">The message parity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if 1 &#x2264; position &#x2264; total &#x2264; 16 is violated. </exception>
        internal DataSegmentStructuredAppend(int position, int total, byte parity)
            : base(DataSegmentMode.StructuredAppend, StructuredAppendBitLength)
        {
            if (total < 1 || total > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(total), total.ToString(), "total must be between 1 and 16");
            }
            if (position < 1 || position > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(position), position.ToString(), "position must be between 1 and 16");
            }

            if (position > total)
            {
                throw new ArgumentOutOfRangeException(nameof(position), position.ToString(), "position must be less or equal to total");
            }

            Position = position;
            Total = total;
            Parity = parity;
        }
        
        /// <inheritdoc/>
        internal override void WriteToBitStream(BitStream bitStream)
        {
            bitStream.AppendBits((uint)Position - 1, 4);
            bitStream.AppendBits((uint)Total - 1, 4);
            bitStream.AppendBits(Parity, 8);
        }
    }
}