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
    public class PenaltyTest
    {
        // Base penalty for streaks (due to finder patterns)
        private const int BasePenaltyStreaks = 66;
        // Base penalty of 2 by 2 block (due to finder patterns)
        private const int BasePenalty2By2Blocks = 36;
        // Base penalty finder patterns
        private const int BasePenaltyFinderPattern = 360;
        
        [Theory, CombinatorialData]
        public void CalcSameColor_NoStreaks([CombinatorialValues(17, 25, 37)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);

            // streak of 4 placed such that it does not become longer with the checkerboard pattern
            for (var x = 3; x < 7; x += 1)
            {
                modules.Set(x, 3, true);
            }
            modules.Set(7, 3, false);

            if (invert)
            {
                Invert(modules);
            }

            // no streaks
            Assert.Equal(0 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory, CombinatorialData]
        public void CalcSameColor_StreakOfFive([CombinatorialValues(17, 25, 37)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);

            // place streak of 5
            for (var x = 3; x < 8; x += 1)
            {
                modules.Set(x, 3, true);
            }

            if (invert)
            {
                Invert(modules);
            }

            // 1 streak of 5
            Assert.Equal(3 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory, CombinatorialData]
        public void CalcSameColor_StreakOfFiveAtEnd([CombinatorialValues(17, 25, 37)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);

            // streak of 5 placed at the end of the row (requires odd size)
            for (var x = size - 5; x < size; x += 1)
            {
                modules.Set(x, 4, true);
            }

            if (invert)
            {
                Invert(modules);
            }

            // 1 streak of 5
            Assert.Equal(3 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory, CombinatorialData]
        public void CalcSameColor_LongStreakNotCountedMultipleTimes([CombinatorialValues(17, 25, 37)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);

            // streak of 8 placed such that it does not become longer with the checkerboard pattern
            for (var x = 3; x < 12; x += 1)
            {
                modules.Set(x, 3, true);
            }

            if (invert)
            {
                Invert(modules);
            }

            // 1 streak of 9
            Assert.Equal(7 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory]
        [InlineData(65, 60)]    // straddles word boundary at column 63/64
        [InlineData(129, 60)]   // straddles word boundary at column 63/64
        [InlineData(129, 124)]  // straddles word boundary at column 127/128
        [InlineData(177, 60)]
        [InlineData(177, 124)]
        public void CalcSameColor_StreakAcrossWordBoundary(int size, int startCol)
        {
            var modules = CreateCheckerboard(size);

            // place streak of 5 straddling a word boundary
            for (var x = startCol; x < startCol + 5; x += 1)
            {
                modules.Set(x, 3, true);
            }
            // ensure the checkerboard does not extend the streak
            modules.Set(startCol - 1, 3, false);
            if (startCol + 5 < size)
            {
                modules.Set(startCol + 5, 3, false);
            }

            Assert.Equal(3 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory]
        [InlineData(65)]
        [InlineData(129)]
        [InlineData(177)]
        public void CalcSameColor_StreakAtRowEnd_LargeSize(int size)
        {
            var modules = CreateCheckerboard(size);

            // streak of 5 placed at the end of the row (requires odd size)
            for (var x = size - 5; x < size; x += 1)
            {
                modules.Set(x, 4, true);
            }

            Assert.Equal(3 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory]
        [InlineData(65)]
        [InlineData(129)]
        [InlineData(177)]
        public void CalcSameColor_StreakAtRowStart_LargeSize(int size)
        {
            var modules = CreateCheckerboard(size);

            // streak of 5 placed at the start of the row
            for (var x = 0; x < 5; x += 1)
            {
                modules.Set(x, 4, true);
            }

            Assert.Equal(3 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory]
        [InlineData(65)]
        [InlineData(129)]
        [InlineData(177)]
        public void CalcSameColor_NoSpuriousRunInPadding(int size)
        {
            var modules = CreateCheckerboard(size);

            // 4-long zero run at the end of row 2; must not be extended into a
            // 5+ run by the padding zeros past the row's last valid column.
            for (var x = size - 4; x < size; x += 1)
            {
                modules.Set(x, 2, false);
            }

            Assert.Equal(0 - BasePenaltyStreaks, Penalty.CalcSameColor(modules));
        }

        [Theory, CombinatorialData]
        public void CalcSameColor_BasePenalty([CombinatorialValues(21, 29, 37, 65, 129, 177)]int size)
        {
            var modules = CreateCheckerboardWithFinders(size);
            
            // ensure the white area around the finder pattern does not extend into the data area
            modules.Set(7, 8, true);
            modules.Set(8, 7, true);
            modules.Set(7, size - 9, true);
            modules.Set(8, size - 8, true);
            modules.Set(size - 8, 8, true);
            modules.Set(size - 9, 7, true);
            
            Assert.Equal(0, Penalty.CalcSameColor(modules));
            
            modules.Transpose();
            Assert.Equal(0, Penalty.CalcSameColor(modules));
        }

        [Theory, CombinatorialData]
        public void Calc2By2Blocks_NoBlocks([CombinatorialValues(17, 25, 37, 65, 129, 177)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);
            if (invert)
            {
                Invert(modules);
            }

            // no blocks
            Assert.Equal(0 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));

            Transpose(modules);
            Assert.Equal(0 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));
        }

        [Theory, CombinatorialData]
        public void Calc2By2Blocks_FindBlocks([CombinatorialValues(17, 25, 37, 65, 129, 177)] int size, [CombinatorialValues(false, true)] bool invert)
        {
            var modules = CreateCheckerboard(size);

            // block 1
            modules.Set(4, 3, true);
            modules.Set(5, 3, true);
            modules.Set(4, 4, true);
            modules.Set(5, 4, true);

            // block 2
            modules.Set(6, 8, true);
            modules.Set(7, 8, true);
            modules.Set(6, 9, true);
            modules.Set(7, 9, true);

            if (invert)
            {
                Invert(modules);
            }

            // 2 blocks
            Assert.Equal(6 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));

            Transpose(modules);
            Assert.Equal(6 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));
        }

        [Theory]
        [InlineData(65)]
        [InlineData(129)]
        [InlineData(177)]
        public void Calc2By2Blocks_BlockAtWordBoundary(int size)
        {
            // Block straddling the boundary between word 0 and word 1 (columns 63 and 64).
            var modules = CreateCheckerboard(size);
            modules.Set(63, 5, true);
            modules.Set(64, 5, true);
            modules.Set(63, 6, true);
            modules.Set(64, 6, true);

            Assert.Equal(3 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));
        }

        [Theory]
        [InlineData(17)]
        [InlineData(25)]
        [InlineData(65)]
        [InlineData(129)]
        [InlineData(177)]
        public void Calc2By2Blocks_BlockAtLastValidPosition(int size)
        {
            // Block at the very last valid position (size-2, size-2).
            var modules = CreateCheckerboard(size);
            modules.Set(size - 2, size - 2, true);
            modules.Set(size - 1, size - 2, true);
            modules.Set(size - 2, size - 1, true);
            modules.Set(size - 1, size - 1, true);

            Assert.Equal(3 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));
        }

        [Theory]
        [InlineData(21, 3)]
        [InlineData(65, 61)]
        [InlineData(69, 62)]
        [InlineData(73, 63)]
        [InlineData(77, 64)]
        [InlineData(81, 65)]
        public void Calc2By2Blocks_OverlappingBlocksCountedMultipleTimes(int size, int x)
        {
            var modules = CreateCheckerboard(size);
            modules.FillRect(x, 4, 3, 3);
            Assert.Equal(12 - BasePenalty2By2Blocks, Penalty.Calc2By2Blocks(modules));
        }

        [Theory, CombinatorialData]
        public void Calc2By2Blocks_BasePenalty([CombinatorialValues(21, 29, 37, 65, 129, 177)]int size)
        {
            var modules = CreateCheckerboardWithFinders(size);
            Assert.Equal(0, Penalty.Calc2By2Blocks(modules));
            
            modules.Transpose();
            Assert.Equal(0, Penalty.Calc2By2Blocks(modules));
        }

        [Theory]
        [InlineData(17, 0.5, 0)]
        [InlineData(21, 0.545, 0)]
        [InlineData(21, 0.455, 0)]
        [InlineData(21, 0.555, 10)]
        [InlineData(21, 0.445, 10)]
        [InlineData(37, 0.595, 10)]
        [InlineData(37, 0.405, 10)]
        [InlineData(37, 0.605, 20)]
        [InlineData(37, 0.395, 20)]
        public void CalcColorBalance(int size, double percent, int expectedPenalty)
        {
            var modules = Fill(size, percent);
            Assert.Equal(expectedPenalty, Penalty.CalcColorBalance(modules));

            Invert(modules);
            Assert.Equal(expectedPenalty, Penalty.CalcColorBalance(modules));

            Transpose(modules);
            Assert.Equal(expectedPenalty, Penalty.CalcColorBalance(modules));
        }

        [Theory, CombinatorialData]
        public void CalcColorBalance_BasePenalty([CombinatorialValues(21, 29, 37, 65, 129, 177)]int size)
        {
            var modules = CreateCheckerboardWithFinders(size);
            Assert.Equal(0, Penalty.CalcColorBalance(modules));
            
            modules.Transpose();
            Assert.Equal(0, Penalty.CalcColorBalance(modules));
        }

        [Theory]
        [InlineData(25, 0.4)]
        [InlineData(37, 0.55)]
        [InlineData(65, 0.5)]
        public void Calculate_EarlyStopHonorsContract(int size, double fillPercent)
        {
            var modules = Fill(size, fillPercent);
            var scoringMatrix = ScoringMatrix.From(modules);

            // Baseline: int.MaxValue threshold disables early-stop, so the function
            // runs to completion and returns the exact penalty.
            var truePenalty = Penalty.Calculate(scoringMatrix, int.MaxValue);

            // Contract: for any threshold T, the returned value is either the exact
            // penalty (no bailout) or some partial sum >= T (bailout fired). Either
            // way, comparing the result against T with strict less-than yields the
            // same mask-selection decision as a full computation.
            for (var threshold = -10; threshold <= truePenalty + 50; threshold += 1)
            {
                var result = Penalty.Calculate(scoringMatrix, threshold);
                Assert.True(result == truePenalty || result >= threshold,
                    $"threshold={threshold}, result={result}, truePenalty={truePenalty}");
            }
        }

        [Theory, CombinatorialData]
        public void CalcFinderPattern_NoPenalty([CombinatorialValues(19, 25, 37)] int size)
        {
            var modules = CreateCheckerboard(size);
            DrawFinderPattern(modules, 3, 3, leading: 0, trailing: 3);
            DrawFinderPattern(modules, 8, 6, leading: 3, trailing: 0);

            Assert.Equal(0 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));

            Invert(modules);
            Assert.Equal(0 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));
        }

        [Theory, CombinatorialData]
        public void CalcFinderPattern_Single([CombinatorialValues(true, false)] bool isLeading)
        {
            var modules = CreateCheckerboard(25);

            DrawFinderPattern(modules, 4, 3, leading: isLeading ? 4 : 1, trailing: isLeading ? 1 : 4);

            Assert.Equal(40 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));

            Invert(modules);
            Assert.Equal(0 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));
        }

        [Theory]
        [InlineData(0, 7)]
        [InlineData(1, 7)]
        [InlineData(2, 7)]
        [InlineData(3, 7)]
        [InlineData(4, 7)]
        [InlineData(0, 8)]
        [InlineData(1, 8)]
        [InlineData(2, 8)]
        [InlineData(3, 8)]
        [InlineData(4, 8)]
        public void CalcFinderPattern_SingleAtLeftBorder(int x, int y)
        {
            var modules = CreateCheckerboard(25);

            DrawFinderPattern(modules, x, y, leading: x, trailing: 1);

            Assert.Equal(40 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));

            Invert(modules);
            Assert.Equal(0 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));
        }

        [Theory]
        [InlineData(0, 7)]
        [InlineData(1, 7)]
        [InlineData(2, 7)]
        [InlineData(3, 7)]
        [InlineData(4, 7)]
        [InlineData(0, 8)]
        [InlineData(1, 8)]
        [InlineData(2, 8)]
        [InlineData(3, 8)]
        [InlineData(4, 8)]
        public void CalcFinderPattern_SingleAtRightBorder(int x, int y)
        {
            var modules = CreateCheckerboard(25);
            var size = modules.Size;

            DrawFinderPattern(modules, size - x - 7, y, leading: 1, trailing: x);

            Assert.Equal(40 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));

            Invert(modules);
            Assert.Equal(0 - BasePenaltyFinderPattern, Penalty.CalcFinderPattern(modules));
        }

        [Theory, CombinatorialData]
        public void CalcFinderPattern_BasePenalty([CombinatorialValues(21, 29, 37, 65, 129, 177)]int size)
        {
            var modules = CreateCheckerboardWithFinders(size);
            Assert.Equal(0, Penalty.CalcFinderPattern(modules));
            
            modules.Transpose();
            Assert.Equal(0, Penalty.CalcFinderPattern(modules));
        }

        private static BitMatrix Fill(int size, double percent)
        {
            var result = new BitMatrix(size);
            var error = 0.0;
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    error += percent;
                    if (error > 0.5)
                    {
                        result.Set(x, y, true);
                        error -= 1;
                    }
                }
            }
            return result;
        }

        private static BitMatrix CreateCheckerboard(int size)
        {
            var matrix = new BitMatrix(size);
            for (var y = 0; y < size; y += 1)
            {
                for (var x = y % 2; x < size; x += 2)
                {
                    matrix.Set(x, y, true);
                }
            }
            return matrix;
        }

        private static BitMatrix CreateCheckerboardWithFinders(int size)
        {
            var matrix = CreateCheckerboard(size);

            matrix.Invert();
            matrix.FillRect(0, 0, 8, 8);
            matrix.FillRect(0, size - 8, 8, 8);
            matrix.FillRect(size - 8, 0, 8, 8);
            matrix.Invert();
            
            matrix.FillRect(0, 0, 7, 7);
            matrix.FillRect(0, size - 7, 7, 7);
            matrix.FillRect(size - 7, 0, 7, 7);

            matrix.Invert();
            matrix.FillRect(1, 1, 5, 5);
            matrix.FillRect(1, size - 6, 5, 5);
            matrix.FillRect(size - 6, 1, 5, 5);
            matrix.Invert();
            
            matrix.FillRect(2, 2, 3, 3);
            matrix.FillRect(2, size - 5, 3, 3);
            matrix.FillRect(size - 5, 2, 3, 3);

            return matrix;
        }

        private static void Invert(BitMatrix modules)
        {
            var size = modules.Size;
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    modules.Set(x, y, !modules.Get(x, y));
                }
            }
        }

        [SuppressMessage("squid", "S2234")]
        // Transpose modules (mirror diagonally)
        private static void Transpose(BitMatrix modules)
        {
            var size = modules.Size;
            for (var y = 0; y < size; y += 1)
            {
                for (var x =  y + 1; x < size; x += 1)
                {
                    var tmp = modules.Get(x, y);
                    modules.Set(x, y, modules.Get(y, x));
                    modules.Set(y, x, tmp);
                }
            }
        }

        private static void DrawFinderPattern(BitMatrix modules, int x, int y, int leading = 0, int trailing = 0)
        {
            for (var i = 0; i < leading; i += 1)
            {
                modules.Set(x - leading + i, y, false);
            }

            modules.Set(x, y, true);
            modules.Set(x + 1, y, false);
            modules.Set(x + 2, y, true);
            modules.Set(x + 3, y, true);
            modules.Set(x + 4, y, true);
            modules.Set(x + 5, y, false);
            modules.Set(x + 6, y, true);

            for (var i = 0; i < trailing; i += 1)
            {
                modules.Set(x + 7 + i, y, false);
            }

        }
    }
}
