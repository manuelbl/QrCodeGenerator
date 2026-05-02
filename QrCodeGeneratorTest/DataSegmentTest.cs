/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class DataSegmentTest : DataSegmentTestBase
    {
        #region Constructor / Properties

        [Theory]
        [InlineData(DataSegmentMode.Numeric, "12345678")]
        [InlineData(DataSegmentMode.Alphanumeric, "ABCD")]
        [InlineData(DataSegmentMode.Binary, "abdefghijk")]
        public void GetByteLength(DataSegmentMode mode, string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var segment = DataSegment.MakeSegment(mode, new ArraySegment<byte>(data));
            Assert.Equal(data.Length, segment.DataLength);
        }

        [Fact]
        public void MakeSegment_RejectsUnsupportedMode()
        {
            var dataBytes = new ArraySegment<byte>(Array.Empty<byte>());
            Assert.Throws<ArgumentOutOfRangeException>(() => DataSegment.MakeSegment(DataSegmentMode.ECI, dataBytes));
        }

        [Fact]
        public void GetByteLengthKanji()
        {
            var data = ECI.ShiftJIS.GetEncoding().GetBytes("氏サケニイ組品ヱ");
            var segment = new DataSegmentKanji(new ArraySegment<byte>(data));
            Assert.Equal(16, segment.DataLength);
        }

        #endregion
    
        #region Length Calculation

        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1, 9, 18)]
        [InlineData(DataSegmentMode.Numeric, 2, 9, 21)]
        [InlineData(DataSegmentMode.Numeric, 3, 9, 24)]
        [InlineData(DataSegmentMode.Numeric, 4, 9, 28)]
        [InlineData(DataSegmentMode.Numeric, 4, 10, 30)]
        [InlineData(DataSegmentMode.Numeric, 4, 26, 30)]
        [InlineData(DataSegmentMode.Numeric, 4, 27, 32)]
        [InlineData(DataSegmentMode.Numeric, 4, 40, 32)]
        [InlineData(DataSegmentMode.Alphanumeric, 1, 9, 19)]
        [InlineData(DataSegmentMode.Alphanumeric, 2, 9, 24)]
        [InlineData(DataSegmentMode.Alphanumeric, 3, 9, 30)]
        [InlineData(DataSegmentMode.Alphanumeric, 3, 10, 32)]
        [InlineData(DataSegmentMode.Alphanumeric, 3, 26, 32)]
        [InlineData(DataSegmentMode.Alphanumeric, 3, 27, 34)]
        [InlineData(DataSegmentMode.Kanji, 2, 9, 25)]
        [InlineData(DataSegmentMode.Kanji, 4, 9, 38)]
        [InlineData(DataSegmentMode.Kanji, 4, 10, 40)]
        [InlineData(DataSegmentMode.Kanji, 4, 26, 40)]
        [InlineData(DataSegmentMode.Kanji, 4, 27, 42)]
        [InlineData(DataSegmentMode.Binary, 1, 9, 20)]
        [InlineData(DataSegmentMode.Binary, 2, 9, 28)]
        [InlineData(DataSegmentMode.Binary, 3, 9, 36)]
        [InlineData(DataSegmentMode.Binary, 3, 10, 44)]
        [InlineData(DataSegmentMode.Binary, 3, 26, 44)]
        [InlineData(DataSegmentMode.Binary, 3, 27, 44)]
        public void CalculateBitLength(DataSegmentMode mode, int dataLength, int version, int expectedLength)
        {
            Assert.Equal(expectedLength, DataSegment.GetBitLength(mode, dataLength, version));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1, 14)]
        [InlineData(DataSegmentMode.Numeric, 9, 14)]
        [InlineData(DataSegmentMode.Numeric, 10, 16)]
        [InlineData(DataSegmentMode.Numeric, 26, 16)]
        [InlineData(DataSegmentMode.Numeric, 27, 18)]
        [InlineData(DataSegmentMode.Numeric, 40, 18)]
        [InlineData(DataSegmentMode.Alphanumeric, 1, 13)]
        [InlineData(DataSegmentMode.Alphanumeric, 9, 13)]
        [InlineData(DataSegmentMode.Alphanumeric, 10, 15)]
        [InlineData(DataSegmentMode.Alphanumeric, 26, 15)]
        [InlineData(DataSegmentMode.Alphanumeric, 27, 17)]
        [InlineData(DataSegmentMode.Alphanumeric, 40, 17)]
        [InlineData(DataSegmentMode.Kanji, 1, 12)]
        [InlineData(DataSegmentMode.Kanji, 9, 12)]
        [InlineData(DataSegmentMode.Kanji, 10, 14)]
        [InlineData(DataSegmentMode.Kanji, 26, 14)]
        [InlineData(DataSegmentMode.Kanji, 27, 16)]
        [InlineData(DataSegmentMode.Kanji, 40, 16)]
        [InlineData(DataSegmentMode.Binary, 1, 12)]
        [InlineData(DataSegmentMode.Binary, 9, 12)]
        [InlineData(DataSegmentMode.Binary, 10, 20)]
        [InlineData(DataSegmentMode.Binary, 26, 20)]
        [InlineData(DataSegmentMode.Binary, 27, 20)]
        [InlineData(DataSegmentMode.Binary, 40, 20)]
        [InlineData(DataSegmentMode.ECI, 1, 4)]
        [InlineData(DataSegmentMode.ECI, 9, 4)]
        [InlineData(DataSegmentMode.ECI, 10, 4)]
        [InlineData(DataSegmentMode.ECI, 26, 4)]
        [InlineData(DataSegmentMode.ECI, 27, 4)]
        [InlineData(DataSegmentMode.ECI, 40, 4)]
        public void CalculateHeaderLength(DataSegmentMode mode, int version, int expectedLength)
        {
            Assert.Equal(expectedLength, DataSegment.GetHeaderLength(mode, version));
        }

        [Fact]
        public void CalculateBitLength_InvalidMode()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DataSegment.GetBitLength(DataSegmentMode.ECI, 0, 1));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric)]
        [InlineData(DataSegmentMode.Alphanumeric)]
        [InlineData(DataSegmentMode.Binary)]
        public void CalculateByteLength(DataSegmentMode mode)
        {
            for (var i = 0; i < 100; i += 1)
            {
                var bitLength = DataSegment.GetBitLength(mode, i, 12) - DataSegment.GetHeaderLength(mode, 12);
                Assert.Equal(i, CalcByteLength(mode, bitLength));
            }
        }

        [Fact]
        public void CalculateByteLength_Kanji()
        {
            for (var i = 0; i < 100; i += 2)
            {
                var bitLength = DataSegment.GetBitLength(DataSegmentMode.Kanji, i, 12) - DataSegment.GetHeaderLength(DataSegmentMode.Kanji, 12);
                Assert.Equal(i, CalcByteLength(DataSegmentMode.Kanji, bitLength));
            }
        }

        [Fact]
        public void CalculateByteLength_ECI()
        {
            Assert.Equal(0, CalcByteLength(DataSegmentMode.ECI, 8));
            Assert.Equal(0, CalcByteLength(DataSegmentMode.ECI, 16));
            Assert.Equal(0, CalcByteLength(DataSegmentMode.ECI, 24));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric, 9)]
        [InlineData(DataSegmentMode.Alphanumeric, 5)]
        [InlineData(DataSegmentMode.Kanji, 4)]
        [InlineData(DataSegmentMode.Binary, 3)]
        public void GetByteCount_Ok(DataSegmentMode mode, int expectedBytes)
        {
            Assert.Equal(expectedBytes, DataSegment.GetByteCount(mode, 31));
        }

        [Theory]
        [InlineData(DataSegmentMode.ECI)]
        [InlineData(DataSegmentMode.StructuredAppend)]
        public void GetByteCount_RejectsUnsupportedMode(DataSegmentMode mode)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DataSegment.GetByteCount(mode, 17));
        }
    
        #endregion
    
        #region Factory Methods

        [Theory]
        [InlineData("ABC")]
        [InlineData("012345")]
        [InlineData("Hello World!")]
        [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed")]
        [InlineData("ABCDEF01234]j!G$PGGB?P,mKq#7in/PW.kT$8}y0982ABDEF98765")]
        [InlineData("abcdefg01234567abcdefg")]
        [InlineData("")]
        public void EncodeFromText(string text)
        {
            var segments = DataSegment.FromText(text);
            var decodedText = DataSegment.GetText(segments);
            Assert.Equal(text, decodedText);
            DataSegment previousSegment = null;
            foreach (var segment in segments)
            {
                if (previousSegment != null)
                {
                    Assert.NotEqual(segment.Mode, previousSegment.Mode);
                }
                previousSegment = segment;
            }
        }

        [Fact]
        public void EncodeFromText_ThrowsOnNullText()
        {
            Assert.Throws<ArgumentNullException>(() => DataSegment.FromText(null));
        }

        [Fact]
        public void EncodeFromBinaryData_ThrowsOnNullText()
        {
            Assert.Throws<ArgumentNullException>(() => DataSegment.FromBinaryData(null));
        }

        [Theory]
        [InlineData("迷テ正脳に整立スナマ搭込りざリゃ時酔エ後下", 2)]
        [InlineData("012345迷テ正脳に整立スナマ搭込りざリゃ時酔エ後下abcdef", 4)]
        public void EncodeFromTextKanji(string text, int numSegments)
        {
            var segments = DataSegment.FromText(text, ECI.ShiftJIS);
            Assert.Equal(numSegments, segments.Count);
            Assert.Equal(DataSegmentMode.ECI, segments[0].Mode);
            Assert.Equal(ECI.ShiftJIS, (segments[0] as DataSegmentEci)?.Designator);
            Assert.Contains(segments, segment => segment.Mode == DataSegmentMode.Kanji);
            var decodedText = DataSegment.GetText(segments);
            Assert.Equal(text, decodedText);
        }

        [Fact]
        public void EncodeWithoutEci()
        {
            const string text = "😩💦👩❤️💋👩";
            var segments = DataSegment.FromText(text, ECI.None, Encoding.UTF8);
            var segment = Assert.Single(segments);
            Assert.Equal(text, Encoding.UTF8.GetString(segment.DataBytes.Array));
        
            var exception = Assert.Throws<ArgumentException>(() => DataSegment.FromText(text, ECI.None, encoding: null));
            Assert.Contains("Encoding must be specified", exception.Message);
        }

        [Fact]
        public void EncodeWithKanjiMode()
        {
            const string text = "‡U‡T‡K‡U‡A";
            var win1252 = Encoding.GetEncoding("windows-1252");
            var segments = DataSegment.FromText(text, ECI.None, win1252, kanjiStrategy: KanjiStrategy.Enabled);
            var segment = Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Kanji, segment.Mode);
        
            segments = DataSegment.FromText(text, ECI.None, win1252, kanjiStrategy: KanjiStrategy.Disabled);
            segment = Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Binary, segment.Mode);

            segments = DataSegment.FromText(text, ECI.None, win1252, kanjiStrategy: KanjiStrategy.Automatic);
            segment = Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Binary, segment.Mode);
        }

        [Fact]
        public void CompactSegments()
        {
            var segments = DataSegment.FromText("4567");
            Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Numeric, segments[0].Mode);
        
            segments = DataSegment.FromText("ABCDEFGHIJK");
            Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Alphanumeric, segments[0].Mode);
        
            segments = DataSegment.FromText("abcdefghijklmn");
            Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Binary, segments[0].Mode);
        
            segments = DataSegment.FromText("01234567ABCDEFGHIJK");
            Assert.Equal(2, segments.Count);
            Assert.Equal(DataSegmentMode.Numeric, segments[0].Mode);
            Assert.Equal(DataSegmentMode.Alphanumeric, segments[1].Mode);
        
            segments = DataSegment.FromText("abcdefg0123hijklmno");
            Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Binary, segments[0].Mode);
        }

        [Fact]
        public void SelectEncoding()
        {
            // ISO-8859-1
            var text = "4q5A67ghijÒ×é©ÿ¡&~";
            var segments = DataSegment.FromText(text);
            Assert.Single(segments);
            Assert.Equal(DataSegmentMode.Binary, segments[0].Mode);
            Assert.Equal(text, DataSegment.GetText(segments));
        
            // UTF-8
            text = "€4q5A67ghijÒ×é©ÿ¡&~";
            segments = DataSegment.FromText(text);
            Assert.Equal(2, segments.Count);
            Assert.Equal(DataSegmentMode.ECI, segments[0].Mode);
            Assert.Equal(ECI.UTF8, (segments[0] as DataSegmentEci)?.Designator);
            Assert.Equal(DataSegmentMode.Binary, segments[1].Mode);
            Assert.Equal(text, DataSegment.GetText(segments));
        }

        [Fact]
        public void MakeEciSegment()
        {
            var segment = DataSegment.MakeECISegment(ECI.Latin9);
            Assert.IsType<DataSegmentEci>(segment);
            var eciSegment = segment as DataSegmentEci;
            Debug.Assert(eciSegment != null);
            Assert.Equal(ECI.Latin9, eciSegment.Designator);
        }
    
        [Fact]
        public void MakeStructuredAppend()
        {
            var segment = DataSegment.MakeStructuredAppend(1, 8, 0x3f);
            Assert.IsType<DataSegmentStructuredAppend>(segment);
            var structuredAppendSegment = segment as DataSegmentStructuredAppend;
            Debug.Assert(structuredAppendSegment != null);
            Assert.Equal(1, structuredAppendSegment.Position);
            Assert.Equal(8, structuredAppendSegment.Total);
            Assert.Equal(0x3f, structuredAppendSegment.Parity);
        }
    
        #endregion
    
        #region Bit Stream

        [Theory]
        [MemberData(nameof(DataSegmentsTheoryData))]
        public void CreateBitStream(List<DataSegment> segments, int version, int expectedBitLength)
        {
            var bitStream = DataSegment.CreateBitStream(segments, version, (expectedBitLength + 100) / 8);
            Assert.Equal(expectedBitLength, bitStream.Length);
            Assert.Equal(0u, bitStream.ExtractBits(bitStream.Length - 4, 4));
        }
    
        public static TheoryData<List<DataSegment>, int, int> DataSegmentsTheoryData {
            get
            {
#if NET
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
                var data = new TheoryData<List<DataSegment>, int, int>();
                var segments = new List<DataSegment> {
                    new DataSegmentByte(new ArraySegment<byte>(Encoding.UTF8.GetBytes("abc")))
                };
                data.Add(segments, 8, 4 + 8 + 3 * 8 + 4);
                data.Add(segments, 15, 4 + 16 + 3 * 8 + 4);
                data.Add(segments, 30, 4 + 16 + 3 * 8 + 4);

                segments = new List<DataSegment> {
                    new DataSegmentEci(ECI.UTF8),
                    new DataSegmentByte(new ArraySegment<byte>(Encoding.UTF8.GetBytes("abc")))
                };
                data.Add(segments, 8, 4 + 8 + 4 + 8 + 3 * 8 + 4);
                data.Add(segments, 15, 4 + 8 + 4 + 16 + 3 * 8 + 4);
                data.Add(segments, 30, 4 + 8 + 4 + 16 + 3 * 8 + 4);

                segments = new List<DataSegment> {
                    new DataSegmentNumeric(new ArraySegment<byte>(Encoding.UTF8.GetBytes("137")))
                };
                data.Add(segments, 8, 4 + 10 + 10 + 4);
                data.Add(segments, 15, 4 + 12 + 10 + 4);
                data.Add(segments, 30, 4 + 14 + 10 + 4);

                segments = new List<DataSegment> {
                    new DataSegmentAlphanumeric(new ArraySegment<byte>(Encoding.UTF8.GetBytes("CDEF")))
                };
                data.Add(segments, 8, 4 + 9 + 22 + 4);
                data.Add(segments, 15, 4 + 11 + 22 + 4);
                data.Add(segments, 30, 4 + 13 + 22 + 4);

                segments = new List<DataSegment> {
                    new DataSegmentEci(ECI.ShiftJIS),
                    new DataSegmentKanji(new ArraySegment<byte>(ECI.ShiftJIS.GetEncoding().GetBytes("け希")))
                };
                data.Add(segments, 8, 4 + 8 + 4 + 8 + 26 + 4);
                data.Add(segments, 15, 4 + 8 + 4  + 10 + 26 + 4);
                data.Add(segments, 30, 4 + 8 + 4  + 12 + 26 + 4);

                return data;
            }
        }
    
        [Fact]
        public void GetBitLength()
        {
            var segments = new List<DataSegment> {
                new DataSegmentEci(ECI.UTF8),
                new DataSegmentAlphanumeric(new ArraySegment<byte>(Encoding.UTF8.GetBytes("ABC012")))
            };

            Assert.Equal(segments[0].GetTotalLength(8) + segments[1].GetTotalLength(8), DataSegment.GetBitLength(segments, 8));
            Assert.Equal(segments[0].GetTotalLength(16) + segments[1].GetTotalLength(16), DataSegment.GetBitLength(segments, 16));
            Assert.Equal(segments[0].GetTotalLength(28) + segments[1].GetTotalLength(28), DataSegment.GetBitLength(segments, 28));
        }

        [Theory]
        [InlineData(DataSegmentMode.Numeric, 1)]
        [InlineData(DataSegmentMode.Alphanumeric, 2)]
        [InlineData(DataSegmentMode.StructuredAppend, 3)]
        [InlineData(DataSegmentMode.Binary, 4)]
        [InlineData(DataSegmentMode.ECI, 7)]
        [InlineData(DataSegmentMode.Kanji, 8)]
        public void GetModeIndicator(DataSegmentMode mode, uint expectedSymbol)
        {
            var symbol = DataSegment.GetModeIndicator(mode);
            Assert.Equal(expectedSymbol, symbol);
            Assert.InRange(symbol, 0u, 15u);
        }
    
