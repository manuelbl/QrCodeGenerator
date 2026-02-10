/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// A module matrix paired with its transpose, kept in sync, used while selecting
    /// the mask pattern.
    /// <para>
    /// The <see cref="Rows"/> view is the matrix as stored; the <see cref="Columns"/>
    /// view is its transpose. Penalty rules that scan rows read <see cref="Rows"/>;
    /// rules that scan columns read <see cref="Columns"/>, so the column rules reuse the
    /// row algorithm instead of duplicating it (see <see cref="Penalty"/>).
    /// </para>
    /// <para>
    /// Every mutation (<see cref="Xor"/>, <see cref="SetFormatBit"/>) updates both views
    /// together, so they cannot drift apart. This struct owns that invariant; callers
    /// never maintain the transpose by hand.
    /// </para>
    /// </summary>
    internal readonly struct ScoringMatrix
    {
        private ScoringMatrix(BitMatrix rows, BitMatrix columns)
        {
            Rows = rows;
            Columns = columns;
        }

        /// <summary>
        /// Creates a scoring matrix for the given module matrix.
        /// <para>
        /// The matrix is taken as the <see cref="Rows"/> view <em>without copying</em>:
        /// it is mutated in place by <see cref="Xor"/>, <see cref="SetFormatBit"/> and
        /// <see cref="Finish"/>, and remains the matrix returned by <see cref="Finish"/>.
        /// The <see cref="Columns"/> view is built as its transpose.
        /// </para>
        /// </summary>
        /// <param name="modules">The module matrix to score (used as <see cref="Rows"/>).</param>
        /// <returns>A new scoring matrix.</returns>
        internal static ScoringMatrix From(BitMatrix modules)
        {
            var columns = modules.Copy();
            columns.Transpose();
            return new ScoringMatrix(modules, columns);
        }

        /// <summary>The matrix as stored.</summary>
        internal BitMatrix Rows { get; }

        /// <summary>The transpose of the matrix.</summary>
        internal BitMatrix Columns { get; }

        /// <summary>The side length (in modules) of the matrix.</summary>
        internal int Size => Rows.Size;

        /// <summary>
        /// XORs the given mask into both views, keeping them in sync.
        /// </summary>
        /// <param name="mask">The mask pair to XOR in.</param>
        internal void Xor(MaskPair mask)
        {
            Rows.Xor(mask.Rows);
            Columns.Xor(mask.Columns);
        }

        /// <summary>
        /// Sets a format-information bit, updating both views so they stay in sync.
        /// </summary>
        /// <param name="x">The x-coordinate in the <see cref="Rows"/> view.</param>
        /// <param name="y">The y-coordinate in the <see cref="Rows"/> view.</param>
        /// <param name="value">The bit value.</param>
        [SuppressMessage("csharpsquid", "S2234")]
        internal void SetFormatBit(int x, int y, bool value)
        {
            Rows.Set(x, y, value);
            Columns.Set(y, x, value);
        }

        /// <summary>
        /// Applies the chosen mask to the finished matrix and returns it.
        /// <para>
        /// Only the <see cref="Rows"/> view is updated: the <see cref="Columns"/> view is
        /// discarded once the mask pattern has been selected, so its mask XOR is skipped.
        /// After <see cref="Finish"/> the two views are no longer in sync.
        /// </para>
        /// </summary>
        /// <param name="mask">The chosen mask pattern's mask pair.</param>
        /// <returns>The finished module matrix (the <see cref="Rows"/> view).</returns>
        internal BitMatrix Finish(MaskPair mask)
        {
            Rows.Xor(mask.Rows);
            return Rows;
        }
    }
}
