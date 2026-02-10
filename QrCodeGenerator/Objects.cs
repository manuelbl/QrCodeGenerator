/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Helper functions to check for valid arguments.
    /// </summary>
    internal static class Objects
    {
        /// <summary>
        /// Ensures that the specified argument is <i>not null</i>.
        /// <para>
        /// Throws a <see cref="ArgumentNullException"/> exception if the argument is <c>null</c>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="arg">The argument to check.</param>
        /// <param name="paramName">The parameter name for the exception.</param>
        /// <exception cref="ArgumentNullException">The specified argument is <c>null</c>.</exception>
        internal static void RequireNonNull<T>(T arg, string paramName) where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
