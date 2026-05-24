/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// The two views of a single mask pattern: the mask as stored (<see cref="Rows"/>)
    /// and its transpose (<see cref="Columns"/>).
    /// <para>
    /// A mask pair is cached per (mask pattern, version) and XORed into a
    /// <see cref="ScoringMatrix"/> as a unit, so the scoring matrix and its transpose
    /// stay in sync. The contained matrices are shared and cached; callers must not
    /// mutate them.
    /// </para>
    /// </summary>
    internal readonly struct MaskPair
    {
        /// <summary>
        /// Creates a mask pair from a mask and its transpose.
        /// </summary>
        /// <param name="rows">The mask as stored.</param>
        /// <param name="columns">The transpose of the mask.</param>
        internal MaskPair(BitMatrix rows, BitMatrix columns)
        {
            Rows = rows;
            Columns = columns;
        }

        /// <summary>The mask as stored.</summary>
        internal BitMatrix Rows { get; }

        /// <summary>The transpose of the mask.</summary>
        internal BitMatrix Columns { get; }
    }
}
