/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// A chunk of payload data to be carried by the QR code.
    /// <para>
    /// The payload data carried by a QR code is a sequence of data segments.
    /// Each segment is a sequence of bytes encoded with a specific encoding
    /// mode: numeric, alphanumeric, Kanji, or binary.
    /// Additionally, a segment can contain an extended character set
    /// indicator (ECI) or a Structured Append header.
    /// </para>
    /// <para>
    /// Instances of this class represent such a data segment.
    /// They retain the data in the original unencoded form until
    /// the bit stream for the QR code is created.
    /// </para>
    /// </summary>
    public abstract class DataSegment
    {
        #region Properties
        
        /// <summary>The data segment mode.</summary>
        /// <value>The data segment mode.</value>
        public DataSegmentMode Mode { get; }

        /// <summary>The data bytes.</summary>
        /// <value>The data bytes (if any).</value>
        public ArraySegment<byte> DataBytes { get; protected set; }
        
        /// <summary>The data length, in bytes.</summary>
        /// <value>The data length, in bytes.</value>
        public int DataLength => DataBytes.Count;

        /// <summary>
        /// The length of the encoded segment, in bits.
        /// <para>
        /// The length is without the header (mode indicator and count indicator).
        /// </para>
        /// </summary>
        public int EncodedLength { get; }
        
        /// <summary>
        /// Gets the encoded segment length, in bits.
        /// <para>
        /// The length includes the mode indicator, the count indicator, and the data bits.
        /// It depends on the QR code version.
        /// </para>
        /// </summary>
        /// <param name="version">The QR code version.</param>
        /// <returns>The segment length, in bits.</returns>
        public int GetTotalLength(int version) => GetHeaderLength(Mode, version) + EncodedLength;
        
        #endregion
        
        #region Factory methods

        /// <summary>
        /// Creates optimal data segments for the specified text string.
        /// <para>
        /// The optimal segments are the segments resulting in the smallest
        /// number of bits. For a given text, they slightly differ depending
        /// on the QR code version. If the QR code version is not known, the
        /// default value (20) will produce the optimal result except for rare
        /// cases.
        /// </para>
        /// <para>
        /// If <paramref name="eci"/> is <c>null</c> or <see cref="ECI.Automatic"/>,
        /// then Latin-1 / ISO-8859-1 character encoding is used if the text
        /// can be encoded without loss in this encoding. If so, no ECI
        /// segment is added. Otherwise, the text is encoded in UTF-8 and
        /// an ECI segment is added. The parameter <paramref name="encoding"/>
        /// will be ignored.
        /// </para>
        /// <para>
        /// If <paramref name="eci"/> is <see cref="ECI.None"/>, then no ECI
        /// segment is added. The text will be encoded using the specified
        /// encoding. The parameter <paramref name="encoding"/> is mandatory
        /// in this case. 
        /// </para>
        /// <para>
        /// In all other cases, the specified ECI segment is added. The text
        /// is encoded using the specified encoding.
        /// If <paramref name="encoding"/> is <c>null</c>, then the encoding
        /// is derived from <paramref name="eci"/>.
        /// </para>
        /// <para>
        /// <paramref name="kanjiStrategy"/> controls if Kanji mode is considered
        /// for encoding data segments. For the best compatibility, it should only
        /// be used if the text is encoded in Shift-JIS. This is the default
        /// behavior.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Most encodings other than Latin-1 / ISO-8859-1 and the UTF encodings require that
        /// the encoding is registered (except for .NET Framework 4.x), typically using:
        /// <code>Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);</code>
        /// </remarks>
        /// <param name="text">The text to encode.</param>
        /// <param name="eci">The extended character set indicator segment to add.</param>
        /// <param name="encoding">The character set encoding to use, or <c>null</c> if it should be derived from the ECI.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="kanjiStrategy">Controls if Kanji mode is used for data segments.</param>
        /// <returns>List of QR data segments representing the text.</returns>
        public static List<DataSegment> FromText(
            string text,
            ECI eci = null,
            Encoding encoding = null,
            int version = 20,
            KanjiStrategy kanjiStrategy = KanjiStrategy.Automatic
        )
        {
            Objects.RequireNonNull(text, nameof(text));
            eci = eci ?? ECI.Automatic;
            byte[] data = null;
            
            switch (eci.Value)
            {
                case ECI.AutomaticValue:
                    // try ISO-8859-1 encoding
                    try
                    {
                        data = ECI.Latin1.GetEncoding().GetBytes(text);
                        eci = ECI.None;
                    }
                    catch (EncoderFallbackException)
                    {
                        // Cannot encode as ISO-8859-1 without loss, use UTF-8
                        eci = ECI.UTF8;
                        encoding = ECI.UTF8.GetEncoding();
                    }
                    break;
                
                case ECI.NoneValue:
                {
                    if (encoding == null)
                    {
                        throw new ArgumentException("Encoding must be specified for eci == ECI.None", nameof(encoding));
                    }
                    break;
                }
                
                default:
                    encoding = encoding ?? eci.GetEncoding();
                    break;
            }

            data = data ?? encoding.GetBytes(text);

            bool considerKanjiMode;
            switch (kanjiStrategy)
            {
                case KanjiStrategy.Enabled:
                    considerKanjiMode = true;
                    break;
                case KanjiStrategy.Disabled:
                    considerKanjiMode = false;
                    break;
                default:
                    considerKanjiMode = Equals(eci, ECI.ShiftJIS);
                    break;
            }
            
            return FromBinaryData(data, eci, version, considerKanjiMode);
        }

        /// <summary>
        /// Creates optimal data segments for the specified binary data.
        /// <para>
        /// The optimal segments are the segments representing the data with
        /// the smallest number of bits. For a given text, they slightly
        /// differ depending on the QR code version. If the QR code version
        /// is not known, the default value (20) will produce the optimal
        /// result except for rare cases.
        /// </para>
        /// <para>
        /// The specified data can be text data that has already been encoded
        /// using a text encoding.
        /// </para>
        /// <para>
        /// The resulting data segments will refer to the <paramref name="data"/>
        /// byte array. It must not be changed until the QR code has been generated.
        /// </para>
        /// </summary>
        /// <param name="data">The binary data to encode.</param>
        /// <param name="eci">The extended character set indicator segment to add.
        /// If <see cref="ECI.None"/> is specified, the ECI segment if omitted.
        /// If <c>null</c> is specified, the ECI designator for binary data is used.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="considerKanjiMode">Controls if Kanji mode is considered for data segments.</param>
        /// <returns>A list of QR data segments representing the data.</returns>
        public static List<DataSegment> FromBinaryData(
            byte[] data,
            ECI eci = null,
            int version = 20,
            bool considerKanjiMode = false
        )
        {
            Objects.RequireNonNull(data, nameof(data));
            
            var dataSegments = SegmentCompaction.BuildSegments(new ArraySegment<byte>(data), version, considerKanjiMode);
            if (!Equals(eci, ECI.None))
            {
                dataSegments.Insert(0, new DataSegmentEci(eci ?? ECI.BinaryData));
            }
            
            return dataSegments;
        }

        #endregion
        
        #region Bit Stream

        /// <summary>
        /// Creates a bit stream for the specified data segments.
        /// <para>
        /// The bit stream will end with the terminator. If the capacity is not sufficient
        /// for the terminator, it will be truncated or omitted entirely (as per the
        /// QR code specification).
        /// </para>
        /// </summary>
        /// <param name="segments">The segments to encode in the bit stream.</param>
        /// <param name="version">The QR code version.</param>
        /// <param name="capacity">The capacity for the bit stream, in bytes.</param>
        /// <returns>The bit stream with the encoded segments.</returns>
        internal static BitStream CreateBitStream(List<DataSegment> segments, int version, int capacity)
        {
            var bitStream = new BitStream(capacity);
            foreach (var segment in segments)
            {
                // mode indicator
                bitStream.AppendBits(GetModeIndicator(segment.Mode), 4);
                // character count indicator
                if (segment.Mode >= DataSegmentMode.Numeric && segment.Mode <= DataSegmentMode.Binary)
                {
                    bitStream.AppendBits((uint)segment.DataBytes.Count,
                        GetCountIndicatorLength(segment.Mode, version));
                }
                // data bit stream
                segment.WriteToBitStream(bitStream);
            }
            
            Debug.Assert(bitStream.Length <= 8 * capacity);
            
            // terminator
            var terminatorLength = Math.Min(4, capacity * 8 - bitStream.Length);
            if (terminatorLength > 0)
            {
                bitStream.AppendBits(0, terminatorLength);
            }

            return bitStream;
        }

        /// <summary>
        /// Writes the encoded segment data to the specified bit stream.
        /// <para>
        /// The mode indicator and character count indicator are not written.
        /// </para>
        /// </summary>
        /// <param name="bitStream">The bit stream.</param>
        internal abstract void WriteToBitStream(BitStream bitStream);

        internal static uint GetModeIndicator(DataSegmentMode mode)
        {
            uint symbol = 0;
            switch (mode)
            {
                case DataSegmentMode.Numeric: symbol = 1; break;
                case DataSegmentMode.Alphanumeric: symbol = 2; break;
                case DataSegmentMode.Kanji: symbol = 8; break;
                case DataSegmentMode.Binary: symbol = 4; break;
                case DataSegmentMode.ECI: symbol = 7; break;
                case DataSegmentMode.StructuredAppend: symbol = 3; break;
            }
            return symbol;
        }
        
        #endregion
        

        #region Constructor

        /// <summary>
        /// Creates a new instance with the given data segment mode and encoded data length.
        /// </summary>
        /// <param name="mode">The data segment mode.</param>
        /// <param name="encodedLength">The length of the encoded data, without mode and count indicator, in bits.</param>
        protected DataSegment(DataSegmentMode mode, int encodedLength)
        {
            Mode = mode;
            EncodedLength = encodedLength;
        }

        /// <summary>
        /// Creates a new data segment for encoding the specified data.
        /// <para>
        /// This method only supports the modes for encoding data: numeric,
        /// alphanumeric, Kanji and byte. 
        /// </para>
        /// </summary>
        /// <param name="dataSegmentMode">The data segment mode.</param>
        /// <param name="dataBytes">The character or binary data.</param>
        /// <returns>The new data segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported data segment mode is specified.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>dataBytes.Array</c> is <c>null</c>.</exception>
        public static DataSegment MakeSegment(DataSegmentMode dataSegmentMode, ArraySegment<byte> dataBytes)
        {
            Objects.RequireNonNull(dataBytes.Array, "dataBytes.Array");
            
            switch (dataSegmentMode)
            {
                case DataSegmentMode.Numeric:
                    return new DataSegmentNumeric(dataBytes);
                case DataSegmentMode.Alphanumeric:
                    return new DataSegmentAlphanumeric(dataBytes);
                case DataSegmentMode.Kanji:
                    return new DataSegmentKanji(dataBytes);
                case DataSegmentMode.Binary:
                    return new DataSegmentByte(dataBytes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataSegmentMode), dataSegmentMode, "This mode is not supported by this function.");
            }
        }

        /// <summary>
        /// Creates a new ECI data segment.
        /// </summary>
        /// <param name="designator">The ECI designator.</param>
        /// <returns>An ECI data segment.</returns>
        /// <seealso cref="ECI"/>
        public static DataSegment MakeECISegment(ECI designator)
        {
            return new DataSegmentEci(designator);
        }

        /// <summary>
        /// Creates a structured append data segment.
        /// </summary>
        /// <param name="position">The position of the QR code within the message (1 to 16).</param>
        /// <param name="total">The total number of QR code used for the message (1 to 16).</param>
        /// <param name="parity">The message parity.</param>
        /// <returns>A structured append data segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if 1 &#x2264; position &#x2264; total &#x2264; 16 is violated. </exception>
        public static DataSegment MakeStructuredAppend(int position, int total, byte parity)
        {
            return new DataSegmentStructuredAppend(position, total, parity);
        }
        
        #endregion
        
        #region Length Calculation

        /// <summary>
        /// Calculates the encoded length of the payload.
        /// <para>
        /// The length does not include the mode indicator and  the count indicator.
        /// </para>
        /// <para>
        /// This function only supports the encoding modes <em>Numeric</em>,
        /// <em>AlphaNumeric</em>, <em>Kanji</em> and <em>Binary</em>.
        /// </para>
        /// </summary>
        /// <param name="mode">The encoding mode.</param>
        /// <param name="length">The number of bytes to encode.</param>
        /// <returns>The payload length, in bits.</returns>
        private static int GetDataBitLength(DataSegmentMode mode, int length)
        {
            switch (mode)
            {
                case DataSegmentMode.Numeric:
                    return DataSegmentNumeric.GetNumericBitLength(length);
                case DataSegmentMode.Alphanumeric:
                    return DataSegmentAlphanumeric.GetAlphanumericBitLength(length);
                case DataSegmentMode.Kanji:
                    return DataSegmentKanji.GetKanjiBitLength(length);
                case DataSegmentMode.Binary:
                    return DataSegmentByte.GetByteBitLength(length);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mode is not supported by this function.");
            }
        }
        
        /// <summary>
        /// Calculates the segment length.
        /// <para>
        /// The segment length includes the mode indicator, the count indicator, and the data bits.
        /// </para>
        /// <para>
        /// This function only supports the encoding modes <em>Numeric</em>,
        /// <em>AlphaNumeric</em>, <em>Kanji</em> and <em>Binary</em>.
        /// </para>
        /// </summary>
        /// <param name="mode">The encoding mode.</param>
        /// <param name="length">The number of bytes to encode.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>The segment length, in bits.</returns>
        internal static int GetBitLength(DataSegmentMode mode, int length, int version)
        {
            return GetHeaderLength(mode, version) + GetDataBitLength(mode, length);
        }

        internal static int GetBitLength(IEnumerable<DataSegment> segments, int version)
        {
            return segments.Sum(segment => segment.GetTotalLength(version));
        }

        /// <summary>
        /// Gets the number of bytes that will fit in a segment with the given bit length.
        /// <para>
        /// The specified bit length is without the size of mode and count indicator.
        /// </para>
        /// </summary>
        /// <param name="mode">The data segment mode.</param>
        /// <param name="bitLength">The encoded data length, in bits.</param>
        /// <returns>The number of bytes or characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unsupported mode is specified.</exception>
        internal static int GetByteCount(DataSegmentMode mode, int bitLength)
        {
            switch (mode)
            {
                case DataSegmentMode.Numeric:
                    return DataSegmentNumeric.GetNumericByteCount(bitLength);
                case DataSegmentMode.Alphanumeric:
                    return DataSegmentAlphanumeric.GetAlphanumericByteCount(bitLength);
                case DataSegmentMode.Kanji:
                    return DataSegmentKanji.GetKanjiByteCount(bitLength);
                case DataSegmentMode.Binary:
                    return DataSegmentByte.GetByteByteCount(bitLength);
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, "This mode is not supported by this function.");
            }
        }
        
        /// <summary>
        /// Calculates the length of the header (mode indicator and count indicator).
        /// </summary>
        /// <param name="mode">The encoding mode.</param>
        /// <param name="version">The QR code version.</param>
        /// <returns>The header length, in bits.</returns>
        internal static int GetHeaderLength(DataSegmentMode mode, int version)
        {
            return 4 + GetCountIndicatorLength(mode, version);
        }

        private static int GetCountIndicatorLength(DataSegmentMode mode, int version)
        {
            // Groups are: 1 to 9, 10 to 26, 27 to 40
            var versionGroup = (version + 7) / 17;
            return CountIndicatorLength[(int)mode * 3 - 3 + versionGroup];
        }

        private static readonly int[] CountIndicatorLength =
        {
            10, 12, 14, // numeric
            9, 11, 13,  // alphanumeric
            8, 10, 12,  // Kanji
            8, 16, 16,  // binary
            0, 0, 0,    // ECI designator
            0, 0, 0     // structured append
        };
        
        #endregion
        

        #region Text

        /// <summary>
        /// Gets the text encoded in the given sequence of segments.
        /// </summary>
        /// <param name="segments">The segments.</param>
        /// <returns>The text.</returns>
        public static string GetText(IEnumerable<DataSegment> segments)
        {
            var encoding = ECI.Latin1.GetEncoding();
            var text = new StringBuilder();
            foreach (var segment in segments)
            {
                if (segment is DataSegmentEci eci)
                {
                    encoding = eci.Designator.GetEncoding();
                }
                else if (segment is DataSegmentStructuredAppend)
                {
                    // ignore
                }
                else
                {
                    var dataBytes = segment.DataBytes;
                    Debug.Assert(dataBytes.Array != null);
                    text.Append(encoding.GetString(dataBytes.Array, dataBytes.Offset, dataBytes.Count));
                }
            }
            return text.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[Mode: {Mode}; Data length: {DataLength}]";
        }
        
        #endregion
    }
}

