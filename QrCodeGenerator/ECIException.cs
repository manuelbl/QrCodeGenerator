/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Exception thrown when an invalid ECI value is specified.
    /// </summary>
    [SuppressMessage("csharpsquid", "S101")]
    public class ECIException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ECIException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">Error message that explains the error.</param>
        public ECIException(string message) : base(message)
        {
        }
    }
}