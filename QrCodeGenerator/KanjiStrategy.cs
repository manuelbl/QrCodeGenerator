/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Controls if the Kanji mode is used for data segments.
    /// <para>
    /// Kanji mode allows efficiently encoding text encoded in Shift-JIS,
    /// an encoding for Japanese text. It can also be used for any
    /// other data if it contains suitable byte combinations. However,
    /// many QR code scanners incorrectly assume that if Kanji mode
    /// is used, then it must be text encoded in Shift-JIS.
    /// </para>
    /// <para>
    /// For the best compatibility with non-standard-compliant scanners,
    /// Kanji mode should only be used if the data is encoded in Shift-JIS. 
    /// </para>
    /// </summary>
    public enum KanjiStrategy
    {
        /// <summary>
        /// Use Kanji mode to encode data segments if the text is encoded
        /// in Shift-JIS (and if it is beneficial for compactness).
        /// </summary>
        Automatic,
        /// <summary>
        /// Use Kanji mode if it is beneficial for data segment encoding,
        /// regardless of the text encoding.
        /// </summary>
        Enabled,
        /// <summary>
        /// Do not use Kanji mode.
        /// </summary>
        Disabled
    }
}