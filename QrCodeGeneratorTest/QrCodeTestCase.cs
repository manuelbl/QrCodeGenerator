/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Collections.Generic;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public record QrCodeTestCase(
        int Index,
        List<DataSegment> Segments,
        string[] ExpectedModules,
        Ecc RequestedEcc,
        int MinVersion,
        int MaxVersion,
        bool BoostEcl,
        Ecc EffectiveEcc,
        int EffectiveVersion,
        int EffectiveMask)
    {
        public override string ToString()
        {
            return $"Case {Index}";
        }
    }
}
