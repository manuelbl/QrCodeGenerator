/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Net.Codecrete.QrCodeGenerator.Profiling;

/// <summary>
/// Collects statistics about penalty contributions and data mask pattern selection
/// across the sample payloads.
/// </summary>
/// <remarks>
/// <para>
/// The penalty score and the selected mask pattern are a deterministic function of
/// (payload, ECC level), so a single pass over the sample data is sufficient — repeating
/// it would only duplicate identical samples without adding information.
/// </para>
/// <para>
/// Passing an <see cref="EncodingInfo"/> forces the library to fully evaluate the penalty
/// score for all eight mask patterns (disabling the early-stop optimisation), which is what
/// makes the per-bucket breakdown available.
/// </para>
/// </remarks>
internal static class StatisticsCollector
{
    public static void Run()
    {
        var payloads = SampleData.Payloads;
        var eccLevels = new[] { QrCode.Ecc.Low, QrCode.Ecc.Medium, QrCode.Ecc.Quartile, QrCode.Ecc.High };

        // Penalty buckets, named to match the rules in Penalty.CalculateFully.
        var blocks = new Bucket("2x2Blocks");
        var sameColorCols = new Bucket("SameColorCols");
        var sameColorRows = new Bucket("SameColorRows");
        var finderRows = new Bucket("FinderRows");
        var finderCols = new Bucket("FinderCols");
        var colorBalance = new Bucket("ColorBalance");

        var maskCounts = new long[8];

        foreach (var payload in payloads)
        {
            foreach (var eccLevel in eccLevels)
            {
                var info = new EncodingInfo();
                var qr = QrCode.EncodeTextAdvanced(payload, eccLevel, encodingInfo: info);

                // Penalty statistics cover all eight candidate mask patterns.
                foreach (var penalty in info.Penalties)
                {
                    blocks.Add(penalty.Blocks);
                    sameColorCols.Add(penalty.VerticalStreaks);
                    sameColorRows.Add(penalty.HorizontalStreaks);
                    finderRows.Add(penalty.HorizontalFinderPatterns);
                    finderCols.Add(penalty.VerticalFinderPatterns);
                    colorBalance.Add(penalty.ColorBalance);
                }

                // Mask statistics cover the actually selected pattern.
                maskCounts[qr.Mask] += 1;
            }
        }

        var buckets = new[] { blocks, sameColorCols, sameColorRows, finderRows, finderCols, colorBalance };
        PrintPenaltyTable(buckets);
        Console.WriteLine();
        PrintMaskTable(maskCounts);
    }

    private static void PrintPenaltyTable(IReadOnlyList<Bucket> buckets)
    {
        var sampleCount = buckets[0].Count;
        var totalMean = buckets.Sum(b => b.Mean);

        Console.WriteLine("# Penalty Contribution");
        Console.WriteLine();
        Console.WriteLine($"Penalty contribution statistics (samples={sampleCount.ToString("N0", CultureInfo.InvariantCulture)})");
        Console.WriteLine();

        var headers = new[] { "Bucket", "Min", "Max", "Mean", "StdDev", "Share%" };
        var rightAlign = new[] { false, true, true, true, true, true };
        var rows = buckets
            .OrderByDescending(b => b.Mean)
            .Select(b => new[]
            {
                b.Name,
                b.Min.ToString(CultureInfo.InvariantCulture),
                b.Max.ToString(CultureInfo.InvariantCulture),
                b.Mean.ToString("F2", CultureInfo.InvariantCulture),
                b.StdDev.ToString("F2", CultureInfo.InvariantCulture),
                (totalMean > 0 ? b.Mean / totalMean * 100 : 0).ToString("F2", CultureInfo.InvariantCulture)
            })
            .ToList();

        PrintTable(headers, rightAlign, rows);
    }

    private static void PrintMaskTable(long[] maskCounts)
    {
        var total = maskCounts.Sum();

        Console.WriteLine("# Mask Pattern Selection");
        Console.WriteLine();
        Console.WriteLine($"Mask pattern selection (samples={total.ToString("N0", CultureInfo.InvariantCulture)})");
        Console.WriteLine();

        var headers = new[] { "Pattern", "Count", "Share%" };
        var rightAlign = new[] { true, true, true };
        var rows = Enumerable.Range(0, maskCounts.Length)
            .OrderByDescending(p => maskCounts[p])
            .Select(p => new[]
            {
                p.ToString(CultureInfo.InvariantCulture),
                maskCounts[p].ToString("N0", CultureInfo.InvariantCulture),
                (total > 0 ? (double)maskCounts[p] / total * 100 : 0).ToString("F2", CultureInfo.InvariantCulture)
            })
            .ToList();

        PrintTable(headers, rightAlign, rows);
    }

    private static void PrintTable(string[] headers, bool[] rightAlign, IReadOnlyList<string[]> rows)
    {
        var widths = new int[headers.Length];
        for (var c = 0; c < headers.Length; c += 1)
        {
            widths[c] = headers[c].Length;
            foreach (var row in rows)
            {
                widths[c] = Math.Max(widths[c], row[c].Length);
            }
        }

        Console.WriteLine(BuildRow(headers, widths, rightAlign));
        Console.WriteLine(BuildSeparator(widths, rightAlign));
        foreach (var row in rows)
        {
            Console.WriteLine(BuildRow(row, widths, rightAlign));
        }
    }

    private static string BuildRow(string[] cells, int[] widths, bool[] rightAlign)
    {
        var builder = new StringBuilder("|");
        for (var c = 0; c < cells.Length; c += 1)
        {
            var cell = rightAlign[c]
                ? cells[c].PadLeft(widths[c])
                : cells[c].PadRight(widths[c]);
            builder.Append(' ').Append(cell).Append(" |");
        }
        return builder.ToString();
    }

    private static string BuildSeparator(int[] widths, bool[] rightAlign)
    {
        // Each separator cell spans the column width plus the two padding spaces used
        // by BuildRow, with a trailing colon marking right-aligned columns.
        var builder = new StringBuilder("|");
        for (var c = 0; c < widths.Length; c += 1)
        {
            if (rightAlign[c])
            {
                builder.Append(new string('-', widths[c] + 1)).Append(':');
            }
            else
            {
                builder.Append(new string('-', widths[c] + 2));
            }
            builder.Append('|');
        }
        return builder.ToString();
    }

    /// <summary>
    /// Accumulates count, min, max, mean, and population standard deviation for one penalty bucket.
    /// </summary>
    private sealed class Bucket
    {
        private long _sum;
        private double _sumSquares;

        public Bucket(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public long Count { get; private set; }

        public int Min { get; private set; } = int.MaxValue;

        public int Max { get; private set; } = int.MinValue;

        public double Mean => Count > 0 ? (double)_sum / Count : 0;

        public double StdDev
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }
                var mean = Mean;
                var variance = _sumSquares / Count - mean * mean;
                return Math.Sqrt(Math.Max(0, variance));
            }
        }

        public void Add(int value)
        {
            Count += 1;
            _sum += value;
            _sumSquares += (double)value * value;
            if (value < Min)
            {
                Min = value;
            }
            if (value > Max)
            {
                Max = value;
            }
        }
    }
}
