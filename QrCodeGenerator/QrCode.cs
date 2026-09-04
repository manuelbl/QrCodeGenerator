/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Represents a QR code containing text or binary data.
    /// <para>
    /// QR codes are a type of two-dimensional barcodes, invented by Denso Wave and
    /// described in the ISO/IEC 18004 standard "QR code bar code symbology specification".
    /// </para>
    /// <para>
    /// The data is represented as square grid of dark and light pixels
    /// (called <i>modules</i> by the QR code specification).
    /// </para>
    /// <para>
    /// Depending on the amount of data, different sizes of the QR code are used.
    /// The QR code specification calls them <em>version</em>.
    /// Versions 1 (smallest) to 40 (largest) are available.
    /// </para>
    /// <para>
    /// QR codes include error correction data to ensure the QR code
    /// can be read despite difficult lighting conditions (e.g. reflections),
    /// dirt or partially covered pixels. Four error correction levels are
    /// available. The highest can recover about 30% of missing or damaged data.
    /// </para>
    /// <para>
    /// A QR code instance can be converted to a PNG (<see cref="ToPngBitmap"/>),
    /// SVG (<see cref="ToSvgString"/>) or BMP (<see cref="ToBmpBitmap"/>) image.
    /// Or the QR code can be drawn or printed with custom code by either
    /// processing a list of rectangles covering the dark modules (<see cref="ToRectangles"/>),
    /// by filling the outline of the dark modules (<see cref="ToOutlines"/>),
    /// or by querying the color of individual modules with <see cref="GetModule"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// To create a QR code instance, use one of the static factory methods:
    /// </para>
    /// <ul>
    /// <li><see cref="EncodeText"/></li>
    /// <li><see cref="EncodeTextAdvanced"/></li>
    /// <li><see cref="EncodeBinary"/></li>
    /// <li><see cref="EncodeTextInMultipleCodes"/></li>
    /// <li><see cref="EncodeTextInMultipleBalancedCodes"/></li>
    /// </ul>
    /// <para>
    /// For even more control about the QR code contents, first create the data
    /// segments and then use <see cref="EncodeSegments"/> to create the QR code.
    /// </para>
    /// </remarks>
    /// <seealso cref="DataSegment"/>
    public class QrCode
    {
        #region Factory Functions

        /// <summary>
        /// Creates a QR code representing the specified text using the specified error correction level.
        /// <para>
        /// The smallest possible QR code version (size) is automatically chosen.
        /// The resulting ECC level will be higher than the one specified
        /// if it can be achieved without increasing the QR code size (version).
        /// </para>
        /// </summary>
        /// <param name="text">The text to be encoded.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <returns>A QR code instance representing the specified text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="DataTooLongException">The text is too long to fit in the largest QR code size (version)
        /// at the specified error correction level.</exception>
        public static QrCode EncodeText(string text, Ecc ecl)
        {
            return EncodeTextAdvanced(text, ecl);
        }

        /// <summary>
        /// Creates a QR code representing the specified text.
        /// <para>
        /// This method offers several optional parameters to control the QR code creation.
        /// </para>
        /// <para>
        /// The text will be embedded as a sequence of data segments using different modes.
        /// They are chosen such that the smallest number of bits is needed to represent the text.
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
        /// be used if the text is encoded in Shift-JIS.
        /// </para>
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="minimumEcl">The minimal error correction level.</param>
        /// <param name="boostEcl">If <c>true</c>, the error correction level will be increased
        /// if it is possible without increasing the QR code size.
        /// If <c>false</c>, the minimal error correction level will be used.</param>
        /// <param name="minVersion">The minimal QR code version to consider.</param>
        /// <param name="maxVersion">The maximal QR code version to consider.</param>
        /// <param name="eci">The extended character set indicator segment to add,
        /// or <c>null</c> if it should be automatically determined.</param>
        /// <param name="encoding">The character set encoding to use,
        /// or <c>null</c> if it should be derived from the ECI.</param>
        /// <param name="kanjiStrategy">Controls if Kanji mode is used for data segments.</param>
        /// <param name="encodingInfo">If an instance is provided, it will be filled with information about the QR code encoding.
        /// Collecting the encoding information will slow down the QR code generation.</param>
        /// <returns>A QR code instance representing the specified text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">1 &#x2264; minVersion &#x2264; maxVersion &#x2264; 40 is violated.</exception>
        /// <exception cref="DataTooLongException">The text is too long to fit into a QR code of the specified
        /// maximum size (version).</exception>
        /// <remarks>
        /// Most encodings other than Latin-1 / ISO-8859-1 and the UTF encodings require that
        /// the encoding is registered (except for .NET Framework 4.x), typically using:
        /// <code>Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);</code>
        /// </remarks>
        [SuppressMessage("csharpsquid", "S107")]
        public static QrCode EncodeTextAdvanced(
            string text,
            Ecc minimumEcl,
            bool boostEcl = true,
            int minVersion = MinVersion,
            int maxVersion = MaxVersion,
            ECI eci = null,
            Encoding encoding = null,
            KanjiStrategy kanjiStrategy = KanjiStrategy.Automatic,
            EncodingInfo encodingInfo = null
        )
        {
            Objects.RequireNonNull(text, nameof(text));
            ValidateVersion(minVersion, maxVersion);
            
            var source = DataSegment.PrepareText(text, eci, encoding, kanjiStrategy);
            return BuildInVersionGroups(source, (int) minimumEcl, minVersion, maxVersion, boostEcl, encodingInfo);
        }

        /// <summary>
        /// Groups of versions sharing the same count-indicator widths (ISO/IEC 18004, Table 3).
        /// </summary>
        private static readonly (int Min, int Max)[] VersionGroups = { (1, 9), (10, 26), (27, 40) };

        /// <summary>
        /// Builds the smallest QR code in the version range, compacting the segments per version group.
        /// <para>
        /// The segment compaction depends on the version only through the count-indicator widths,
        /// so the optimal segments are the same for all versions of a group. The groups are tried in
        /// ascending order; the first group whose optimal segments fit yields the smallest QR code.
        /// </para>
        /// </summary>
        /// <param name="source">The data prepared for segment compaction.</param>
        /// <param name="ecc">The (minimum) error correction level.</param>
        /// <param name="minVersion">The minimal QR code version.</param>
        /// <param name="maxVersion">The maximal QR code version.</param>
        /// <param name="boostEcl">If <c>true</c>, increase the error correction level if possible.</param>
        /// <param name="encodingInfo">Optional diagnostics, or <c>null</c>.</param>
        private static QrCode BuildInVersionGroups(SegmentSource source, int ecc, int minVersion, int maxVersion,
            bool boostEcl, EncodingInfo encodingInfo)
        {
            var minBitLength = source.MinBitLength;

            foreach (var (groupMin, groupMax) in VersionGroups)
            {
                var lowest = Math.Max(minVersion, groupMin);
                var highest = Math.Min(maxVersion, groupMax);
                if (lowest > highest)
                {
                    continue;
                }

                // The last group in range builds unconditionally so that a too long payload throws.
                var isLastGroup = highest == maxVersion;
                if (!isLastGroup && !VersionPlanner.Fits(minBitLength, highest, ecc))
                {
                    continue;
                }

                var segments = source.ToSegments(highest);
                if (isLastGroup || VersionPlanner.Fits(segments, ecc, highest))
                {
                    return QrCodeBuilder.Build(segments, ecc, lowest, highest, boostEcl, encodingInfo);
                }
            }

            throw new InvalidOperationException("Version range does not overlap any version group");
        }

        /// <summary>
        /// Creates multiple QR codes representing the specified text.
        /// <para>
        /// The result will consist of the minimal number of QR codes needed
        /// to encode the text with the given error correction level and version (size of QR code).
        /// If multiple QR codes are required, <em>Structured Append</em> data is included to link the QR codes.
        /// </para>
        /// <para>
        /// If the text can be losslessly encoded in Latin-1, the encoding will be used and no
        /// extended channel indicators (ECI) will be added as Latin-1 is the default QR code text encoding.
        /// Otherwise, the text will be encoded in UTF-8, and the required ECI will be added to
        /// each QR code. 
        /// </para>
        /// <para>
        /// UTF-8 text will be split just that the splits are at character boundaries and not in the middle
        /// of a multibyte encoding of a character. Even though this is not explicitly mandated by
        /// the QR code specification, it increases compatibility with QR code scanners.
        /// </para>
        /// </summary>
        /// <param name="text">The text to be encoded. The full range of Unicode characters may be used.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <param name="version">The version (size of QR code) to use. Default is 29.</param>
        /// <returns>A list of QR codes representing the specified text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="DataTooLongException">The text is too long to fit the maximum of 16 QR codes with the given parameters.</exception>
        public static List<QrCode> EncodeTextInMultipleCodes(string text, Ecc ecl, int version = 29)
        {
            Objects.RequireNonNull(text, nameof(text));
            if (version < 1 || version > 40)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "'version' must be between 1 and 40.");
            }

            var qrCodes = StructuredAppend.BuildFromText(text, version, ecl);

            if (qrCodes.Count <= 2)
            {
                // test if it fits in a single QR code
                try
                {
                    var segments = DataSegment.FromText(text, version: version);
                    return new List<QrCode>
                        { QrCodeBuilder.Build(segments, (int)ecl, minVersion: version, maxVersion: version) };
                }
                catch (DataTooLongException)
                {
                    // fall through
                }
            }
            
            return qrCodes.ConvertAll(segments =>
                QrCodeBuilder.Build(segments, (int) ecl, minVersion: version, maxVersion: version));
        }

        /// <summary>
        /// Creates multiple evenly balanced QR codes representing the specified text.
        /// <para>
        /// The result will consist of the least number of QR codes needed to encode the text
        /// with the given error correction level, using a version (size) between
        /// <paramref name="minVersion"/> and <paramref name="maxVersion"/>.
        /// If multiple QR codes are required, <em>Structured Append</em> data is included to link them.
        /// </para>
        /// <para>
        /// Compared to <see cref="EncodeTextInMultipleCodes"/>, this method distributes the payload
        /// evenly among the codes instead of filling all but the last one to capacity. It first
        /// determines the least number of codes required, then reduces the shared version as much as
        /// possible (down to <paramref name="minVersion"/>) while still fitting that number of codes,
        /// and finally spreads the payload so the fullest code is as empty as possible. All resulting
        /// QR codes use the same version. The error correction level is increased if possible (boosted)
        /// for each QR code separately. So the resulting QR codes can use different error correction
        /// levels.
        /// </para>
        /// <para>
        /// If the text can be losslessly encoded in Latin-1, the encoding will be used and no
        /// extended channel indicators (ECI) will be added as Latin-1 is the default QR code text encoding.
        /// Otherwise, the text will be encoded in UTF-8, and the required ECI will be added to each QR code.
        /// UTF-8 text is split at character boundaries, not in the middle of a multibyte character.
        /// </para>
        /// <para>
        /// If the text fits into a single QR code, a single standalone code without structured append
        /// data is returned, using the smallest version at least <paramref name="minVersion"/>.
        /// </para>
        /// </summary>
        /// <param name="text">The text to be encoded. The full range of Unicode characters may be used.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <param name="minVersion">The minimum version (size) to use. Default is 10.</param>
        /// <param name="maxVersion">The maximum version (size) to use. Default is 29.</param>
        /// <returns>A list of QR codes representing the specified text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minVersion"/> or <paramref name="maxVersion"/> is out of range, or <paramref name="minVersion"/> is greater than <paramref name="maxVersion"/>.</exception>
        /// <exception cref="DataTooLongException">The text is too long to fit the maximum of 16 QR codes with the given parameters.</exception>
        public static List<QrCode> EncodeTextInMultipleBalancedCodes(string text, Ecc ecl, int minVersion = 10, int maxVersion = 29)
        {
            Objects.RequireNonNull(text, nameof(text));
            ValidateVersion(minVersion, maxVersion);

            var (qrCodes, version) = StructuredAppend.BuildBalancedFromText(text, minVersion, maxVersion, ecl);

            if (qrCodes.Count == 1)
            {
                // fits in a single QR code: emit it standalone (without structured append data),
                // using the smallest version at least minVersion.
                var segments = DataSegment.FromText(text, version: maxVersion);
                return new List<QrCode>
                    { QrCodeBuilder.Build(segments, (int)ecl, minVersion: minVersion, maxVersion: maxVersion) };
            }

            // Keep every code identical: same version and same ecl (possibly boosted).
            return qrCodes.ConvertAll(segments =>
                QrCodeBuilder.Build(segments, (int)ecl, minVersion: version, maxVersion: version));
        }

        /// <summary>
        /// Creates a QR code representing the specified binary data using the specified error correction level.
        /// <para>
        /// The smallest possible QR code version (size) is automatically chosen.
        /// The resulting ECC level will be higher than the one specified
        /// if it can be achieved without increasing the QR code size (version).
        /// </para>
        /// <para>
        /// Unless <paramref name="omitEci"/> is <c>true</c>, the QR code segments will start with
        /// an extended channel interpretation (ECI) code indicating binary data.
        /// </para>
        /// </summary>
        /// <param name="data">The binary data to encode.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <param name="omitEci">If <c>true</c>, no ECI is inserted to indicate binary data.</param>
        /// <returns>A QR code representing the specified data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="DataTooLongException">The specified data is too long to fit in the largest QR code size (version)
        /// at the specified error correction level.</exception>
        public static QrCode EncodeBinary(byte[] data, Ecc ecl, bool omitEci = false)
        {
            Objects.RequireNonNull(data, nameof(data));
            var source = DataSegment.PrepareBinaryData(data, omitEci ? ECI.None : ECI.BinaryData, false);
            return BuildInVersionGroups(source, (int)ecl, MinVersion, MaxVersion, true, null);
        }

        /// <summary>
        /// Creates a QR code representing the specified data segments.
        /// <para>
        /// The smallest possible QR code version (size) is used. The range of versions can be
        /// restricted by the <paramref name="minVersion"/> and <paramref name="maxVersion"/> parameters.
        /// </para>
        /// <para>
        /// If <paramref name="boostEcl"/> is <c>true</c>, the resulting ECC level will be higher than the
        /// one specified if it can be achieved without increasing the QR code size (version).
        /// </para>
        /// </summary>
        /// <param name="segments">The segments to encode.</param>
        /// <param name="ecl">The minimal or fixed error correction level to use.</param>
        /// <param name="minVersion">The minimum version (size) of the QR code (between 1 and 40).</param>
        /// <param name="maxVersion">The maximum version (size) of the QR code (between 1 and 40).</param>
        /// <param name="boostEcl">If <c>true</c>, the ECC level will be increased if it can be achieved
        /// without increasing the size (version).</param>
        /// <returns>A QR code representing the segments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="segments"/> or any list element is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">1 &#x2264; minVersion &#x2264; maxVersion &#x2264; 40 is violated.</exception>
        /// <exception cref="DataTooLongException">The segments are too long to fit in a QR code with the specified
        /// maximum size (version) and at the specified error correction level.</exception>
        public static QrCode EncodeSegments(
            List<DataSegment> segments,
            Ecc ecl,
            int minVersion = MinVersion,
            int maxVersion = MaxVersion,
            bool boostEcl = true)
        {
            Objects.RequireNonNull(segments, nameof(segments));
            ValidateVersion(minVersion, maxVersion);
            
            return QrCodeBuilder.Build(segments, (int)ecl, minVersion, maxVersion, boostEcl);
        }

        private static void ValidateVersion(int minVersion, int maxVersion)
        {
            if (minVersion < 1 || minVersion > 40)
            {
                throw new ArgumentOutOfRangeException(nameof(minVersion), "'minVersion' must be between 1 and 40.");
            }
            if (maxVersion < 1 || maxVersion > 40)
            {
                throw new ArgumentOutOfRangeException(nameof(maxVersion), "'maxVersion' must be between 1 and 40.");
            }
            if (minVersion > maxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(minVersion), "'minVersion' must be equal or less than 'maxVersion'.");
            }
        }

        #endregion


        #region Public immutable properties

        /// <summary>
        /// The version (size) of this QR code (between 1 for the smallest and 40 for the biggest).
        /// </summary>
        /// <value>The QR code version (size).</value>
        public int Version => (_modules.Size - 17) / 4;

        /// <summary>
        /// The width and height of this QR code, in modules (pixels).
        /// <para>
        /// The size is a value between 21 and 177.
        /// This is equal to version &#xD7; 4 + 17.
        /// </para>
        /// <para>
        /// The size does not include the border. The QR code should be
        /// displayed or printed such that it has a light area around it,
        /// which is at least 4 pixels wide.
        /// </para>
        /// </summary>
        /// <value>The QR code size.</value>
        public int Size => _modules.Size;

        /// <summary>
        /// The error correction level used for this QR code.
        /// </summary>
        /// <value>The error correction level.</value>
        public Ecc ErrorCorrectionLevel { get; }

        /// <summary>
        /// The index of the mask pattern used for this QR code (between 0 and 7).
        /// </summary>
        /// <value>The mask pattern index.</value>
        public int Mask { get; }

        #endregion


        #region Private grids of modules/pixels, with dimensions of size * size

        // The modules of this QR code (false = light, true = dark).
        private readonly BitMatrix _modules;

        #endregion


        #region  Constructor (internal)

        internal QrCode(BitMatrix modules, Ecc ecl, int mask)
        {
            _modules = modules;
            ErrorCorrectionLevel = ecl;
            Mask = mask;
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Gets the color of the module (pixel) at the specified coordinates.
        /// <para>
        /// The top left corner has the coordinates (x=0, y=0). <i>x</i>-coordinates extend from left to right,
        /// <i>y</i>-coordinates extend from top to bottom.
        /// </para>
        /// <para>
        /// If coordinates outside the bounds of this QR code are specified, light (<c>false</c>) is returned.
        /// </para>
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The color of the specified module: <c>true</c> for dark modules and <c>false</c>
        /// for light modules (or if the coordinates are outside the bounds).</returns>
        public bool GetModule(int x, int y)
        {
            return 0 <= x && x < Size && 0 <= y && y < Size && _modules.Get(x, y);
        }

        /// <summary>
        /// Creates an SVG image of this QR code.
        /// <para>
        /// The image uses Unix newlines (\n), regardless of the platform.
        /// </para>
        /// <para>
        /// Colors are specified using CSS color data type. Examples of valid values are
        /// "#339966", "fuchsia", "rgba(137, 23, 89, 0.3)".
        /// </para>
        /// <para>
        /// If <paramref name="background"/> is <c>null</c>, no background rectangle is drawn and
        /// the light modules and the border are transparent.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size.</param>
        /// <param name="foreground">The foreground color. Defaults to black.</param>
        /// <param name="background">The background color, or <c>null</c> for a transparent
        /// background. Defaults to white.</param>
        /// <returns>The SVG image, as a string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="border"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="foreground"/> is <c>null</c>.</exception>
        public string ToSvgString(int border, string foreground = "#000000", string background = "#ffffff")
        {
            return SvgBuilder.ToSvgString(this, border, foreground, background);
        }

        /// <summary>
        /// Creates a graphics path of this QR code valid in SVG or XAML.
        /// <para>
        /// The graphics path uses a coordinate system where each module is 1 unit wide and tall,
        /// and the top left module is offset vertically and horizontally by <i>border</i> units.
        /// </para>
        /// <para>
        /// Note that a border width other than 0 only make sense if the bounding box of the QR code
        /// is explicitly set by the graphics using this path. If the bounding box of this path is
        /// automatically derived, at least the right and bottom border will be missing.
        /// </para>
        /// <para>
        /// The path consists of one closed sub-path per outline polygon (see
        /// <see cref="ToOutlines"/>) and will look like this:
        /// <c>M3,3h7v7h-7z M4,4v5h5v-5z ... M70,71h1v1h-1z</c>. As the polygons around holes wind
        /// the other way than the polygons around groups of dark modules, no fill rule needs to be
        /// specified: the nonzero rule (the SVG default) and the even-odd rule (the XAML default)
        /// both fill the path to the QR code.
        /// </para>
        /// <para>
        /// The path is valid for SVG (<c>&lt;path d="M3,3h..." /&gt;</c>) and for XAML
        /// (<c>&lt;Path Data="M3,3h..." /&gt;</c>). For programmatic geometry creation in WPF see
        /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.geometry.parse?view=windowsdesktop-6.0">Geometry.Parse(String)</a>.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size</param>
        /// <returns>The graphics path</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if border is negative</exception>
        public string ToGraphicsPath(int border = 0)
        {
            return SvgBuilder.ToGraphicsPath(this, border);
        }

        /// <summary>
        /// Gets the dark modules of this QR code as a list of rectangles.
        /// <para>
        /// Adjacent dark modules are merged into larger rectangles so that most rectangles
        /// cover more than a single module, reducing the number of shapes to draw. This is
        /// the same set of rectangles used internally to render SVG and XAML output.
        /// </para>
        /// <para>
        /// The rectangles use the same coordinate system as <see cref="GetModule"/>: the
        /// top-left module is at (x=0, y=0) and each unit is one module (no border is included).
        /// </para>
        /// <para>
        /// The rectangles are non-overlapping and their union is exactly the set of dark modules.
        /// The order in which the rectangles appear in the list is unspecified.
        /// </para>
        /// <para>
        /// This method is intended for rendering the QR code to graphics formats that are not
        /// directly supported by this library. Where adjacent shapes leave hairline gaps
        /// (typically with anti-aliasing), fill a single path built from <see cref="ToOutlines"/>
        /// instead.
        /// </para>
        /// </summary>
        /// <returns>The list of rectangles covering the dark modules.</returns>
        /// <seealso cref="QrRectangle"/>
        public IReadOnlyList<QrRectangle> ToRectangles()
        {
            return RectangleBuilder.Build(_modules);
        }

        /// <summary>
        /// Gets the outline of the dark modules of this QR code, as a list of closed polygons.
        /// <para>
        /// Each polygon traces the boundary of a group of dark modules connected horizontally or
        /// vertically, or of a hole within such a group &#x2014; most notably the light ring of a
        /// finder pattern. Polygons around groups run clockwise, polygons around holes
        /// counterclockwise, so filling all of them as one path produces the QR code under both the
        /// nonzero and the even-odd fill rule. This is the geometry the SVG and XAML output is built
        /// from: a single filled path has no seams between adjacent shapes, where anti-aliased
        /// rendering would otherwise show hairlines.
        /// </para>
        /// <para>
        /// The vertices use the same coordinate system as <see cref="GetModule"/>, on the grid of
        /// module corners: the top-left corner of the QR code is at (x=0, y=0) and each unit is one
        /// module (no border is included).
        /// </para>
        /// </summary>
        /// <returns>The polygons, ordered by their start vertex in reading order.</returns>
        /// <seealso cref="QrPolygon"/>
        public IReadOnlyList<QrPolygon> ToOutlines()
        {
            return OutlineBuilder.Build(_modules);
        }

        /// <summary>
        /// Creates a bitmap in the BMP format.
        /// <para>
        /// The bitmap uses 1 bit per pixel and a color table with 2 entries.
        /// </para>
        /// <para>
        /// Color values can be created with <see cref="RgbColor(byte, byte, byte)"/>.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size. Defaults to 0.</param>
        /// <param name="scale">The width and height, in pixels, of each module. Defaults to 1.</param>
        /// <param name="foreground">The foreground color (dark modules), in RGB value (little endian). Defaults to black.</param>
        /// <param name="background">The background color (light modules), in RGB value (little endian). Defaults to white.</param>
        /// <returns>Bitmap data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="border"/> is negative,
        /// <paramref name="scale"/> is less than 1 or the resulting image is wider than 32,768 pixels.</exception>
        public byte[] ToBmpBitmap(int border = 0, int scale = 1, int foreground = 0x000000, int background = 0xffffff)
        {
            return BmpBuilder.ToImage(this, border, scale, foreground, background);
        }

        /// <summary>
        /// Creates a bitmap in the PNG format, using the specified foreground and background colors for dark and light modules.
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size. Defaults to 0.</param>
        /// <param name="scale">The width and height, in pixels, of each module. Defaults to 1.</param>
        /// <param name="foreground">The foreground color (dark modules), in RGB value (little endian). Defaults to black.</param>
        /// <param name="background">The background color (light modules), in RGB value (little endian). Defaults to white.</param>
        /// <returns>Bitmap data</returns>
        public byte[] ToPngBitmap(int border = 0, int scale = 1, int foreground = 0x000000, int background = 0xffffff)
        {
            return PngBuilder.ToImage(this, scale, border, foreground, background);
        }

        /// <summary>
        /// Creates an RGB color value in little endian format.
        /// </summary>
        /// <param name="red">Red component.</param>
        /// <param name="green">Green component.</param>
        /// <param name="blue">Blue component.</param>
        /// <returns>RGB color value</returns>
        public static int RgbColor(byte red, byte green, byte blue)
        {
            return (red << 16) | (green << 8) | blue;
        }

        #endregion



        #region Constants and tables

        /// <summary>
        /// The minimum version (size) supported in the QR code standard – namely 1.
        /// </summary>
        /// <value>The minimum version.</value>
        public const int MinVersion = 1;

        /// <summary>
        /// The maximum version (size) supported in the QR code standard – namely 40.
        /// </summary>
        /// <value>The maximum version.</value>
        public const int MaxVersion = 40;
        
        #endregion


        #region Error Correction Level

        /// <summary>
        /// Error correction level in QR code symbol.
        /// </summary>
        public enum Ecc
        {
            /// <summary>
            /// Low error correction level. The QR code can tolerate about 7% erroneous codewords.
            /// </summary>
            /// <value>Low error correction level.</value>
            Low = 0,

            /// <summary>
            /// Medium error correction level. The QR code can tolerate about 15% erroneous codewords.
            /// </summary>
            /// <value>Medium error correction level.</value>
            Medium = 1,

            /// <summary>
            /// Quartile error correction level. The QR code can tolerate about 25% erroneous codewords.
            /// </summary>
            /// <value>Quartile error correction level.</value>
            Quartile = 2,

            /// <summary>
            /// High error correction level. The QR code can tolerate about 30% erroneous codewords.
            /// </summary>
            /// <value>High error correction level.</value>
            High = 3
        }

        #endregion
    }
}
