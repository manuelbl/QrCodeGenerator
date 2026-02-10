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
    /// Data segment for Extended Channel Interpretation (ECI).
    /// </summary>
    internal class DataSegmentEci : DataSegment
    {
        /// <summary>
        /// Extended Channel Interpretation (ECI) designator.
        /// </summary>
        internal ECI Designator { get;  }

        internal DataSegmentEci(ECI eci)
            : base(DataSegmentMode.ECI, GetEciBitLength(eci.Value))
        {
            if (eci.Value < 0 || eci.Value > 999999)
            {
                throw new ArgumentOutOfRangeException(nameof(eci), eci, "ECI value must be between 0 and 999999");
            }
            
            Designator = eci;
        }
        
        private static int GetEciBitLength(int value)
        {
            if (value <= 127)
            {
                return 8;
            }
            if (value <= 16383)
            {
                return 16;
            }

            return 24;
        }

        internal override void WriteToBitStream(BitStream bitStream)
        {
            var eciValue = (uint)Designator.Value;
            if (eciValue <= 127)
            {
                bitStream.AppendBits(eciValue, 8);
            }
            else if (eciValue <= 16383)
            {
                bitStream.AppendBits(0b_10000000_00000000 | eciValue, 16);
            }
            else if (eciValue <= 999999)
            {
                bitStream.AppendBits(0b_11000000_00000000_00000000 | eciValue, 24);
            }
        }
    }
}

