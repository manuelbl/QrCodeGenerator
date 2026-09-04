/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Collections.Generic;

namespace Net.Codecrete.QrCodeGenerator
{
    /// <summary>
    /// Traces the outline of the dark modules of a QR code as closed polygons.
    /// <para>
    /// A boundary edge is a side of a dark module whose neighbor is light or outside the QR code.
    /// Every boundary edge is directed so that the dark module lies to its <i>right</i>: the top
    /// side of a dark module points east, the right side south, the bottom side west and the left
    /// side north. Under that orientation, each directed edge has exactly one successor, so the
    /// boundary edges partition into closed loops &#x2014; and a loop around a group of dark modules
    /// comes out clockwise, a loop around a hole counterclockwise, without either case being
    /// detected.
    /// </para>
    /// <para>
    /// The walk keeps the dark module on its right. Arriving at a corner point, the two modules
    /// ahead decide the direction: a light module ahead-right turns the walk right, a dark module
    /// ahead-left turns it left, otherwise it continues straight. The right turn is the whole
    /// connectivity rule of the outline: at a corner where two dark modules touch only diagonally,
    /// turning right hugs the module the walk came along, so diagonal neighbors stay in separate
    /// loops &#x2014; groups are connected horizontally and vertically only. Seen from the light
    /// side, the same right turn merges two diagonally touching light areas into one hole, the usual
    /// duality of 4-connected foreground and 8-connected background. Such a loop passes through the
    /// shared corner twice.
    /// </para>
    /// <para>
    /// A vertex is recorded only where the direction changes, so collinear edges collapse and the
    /// horizontal and vertical edges of a loop strictly alternate. Every loop contains at least one
    /// east edge (the topmost run of a group, or the underside of a hole), so scanning for dark
    /// modules with a light module above finds every loop, and marking the east edges walked makes
    /// the scan skip loops already traced. Each loop is rotated to start at its topmost, then
    /// leftmost vertex, and the loops are sorted by that start vertex in reading order.
    /// </para>
    /// </summary>
    internal static class OutlineBuilder
    {
        // The x steps of the directions east, south, west and north.
        private static readonly int[] StepX = { 1, 0, -1, 0 };

        // The y steps of the directions east, south, west and north.
        private static readonly int[] StepY = { 0, 1, 0, -1 };

        private const int East = 0;
        private const int South = 1;
        private const int West = 2;

        // Traces the outline of the dark modules of the given matrix, which is not modified.
        // The polygons are ordered by their start vertex in reading order.
        internal static IReadOnlyList<QrPolygon> Build(BitMatrix modules)
        {
            var size = modules.Size;

            // whether the east edge along the top of module (x, y) has been walked
            var walkedEastEdges = new BitMatrix(size);

            var polygons = new List<QrPolygon>();
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    if (modules.Get(x, y) && !IsDark(modules, x, y - 1) && !walkedEastEdges.Get(x, y))
                    {
                        polygons.Add(TraceLoop(modules, walkedEastEdges, x, y));
                    }
                }
            }

            polygons.Sort((a, b) =>
            {
                var startA = a.Vertices[0];
                var startB = b.Vertices[0];
                return startA.Y != startB.Y ? startA.Y - startB.Y : startA.X - startB.X;
            });
            return polygons;
        }

        // Walks the loop containing the east edge that starts at the corner point (x, y),
        // and marks the east edges it walks.
        private static QrPolygon TraceLoop(BitMatrix modules, BitMatrix walkedEastEdges, int x, int y)
        {
            var vertices = new List<QrPoint>();

            var vx = x;
            var vy = y;
            var direction = East;
            do
            {
                if (direction == East)
                {
                    walkedEastEdges.Set(vx, vy, true);
                }
                vx += StepX[direction];
                vy += StepY[direction];

                var turned = Turn(modules, vx, vy, direction);
                if (turned != direction)
                {
                    vertices.Add(new QrPoint(vx, vy));
                    direction = turned;
                }

                // The loop is closed when the walk is about to repeat the edge it started on. The
                // start vertex alone is not enough: a loop pinched at the start vertex passes
                // through it twice, in different directions.
            } while (vx != x || vy != y || direction != East);

            return new QrPolygon(RotateToTopLeft(vertices));
        }

        // Decides the direction in which the walk leaves the corner point (x, y), arrived at in the
        // given direction: a light module ahead-right turns it right, a dark module ahead-left turns
        // it left, otherwise it continues straight.
        private static int Turn(BitMatrix modules, int x, int y, int direction)
        {
            // Of the four modules around the corner point, the one ahead-right and the one
            // ahead-left. Heading east they are the ones below and above the continuing edge; the
            // other directions follow by rotation.
            bool aheadRight;
            bool aheadLeft;
            switch (direction)
            {
                case East:
                    aheadRight = IsDark(modules, x, y);
                    aheadLeft = IsDark(modules, x, y - 1);
                    break;
                case South:
                    aheadRight = IsDark(modules, x - 1, y);
                    aheadLeft = IsDark(modules, x, y);
                    break;
                case West:
                    aheadRight = IsDark(modules, x - 1, y - 1);
                    aheadLeft = IsDark(modules, x - 1, y);
                    break;
                default: // north
                    aheadRight = IsDark(modules, x, y - 1);
                    aheadLeft = IsDark(modules, x - 1, y - 1);
                    break;
            }

            if (!aheadRight)
            {
                return (direction + 1) % 4;
            }
            return aheadLeft ? (direction + 3) % 4 : direction;
        }

        // Gets the color of the module at the given coordinates, with everything outside the
        // QR code light.
        private static bool IsDark(BitMatrix modules, int x, int y)
        {
            var size = modules.Size;
            return 0 <= x && x < size && 0 <= y && y < size && modules.Get(x, y);
        }

        // Rotates the vertices so the loop starts at its topmost, then leftmost vertex.
        //
        // That vertex is a corner of every rectilinear loop, so it is in the list: the interior
        // points of a horizontal run share their y with an endpoint further left, those of a
        // vertical run lie below an endpoint.
        private static List<QrPoint> RotateToTopLeft(List<QrPoint> vertices)
        {
            var start = 0;
            for (var i = 1; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                var best = vertices[start];
                if (vertex.Y < best.Y || (vertex.Y == best.Y && vertex.X < best.X))
                {
                    start = i;
                }
            }

            if (start == 0)
            {
                return vertices;
            }

            var rotated = new List<QrPoint>(vertices.Count);
            rotated.AddRange(vertices.GetRange(start, vertices.Count - start));
            rotated.AddRange(vertices.GetRange(0, start));
            return rotated;
        }
    }
}
