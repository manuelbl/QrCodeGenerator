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
    /// A corner point of a <see cref="QrPolygon"/>.
    /// <para>
    /// The point lies on the grid of module corners: the top-left corner of the QR code
    /// is at (x=0, y=0), <i>x</i> extends to the right and <i>y</i> extends downwards.
    /// Each unit is one module (QR code pixel); no border is included. A coordinate
    /// therefore ranges from 0 to the size of the QR code &#x2014; one more than the largest
    /// module coordinate, as a point names a corner, not a module.
    /// </para>
    /// </summary>
    /// <seealso cref="QrPolygon"/>
    public readonly struct QrPoint : IEquatable<QrPoint>
    {
        /// <summary>
        /// Initializes a new point with the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public QrPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The x-coordinate of this point.
        /// </summary>
        /// <value>The x-coordinate, in modules.</value>
        public int X { get; }

        /// <summary>
        /// The y-coordinate of this point.
        /// </summary>
        /// <value>The y-coordinate, in modules.</value>
        public int Y { get; }

        /// <summary>
        /// Deconstructs this point into its coordinates.
        /// </summary>
        /// <param name="x">Receives the x-coordinate.</param>
        /// <param name="y">Receives the y-coordinate.</param>
        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// Determines whether this point is equal to the specified point.
        /// </summary>
        /// <param name="other">The point to compare with.</param>
        /// <returns><c>true</c> if both points have the same coordinates.</returns>
        public bool Equals(QrPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Determines whether this point is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="QrPoint"/>
        /// with the same coordinates.</returns>
        public override bool Equals(object obj)
        {
            return obj is QrPoint other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this point.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this point.
        /// </summary>
        /// <returns>A string with the coordinates.</returns>
        public override string ToString()
        {
            return $"QrPoint(X={X}, Y={Y})";
        }

        /// <summary>
        /// Determines whether two points are equal.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><c>true</c> if both points have the same coordinates.</returns>
        public static bool operator ==(QrPoint left, QrPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two points are not equal.
        /// </summary>
        /// <param name="left">The first point.</param>
        /// <param name="right">The second point.</param>
        /// <returns><c>true</c> if the points differ in a coordinate.</returns>
        public static bool operator !=(QrPoint left, QrPoint right)
        {
            return !left.Equals(right);
        }
    }
}
