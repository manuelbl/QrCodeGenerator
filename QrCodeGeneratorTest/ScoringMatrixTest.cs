/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test
{
    public class ScoringMatrixTest
    {
        [Theory, CombinatorialData]
        public void From_ColumnsAreTransposeOfRows([CombinatorialValues(21, 64, 65, 177)] int size)
        {
            var matrix = ScoringMatrix.From(BuildPattern(size, 0xC0FFEE));

            AssertColumnsAreTranspose(matrix);
        }

        [Theory, CombinatorialData]
        public void Xor_KeepsColumnsInSyncWithRows([CombinatorialValues(21, 64, 65, 177)] int size)
        {
            var matrix = ScoringMatrix.From(BuildPattern(size, 0xC0FFEE));
            var mask = MakeMask(size, 0xABCDEF);

            matrix.Xor(mask);

            AssertColumnsAreTranspose(matrix);
        }

        [Theory, CombinatorialData]
        public void Xor_Twice_RestoresAndStaysInSync([CombinatorialValues(21, 65, 177)] int size)
        {
            var original = BuildPattern(size, 0xC0FFEE);
            var matrix = ScoringMatrix.From(original.Copy());
            var mask = MakeMask(size, 0xABCDEF);

            matrix.Xor(mask);
            matrix.Xor(mask);

            AssertMatricesEqual(original, matrix.Rows);
            AssertColumnsAreTranspose(matrix);
        }

        [SuppressMessage("csharpsquid", "S2234")]
        [Theory, CombinatorialData]
        public void SetFormatBit_UpdatesBothViewsInSync([CombinatorialValues(21, 64, 65, 177)] int size)
        {
            var matrix = ScoringMatrix.From(new BitMatrix(size));

            // coordinates spanning word boundaries and both axes
            (int x, int y, bool value)[] bits =
            {
                (8, 0, true), (0, 8, true), (8, 8, true), (7, 8, true), (8, 7, true),
                (size - 1, 8, true), (8, size - 1, true), (size - 1, size - 1, true),
                (8, 8, false)
            };

            foreach (var (x, y, value) in bits)
            {
                matrix.SetFormatBit(x, y, value);
                Assert.Equal(value, matrix.Rows.Get(x, y));
                Assert.Equal(value, matrix.Columns.Get(y, x));
            }

            AssertColumnsAreTranspose(matrix);
        }

        [Theory, CombinatorialData]
        public void Finish_AppliesMaskToRowsOnly([CombinatorialValues(21, 65, 177)] int size)
        {
            var matrix = ScoringMatrix.From(BuildPattern(size, 0xC0FFEE));
            var mask = MakeMask(size, 0xABCDEF);

            var expectedRows = matrix.Rows.Copy();
            expectedRows.Xor(mask.Rows);
            // 'Columns' is discarded after mask selection, so Finish must leave it untouched.
            var expectedColumns = matrix.Columns.Copy();

            var result = matrix.Finish(mask);

            AssertMatricesEqual(expectedRows, result);
            AssertMatricesEqual(expectedRows, matrix.Rows);
            AssertMatricesEqual(expectedColumns, matrix.Columns);
        }

        private static MaskPair MakeMask(int size, uint seed)
        {
            var rows = BuildPattern(size, seed);
            var columns = rows.Copy();
            columns.Transpose();
            return new MaskPair(rows, columns);
        }

        [SuppressMessage("csharpsquid", "S2234")]
        private static void AssertColumnsAreTranspose(ScoringMatrix matrix)
        {
            var size = matrix.Size;
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    Assert.Equal(matrix.Rows.Get(x, y), matrix.Columns.Get(y, x));
                }
            }
        }

        private static void AssertMatricesEqual(BitMatrix expected, BitMatrix actual)
        {
            Assert.Equal(expected.Size, actual.Size);
            for (var y = 0; y < expected.Size; y += 1)
            {
                for (var x = 0; x < expected.Size; x += 1)
                {
                    Assert.Equal(expected.Get(x, y), actual.Get(x, y));
                }
            }
        }

        private static BitMatrix BuildPattern(int size, uint seed)
        {
            var matrix = new BitMatrix(size);
            var state = seed;
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    state = state * 1664525u + 1013904223u;
                    if ((state & 1u) != 0)
                    {
                        matrix.Set(x, y, true);
                    }
                }
            }
            return matrix;
        }
    }
}
