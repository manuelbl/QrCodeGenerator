using System;
using System.Diagnostics;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Extensions members for <see cref="ArraySegment{T}"/>.
    /// <remarks>
    /// These extension members have been added to later .NET frameworks.
    /// They are only required for compatibility with .NET Standard 2.0.
    /// </remarks>
    /// </summary>
    internal static class ArraySegmentExtensions
    {
        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="segment">The array segment.</param>
        /// <param name="index">The index.</param>
        /// <typeparam name="T">The type of the elements in the array segment.</typeparam>
        /// <returns>The element.</returns>
        /// <remarks>In later versions of the .NET Framework, this item accessor is not
        /// needed as the struct includes an indexer.</remarks>
        internal static T At<T>(this ArraySegment<T> segment, int index)
        {
            Trace.Assert(segment.Array != null);
            return segment.Array[segment.Offset + index];
        }

        /// <summary>
        /// Forms a slice of the specified length out of the current array segment starting at the specified index.
        /// </summary>
        /// <param name="segment">The array segment.</param>
        /// <param name="startIndex">The index at which the slice starts.</param>
        /// <param name="length">The length of the slice.</param>
        /// <typeparam name="T">The type of the elements in the array segment.</typeparam>
        /// <returns>An array segment.</returns>
        /// <remarks>In later versions of the .NET Framework, this method is not
        /// needed as it was added to the struct.</remarks>
        internal static ArraySegment<T> MakeSlice<T>(this ArraySegment<T> segment, int startIndex, int length)
        {
            Trace.Assert(segment.Array != null);
            return new ArraySegment<T>(segment.Array, segment.Offset + startIndex, length);
        }
    }
}