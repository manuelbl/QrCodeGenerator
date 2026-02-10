/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Data segment mode.
    /// <para>
    /// Data segments can contain bytes as payload, or they can
    /// have a special function like ECI data segments. The mode
    /// describes what type of segment it is, and how the
    /// bytes are encoded.
    /// </para>
    /// </summary>
    public enum DataSegmentMode
    {
        /// <summary>
        /// Numeric mode.
        /// <para>
        /// Data segment with payload consisting of the decimal digits 0 to 9.
        /// </para>
        /// </summary>
        Numeric = 1,
            
        /// <summary>
        /// Alphanumeric mode.
        /// <para>
        /// Data segment with payload consisting of 0 to 9,
        /// A to Z (uppercase only), space, dollar, percent, asterisk, plus,
        /// hyphen, period, slash, colon.
        /// </para>
        /// </summary>
        Alphanumeric = 2,
            
        /// <summary>
        /// Kanji mode.
        /// <para>
        /// Data segment with payload consisting of double-byte Shift JIS
        /// characters, or any byte sequences with the same byte values.
        /// </para>
        /// </summary>
        Kanji = 3,
            
        /// <summary>
        /// Binary data.
        /// <para>
        /// Data segment with payload consisting of arbitrary binary data.
        /// </para>
        /// </summary>
        Binary = 4,
            
        /// <summary>
        /// Extended character set indicator (ECI).
        /// <para>
        /// Data segment indicating the character encoding of the following data segments.
        /// </para>
        /// </summary>
        ECI = 5,
        
        /// <summary>
        /// Structured append.
        /// <para>
        /// Data segment indicating the sequence number and total number of QR codes
        /// if data is split across multiple QR codes.
        /// </para>
        /// </summary>
        StructuredAppend = 6
    }
}