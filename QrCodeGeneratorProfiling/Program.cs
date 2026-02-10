/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace Net.Codecrete.QrCodeGenerator.Profiling;

/// <summary>
/// Entry point for the QrCodeGenerator profiling harness.
/// </summary>
/// <remarks>
/// <para>
/// The same executable serves two scenarios:
/// </para>
/// <list type="bullet">
///   <item><c>benchmark</c> — runs BenchmarkDotNet for statistically sound measurements.</item>
///   <item><c>profile [iterations]</c> — runs a plain loop suitable for attaching
///   JetBrains Rider's dotTrace or dotMemory to identify hotspots in <see cref="QrCode.EncodeText"/>.</item>
/// </list>
/// </remarks>
public static class Program
{
    private const int DefaultProfileIterations = 500;

    public static int Main(string[] args)
    {
        var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "help";

        switch (mode)
        {
            case "benchmark":
                BenchmarkRunner.Run<EncodeTextBenchmarks>();
                return 0;

            case "profile":
                var iterations = DefaultProfileIterations;
                if (args.Length > 1 && !int.TryParse(args[1], out iterations))
                {
                    Console.Error.WriteLine($"Invalid iteration count '{args[1]}'.");
                    return 1;
                }
                RunProfileLoop(iterations);
                return 0;

            default:
                PrintUsage();
                return mode == "help" ? 0 : 1;
        }
    }

    private static void RunProfileLoop(int iterations)
    {
        var payloads = SampleData.Payloads;
        var eccLevels = new[] { QrCode.Ecc.Low, QrCode.Ecc.Medium, QrCode.Ecc.Quartile, QrCode.Ecc.High };

        Console.WriteLine($"Profile loop: {iterations} iterations × {payloads.Count} payloads × {eccLevels.Length} ECC levels");
        Console.WriteLine($"Total EncodeText calls: {(long)iterations * payloads.Count * eccLevels.Length:N0}");

        // Warm-up: ensures JIT tiering settles before the timed section so attached
        // profilers see steady-state code paths.
        Warmup(payloads, eccLevels);

        var stopwatch = Stopwatch.StartNew();
        var checksum = 0L;

        for (var iter = 0; iter < iterations; iter++)
        {
            foreach (var payload in payloads)
            {
                foreach (var eccLevel in eccLevels)
                {
                    var qr = QrCode.EncodeText(payload, eccLevel);
                    checksum += qr.Size;
                }
            }
        }

        stopwatch.Stop();

        // Print checksum so the compiler cannot elide the loop and so runs can be compared.
        Console.WriteLine($"Elapsed: {stopwatch.Elapsed} (checksum={checksum})");
    }

    private static void Warmup(System.Collections.Generic.IReadOnlyList<string> payloads, QrCode.Ecc[] eccLevels)
    {
        foreach (var payload in payloads)
        {
            foreach (var eccLevel in eccLevels)
            {
                _ = QrCode.EncodeText(payload, eccLevel);
            }
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("QrCodeGenerator profiling harness");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -c Release -- benchmark           Run BenchmarkDotNet.");
        Console.WriteLine("  dotnet run -c Release -- profile [N]         Run a plain loop (N iterations, default {0}) suitable for Rider profiling.", DefaultProfileIterations);
        Console.WriteLine();
        Console.WriteLine("Tip: attach Rider's dotTrace / dotMemory to the 'profile' process to inspect QrCode.EncodeText hotspots.");
    }
}
