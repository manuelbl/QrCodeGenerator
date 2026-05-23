/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Collections.Concurrent;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Single source of truth for the fixed-pattern geometry of a QR code version.
    /// <para>
    /// The fixed patterns are the modules placed identically for a given version
    /// regardless of payload: finder patterns, separators, timing patterns,
    /// alignment patterns and version information, plus the reserved area for the
    /// format information.
    /// </para>
    /// <para>
    /// <see cref="BuildFixedPatterns"/> produces two views of the same geometry in a
    /// single walk:
    /// </para>
    /// <list type="bullet">
    /// <item><c>drawn</c> — the actual dark/light modules of the fixed patterns.</item>
    /// <item><c>reserved</c> — the union of all fixed-pattern footprints, i.e. every
    /// module the payload must not use. A footprint is reserved even where its modules
    /// are light (separators, the light rings of a finder, format/version <c>0</c> bits),
    /// so the reserved view cannot be derived from the drawn one.</item>
    /// </list>
    /// <para>
    /// The format information is reserve-only here; its bits depend on the error
    /// correction level and the chosen mask pattern and are drawn later by
    /// <see cref="QrCodeBuilder.DrawFormatInformation(BitMatrix, int, int)"/>.
    /// </para>
    /// </summary>
    internal static class FixedPatterns
    {
        #region Caches

        // version -> (drawn modules, payload-area map). The payload-area map is the
        // inverted reserved mask (the modules the payload zig-zag may fill). Both are
        // computed by a single BuildFixedPatterns walk and cached together.
        // The cached instances are shared and must not be mutated (CreateWithFixedPatterns
        // hands out a Copy; GetPayloadAreaMap's result is treated as read-only by callers).
        private static readonly ConcurrentDictionary<int, (BitMatrix Drawn, BitMatrix PayloadAreaMap)> Cache
            = new ConcurrentDictionary<int, (BitMatrix, BitMatrix)>();

        private static (BitMatrix Drawn, BitMatrix PayloadAreaMap) GetCached(int version)
        {
            return Cache.GetOrAdd(version, ComputeCached);
        }

        private static (BitMatrix Drawn, BitMatrix PayloadAreaMap) ComputeCached(int version)
        {
            var (drawn, reserved) = BuildFixedPatterns(version);
            // Turn the reserved mask into the payload-area map (where data goes).
            reserved.Invert();
            return (drawn, reserved);
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Creates a <see cref="BitMatrix"/> for the given version with the fixed
        /// patterns (finder, timing, alignment, version) already drawn.
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>A new (mutable) <see cref="BitMatrix"/> instance.</returns>
        internal static BitMatrix CreateWithFixedPatterns(int version)
        {
            return GetCached(version).Drawn.Copy();
        }

        /// <summary>
        /// Returns the payload-area map for the given version: a <see cref="BitMatrix"/>
        /// with all bits set where payload data goes (the complement of the reserved
        /// modules).
        /// <para>
        /// The returned matrix is shared and cached. Callers must not mutate it.
        /// </para>
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>A shared <see cref="BitMatrix"/> instance.</returns>
        internal static BitMatrix GetPayloadAreaMap(int version)
        {
            return GetCached(version).PayloadAreaMap;
        }

        #endregion

        #region Single walk

        /// <summary>
        /// Builds the fixed-pattern geometry for the given version in a single walk.
        /// <para>
        /// Each region stamps its dark modules into <c>drawn</c> and reserves its
        /// footprint into <c>reserved</c> in the same place, so the two views cannot
        /// drift apart. The drawn invariant <c>drawn AND NOT reserved == 0</c> holds
        /// by construction.
        /// </para>
        /// <para>
        /// Draw order is load-bearing: alignment patterns are drawn after the timing
        /// patterns so they overwrite the timing line where the two overlap.
        /// </para>
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The drawn modules and the reserved-module mask.</returns>
        internal static (BitMatrix Drawn, BitMatrix Reserved) BuildFixedPatterns(int version)
        {
            var size = QrCodeBuilder.GetSize(version);
            var drawn = new BitMatrix(size);
            var reserved = new BitMatrix(size);

            // Single asymmetric dark module
            drawn.Set(8, size - 8, true);
            reserved.Set(8, size - 8, true);

            // Format information (reserve-only; bits drawn later, ecc/mask-dependent)
            ReserveFormatInformation(reserved);

            // Version information (versions 7 and up)
            DrawVersionInformation(drawn, version);
            ReserveVersionInformation(reserved, version);

            // Finder patterns and adjacent separators (footprint 8x8, pattern 7x7)
            DrawFinderPattern(drawn, 0, 0);
            DrawFinderPattern(drawn, size - 7, 0);
            DrawFinderPattern(drawn, 0, size - 7);
            reserved.FillRect(0, 0, 8, 8);
            reserved.FillRect(size - 8, 0, 8, 8);
            reserved.FillRect(0, size - 8, 8, 8);

            // Timing patterns
            DrawTimingPattern(drawn);
            reserved.FillRect(8, 6, size - 16, 1);
            reserved.FillRect(6, 8, 1, size - 16);

            // Alignment patterns (drawn after timing so they overwrite it on overlap)
            DrawAndReserveAlignmentPatterns(drawn, reserved, version);

            return (drawn, reserved);
        }

        #endregion

        #region Finder patterns

        private static void DrawFinderPattern(BitMatrix modules, int x, int y)
        {
            for (var i = 0; i < 7; i += 1)
            {
                modules.Set(x + i, y,     true);
                modules.Set(x + i, y + 6, true);
            }

            for (var i = 1; i < 6; i += 1)
            {
                modules.Set(x,     y + i, true);
                modules.Set(x + 1, y + i, false);
                modules.Set(x + 5, y + i, false);
                modules.Set(x + 6, y + i, true);
            }

            for (var i = 2; i < 5; i += 1)
            {
                modules.Set(x + i, y + 1, false);
                modules.Set(x + i, y + 5, false);
            }

            for (var i = 2; i < 5; i += 1)
            {
                modules.Set(x + i, y + 2, true);
                modules.Set(x + i, y + 3, true);
                modules.Set(x + i, y + 4, true);
            }
        }

        #endregion

        #region Alignment patterns

        private static void DrawAndReserveAlignmentPatterns(BitMatrix drawn, BitMatrix reserved, int version)
        {
            if (version == 1)
            {
                // no alignment patterns in version 1
                return;
            }

            var positions = QrCodeBuilder.GetAlignmentPatternPosition(version);
            var numPositions = positions.Length;

            for (var x = 0; x < numPositions; x += 1)
            {
                for (var y = 0; y < numPositions; y += 1)
                {
                    if ((x == 0 && y == 0) || (x == numPositions - 1 && y == 0) || (x == 0 && y == numPositions - 1))
                    {
                        // no alignment patterns near the 3 finder patterns
                        continue;
                    }

                    reserved.FillRect(positions[x] - 2, positions[y] - 2, 5, 5);
                    DrawAlignmentPattern(drawn, positions[x], positions[y]);
                }
            }
        }

        private static void DrawAlignmentPattern(BitMatrix modules, int x, int y)
        {
            for (var i = -2; i <= 2; i += 1)
            {
                modules.Set(x + i, y - 2, true);
                modules.Set(x + i, y + 2, true);
            }

            for (var i = -1; i <= 1; i += 1)
            {
                modules.Set(x - 2, y + i, true);
                modules.Set(x + 2, y + i, true);
            }

            modules.Set(x, y, true);
        }

        #endregion

        #region Timing patterns

        private static void DrawTimingPattern(BitMatrix modules)
        {
            var size = modules.Size;
            for (var x = 8; x < size - 8; x += 1)
            {
                var isDark = ((x + 1) & 1) != 0;
                modules.Set(x, 6, isDark);
                modules.Set(6, x, isDark);
            }
        }

        #endregion

        #region Version information

        private static void ReserveVersionInformation(BitMatrix reserved, int version)
        {
            if (version < 7)
            {
                // no version information for versions 1-6
                return;
            }

            var size = reserved.Size;
            // left bottom corner
            reserved.FillRect(0, size - 11, 6, 3);

            // top right corner
            reserved.FillRect(size - 11, 0, 3, 6);
        }

        private static void DrawVersionInformation(BitMatrix modules, int version)
        {
            if (version < 7)
            {
                // no version information for versions 1-6
                return;
            }

            var size = modules.Size;
            var bits = QrCodeBuilder.GetVersionInformationBits(version);

            for (var bit = 0; bit < 18; bit += 1)
            {
                var isDark = (bits & (1 << bit)) != 0;
                var x = bit / 3;
                var y = bit % 3;

                // left bottom corner
                modules.Set(x, size - 11 + y, isDark);

                // top right corner
                modules.Set(size - 11 + y, x, isDark);
            }
        }

        #endregion

        #region Format information

        private static void ReserveFormatInformation(BitMatrix reserved)
        {
            reserved.FillRect(8, 0, 1, 9);
            reserved.FillRect(0, 8, 8, 1);
            reserved.FillRect(reserved.Size - 8, 8, 8, 1);
            reserved.FillRect(8, reserved.Size - 7, 1, 7);
        }

        #endregion
    }
}
