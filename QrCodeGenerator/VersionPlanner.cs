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
    /// Chooses the QR code version (size) and error correction level for a set of
    /// data segments: the smallest version in range that fits, then the highest ECC
    /// level that still fits at that version (if boosting is enabled).
    /// </summary>
    internal static class VersionPlanner
    {
        /// <summary>
        /// Plans the version and error correction level for the given data segments.
        /// <para>
        /// The smallest version in <c>[minVersion, maxVersion]</c> that fits the data
        /// is selected. If <paramref name="boostEcc"/> is <c>true</c>,
        /// <paramref name="ecc"/> is treated as the minimum level and is increased as
        /// far as possible without growing the version.
        /// </para>
        /// </summary>
        /// <param name="dataSegments">The data segments to encode.</param>
        /// <param name="ecc">The (minimum) error correction level.</param>
        /// <param name="minVersion">The minimal QR code version.</param>
        /// <param name="maxVersion">The maximal QR code version.</param>
        /// <param name="boostEcc">If <c>true</c>, increase the error correction level if possible.</param>
        /// <returns>The chosen version and error correction level.</returns>
        /// <exception cref="DataTooLongException">Thrown if the data does not fit into a QR code in the given range.</exception>
        internal static (int Version, int Ecc) Plan(List<DataSegment> dataSegments, int ecc,
            int minVersion = 1, int maxVersion = 40, bool boostEcc = true)
        {
            var (version, bitLength) = FindSmallestVersion(dataSegments, ecc, minVersion, maxVersion);

            if (boostEcc)
            {
                while (ecc < 3 && Fits(bitLength, version, ecc + 1))
                {
                    ecc += 1;
                }
            }

            return (version, ecc);
        }

        private static (int Version, int BitLength) FindSmallestVersion(List<DataSegment> dataSegments, int ecc, int minVersion, int maxVersion)
        {
            var bitLength = 0;
            int version;
            // find the smallest version that fits the data
            for (version = minVersion; version <= maxVersion; version += 1)
            {
                if (version == minVersion || version == 1 || version == 10 || version == 27)
                {
                    // update the bit length as it changes at these versions
                    bitLength = DataSegment.GetBitLength(dataSegments, version);
                }

                if (Fits(bitLength, version, ecc))
                {
                    break;
                }

                if (version == maxVersion)
                {
                    if (version < 40)
                    {
                        throw new DataTooLongException(
                            $"Data is too long to fit into a QR code with version {version} and error correction level {"LMQH"[ecc]}.");
                    }

                    throw new DataTooLongException(
                        $"Data is too long to fit into a QR code with error correction level {"LMQH"[ecc]}");
                }
            }

            return (version, bitLength);
        }

        /// <summary>
        /// Tests if the given number of data bits will fit into a QR code of the given version and error correction level.
        /// </summary>
        /// <param name="bitLength">The number of data bits.</param>
        /// <param name="version">The version of the QR code.</param>
        /// <param name="ecc">The error correction level.</param>
        /// <returns>True if the data fits, false otherwise.</returns>
        private static bool Fits(int bitLength, int version, int ecc)
        {
            return bitLength <= 8 * QrCodeParameters.GetCodewordDataCapacity(version, ecc);
        }
    }
}
