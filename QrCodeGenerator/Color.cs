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

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Represents a 32 bpp color. Comprised of 4 <see cref="byte"/> values as the red (R), green (G), blue (B) and alpha (A) components.
    /// </summary>
    public readonly struct Color : IEquatable<Color>
    {
        /// <summary>
        /// RGBA(0, 0, 0, 255)
        /// </summary>
        public static readonly Color Black = new Color(0, 0, 0);
        /// <summary>
        /// RGBA(255, 255, 255, 255)
        /// </summary>
        public static readonly Color White = new Color(255, 255, 255);

        /// <summary>
        /// The red component.
        /// </summary>
        public readonly byte Red;
        /// <summary>
        /// The green component.
        /// </summary>
        public readonly byte Green;
        /// <summary>
        /// The blue component. 
        /// </summary>
        public readonly byte Blue;
        /// <summary>
        /// The alpha component.
        /// </summary>
        public readonly byte Alpha;

        /// <summary>
        /// Creates a new instance of the structure.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <param name="alpha">The alpha component.</param>
        public Color(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <inheritdoc/>
        public bool Equals(Color other)
        {
            return Red.Equals(other.Red) &&
                   Green.Equals(other.Green) &&
                   Blue.Equals(other.Blue) &&
                   Alpha.Equals(other.Alpha);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is Color clr && Equals(clr);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Red, Green, Blue, Alpha).GetHashCode();
        }

        /// <summary>
        /// Indicates whether two <see cref="Color"/> objects are equal. 
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if the <paramref name="left"/> object is equal to the <paramref name="right"/> object; otherwise, false.</returns>
        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }
        
        /// <inheritdoc cref="op_Equality"/>
        /// <summary>
        /// Indicates whether two <see cref="Color"/> objects are not equal. 
        /// </summary>
        /// <returns>true if the <paramref name="left"/> object is not equal to the <paramref name="right"/> object; otherwise, false.</returns>
        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }
    }
}