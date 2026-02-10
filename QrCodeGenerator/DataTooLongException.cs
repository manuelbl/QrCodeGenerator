/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// The exception that is thrown when the supplied data does not fit in the QR code.
    /// </summary>
    /// <remarks>
    /// Ways to handle this exception include:
    /// <ul>
    ///   <li>Decrease the error correction level (see <see cref="QrCode.Ecc"/>).</li>
    ///   <li>Increase the <c>maxVersion</c> argument if the method provides control over the version.
    ///       If not, the method tried up to the maximum version of 40.</li>
    ///   <li>Reduce the amount of text or binary data.</li>
    /// </ul>
    /// </remarks>
    public class DataTooLongException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTooLongException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataTooLongException(string message)
            : base(message)
        { }
    }
}
