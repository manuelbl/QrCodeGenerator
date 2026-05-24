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
    /// A rectangular block of dark modules within a QR code.
    /// <para>
    /// The coordinates use the same system as <see cref="QrCode.GetModule"/>:
    /// the top-left module is at (x=0, y=0), <i>x</i> extends to the right and
    /// <i>y</i> extends downwards. Each unit is one module (QR code pixel);
    /// no border is included.
    /// </para>
    /// <para>
    /// Instances are produced by <see cref="QrCode.ToRectangles"/>, which merges
    /// adjacent dark modules into larger rectangles to reduce the number of shapes
    /// that need to be drawn.
    /// </para>
    /// </summary>
    /// <seealso cref="QrCode.ToRectangles"/>
    public readonly struct QrRectangle : IEquatable<QrRectangle>
    {
        /// <summary>
        /// Initializes a new rectangle with the specified position and size.
        /// </summary>
        /// <param name="x">The x-coordinate of the top-left module.</param>
        /// <param name="y">The y-coordinate of the top-left module.</param>
        /// <param name="width">The width, in modules.</param>
        /// <param name="height">The height, in modules.</param>
        public QrRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// The x-coordinate of the top-left module of this rectangle.
        /// </summary>
        /// <value>The x-coordinate, in modules.</value>
        public int X { get; }

        /// <summary>
        /// The y-coordinate of the top-left module of this rectangle.
        /// </summary>
        /// <value>The y-coordinate, in modules.</value>
        public int Y { get; }

        /// <summary>
        /// The width of this rectangle.
        /// </summary>
        /// <value>The width, in modules.</value>
        public int Width { get; }

        /// <summary>
        /// The height of this rectangle.
        /// </summary>
        /// <value>The height, in modules.</value>
        public int Height { get; }

        /// <summary>
        /// Deconstructs this rectangle into its position and size.
        /// </summary>
        /// <param name="x">Receives the x-coordinate of the top-left module.</param>
        /// <param name="y">Receives the y-coordinate of the top-left module.</param>
        /// <param name="width">Receives the width, in modules.</param>
        /// <param name="height">Receives the height, in modules.</param>
        public void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        /// <summary>
        /// Determines whether this rectangle is equal to the specified rectangle.
        /// </summary>
        /// <param name="other">The rectangle to compare with.</param>
        /// <returns><c>true</c> if both rectangles have the same position and size.</returns>
        public bool Equals(QrRectangle other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        /// <summary>
        /// Determines whether this rectangle is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="QrRectangle"/>
        /// with the same position and size.</returns>
        public override bool Equals(object obj)
        {
            return obj is QrRectangle other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this rectangle.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Width;
                hash = hash * 31 + Height;
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this rectangle.
        /// </summary>
        /// <returns>A string with the position and size.</returns>
        public override string ToString()
        {
            return $"QrRectangle(X={X}, Y={Y}, Width={Width}, Height={Height})";
        }

        /// <summary>
        /// Determines whether two rectangles are equal.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><c>true</c> if both rectangles have the same position and size.</returns>
        public static bool operator ==(QrRectangle left, QrRectangle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two rectangles are not equal.
        /// </summary>
        /// <param name="left">The first rectangle.</param>
        /// <param name="right">The second rectangle.</param>
        /// <returns><c>true</c> if the rectangles differ in position or size.</returns>
        public static bool operator !=(QrRectangle left, QrRectangle right)
        {
            return !left.Equals(right);
        }
    }
}
