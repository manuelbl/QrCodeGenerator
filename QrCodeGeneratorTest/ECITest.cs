/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class ECITest
    {
        static ECITest()
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        #region Encoding

        public static TheoryData<ECI, string> EciToEncodingMap => new TheoryData<ECI, string>
        {
            {  ECI.CodePage437, "ibm437" },
            { ECI.Latin1, "iso-8859-1" },
            { ECI.Iso8859_1, "iso-8859-1" },
            { ECI.Latin2, "iso-8859-2" },
            { ECI.Iso8859_2, "iso-8859-2" },
            { ECI.Latin3, "iso-8859-3" },
            { ECI.Iso8859_3, "iso-8859-3" },
            { ECI.Latin4, "iso-8859-4" },
            { ECI.Iso8859_4, "iso-8859-4" },
            { ECI.LatinCyrillic, "iso-8859-5" },
            { ECI.Iso8859_5, "iso-8859-5" },
            { ECI.LatinArabic, "iso-8859-6" },
            { ECI.Iso8859_6, "iso-8859-6" },
            { ECI.LatinGreek, "iso-8859-7" },
            { ECI.Iso8859_7, "iso-8859-7" },
            { ECI.LatinHebrew, "iso-8859-8" },
            { ECI.Iso8859_8, "iso-8859-8" },
            { ECI.Latin5, "iso-8859-9" },
            { ECI.Iso8859_9, "iso-8859-9" },
            { ECI.Latin7, "iso-8859-13" },
            { ECI.Iso8859_13, "iso-8859-13" },
            { ECI.Latin9, "iso-8859-15" },
            { ECI.Iso8859_15, "iso-8859-15" },
            { ECI.ShiftJIS, "shift_jis" },
            { ECI.Windows1250, "windows-1250" },
            { ECI.Windows1251, "windows-1251" },
            { ECI.Windows1252, "windows-1252" },
            { ECI.Windows1256, "windows-1256" },
            { ECI.UTF8, "utf-8" },
            { ECI.UTF16LE, "utf-16" },
            { ECI.UTF16BE, "utf-16BE" },
            { ECI.UTF32LE, "utf-32" },
            { ECI.UTF32BE, "utf-32BE" },
            { ECI.US_ASCII, "us-ascii" },
            { ECI.Big5, "big5" },
            { ECI.GB2312, "gb2312" },
            { ECI.KS_X1001, "ks_c_5601-1987" },
            { ECI.GB18030, "gb18030" }
        };

        [Theory]
        [MemberData(nameof(EciToEncodingMap))]
        public void Encoding_IsAvailable(ECI eci, string encodingName)
        {
            var encoding = eci.GetEncoding();
            Assert.Equal(encodingName, encoding.WebName, ignoreCase: true);
        }

        public static TheoryData<ECI> EciWithoutEncoding => new TheoryData<ECI>
        {
            ECI.None,
            ECI.Automatic,
            ECI.Iso646Inv,
            ECI.BinaryData,
            ECI.FromValue(100)
        };

        [Theory]
        [MemberData(nameof(EciWithoutEncoding))]
        public void Encoding_NoAssociation(ECI eci)
        {
            var exception = Assert.Throws<ECIException>(eci.GetEncoding);
            Assert.Contains("Unsupported ECI value", exception.Message);
            Assert.Contains("not associated with a character set encoding", exception.Message);
        }
    
        public static TheoryData<ECI> EciWithUnavailableEncoding => new TheoryData<ECI>
        {
            ECI.GBK,
            ECI.Latin6,
            ECI.Iso8859_10,
            ECI.LatinThai,
            ECI.Iso8859_11,
            ECI.Latin8,
            ECI.Iso8859_14,
            ECI.Latin10,
            ECI.Iso8859_16
        };

        [Theory]
        [MemberData(nameof(EciWithUnavailableEncoding))]
        public void ECICode_IsInvalid(ECI eci)
        {
            var exception = Assert.Throws<ECIException>(eci.GetEncoding);
            Assert.Contains("Unsupported ECI value", exception.Message);
            Assert.Contains("not available on this platform", exception.Message);
        }
    
        #endregion

        #region Hash Code
    
        [Fact]
        public void GetHashCode_IsValid()
        {
            Assert.Equal(3, ECI.Latin1.GetHashCode());
            Assert.Equal(26, ECI.UTF8.GetHashCode());
            Assert.Equal(23, ECI.Windows1252.GetHashCode());
            Assert.Equal(31, ECI.GBK.GetHashCode());
        }
    
        #endregion

        #region Equals
    
        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            var eci = ECI.UTF8;
            Assert.True(eci.Equals(eci));
        }

        [Fact]
        public void Equals_SameValue_ReturnsTrue()
        {
            var eci1 = ECI.UTF8;
            var eci2 = ECI.UTF8;
            Assert.True(eci1.Equals(eci2));
        }

        [Fact]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var eci1 = ECI.UTF8;
            var eci2 = ECI.Latin1;
            Assert.False(eci1.Equals(eci2));
        }

        [Fact]
        public void Equals_FromValueWithSameValue_ReturnsTrue()
        {
            var eci1 = ECI.UTF8;
            var eci2 = ECI.FromValue(26);
            Assert.True(eci1.Equals(eci2));
        }

        [Fact]
        public void Equals_FromValueWithDifferentValue_ReturnsFalse()
        {
            var eci1 = ECI.UTF8;
            var eci2 = ECI.FromValue(100);
            Assert.False(eci1.Equals(eci2));
        }

        [Fact]
        public void EqualsObject_SameInstance_ReturnsTrue()
        {
            var eci = ECI.UTF8;
            Assert.True(eci.Equals((object)eci));
        }

        [Fact]
        public void EqualsObject_SameValue_ReturnsTrue()
        {
            var eci1 = ECI.UTF8;
            object eci2 = ECI.UTF8;
            Assert.True(eci1.Equals(eci2));
        }

        [Fact]
        public void EqualsObject_DifferentValue_ReturnsFalse()
        {
            var eci1 = ECI.UTF8;
            object eci2 = ECI.Latin1;
            Assert.False(eci1.Equals(eci2));
        }

        [Fact]
        public void EqualsObject_Null_ReturnsFalse()
        {
            var eci = ECI.UTF8;
            Assert.False(eci.Equals((object)null));
        }

        [Fact]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void EqualsObject_DifferentType_ReturnsFalse()
        {
            var eci = ECI.UTF8;
            var obj = "UTF8";
            Assert.False(eci.Equals(obj));
        }

        [Fact]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void EqualsObject_DifferentTypeWithSameValue_ReturnsFalse()
        {
            var eci = ECI.UTF8;
            var obj = 26; // Same value as UTF8
            Assert.False(eci.Equals(obj));
        }

        [Fact]
        public void EqualsObject_FromValueWithSameValue_ReturnsTrue()
        {
            var eci1 = ECI.UTF8;
            object eci2 = ECI.FromValue(26);
            Assert.True(eci1.Equals(eci2));
        }
    
        #endregion
    }
}