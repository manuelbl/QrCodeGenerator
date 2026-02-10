/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Information about the penalty score of a data mask pattern.
    /// <para>
    /// QR code use one out of eight patterns to improve the readability of the code.
    /// The penalty score measures how difficult it is to read a QR code with a given pattern.
    /// </para>
    /// <para>
    /// The penaly score information can be collected for analysis purposes, but it is not used by the library itself.
    /// If the information is collected, the QR code generation will be slower as the library needs to
    /// fully calculate the penalty score even if it is already clear that the pattern is not the best one.
    /// </para>
    /// <para>
    /// Note that in deviation from the QR code specification, the penalty score does not include the score
    /// contributed by the finder patterns.
    /// </para>
    /// </summary>
    public struct PenaltyScore
    {
        /// <summary>
        /// Penalty score for long horizontal runs of the same color.
        /// </summary>
        public int HorizontalStreaks { get; set; }
        /// <summary>
        /// Penalty score for long vertical runs of the same color.
        /// </summary>
        public int VerticalStreaks { get; set; }
        /// <summary>
        /// Penalty score for blocks of 2x2 modules of the same color.
        /// </summary>
        public int Blocks { get; set; }
        /// <summary>
        /// Penalty score for patterns that look like the finder patterns (1:1:3:1:1 ratio) in the horizontal direction.
        /// </summary>
        public int HorizontalFinderPatterns { get; set; }
        /// <summary>
        /// Penalty score for patterns that look like the finder patterns (1:1:3:1:1 ratio) in the vertical direction.
        /// </summary>
        public int VerticalFinderPatterns { get; set; }
        /// <summary>
        /// Penalty score for an inbalance of the number of dark and light modules.
        /// </summary>
        public int ColorBalance { get; set; }
        /// <summary>
        /// The total penalty score for the data mask pattern.
        /// </summary>
        public int Total { get; set; }
    }
}