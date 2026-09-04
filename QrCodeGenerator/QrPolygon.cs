/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// A closed loop of the outline of the dark modules of a QR code.
    /// <para>
    /// Consecutive vertices are joined by axis-parallel edges, alternating between horizontal
    /// and vertical; the closing edge from the last vertex back to the first one is implied.
    /// The first vertex is the topmost one, the leftmost of those. A vertex can occur twice in
    /// the loop where the outline touches itself at a single corner point.
    /// </para>
    /// <para>
    /// A loop around a group of dark modules runs clockwise (with <i>y</i> extending downwards),
    /// a loop around a hole within such a group runs counterclockwise. With this winding, filling
    /// the polygons of a QR code produces the dark modules under both the nonzero rule (the SVG
    /// default) and the even-odd rule (the XAML default).
    /// </para>
    /// <para>
    /// Instances are produced by <see cref="QrCode.ToOutlines"/>.
    /// </para>
    /// </summary>
    /// <seealso cref="QrCode.ToOutlines"/>
    public sealed class QrPolygon : IEquatable<QrPolygon>
    {
        // The vertices, copied from the ones passed to the constructor.
        private readonly QrPoint[] _vertices;

        /// <summary>
        /// Initializes a new polygon with the specified vertices.
        /// </summary>
        /// <param name="vertices">The corners of the loop, in drawing order (copied).</param>
        /// <exception cref="ArgumentNullException"><paramref name="vertices"/> is <c>null</c>.</exception>
        public QrPolygon(IReadOnlyList<QrPoint> vertices)
        {
            Objects.RequireNonNull(vertices, nameof(vertices));

            _vertices = new QrPoint[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
            {
                _vertices[i] = vertices[i];
            }
        }

        /// <summary>
        /// The corners of this loop, in drawing order.
        /// </summary>
        /// <value>The vertices.</value>
        public IReadOnlyList<QrPoint> Vertices => _vertices;

        /// <summary>
        /// Determines whether this polygon is equal to the specified polygon.
        /// </summary>
        /// <param name="other">The polygon to compare with.</param>
        /// <returns><c>true</c> if both polygons have the same vertices, in the same order.</returns>
        public bool Equals(QrPolygon other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (_vertices.Length != other._vertices.Length)
            {
                return false;
            }

            for (var i = 0; i < _vertices.Length; i++)
            {
                if (_vertices[i] != other._vertices[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines whether this polygon is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="QrPolygon"/>
        /// with the same vertices, in the same order.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as QrPolygon);
        }

        /// <summary>
        /// Returns the hash code for this polygon.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                foreach (var vertex in _vertices)
                {
                    hash = hash * 31 + vertex.GetHashCode();
                }
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this polygon.
        /// </summary>
        /// <returns>A string with the vertices.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("QrPolygon(");
            for (var i = 0; i < _vertices.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(", ");
                }
                sb.Append('(').Append(_vertices[i].X).Append(',').Append(_vertices[i].Y).Append(')');
            }
            return sb.Append(')').ToString();
        }

        /// <summary>
        /// Determines whether two polygons are equal.
        /// </summary>
        /// <param name="left">The first polygon.</param>
        /// <param name="right">The second polygon.</param>
        /// <returns><c>true</c> if both polygons have the same vertices, in the same order.</returns>
        public static bool operator ==(QrPolygon left, QrPolygon right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
        }

        /// <summary>
        /// Determines whether two polygons are not equal.
        /// </summary>
        /// <param name="left">The first polygon.</param>
        /// <param name="right">The second polygon.</param>
        /// <returns><c>true</c> if the polygons differ in a vertex.</returns>
        public static bool operator !=(QrPolygon left, QrPolygon right)
        {
            return !(left == right);
        }
    }
}
