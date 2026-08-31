/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

#if NET

using System.Runtime.CompilerServices;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    internal static class TestAssemblyInit
    {
        /// <summary>
        /// Registers the code page encodings (needed for Shift-JIS and other non-Unicode encodings).
        /// </summary>
        /// <remarks>
        /// This must happen before any test code runs. ZXing.Net resolves and caches its Shift-JIS
        /// encoding in a type initializer and silently falls back to UTF-8 if the encoding is
        /// unavailable at that moment. Registering from a test class instead would make the outcome
        /// depend on the order in which the tests happen to run.
        /// </remarks>
        [ModuleInitializer]
        internal static void RegisterEncodings()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}

#endif
