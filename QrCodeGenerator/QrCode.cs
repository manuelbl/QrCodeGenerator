/* 
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 * Copyright (c) Project Nayuki (MIT License)
 * https://www.nayuki.io/page/qr-code-generator-library
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Represents a QR code containing text or binary data.
    /// <para>
    /// Instances of this class represent an immutable square grid of dark and light pixels
    /// (called <i>modules</i> by the QR code specification).
    /// Static factory methods are provided to create QR codes from text or binary data.
    /// Some of the methods provide detailed control about the encoding parameters such a QR
    /// code size (called <i>version</i> by the standard), error correction level and mask.
    /// </para>
    /// <para>
    /// QR codes are a type of two-dimensional barcodes, invented by Denso Wave and
    /// described in the ISO/IEC 18004 standard.
    /// </para>
    /// <para>
    /// This class covers the QR Code Model 2 specification, supporting all versions (sizes)
    /// from 1 to 40, all 4 error correction levels, and 4 character encoding modes.</para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// To create a QR code instance:
    /// </para>
    /// <ul>
    ///   <li>High level: Take the payload data and call <see cref="EncodeText(string, Ecc)"/>
    ///       or <see cref="EncodeBinary(byte[], Ecc)"/>.</li>
    ///   <li>Mid level: Custom-make a list of <see cref="QrSegment"/> instances and call
    ///       <see cref="EncodeSegments"/></li>
    ///   <li>Low level: Custom-make an array of data codeword bytes (including segment headers and
    ///       final padding, excluding error correction codewords), supply the appropriate version number,
    ///       and call the <see cref="QrCode(int, Ecc, byte[], int)"/>.</li>
    /// </ul>
    /// </remarks>
    /// <seealso cref="QrSegment"/>
    public class QrCode
    {
        #region Static factory functions (high level)

        /// <summary>
        /// Creates a QR code representing the specified text using the specified error correction level.
        /// <para>
        /// As a conservative upper bound, this function is guaranteed to succeed for strings with up to 738
        /// Unicode code points (not UTF-16 code units) if the low error correction level is used. The smallest possible
        /// QR code version (size) is automatically chosen. The resulting ECC level will be higher than the one
        /// specified if it can be achieved without increasing the size (version).
        /// </para>
        /// </summary>
        /// <param name="text">The text to be encoded. The full range of Unicode characters may be used.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <returns>The created QR code instance representing the specified text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> or <paramref name="ecl"/> is <c>null</c>.</exception>
        /// <exception cref="DataTooLongException">The text is too long to fit in the largest QR code size (version)
        /// at the specified error correction level.</exception>
        public static QrCode EncodeText(string text, Ecc ecl)
        {
            Objects.RequireNonNull(text);
            Objects.RequireNonNull(ecl);
            var segments = QrSegment.MakeSegments(text);
            return EncodeSegments(segments, ecl);
        }

        /// <summary>
        /// Creates a QR code representing the specified binary data using the specified error correction level.
        /// <para>
        /// This function encodes the data in the binary segment mode. The maximum number of
        /// bytes allowed is 2953. The smallest possible QR code version is automatically chosen.
        /// The resulting ECC level will be higher than the one specified if it can be achieved without increasing the size (version).
        /// </para>
        /// </summary>
        /// <param name="data">The binary data to encode.</param>
        /// <param name="ecl">The minimum error correction level to use.</param>
        /// <returns>The created QR code representing the specified data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> or <paramref name="ecl"/> is <c>null</c>.</exception>
        /// <exception cref="DataTooLongException">The specified data is too long to fit in the largest QR code size (version)
        /// at the specified error correction level.</exception>
        public static QrCode EncodeBinary(byte[] data, Ecc ecl)
        {
            Objects.RequireNonNull(data);
            Objects.RequireNonNull(ecl);
            var seg = QrSegment.MakeBytes(data);
            return EncodeSegments(new List<QrSegment> { seg }, ecl);
        }

        #endregion


        #region Static factory functions (mid level)

        /// <summary>
        /// Creates a QR code representing the specified segments with the specified encoding parameters.
        /// <para>
        /// The smallest possible QR code version (size) is used. The range of versions can be
        /// restricted by the <paramref name="minVersion"/> and <paramref name="maxVersion"/> parameters.
        /// </para>
        /// <para>
        /// If <paramref name="boostEcl"/> is <c>true</c>, the resulting ECC level will be higher than the
        /// one specified if it can be achieved without increasing the size (version).
        /// </para>
        /// <para>
        /// The QR code mask is usually automatically chosen. It can be explicitly set with the <paramref name="mask"/>
        /// parameter by using a value between 0 to 7 (inclusive). -1 is for automatic mode (which may be slow).
        /// </para>
        /// <para>
        /// This function allows the user to create a custom sequence of segments that switches
        /// between modes (such as alphanumeric and byte) to encode text in less space and gives full control over all
        /// encoding parameters.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This is a mid-level API; the high-level APIs are <see cref="EncodeText(string, Ecc)"/>
        /// and <see cref="EncodeBinary(byte[], Ecc)"/>.
        /// </remarks>
        /// <param name="segments">The segments to encode.</param>
        /// <param name="ecl">The minimal or fixed error correction level to use .</param>
        /// <param name="minVersion">The minimum version (size) of the QR code (between 1 and 40).</param>
        /// <param name="maxVersion">The maximum version (size) of the QR code (between 1 and 40).</param>
        /// <param name="mask">The mask number to use (between 0 and 7), or -1 for automatic mask selection.</param>
        /// <param name="boostEcl">If <c>true</c> the ECC level wil be increased if it can be achieved without increasing the size (version).</param>
        /// <returns>The created QR code representing the segments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="segments"/>, any list element, or <paramref name="ecl"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">1 &#x2264; minVersion &#x2264; maxVersion &#x2264; 40
        /// or -1 &#x2264; mask &#x2264; 7 is violated.</exception>
        /// <exception cref="DataTooLongException">The segments are too long to fit in the largest QR code size (version)
        /// at the specified error correction level.</exception>
        public static QrCode EncodeSegments(List<QrSegment> segments, Ecc ecl, int minVersion = MinVersion, int maxVersion = MaxVersion, int mask = -1, bool boostEcl = true)
        {
            Objects.RequireNonNull(segments);
            Objects.RequireNonNull(ecl);
            if (minVersion < MinVersion || minVersion > maxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(minVersion), "Invalid value");
            }
            if (maxVersion > MaxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(maxVersion), "Invalid value");
            }
            if (mask < -1 || mask > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(mask), "Invalid value");
            }

            // Find the minimal version number to use
            int version, dataUsedBits;
            for (version = minVersion; ; version++)
            {
                var numDataBits = GetNumDataCodewords(version, ecl) * 8;  // Number of data bits available
                dataUsedBits = QrSegment.GetTotalBits(segments, version);
                if (dataUsedBits != -1 && dataUsedBits <= numDataBits)
                {
                    break;  // This version number is found to be suitable
                }

                if (version < maxVersion)
                {
                    continue;
                }

                // All versions in the range could not fit the given data
                var msg = "Segment too long";
                if (dataUsedBits != -1)
                {
                    msg = $"Data length = {dataUsedBits} bits, Max capacity = {numDataBits} bits";
                }

                throw new DataTooLongException(msg);
            }

            // Increase the error correction level while the data still fits in the current version number
            foreach (var newEcl in Ecc.AllValues)
            {  // From low to high
                if (boostEcl && dataUsedBits <= GetNumDataCodewords(version, newEcl) * 8)
                {
                    ecl = newEcl;
                }
            }

            // Concatenate all segments to create the data bit string
            var ba = new BitArray(0);
            foreach (var seg in segments)
            {
                ba.AppendBits(seg.EncodingMode.ModeBits, 4);
                ba.AppendBits((uint)seg.NumChars, seg.EncodingMode.NumCharCountBits(version));
                ba.AppendData(seg.GetData());
            }
            Debug.Assert(ba.Length == dataUsedBits);

            // Add terminator and pad up to a byte if applicable
            var dataCapacityBits = GetNumDataCodewords(version, ecl) * 8;
            Debug.Assert(ba.Length <= dataCapacityBits);
            ba.AppendBits(0, Math.Min(4, dataCapacityBits - ba.Length));
            ba.AppendBits(0, (8 - ba.Length % 8) % 8);
            Debug.Assert(ba.Length % 8 == 0);

            // Pad with alternating bytes until data capacity is reached
            for (uint padByte = 0xEC; ba.Length < dataCapacityBits; padByte ^= 0xEC ^ 0x11)
            {
                ba.AppendBits(padByte, 8);
            }

            // Pack bits into bytes in big endian
            var dataCodewords = new byte[ba.Length / 8];
            for (var i = 0; i < ba.Length; i++)
            {
                if (ba.Get(i))
                {
                    dataCodewords[i >> 3] |= (byte)(1 << (7 - (i & 7)));
                }
            }

            // Create the QR code object
            return new QrCode(version, ecl, dataCodewords, mask);
        }

        #endregion


        #region Public immutable properties

        /// <summary>
        /// The version (size) of this QR code (between 1 for the smallest and 40 for the biggest).
        /// </summary>
        /// <value>The QR code version (size).</value>
        public int Version { get; }

        /// <summary>
        /// The width and height of this QR code, in modules (pixels).
        /// The size is a value between 21 and 177.
        /// This is equal to version &#xD7; 4 + 17.
        /// </summary>
        /// <value>The QR code size.</value>
        public int Size { get; }

        /// <summary>
        /// The error correction level used for this QR code.
        /// </summary>
        /// <value>The error correction level.</value>
        public Ecc ErrorCorrectionLevel { get; }

        /// <summary>
        /// The index of the mask pattern used fort this QR code (between 0 and 7).
        /// <para>
        /// Even if a QR code is created with automatic mask selection (<c>mask</c> = 1),
        /// this property returns the effective mask used.
        /// </para>
        /// </summary>
        /// <value>The mask pattern index.</value>
        public int Mask { get; }

        #endregion


        #region Private grids of modules/pixels, with dimensions of size * size

        // The modules of this QR code (false = light, true = dark).
        // Immutable after constructor finishes. Accessed through GetModule().
        private readonly bool[] _modules;

        // Indicates function modules that are not subjected to masking. Discarded when constructor finishes.
        private readonly bool[] _isFunction;

        #endregion


        #region  Constructor (low level)

        /// <summary>
        /// Constructs a QR code with the specified version number,
        /// error correction level, data codeword bytes, and mask number.
        /// </summary>
        /// <remarks>
        /// This is a low-level API that most users should not use directly. A mid-level
        /// API is the <see cref="EncodeSegments"/> function.
        /// </remarks>
        /// <param name="version">The version (size) to use (between 1 to 40).</param>
        /// <param name="ecl">The error correction level to use.</param>
        /// <param name="dataCodewords">The bytes representing segments to encode (without ECC).</param>
        /// <param name="mask">The mask pattern to use (either -1 for automatic selection, or a value from 0 to 7 for fixed choice).</param>
        /// <exception cref="ArgumentNullException"><paramref name="ecl"/> or <paramref name="dataCodewords"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The version or mask value is out of range,
        /// or the data has an invalid length for the specified version and error correction level.</exception>
        public QrCode(int version, Ecc ecl, byte[] dataCodewords, int mask = -1)
        {
            // Check arguments and initialize fields
            if (version < MinVersion || version > MaxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "Version value out of range");
            }

            if (mask < -1 || mask > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(mask), "Mask value out of range");
            }

            Version = version;
            Size = version * 4 + 17;
            Objects.RequireNonNull(ecl);
            ErrorCorrectionLevel = ecl;
            Objects.RequireNonNull(dataCodewords);
            _modules = new bool[Size * Size];  // Initially all light
            _isFunction = new bool[Size * Size];

            // Compute ECC, draw modules, do masking
            DrawFunctionPatterns();
            var allCodewords = AddEccAndInterleave(dataCodewords);
            DrawCodewords(allCodewords);
            Mask = HandleConstructorMasking(mask);
            _isFunction = null;
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
            return 0 <= x && x < Size && 0 <= y && y < Size && _modules[y * Size + x];
        }

        /// <summary>
        /// Creates an SVG image of this QR code.
        /// <para>
        /// The images uses Unix newlines (\n), regardless of the platform.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size</param>
        /// <returns>The SVG image as a string.</returns>
        public string ToSvgString(int border)
        {
            return ToSvgString(border, "#000000", "#ffffff");
        }

        /// <summary>
        /// Creates an SVG image of this QR code.
        /// <para>
        /// The images uses Unix newlines (\n), regardless of the platform.
        /// </para>
        /// <para>
        /// Colors are specified using CSS color data type. Examples of valid values are
        /// "#339966", "fuchsia", "rgba(137, 23, 89, 0.3)".
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size</param>
        /// <param name="foreground">The foreground color.</param>
        /// <param name="background">The background color.</param>
        public string ToSvgString(int border, string foreground, string background)
        {
            return AsGraphics().ToSvgString(border, foreground, background);
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
        /// The path will look like this: <c>M3,3h7v1h-7z M12,3h1v4h-1z ... M70,71h1v1h-1z</c>. It
        /// is valid for SVG (<c>&lt;path d="M3,3h..." /&gt;</c>) and for XAML
        /// (<c>&lt;Path Data="M3,3h..." /&gt;</c>). For programmatic geometry creation in WPF see
        /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.geometry.parse?view=windowsdesktop-6.0">Geometry.Parse(String)</a>.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size</param>
        /// <returns>The graphics path</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if border is negative</exception>
        public string ToGraphicsPath(int border = 0)
        {
            return AsGraphics().ToGraphicsPath(border);
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
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size.</param>
        /// <param name="scale">The width and height, in pixels, of each module.</param>
        /// <param name="foreground">The foreground color (dark modules), in RGB value (little endian).</param>
        /// <param name="background">The background color (light modules), in RGB value (little endian).</param>
        /// <returns>Bitmap data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="border"/> is negative,
        /// <paramref name="scale"/> is less than 1 or the resulting image is wider than 32,768 pixels.</exception>
        public byte[] ToBmpBitmap(int border, int scale, int foreground, int background)
        {
            return AsGraphics().ToBmpBitmap(border, scale, foreground, background);
        }

        /// <summary>
        /// Creates bitmap in the BMP format data using black for dark modules and white for light modules.
        /// <para>
        /// The bitmap uses 1 bit per pixel and a color table with 2 entries.
        /// </para>
        /// </summary>
        /// <param name="border">The border width, as a factor of the module (QR code pixel) size.</param>
        /// <param name="scale">The width and height, in pixels, of each module.</param>
        /// <returns>Bitmap data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="border"/> is negative,
        /// <paramref name="scale"/> is less than 1 or the resulting image is wider than 32,768 pixels.</exception>
        public byte[] ToBmpBitmap(int border = 0, int scale = 1)
        {
            return ToBmpBitmap(border, scale, 0x000000, 0xffffff);
        }

        /// <summary>
        /// Creates an RGB color value in little endian format.
        /// </summary>
        /// <param name="red">Red component.</param>
        /// <param name="green">Green component.</param>
        /// <param name="blue">Blue component.</param>
        /// <returns>RGB color value</returns>
        public int RgbColor(byte red, byte green, byte blue)
        {

            return (red << 16) | (green << 8) | blue;
        }

        #endregion


        #region Private helper methods for constructor: Drawing function modules

        // Reads this object's version field, and draws and marks all function modules.
        private void DrawFunctionPatterns()
        {
            // Draw horizontal and vertical timing patterns
            for (var i = 0; i < Size; i++)
            {
                SetFunctionModule(6, i, i % 2 == 0);
                SetFunctionModule(i, 6, i % 2 == 0);
            }

            // Draw 3 finder patterns (all corners except bottom right; overwrites some timing modules)
            DrawFinderPattern(3, 3);
            DrawFinderPattern(Size - 4, 3);
            DrawFinderPattern(3, Size - 4);

            // Draw numerous alignment patterns
            var alignPatPos = GetAlignmentPatternPositions();
            var numAlign = alignPatPos.Length;
            for (var i = 0; i < numAlign; i++)
            {
                for (var j = 0; j < numAlign; j++)
                {
                    // Don't draw on the three finder corners
                    if (!(i == 0 && j == 0 || i == 0 && j == numAlign - 1 || i == numAlign - 1 && j == 0))
                    {
                        DrawAlignmentPattern(alignPatPos[i], alignPatPos[j]);
                    }
                }
            }

            // Draw configuration data
            DrawFormatBits(0);  // Dummy mask value; overwritten later in the constructor
            DrawVersion();
        }


        // Draws two copies of the format bits (with its own error correction code)
        // based on the given mask and this object's error correction level field.
        private void DrawFormatBits(uint mask)
        {
            // Calculate error correction code and pack bits
            var data = (ErrorCorrectionLevel.FormatBits << 3) | mask;  // errCorrLvl is uint2, mask is uint3
            var rem = data;
            for (var i = 0; i < 10; i++)
            {
                rem = (rem << 1) ^ ((rem >> 9) * 0x537);
            }

            var bits = ((data << 10) | rem) ^ 0x5412;  // uint15
            Debug.Assert(bits >> 15 == 0);

            // Draw first copy
            for (var i = 0; i <= 5; i++)
            {
                SetFunctionModule(8, i, GetBit(bits, i));
            }

            SetFunctionModule(8, 7, GetBit(bits, 6));
            SetFunctionModule(8, 8, GetBit(bits, 7));
            SetFunctionModule(7, 8, GetBit(bits, 8));
            for (var i = 9; i < 15; i++)
            {
                SetFunctionModule(14 - i, 8, GetBit(bits, i));
            }

            // Draw second copy
            for (var i = 0; i < 8; i++)
            {
                SetFunctionModule(Size - 1 - i, 8, GetBit(bits, i));
            }

            for (var i = 8; i < 15; i++)
            {
                SetFunctionModule(8, Size - 15 + i, GetBit(bits, i));
            }

            SetFunctionModule(8, Size - 8, true);  // Always dark
        }


        // Draws two copies of the version bits (with its own error correction code),
        // based on this object's version field, iff 7 <= version <= 40.
        private void DrawVersion()
        {
            if (Version < 7)
            {
                return;
            }

            // Calculate error correction code and pack bits
            var rem = (uint)Version;  // version is uint6, in the range [7, 40]
            for (var i = 0; i < 12; i++)
            {
                rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
            }

            var bits = ((uint)Version << 12) | rem;  // uint18
            Debug.Assert(bits >> 18 == 0);

            // Draw two copies
            for (var i = 0; i < 18; i++)
            {
                var bit = GetBit(bits, i);
                var a = Size - 11 + i % 3;
                var b = i / 3;
                SetFunctionModule(a, b, bit);
                SetFunctionModule(b, a, bit);
            }
        }

        // Draws a 9*9 finder pattern including the border separator,
        // with the center module at (x, y). Modules can be out of bounds.
        private void DrawFinderPattern(int x, int y)
        {
            for (var dy = -4; dy <= 4; dy++)
            {
                for (var dx = -4; dx <= 4; dx++)
                {
                    var dist = Math.Max(Math.Abs(dx), Math.Abs(dy));  // Chebyshev/infinity norm
                    int xx = x + dx, yy = y + dy;
                    if (0 <= xx && xx < Size && 0 <= yy && yy < Size)
                    {
                        SetFunctionModule(xx, yy, dist != 2 && dist != 4);
                    }
                }
            }
        }


        // Draws a 5*5 alignment pattern, with the center module
        // at (x, y). All modules must be in bounds.
        private void DrawAlignmentPattern(int x, int y)
        {
            for (var dy = -2; dy <= 2; dy++)
            {
                for (var dx = -2; dx <= 2; dx++)
                {
                    SetFunctionModule(x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
                }
            }
        }


        // Sets the color of a module and marks it as a function module.
        // Only used by the constructor. Coordinates must be in bounds.
        private void SetFunctionModule(int x, int y, bool isDark)
        {
            _modules[y * Size + x] = isDark;
            _isFunction[y * Size + x] = true;
        }

        private Graphics AsGraphics()
        {
            return new Graphics(Size, _modules);
        }

        #endregion


        #region Private helper methods for constructor: Codewords and masking

        // Returns a new byte string representing the given data with the appropriate error correction
        // codewords appended to it, based on this object's version and error correction level.
        private byte[] AddEccAndInterleave(byte[] data)
        {
            Objects.RequireNonNull(data);
            if (data.Length != GetNumDataCodewords(Version, ErrorCorrectionLevel))
            {
                throw new ArgumentOutOfRangeException();
            }

            // Calculate parameter numbers
            int numBlocks = NumErrorCorrectionBlocks[ErrorCorrectionLevel.Ordinal, Version];
            int blockEccLen = EccCodewordsPerBlock[ErrorCorrectionLevel.Ordinal, Version];
            var rawCodewords = GetNumRawDataModules(Version) / 8;
            var numShortBlocks = numBlocks - rawCodewords % numBlocks;
            var shortBlockLen = rawCodewords / numBlocks;

            // Split data into blocks and append ECC to each block
            var blocks = new byte[numBlocks][];
            var rs = new ReedSolomonGenerator(blockEccLen);
            for (int i = 0, k = 0; i < numBlocks; i++)
            {
                var dat = CopyOfRange(data, k, k + shortBlockLen - blockEccLen + (i < numShortBlocks ? 0 : 1));
                k += dat.Length;
                var block = CopyOf(dat, shortBlockLen + 1);
                var ecc = rs.GetRemainder(dat);
                Array.Copy(ecc, 0, block, block.Length - blockEccLen, ecc.Length);
                blocks[i] = block;
            }

            // Interleave (not concatenate) the bytes from every block into a single sequence
            var result = new byte[rawCodewords];
            for (int i = 0, k = 0; i < blocks[0].Length; i++)
            {
                for (var j = 0; j < blocks.Length; j++)
                {
                    // Skip the padding byte in short blocks
                    if (i == shortBlockLen - blockEccLen && j < numShortBlocks)
                    {
                        continue;
                    }

                    result[k] = blocks[j][i];
                    k++;
                }
            }
            return result;
        }


        // Draws the given sequence of 8-bit codewords (data and error correction) onto the entire
        // data area of this QR code. Function modules need to be marked off before this is called.
        private void DrawCodewords(byte[] data)
        {
            Objects.RequireNonNull(data);
            if (data.Length != GetNumRawDataModules(Version) / 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            var i = 0;  // Bit index into the data
                        // Do the funny zigzag scan
            for (var right = Size - 1; right >= 1; right -= 2)
            {
                // Index of right column in each column pair
                if (right == 6)
                {
                    right = 5;
                }

                for (var vert = 0; vert < Size; vert++)
                {
                    // Vertical counter
                    for (var j = 0; j < 2; j++)
                    {
                        var x = right - j;  // Actual x coordinate
                        var upward = ((right + 1) & 2) == 0;
                        var y = upward ? Size - 1 - vert : vert;  // Actual y coordinate
                        if (_isFunction[y * Size + x] || i >= data.Length * 8)
                        {
                            continue;
                        }

                        _modules[y * Size + x] = GetBit(data[(uint)i >> 3], 7 - (i & 7));
                        i++;
                        // If this QR code has any remainder bits (0 to 7), they were assigned as
                        // 0/false/light by the constructor and are left unchanged by this method
                    }
                }
            }
            Debug.Assert(i == data.Length * 8);
        }


        // XORs the codeword modules in this QR code with the given mask pattern.
        // The function modules must be marked and the codeword bits must be drawn
        // before masking. Due to the arithmetic of XOR, calling applyMask() with
        // the same mask value a second time will undo the mask. A final well-formed
        // QR code needs exactly one (not zero, two, etc.) mask applied.
        private void ApplyMask(uint mask)
        {
            if (mask > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(mask), "Mask value out of range");
            }

            var index = 0;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var invert = false;
                    switch (mask)
                    {
                        case 0: invert = (x + y) % 2 == 0; break;
                        case 1: invert = y % 2 == 0; break;
                        case 2: invert = x % 3 == 0; break;
                        case 3: invert = (x + y) % 3 == 0; break;
                        case 4: invert = (x / 3 + y / 2) % 2 == 0; break;
                        case 5: invert = x * y % 2 + x * y % 3 == 0; break;
                        case 6: invert = (x * y % 2 + x * y % 3) % 2 == 0; break;
                        case 7: invert = ((x + y) % 2 + x * y % 3) % 2 == 0; break;
                    }
                    _modules[index] ^= invert & !_isFunction[index];
                    index++;
                }
            }
        }


        // A messy helper function for the constructor. This QR code must be in an unmasked state when this
        // method is called. The given argument is the requested mask, which is -1 for auto or 0 to 7 for fixed.
        // This method applies and returns the actual mask chosen, from 0 to 7.
        private int HandleConstructorMasking(int mask)
        {
            if (mask == -1)
            {
                // Automatically choose best mask
                var minPenalty = int.MaxValue;
                for (uint i = 0; i < 8; i++)
                {
                    ApplyMask(i);
                    DrawFormatBits(i);
                    var penalty = GetPenaltyScore();
                    if (penalty < minPenalty)
                    {
                        mask = (int)i;
                        minPenalty = penalty;
                    }
                    ApplyMask(i);  // Undoes the mask due to XOR
                }
            }
            Debug.Assert(0 <= mask && mask <= 7);
            ApplyMask((uint)mask);  // Apply the final choice of mask
            DrawFormatBits((uint)mask);  // Overwrite old format bits
            return mask;  // The caller shall assign this value to the final-declared field
        }


        // Calculates and returns the penalty score based on state of this QR code's current modules.
        // This is used by the automatic mask choice algorithm to find the mask pattern that yields the lowest score.
        private int GetPenaltyScore()
        {
            var result = 0;
            var finderPenalty = new FinderPenalty(Size);

            // Adjacent modules in row having same color, and finder-like patterns
            var index = 0;
            for (var y = 0; y < Size; y++)
            {
                var runColor = false;
                var runX = 0;
                finderPenalty.Reset();
                for (var x = 0; x < Size; x++)
                {
                    if (_modules[index] == runColor)
                    {
                        runX++;
                        if (runX == 5)
                        {
                            result += PenaltyN1;
                        }
                        else if (runX > 5)
                        {
                            result++;
                        }
                    }
                    else
                    {
                        finderPenalty.AddHistory(runX);
                        if (!runColor)
                        {
                            result += finderPenalty.CountPatterns() * PenaltyN3;
                        }

                        runColor = _modules[index];
                        runX = 1;
                    }

                    index++;
                }
                result += finderPenalty.TerminateAndCount(runColor, runX) * PenaltyN3;
            }

            // Adjacent modules in column having same color, and finder-like patterns
            for (var x = 0; x < Size; x++)
            {
                index = x;
                var runColor = false;
                var runY = 0;
                finderPenalty.Reset();
                for (var y = 0; y < Size; y++)
                {
                    if (_modules[index] == runColor)
                    {
                        runY++;
                        if (runY == 5)
                        {
                            result += PenaltyN1;
                        }
                        else if (runY > 5)
                        {
                            result++;
                        }
                    }
                    else
                    {
                        finderPenalty.AddHistory(runY);
                        if (!runColor)
                        {
                            result += finderPenalty.CountPatterns() * PenaltyN3;
                        }

                        runColor = _modules[index];
                        runY = 1;
                    }

                    index += Size;
                }
                result += finderPenalty.TerminateAndCount(runColor, runY) * PenaltyN3;
            }

            // 2*2 blocks of modules having same color
            index = 0;
            for (var y = 0; y < Size - 1; y++)
            {
                for (var x = 0; x < Size - 1; x++)
                {
                    var color = _modules[index];
                    if (color == _modules[index + 1] &&
                          color == _modules[index + Size] &&
                          color == _modules[index + Size + 1])
                    {
                        result += PenaltyN2;
                    }

                    index++;
                }

                index++;
            }

            // Balance of dark and light modules
            var dark = 0;
            index = 0;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (_modules[index])
                    {
                        dark++;
                    }

                    index++;
                }
            }
            var total = Size * Size;  // Note that size is odd, so dark/total != 1/2
                                      // Compute the smallest integer k >= 0 such that (45-5k)% <= dark/total <= (55+5k)%
            var k = (Math.Abs(dark * 20 - total * 10) + total - 1) / total - 1;
            result += k * PenaltyN4;
            return result;
        }


        #endregion


        #region Private helper functions

        // Returns an ascending list of positions of alignment patterns for this version number.
        // Each position is in the range [0,177), and are used on both the x and y axes.
        // This could be implemented as lookup table of 40 variable-length lists of unsigned bytes.
        private int[] GetAlignmentPatternPositions()
        {
            if (Version == 1)
            {
                return new int[] { };
            }

            var numAlign = Version / 7 + 2;
            int step;
            if (Version == 32)  // Special snowflake
            {
                step = 26;
            }
            else  // step = ceil[(size - 13) / (numAlign*2 - 2)] * 2
            {
                step = (Version * 4 + numAlign * 2 + 1) / (numAlign * 2 - 2) * 2;
            }

            var result = new int[numAlign];
            result[0] = 6;
            for (int i = result.Length - 1, pos = Size - 7; i >= 1; i--, pos -= step)
            {
                result[i] = pos;
            }

            return result;
        }

        // Returns the number of data bits that can be stored in a QR code of the given version number, after
        // all function modules are excluded. This includes remainder bits, so it might not be a multiple of 8.
        // The result is in the range [208, 29648]. This could be implemented as a 40-entry lookup table.
        private static int GetNumRawDataModules(int ver)
        {
            if (ver < MinVersion || ver > MaxVersion)
            {
                throw new ArgumentOutOfRangeException(nameof(ver), "Version number out of range");
            }

            var size = ver * 4 + 17;
            var result = size * size;   // Number of modules in the whole QR code square
            result -= 8 * 8 * 3;        // Subtract the three finders with separators
            result -= 15 * 2 + 1;       // Subtract the format information and dark module
            result -= (size - 16) * 2;  // Subtract the timing patterns (excluding finders)
                                        // The five lines above are equivalent to: int result = (16 * ver + 128) * ver + 64;

            if (ver < 2)
            {
                return result;
            }

            var numAlign = ver / 7 + 2;
            result -= (numAlign - 1) * (numAlign - 1) * 25;  // Subtract alignment patterns not overlapping with timing patterns
            result -= (numAlign - 2) * 2 * 20;  // Subtract alignment patterns that overlap with timing patterns
            // The two lines above are equivalent to: result -= (25 * numAlign - 10) * numAlign - 55;
            if (ver >= 7)
            {
                result -= 6 * 3 * 2;  // Subtract version information
            }
            Debug.Assert(208 <= result && result <= 29648);
            return result;
        }


        // Returns the number of 8-bit data (i.e. not error correction) codewords contained in any
        // QR code of the given version number and error correction level, with remainder bits discarded.
        // This stateless pure function could be implemented as a (40*4)-cell lookup table.
        internal static int GetNumDataCodewords(int ver, Ecc ecl)
        {
            return GetNumRawDataModules(ver) / 8
                - EccCodewordsPerBlock[ecl.Ordinal, ver]
                * NumErrorCorrectionBlocks[ecl.Ordinal, ver];
        }

        // Helper class for getPenaltyScore().
        // Internal the run history is organized in reverse order
        // (compared to Nayuki's code) to avoid the copying when
        // adding to the history.
        private struct FinderPenalty
        {
            private int _length;
            private readonly short[] _runHistory;
            private readonly int _size;

            internal FinderPenalty(int size)
            {
                _length = 0;
                _runHistory = new short[179];
                _size = size;
            }

            internal void Reset()
            {
                _length = 0;
            }

            // Can only be called immediately after a light run is added, and
            // returns either 0, 1, or 2.
            internal int CountPatterns()
            {
                if (_length < 7)
                {
                    return 0;
                }
                int n = _runHistory[_length - 6];
                Debug.Assert(n <= _size * 3);
                var core = n > 0
                           && _runHistory[_length - 5] == n
                           && _runHistory[_length - 4] == n * 3
                           && _runHistory[_length - 3] == n
                           && _runHistory[_length - 2] == n;
                return (core && _runHistory[_length - 7] >= n * 4 && _runHistory[_length - 1] >= n ? 1 : 0)
                     + (core && _runHistory[_length - 1] >= n * 4 && _runHistory[_length - 7] >= n ? 1 : 0);
            }

            // Pushes the given value to the front and drops the last value.
            internal int TerminateAndCount(bool currentRunColor, int currentRunLength)
            {
                if (currentRunColor)
                {
                    // Terminate dark run
                    AddHistory(currentRunLength);
                    currentRunLength = 0;
                }
                currentRunLength += _size;  // Add light border to final run
                AddHistory(currentRunLength);
                return CountPatterns();
            }

            // Adds the given value to the run history
            internal void AddHistory(int run)
            {
                _runHistory[_length] = (short)run;
                _length++;
            }
        }


        private static byte[] CopyOfRange(byte[] original, int from, int to)
        {
            var result = new byte[to - from];
            Array.Copy(original, from, result, 0, to - from);
            return result;
        }


        private static byte[] CopyOf(byte[] original, int newLength)
        {
            var result = new byte[newLength];
            Array.Copy(original, result, Math.Min(original.Length, newLength));
            return result;
        }


        // Returns true iff the i'th bit of x is set to 1.
        private static bool GetBit(uint x, int i)
        {
            return ((x >> i) & 1) != 0;
        }

        #endregion


        #region Constants and tables

        /// <summary>
        /// The minimum version (size) supported in the QR Code Model 2 standard – namely 1.
        /// </summary>
        /// <value>The minimum version.</value>
        public const int MinVersion = 1;

        /// <summary>
        /// The maximum version (size) supported in the QR Code Model 2 standard – namely 40.
        /// </summary>
        /// <value>The maximum version.</value>
        public const int MaxVersion = 40;


        // For use in getPenaltyScore(), when evaluating which mask is best.
        private const int PenaltyN1 = 3;
        private const int PenaltyN2 = 3;
        private const int PenaltyN3 = 40;
        private const int PenaltyN4 = 10;


        private static readonly byte[,] EccCodewordsPerBlock = {
		    // Version: (note that index 0 is for padding, and is set to an illegal value)
            //  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40     Error correction level		    { 255,  7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // Low
            { 255,  7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // Low
		    { 255, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28 },  // Medium
		    { 255, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },  // Quartile
		    { 255, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 }   // High
	    };

        private static readonly byte[,] NumErrorCorrectionBlocks = {
		    // Version: (note that index 0 is for padding, and is set to an illegal value)
		    //  0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40     Error correction level
		    { 255, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4,  4,  4,  4,  4,  6,  6,  6,  6,  7,  8,  8,  9,  9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25 },  // Low
		    { 255, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5,  5,  8,  9,  9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49 },  // Medium
		    { 255, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8,  8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68 },  // Quartile
		    { 255, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81 }   // High
	    };

        #endregion


        #region Public helper enumeration

        /// <summary>
        /// Error correction level in QR code symbol.
        /// </summary>
        public sealed class Ecc
        {
            /// <summary>
            /// Low error correction level. The QR code can tolerate about 7% erroneous codewords.
            /// </summary>
            /// <value>Low error correction level.</value>
            public static readonly Ecc Low = new Ecc(0, 1);

            /// <summary>
            /// Medium error correction level. The QR code can tolerate about 15% erroneous codewords.
            /// </summary>
            /// <value>Medium error correction level.</value>
            public static readonly Ecc Medium = new Ecc(1, 0);

            /// <summary>
            /// Quartile error correction level. The QR code can tolerate about 25% erroneous codewords.
            /// </summary>
            /// <value>Quartile error correction level.</value>
            public static readonly Ecc Quartile = new Ecc(2, 3);

            /// <summary>
            /// High error correction level. The QR code can tolerate about 30% erroneous codewords.
            /// </summary>
            /// <value>High error correction level.</value>
            public static readonly Ecc High = new Ecc(3, 2);


            internal static readonly Ecc[] AllValues = { Low, Medium, Quartile, High };

            /// <summary>
            /// Ordinal number of error correction level (in the range 0 to 3).
            /// </summary>
            /// <remarks>
            /// Higher number represent a higher amount of error tolerance.
            /// </remarks>
            /// <value>Ordinal number.</value>
            public int Ordinal { get; }

            // In the range 0 to 3 (unsigned 2-bit integer).
            internal uint FormatBits { get; }

            // Constructor.
            private Ecc(int ordinal, uint fb)
            {
                Ordinal = ordinal;
                FormatBits = fb;
            }
        }


        #endregion
    }
}