#endregion
    
        #region ToString

        [Fact]
        public void GetText()
        {
            var data = Encoding.GetEncoding(1252).GetBytes("ABCabc€©¶");
            var segments = new List<DataSegment>
            {
                new DataSegmentStructuredAppend(1, 3, 0xd2),
                new DataSegmentEci(ECI.Windows1252)
            };
            segments.AddRange(DataSegment.FromBinaryData(data, ECI.None));
        
            Assert.Equal("ABCabc€©¶", DataSegment.GetText(segments));
        }

        [Fact]
        public void ToStringFunction()
        {
            Assert.Equal("[Mode: Numeric; Data length: 1]", new DataSegmentNumeric(new ArraySegment<byte>(new byte[1])).ToString());
            Assert.Equal("[Mode: Alphanumeric; Data length: 2]", new DataSegmentAlphanumeric(new ArraySegment<byte>(new byte[2])).ToString());
            Assert.Equal("[Mode: Kanji; Data length: 4]", new DataSegmentKanji(new ArraySegment<byte>(new byte[4])).ToString());
            Assert.Equal("[Mode: Binary; Data length: 5]", new DataSegmentByte(new ArraySegment<byte>(new byte[5])).ToString());
            Assert.Equal("[Mode: ECI; Data length: 0]", new DataSegmentEci(ECI.Latin1).ToString());
        }
    
        #endregion
    }
}