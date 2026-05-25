/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class DataSegmentModeInfoTest
    {
        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1)]
        [InlineData(DataSegmentMode.Alphanumeric, 2)]
        [InlineData(DataSegmentMode.Kanji, 8)]
        [InlineData(DataSegmentMode.Binary, 4)]
        [InlineData(DataSegmentMode.ECI, 7)]
        [InlineData(DataSegmentMode.StructuredAppend, 3)]
        public void ModeIndicator_MatchesSpec(DataSegmentMode mode, int expected)
        {
            Assert.Equal(expected, DataSegmentModeInfo.For(mode).ModeIndicator);
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric, true)]
        [InlineData(DataSegmentMode.Alphanumeric, true)]
        [InlineData(DataSegmentMode.Kanji, true)]
        [InlineData(DataSegmentMode.Binary, true)]
        [InlineData(DataSegmentMode.ECI, false)]
        [InlineData(DataSegmentMode.StructuredAppend, false)]
        public void HasCountIndicator_OnlyForDataModes(DataSegmentMode mode, bool expected)
        {
            Assert.Equal(expected, DataSegmentModeInfo.For(mode).HasCountIndicator);
        }

        // Character count indicator widths per ISO/IEC 18004 Table 3, by version
        // group (1-9, 10-26, 27-40). These reproduce the old CountIndicatorLength table.
        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1, 10)]
        [InlineData(DataSegmentMode.Numeric, 9, 10)]
        [InlineData(DataSegmentMode.Numeric, 10, 12)]
        [InlineData(DataSegmentMode.Numeric, 26, 12)]
        [InlineData(DataSegmentMode.Numeric, 27, 14)]
        [InlineData(DataSegmentMode.Numeric, 40, 14)]
        [InlineData(DataSegmentMode.Alphanumeric, 9, 9)]
        [InlineData(DataSegmentMode.Alphanumeric, 10, 11)]
        [InlineData(DataSegmentMode.Alphanumeric, 27, 13)]
        [InlineData(DataSegmentMode.Kanji, 9, 8)]
        [InlineData(DataSegmentMode.Kanji, 10, 10)]
        [InlineData(DataSegmentMode.Kanji, 27, 12)]
        [InlineData(DataSegmentMode.Binary, 9, 8)]
        [InlineData(DataSegmentMode.Binary, 10, 16)]
        [InlineData(DataSegmentMode.Binary, 27, 16)]
        public void GetCountIndicatorLength_MatchesSpec(DataSegmentMode mode, int version, int expected)
        {
            Assert.Equal(expected, DataSegmentModeInfo.For(mode).GetCountIndicatorLength(version));
        }

        [Theory]
        [InlineData(DataSegmentMode.ECI)]
        [InlineData(DataSegmentMode.StructuredAppend)]
        public void GetCountIndicatorLength_ZeroForNonDataModes(DataSegmentMode mode)
        {
            Assert.Equal(0, DataSegmentModeInfo.For(mode).GetCountIndicatorLength(1));
            Assert.Equal(0, DataSegmentModeInfo.For(mode).GetCountIndicatorLength(40));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1, 14)]   // 4 + 10
        [InlineData(DataSegmentMode.Numeric, 10, 16)]  // 4 + 12
        [InlineData(DataSegmentMode.ECI, 1, 4)]        // 4 + 0
        [InlineData(DataSegmentMode.StructuredAppend, 40, 4)]
        public void GetHeaderLength_IsFourPlusCountIndicator(DataSegmentMode mode, int version, int expected)
        {
            Assert.Equal(expected, DataSegmentModeInfo.For(mode).GetHeaderLength(version));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric)]
        [InlineData(DataSegmentMode.Alphanumeric)]
        [InlineData(DataSegmentMode.Kanji)]
        [InlineData(DataSegmentMode.Binary)]
        public void DataModeOperations_AreWired(DataSegmentMode mode)
        {
            var info = DataSegmentModeInfo.For(mode);
            Assert.NotNull(info.EncodedBitLength);
            Assert.NotNull(info.ByteCount);
            Assert.NotNull(info.Create);
        }

        [Theory]
        [InlineData(DataSegmentMode.ECI)]
        [InlineData(DataSegmentMode.StructuredAppend)]
        public void NonDataModeOperations_AreNull(DataSegmentMode mode)
        {
            var info = DataSegmentModeInfo.For(mode);
            Assert.Null(info.EncodedBitLength);
            Assert.Null(info.ByteCount);
            Assert.Null(info.Create);
        }
    }
}
