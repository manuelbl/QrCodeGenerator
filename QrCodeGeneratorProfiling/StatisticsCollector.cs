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
/// Collects statistics about penalty contributions, data mask pattern selection and QR code
/// versions across the sample payloads.
/// </summary>
/// <remarks>
/// <para>
/// The penalty score, the selected mask pattern and the version are a deterministic function of
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
        var versionCounts = new long[41];

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

                // Mask and version statistics cover the generated QR code.
                maskCounts[qr.Mask] += 1;
                versionCounts[qr.Version] += 1;
            }
        }

        var buckets = new[] { blocks, sameColorCols, sameColorRows, finderRows, finderCols, colorBalance };
        PrintPenaltyTable(buckets);
        Console.WriteLine();
        PrintMaskTable(maskCounts);
        Console.WriteLine();
        PrintVersionTable(versionCounts);
    }

    private static void PrintVersionTable(long[] versionCounts)
    {
        var total = versionCounts.Sum();

        Console.WriteLine("# Version Distribution");
        Console.WriteLine();
        Console.WriteLine($"Version distribution (samples={total.ToString("N0", CultureInfo.InvariantCulture)})");
        Console.WriteLine();

        var headers = new[] { "Version", "Count", "Share%" };
        var rightAlign = new[] { true, true, true };
        var rows = Enumerable.Range(1, 40)
            .Where(v => versionCounts[v] > 0)
            .Select(v => new[]
            {
                v.ToString(CultureInfo.InvariantCulture),
                versionCounts[v].ToString("N0", CultureInfo.InvariantCulture),
                Share(versionCounts[v], total)
            })
            .ToList();

        PrintTable(headers, rightAlign, rows);

        // Grouped by BitMatrix row layout: the versions in each group use one, two or three
        // 64-bit words per row.
        Console.WriteLine();
        PrintRowLayoutShare("1-11 (one word per row)", versionCounts, 1, 11, total);
        PrintRowLayoutShare("12-27 (two words per row)", versionCounts, 12, 27, total);
        PrintRowLayoutShare("28-40 (three words per row)", versionCounts, 28, 40, total);
    }

    private static void PrintRowLayoutShare(string label, long[] versionCounts, int firstVersion, int lastVersion, long total)
    {
        var count = 0L;
        for (var v = firstVersion; v <= lastVersion; v += 1)
        {
            count += versionCounts[v];
        }
        Console.WriteLine($"- Versions {label}: {count.ToString("N0", CultureInfo.InvariantCulture)} ({Share(count, total)}%)");
    }

    private static string Share(long count, long total)
    {
        return (total > 0 ? (double)count / total * 100 : 0).ToString("F2", CultureInfo.InvariantCulture);
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
                Share(maskCounts[p], total)
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
