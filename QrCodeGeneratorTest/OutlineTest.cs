/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using Xunit;
using static Net.Codecrete.QrCodeGenerator.QrCode;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    /// <summary>
    /// Tests <see cref="QrCode.ToOutlines"/> and the boundary tracing behind it.
    /// <para>
    /// What has to hold is the invariant the renderers rely on: filling the polygons &#x2014; under
    /// the nonzero rule and under the even-odd rule alike &#x2014; reproduces exactly the dark
    /// modules. That is checked by rasterizing the loops again, across all test cases. The winding,
    /// the alternating axis-parallel edges, the start vertex and the loop order are asserted
    /// alongside, since the documentation promises them. The small hand-built matrices pin the
    /// connectivity decisions: diagonally touching dark modules stay separate loops, diagonally
    /// touching light areas merge into one hole.
    /// </para>
    /// </summary>
    public class OutlineTest
    {
        [Theory]
        [ClassData(typeof(QrCodeDataProvider))]
        public void Outlines_FillToTheDarkModules(QrCodeTestCase testCase)
        {
            var qrCode = EncodeSegments(testCase.Segments, testCase.RequestedEcc, testCase.MinVersion,
                testCase.MaxVersion, testCase.BoostEcl);

            var polygons = qrCode.ToOutlines();

            AssertLoopInvariants(polygons, qrCode);
            AssertFillsToTheDarkModules(polygons, qrCode);

            for (var i = 1; i < polygons.Count; i++)
            {
                var previous = polygons[i - 1].Vertices[0];
                var current = polygons[i].Vertices[0];
                Assert.True(previous.Y < current.Y || (previous.Y == current.Y && previous.X < current.X),
                    $"polygon starting at {current} follows the one starting at {previous}");
            }
        }

        // Asserts the documented shape of every loop: at least four vertices, strictly alternating
        // horizontal and vertical edges (the closing edge included), the topmost-then-leftmost
        // vertex first, and the winding matching what the loop surrounds — clockwise where the
        // module right below the start vertex is dark (a group), counterclockwise where it is
        // light (a hole).
        private static void AssertLoopInvariants(IReadOnlyList<QrPolygon> polygons, QrCode qrCode)
        {
            foreach (var polygon in polygons)
            {
                var vertices = polygon.Vertices;
                var count = vertices.Count;
                Assert.True(count >= 4, $"loop has only {count} vertices");
                Assert.True(count % 2 == 0, $"loop has an odd number of vertices ({count})");

                for (var i = 0; i < count; i++)
                {
                    var from = vertices[i];
                    var to = vertices[(i + 1) % count];
                    var horizontal = from.Y == to.Y;
                    Assert.True(horizontal ? from.X != to.X : from.X == to.X,
                        $"edge {from} to {to} is not axis-parallel");

                    var next = vertices[(i + 2) % count];
                    Assert.True(horizontal ? next.Y != to.Y : next.Y == to.Y,
                        $"edges do not alternate at {to}");
                }

                var start = vertices[0];
                foreach (var vertex in vertices)
                {
                    Assert.True(vertex.Y > start.Y || (vertex.Y == start.Y && vertex.X >= start.X),
                        $"{vertex} lies above or left of the start vertex {start}");
                }

                // At the topmost-then-leftmost vertex, the module diagonally below-right is inside
                // the group for an outer loop and inside the hole for a hole loop.
                var surroundsDarkModules = qrCode.GetModule(start.X, start.Y);
                var area = SignedArea(vertices);
                Assert.True(area != 0 && (area > 0) == surroundsDarkModules,
                    $"loop starting at {start} has signed area {area} but surrounds "
                    + $"{(surroundsDarkModules ? "dark" : "light")} modules");
            }
        }

        // The shoelace sum of the loop, positive for clockwise winding (with y extending downwards).
        private static long SignedArea(IReadOnlyList<QrPoint> vertices)
        {
            var sum = 0L;
            for (var i = 0; i < vertices.Count; i++)
            {
                var from = vertices[i];
                var to = vertices[(i + 1) % vertices.Count];
                sum += (long)from.X * to.Y - (long)to.X * from.Y;
            }
            return sum;
        }

        // Rasterizes the loops again and compares against the modules, once under the nonzero rule
        // and once under the even-odd rule.
        //
        // A scanline through the centre of a module row collects the vertical edges crossing it,
        // each at its x grid line with its direction; sweeping the row left to right, the crossings
        // passed so far give the winding number and the crossing parity at each module centre.
        private static void AssertFillsToTheDarkModules(IReadOnlyList<QrPolygon> polygons, QrCode qrCode)
        {
            var size = qrCode.Size;
            // one extra column: the QR code's right border produces crossings at grid line x = size,
            // to the right of every module centre
            var crossings = new int[size, size + 1];

            foreach (var polygon in polygons)
            {
                var vertices = polygon.Vertices;
                for (var i = 0; i < vertices.Count; i++)
                {
                    var from = vertices[i];
                    var to = vertices[(i + 1) % vertices.Count];
                    if (from.X != to.X)
                    {
                        continue;
                    }
                    var delta = to.Y > from.Y ? 1 : -1;
                    for (var y = Math.Min(from.Y, to.Y); y < Math.Max(from.Y, to.Y); y++)
                    {
                        crossings[y, from.X] += delta;
                    }
                }
            }

            for (var y = 0; y < size; y++)
            {
                var winding = 0;
                var parity = false;
                for (var x = 0; x < size; x++)
                {
                    winding += crossings[y, x];
                    parity ^= (crossings[y, x] & 1) != 0;
                    var isDark = qrCode.GetModule(x, y);
                    Assert.True(winding != 0 == isDark,
                        $"module ({x}, {y}): nonzero fill {(winding != 0 ? "dark" : "light")}, "
                        + $"but the module is {(isDark ? "dark" : "light")}");
                    Assert.True(parity == isDark,
                        $"module ({x}, {y}): even-odd fill {(parity ? "dark" : "light")}, "
                        + $"but the module is {(isDark ? "dark" : "light")}");
                }
            }
        }

        [Fact]
        public void AdjacentModules_FormOneLoop()
        {
            // The top-left finder pattern is a 7×7 ring: finding its outer boundary as a single
            // square proves the modules are traced as one shape rather than one by one.
            var qrCode = EncodeText("A", Ecc.Medium);

            Assert.Contains(Polygon(0, 0, 7, 0, 7, 7, 0, 7), qrCode.ToOutlines());
        }

        [Fact]
        public void Hole_IsWoundTheOtherWay()
        {
            // a 3×3 block with a light centre
            var modules = new BitMatrix(6);
            modules.FillRect(1, 1, 3, 3);
            modules.Set(2, 2, false);

            Assert.Equal(new[]
            {
                Polygon(1, 1, 4, 1, 4, 4, 1, 4),
                Polygon(2, 2, 2, 3, 3, 3, 3, 2)
            }, OutlineBuilder.Build(modules));
        }

        [Fact]
        public void DiagonalModules_StaySeparate()
        {
            var modules = new BitMatrix(5);
            modules.Set(1, 1, true);
            modules.Set(2, 2, true);

            Assert.Equal(new[]
            {
                Polygon(1, 1, 2, 1, 2, 2, 1, 2),
                Polygon(2, 2, 3, 2, 3, 3, 2, 3)
            }, OutlineBuilder.Build(modules));
        }

        [Fact]
        public void DiagonalLightModules_MergeIntoOneHole()
        {
            // A 4×4 dark block with two diagonally touching light modules: dark modules connect
            // only horizontally and vertically, so the light side connects diagonally too, and the
            // hole loop passes through the shared corner (2, 2) twice.
            var modules = new BitMatrix(4);
            modules.FillRect(0, 0, 4, 4);
            modules.Set(1, 1, false);
            modules.Set(2, 2, false);

            Assert.Equal(new[]
            {
                Polygon(0, 0, 4, 0, 4, 4, 0, 4),
                Polygon(1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 2, 2, 2, 2, 1)
            }, OutlineBuilder.Build(modules));
        }

        [Fact]
        public void DarkCentreInsideHole_IsItsOwnLoop()
        {
            // The structure of a finder pattern: a 7×7 ring, a light ring inside it, a dark 3×3
            // centre. Three loops: the outer boundary and the centre clockwise, the hole in
            // between counterclockwise.
            var modules = new BitMatrix(9);
            modules.FillRect(0, 0, 7, 7);
            for (var y = 1; y < 6; y++)
            {
                for (var x = 1; x < 6; x++)
                {
                    modules.Set(x, y, false);
                }
            }
            modules.FillRect(2, 2, 3, 3);

            Assert.Equal(new[]
            {
                Polygon(0, 0, 7, 0, 7, 7, 0, 7),
                Polygon(1, 1, 1, 6, 6, 6, 6, 1),
                Polygon(2, 2, 5, 2, 5, 5, 2, 5)
            }, OutlineBuilder.Build(modules));
        }

        [Fact]
        public void EmptyMatrix_YieldsNoPolygons()
        {
            Assert.Empty(OutlineBuilder.Build(new BitMatrix(21)));
        }

        [Fact]
        public void SourceMatrix_IsNotModified()
        {
            var modules = new BitMatrix(5);
            modules.FillRect(1, 1, 2, 3);

            OutlineBuilder.Build(modules);

            Assert.Equal(6, modules.PopCount());
        }

        [Fact]
        public void QrPolygon_CopiesItsVertices()
        {
            var vertices = new List<QrPoint> { new QrPoint(1, 2), new QrPoint(3, 2) };
            var polygon = new QrPolygon(vertices);
            vertices.Clear();

            Assert.Equal(new[] { new QrPoint(1, 2), new QrPoint(3, 2) }, polygon.Vertices);
        }

        [Fact]
        public void QrPolygon_RejectsNullVertices()
        {
            Assert.Throws<ArgumentNullException>(() => new QrPolygon(null));
        }

        [Fact]
        public void QrPolygon_Equality()
        {
            var a = Polygon(1, 2, 3, 2, 3, 4, 1, 4);
            var b = Polygon(1, 2, 3, 2, 3, 4, 1, 4);
            var c = Polygon(1, 2, 3, 2, 3, 5, 1, 5);
            var d = Polygon(1, 2, 3, 2);

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());

            Assert.False(a.Equals(c));
            Assert.False(a.Equals(d));
            Assert.True(a != c);
            Assert.False(a == c);
            Assert.False(a.Equals("not a polygon"));
            Assert.False(a == null);
            Assert.True(a != null);
            Assert.False(null == a);
        }

        [Fact]
        public void QrPolygon_ToString()
        {
            Assert.Equal("QrPolygon((1,2), (3,2), (3,4), (1,4))", Polygon(1, 2, 3, 2, 3, 4, 1, 4).ToString());
        }

        [Fact]
        public void QrPoint_Properties()
        {
            var point = new QrPoint(2, 3);
            Assert.Equal(2, point.X);
            Assert.Equal(3, point.Y);
        }

        [Fact]
        public void QrPoint_Equality()
        {
            var a = new QrPoint(1, 2);
            var b = new QrPoint(1, 2);
            var c = new QrPoint(1, 3);

            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());

            Assert.False(a.Equals(c));
            Assert.True(a != c);
            Assert.False(a == c);
            Assert.False(a.Equals("not a point"));
        }

        [Fact]
        public void QrPoint_Deconstruct()
        {
            var (x, y) = new QrPoint(2, 3);
            Assert.Equal(2, x);
            Assert.Equal(3, y);
        }

        [Fact]
        public void QrPoint_ToString()
        {
            Assert.Equal("QrPoint(X=2, Y=3)", new QrPoint(2, 3).ToString());
        }

        // Creates a polygon from x/y coordinate pairs.
        private static QrPolygon Polygon(params int[] coordinates)
        {
            var vertices = new QrPoint[coordinates.Length / 2];
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new QrPoint(coordinates[2 * i], coordinates[2 * i + 1]);
            }
            return new QrPolygon(vertices);
        }
    }
}
