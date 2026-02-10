/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Orchestrates the QR code encoding pipeline by wiring together the stages:
    /// version/ECC planning (<see cref="VersionPlanner"/>), codeword assembly
    /// (<see cref="Codewords"/>) and matrix encoding (<see cref="MatrixEncoder"/>).
    /// The parameter tables shared by the stages live in <see cref="QrCodeParameters"/>.
    /// </summary>
    internal static class QrCodeBuilder
    {
        internal static QrCode Build(List<DataSegment> dataSegments, int ecc, int minVersion = 1, int maxVersion = 40, bool boostEcc = true, EncodingInfo encodingInfo = null)
        {
            if (encodingInfo != null)
            {
                encodingInfo.DataSegments = dataSegments;
            }

            var plan = VersionPlanner.Plan(dataSegments, ecc, minVersion, maxVersion, boostEcc);

            var codewords = Codewords.BuildData(dataSegments, plan.Version, plan.Ecc);
            codewords = Codewords.AddErrorCorrection(codewords, plan.Version, plan.Ecc);
            return MatrixEncoder.Encode(codewords, plan.Version, plan.Ecc, encodingInfo);
        }
    }
}
