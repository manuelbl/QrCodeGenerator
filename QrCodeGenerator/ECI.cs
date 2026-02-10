/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// ECI (Extended Channel Interpretation) values for text and data encoding.
    /// <para>
    /// An ECI indicator is used to specify the character encoding of the data that follows.
    /// </para>
    /// <para>
    /// If no ECI indicator is present, the default encoding according to the QR code specification
    /// is ISO-8859-1 (Latin-1). However, many QR code scanners either analyze the data and guess
    /// the encoding or assume UTF-8.
    /// </para>
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("csharpsquid", "S101")]
    [Serializable]
    public sealed class ECI : IEquatable<ECI>
    {
        internal const int NoneValue = -2;

        internal const int AutomaticValue = -1;
        
        /// <summary>
        /// No ECI indicator.
        /// <para>
        /// This value is not a valid ECI value. It is used as a method parameter
        /// to indicate that no ECI indicator should be added.
        /// </para>
        /// </summary>
        public static readonly ECI None = new ECI(NoneValue);
        
        /// <summary>
        /// Automatic ECI selection.
        /// <para>
        /// This value is not a valid ECI value. It is used as a method parameter
        /// to indicate that an appropriate encoding and ECI value should be selected automatically.
        /// </para>
        /// </summary>
        public static readonly ECI Automatic = new ECI(AutomaticValue);
        
        /// <summary>
        /// Code page 437 encoding.
        /// <para>
        /// Also known as CP437, OEM-US, OEM 437, PC-8, or MS-DOS Latin US.
        /// </para>
        /// </summary>
        public static readonly ECI CodePage437 = new ECI(2);
        
        /// <summary>
        /// Latin-1 / ISO/IEC 8859-1 encoding (Western European).
        /// </summary>
        public static readonly ECI Latin1 = new ECI(3);
        
        /// <summary>
        /// Latin-1 / ISO/IEC 8859-1 encoding (Western European).
        /// </summary>
        public static readonly ECI Iso8859_1 = Latin1;
        
        /// <summary>
        /// Latin-2 / ISO/IEC 8859-2 encoding (Central European).
        /// </summary>
        public static readonly ECI Latin2 = new ECI(4);
        
        /// <summary>
        /// Latin-2 / ISO/IEC 8859-2 encoding (Central European).
        /// </summary>
        public static readonly ECI Iso8859_2 = Latin2;
        
        /// <summary>
        /// Latin-3 / ISO/IEC 8859-3 encoding.
        /// </summary>
        public static readonly ECI Latin3 = new ECI(5);
        
        /// <summary>
        /// Latin-3 / ISO/IEC 8859-3 encoding.
        /// </summary>
        public static readonly ECI Iso8859_3 = Latin3;
        
        /// <summary>
        /// Latin-4 / ISO/IEC 8859-4 encoding (Baltic).
        /// </summary>
        public static readonly ECI Latin4 = new ECI(6);
        
        /// <summary>
        /// Latin-4 / ISO/IEC 8859-4 encoding (Baltic).
        /// </summary>
        public static readonly ECI Iso8859_4 = Latin4;
        
        /// <summary>
        /// Latin/Cyrillic / ISO/IEC 8859-5 encoding.
        /// </summary>
        public static readonly ECI LatinCyrillic = new ECI(7);
        
        /// <summary>
        /// Latin/Cyrillic / ISO/IEC 8859-5 encoding.
        /// </summary>
        public static readonly ECI Iso8859_5 = LatinCyrillic;
        
        /// <summary>
        /// Latin/Arabic / ISO/IEC 8859-6 encoding.
        /// </summary>
        public static readonly ECI LatinArabic = new ECI(8);
        
        /// <summary>
        /// Latin/Arabic / ISO/IEC 8859-6 encoding.
        /// </summary>
        public static readonly ECI Iso8859_6 = LatinArabic;
        
        /// <summary>
        /// Latin/Greek / ISO/IEC 8859-7 encoding.
        /// </summary>
        public static readonly ECI LatinGreek = new ECI(9);
        
        /// <summary>
        /// Latin/Greek / ISO/IEC 8859-7 encoding.
        /// </summary>
        public static readonly ECI Iso8859_7 = LatinGreek;
        
        /// <summary>
        /// Latin/Hebrew / ISO/IEC 8859-8 encoding.
        /// </summary>
        public static readonly ECI LatinHebrew = new ECI(10);
        
        /// <summary>
        /// Latin/Hebrew / ISO/IEC 8859-8 encoding.
        /// </summary>
        public static readonly ECI Iso8859_8 = LatinHebrew;
        
        /// <summary>
        /// Latin-5 / ISO/IEC 8859-9 encoding (Turkish).
        /// </summary>
        public static readonly ECI Latin5 = new ECI(11);
        
        /// <summary>
        /// Latin-5 / ISO/IEC 8859-9 encoding (Turkish).
        /// </summary>
        public static readonly ECI Iso8859_9 = Latin5;
        
        /// <summary>
        /// Latin-6 / ISO/IEC 8859-10 encoding.
        /// </summary>
        public static readonly ECI Latin6 = new ECI(12);
        
        /// <summary>
        /// Latin-6 / ISO/IEC 8859-10 encoding.
        /// </summary>
        public static readonly ECI Iso8859_10 = Latin6;
        
        /// <summary>
        /// Latin/Thai / ISO/IEC 8859-11 encoding.
        /// </summary>
        public static readonly ECI LatinThai = new ECI(13);
        
        /// <summary>
        /// Latin/Thai / ISO/IEC 8859-11 encoding.
        /// </summary>
        public static readonly ECI Iso8859_11 = LatinThai;
        
        /// <summary>
        /// Latin-7 / ISO/IEC 8859-13 encoding.
        /// </summary>
        public static readonly ECI Latin7 = new ECI(15);
        
        /// <summary>
        /// Latin-7 / ISO/IEC 8859-13 encoding.
        /// </summary>
        public static readonly ECI Iso8859_13 = Latin7;
        
        /// <summary>
        /// Latin-8 / ISO/IEC 8859-14 (Celtic) encoding.
        /// </summary>
        public static readonly ECI Latin8 = new ECI(16);
        
        /// <summary>
        /// Latin-8/Celtic / ISO/IEC 8859-14 encoding.
        /// </summary>
        public static readonly ECI Iso8859_14 = Latin8;
        
        /// <summary>
        /// Latin-9 / ISO/IEC 8859-15 encoding.
        /// </summary>
        public static readonly ECI Latin9 = new ECI(17);
        
        /// <summary>
        /// Latin-9 / ISO/IEC 8859-15 encoding.
        /// </summary>
        public static readonly ECI Iso8859_15 = Latin9;
        
        /// <summary>
        /// Latin-10 / ISO/IEC 8859-16 encoding.
        /// </summary>
        public static readonly ECI Latin10 = new ECI(18);
        
        /// <summary>
        /// Latin-10 / ISO/IEC 8859-16 encoding.
        /// </summary>
        public static readonly ECI Iso8859_16 = Latin10;
        
        /// <summary>
        /// Shift JIS encoding.
        /// <para>
        /// Also known as SJIS encoding.
        /// </para>
        /// </summary>
        public static readonly ECI ShiftJIS = new ECI(20);
        
        /// <summary>
        /// Windows-1250 encoding.
        /// </summary>
        public static readonly ECI Windows1250 = new ECI(21);
        
        /// <summary>
        /// Windows-1251 encoding.
        /// </summary>
        public static readonly ECI Windows1251 = new ECI(22);
        
        /// <summary>
        /// Windows-1252 encoding.
        /// </summary>
        public static readonly ECI Windows1252 = new ECI(23);
        
        /// <summary>
        /// Windows-1256 encoding.
        /// </summary>
        public static readonly ECI Windows1256 = new ECI(24);
        
        /// <summary>
        /// Unicode UTF-16 big endian encoding.
        /// </summary>
        public static readonly ECI UTF16BE = new ECI(25);
        
        /// <summary>
        /// Unicode UTF-8 encoding.
        /// </summary>
        public static readonly ECI UTF8 = new ECI(26);
        
        /// <summary>
        /// US ASCII encoding.
        /// </summary>
        public static readonly ECI US_ASCII = new ECI(27);
        
        /// <summary>
        /// Big-5 encoding.
        /// </summary>
        public static readonly ECI Big5 = new ECI(28);
        
        /// <summary>
        /// GB/T 2312 encoding.
        /// </summary>
        public static readonly ECI GB2312 = new ECI(29);
        
        /// <summary>
        /// KS X 1001 encoding.
        /// </summary>
        public static readonly ECI KS_X1001 = new ECI(30);
        
        /// <summary>
        /// GBK encoding.
        /// </summary>
        public static readonly ECI GBK = new ECI(31);
        
        /// <summary>
        /// GB 18030 encoding.
        /// </summary>
        public static readonly ECI GB18030 = new ECI(32);
        
        /// <summary>
        /// Unicode UTF-16 little endian encoding.
        /// </summary>
        public static readonly ECI UTF16LE = new ECI(33);
        
        /// <summary>
        /// Unicode UTF-32 big endian encoding.
        /// </summary>
        public static readonly ECI UTF32BE = new ECI(34);
        
        /// <summary>
        /// Unicode UTF-32 little endian encoding.
        /// </summary>
        public static readonly ECI UTF32LE = new ECI(35);
        
        /// <summary>
        /// ISO/IEC 646 invariant set encoding.
        /// </summary>
        public static readonly ECI Iso646Inv = new ECI(170);
        
        /// <summary>
        /// Binary 8-bit data.
        /// <para>
        /// This is not an encoding but rather an indicator that the data contains binary data.
        /// </para>
        /// </summary>
        public static readonly ECI BinaryData = new ECI(899);

        /// <summary>
        /// Creates an ECI designator with the given value.
        /// </summary>
        /// <param name="value">The ECI designator value.</param>
        public static ECI FromValue(int value)
        {
            return new ECI(value);
        }
        
        private ECI(int value)
        {
            Value = value;
        }
        
        /// <summary>
        /// Numeric ECI designator value.
        /// </summary>
        public int Value { get;  }

        /// <summary>
        /// Gets the associated encoding for this ECI value.
        /// </summary>
        /// <remarks>
        /// Note that not all encodings are available on .NET platforms.
        /// And many encodings are only available if
        /// <c>Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)</c>
        /// has been called beforehand.
        /// </remarks>
        /// <returns>Encoding.</returns>
        public Encoding GetEncoding()
        {
            int codePage;
            
            if (Value >= 0 && Value < CodePages.Length)
            {
                codePage = CodePages[Value];
            }
            else
            {
                throw new ECIException($"Unsupported ECI value {Value}: it is not associated with a character set encoding.");
            }

            if (codePage != -1)
            {
                try
                {
                    return Encoding.GetEncoding(codePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());
                }
                catch (NotSupportedException)
                {
                    // ignore and fall through
                }
            }

            throw new ECIException($"Unsupported ECI value: {Value}. Character set encoding is required but not available on this platform.");
        }
        
        private static readonly int[] CodePages = new int[]
        {
            437,   // 0  - CP437
            28591, // 1  - Latin-1 / ISO/IEC 8859-1
            437,   // 2  - CP437
            28591, // 3  - Latin-1 / ISO/IEC 8859-1
            28592, // 4  - Latin-2 / ISO/IEC 8859-2
            28593, // 5  - Latin-3 / ISO/IEC 8859-3
            28594, // 6  - Latin-4 / ISO/IEC 8859-4
            28595, // 7  - Latin/Cyrillic / ISO/IEC 8859-5
            28596, // 8  - Latin/Arabic / ISO/IEC 8859-6
            28597, // 9  - Latin/Greek / ISO/IEC 8859-7
            28598, // 10 - Latin/Hebrew / ISO/IEC 8859-8
            28599, // 11 - Latin-5 / ISO/IEC 8859-9
            28600, // 12 - Latin-6 / ISO/IEC 8859-10
            28601, // 13 - Latin/Thai / ISO/IEC 8859-11
            -1,    // 14
            28603, // 15 - Latin-7 / ISO/IEC 8859-13
            28604, // 16 - Latin-8/Celtic / ISO/IEC 8859-14
            28605, // 17 - Latin-9 / ISO/IEC 8859-15
            28606, // 18 - Latin-10 / ISO/IEC 8859-16
            -1,    // 19
            932,   // 20 - Shift JIS
            1250,  // 21 - Windows-1250
            1251,  // 22 - Windows-1251
            1252,  // 23 - Windows-1252
            1256,  // 24 - Windows-1256
            1201,  // 25 - UTF-16BE
            65001, // 26 - UTF-8
            20127, // 27 - US ASCII
            950,   // 28 - Big-5
            936,   // 29 - GB/T 2312
            949,   // 30 - KS X 1001
            -1,    // 31 - GBK
            54936, // 32 - GB 18030
            1200,  // 33 - UTF-16LE
            12001, // 34 - UTF-32BE
            12000  // 35 - UTF-32LE
        };

        /// <summary>
        /// Determines if the other instance is equal to this instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><c>true</c> if they are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ECI other)
        {
            return other != null && Value == other.Value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ECI)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value;
        }
    }
}
