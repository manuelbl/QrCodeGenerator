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
    public struct QrCodeTestCase
    {
        public QrCodeTestCase(int index,
        List<DataSegment> segments,
        string[] expectedModules,
        Ecc requestedEcc,
        int minVersion,
        int maxVersion,
        bool boostEcl,
        Ecc effectiveEcc,
        int effectiveVersion,
        int effectiveMask)
        {
            Index = index;
            Segments = segments;
            ExpectedModules = expectedModules;
            RequestedEcc = requestedEcc;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
            BoostEcl = boostEcl;
            EffectiveEcc = effectiveEcc;
            EffectiveVersion = effectiveVersion;
            EffectiveMask = effectiveMask;
        }

        public int Index { get; }
        public List<DataSegment> Segments { get; }
        public string[] ExpectedModules { get; }
        public Ecc RequestedEcc { get; }
        public int MinVersion { get; }
        public int MaxVersion { get; }
        public bool BoostEcl { get; }
        public Ecc EffectiveEcc { get; }
        public int EffectiveVersion { get; }
        public int EffectiveMask { get;  }

        public override string ToString()
        {
            return $"Case {Index}";
        }
    }
}
