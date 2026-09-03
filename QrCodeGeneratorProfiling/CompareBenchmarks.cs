/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using ZXing;
using ZXing.QrCode.Internal;
using ZXingEncoder = ZXing.QrCode.Internal.Encoder;
using QRCoderGenerator = QRCoder.QRCodeGenerator;
using SkiaQrGenerator = SkiaSharp.QrCode.QRCodeGenerator;

namespace Net.Codecrete.QrCodeGenerator.Profiling;

/// <summary>
/// BenchmarkDotNet benchmarks comparing QR code generation of this library with other .NET libraries.
/// </summary>
/// <remarks>
/// <para>
/// Each benchmark encodes every sample payload once for each error correction level,
/// like <see cref="EncodeTextBenchmarks.EncodeAll"/>, and measures generation only (no rendering).
/// The libraries use their defaults, except that ZXing.Net is told to use UTF-8 so that
/// characters outside ISO-8859-1 survive.
/// </para>
/// <para>
/// Each library is wrapped in an encoder function taking the payload and the ECC level index
/// (0–3 = L/M/Q/H) and returning the version of the generated QR code. Library instances
/// (where a library has any) are created once per benchmark invocation. The benchmarks sum the
/// versions so the JIT cannot eliminate the calls as dead code; <see cref="PrintAverageVersions"/>
/// uses the same encoders to compare how compactly the libraries encode the payloads.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class CompareBenchmarks
{
    /// <summary>
    /// Encodes a payload at the given ECC level index (0–3 = L/M/Q/H) and returns the QR code version.
    /// </summary>
    public delegate int Encoder(string payload, int ecc);

    /// <summary>
    /// The compared libraries, in the order of the benchmark table.
    /// </summary>
    public static readonly (string Name, Func<Encoder> CreateEncoder)[] Libraries =
    [
        ("QrCodeGenerator", CreateQrCodeGeneratorEncoder),
        ("QRCoder", CreateQRCoderEncoder),
        ("SkiaSharp.QrCode", CreateSkiaSharpQrCodeEncoder),
        ("ZXing.Net", CreateZXingNetEncoder)
    ];

    private IReadOnlyList<string> _payloads = null!;

    static CompareBenchmarks()
    {
        // ZXing.Net looks up Shift_JIS when it considers Kanji mode; the encoding
        // is only available on .NET Core once the code pages provider is registered.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [GlobalSetup]
    public void Setup()
    {
        _payloads = SampleData.Payloads;
    }

    [Benchmark(Baseline = true)]
    public int QrCodeGenerator() => EncodeAll(CreateQrCodeGeneratorEncoder());

    [Benchmark]
    public int QRCoder() => EncodeAll(CreateQRCoderEncoder());

    [Benchmark]
    public int SkiaSharpQrCode() => EncodeAll(CreateSkiaSharpQrCodeEncoder());

    [Benchmark]
    public int ZXingNet() => EncodeAll(CreateZXingNetEncoder());

    private int EncodeAll(Encoder encode)
    {
        var checksum = 0;
        foreach (var payload in _payloads)
        {
            for (var ecc = 0; ecc < 4; ecc++)
            {
                checksum += encode(payload, ecc);
            }
        }
        return checksum;
    }

    private static readonly QrCode.Ecc[] OwnEccLevels =
    [
        QrCode.Ecc.Low, QrCode.Ecc.Medium, QrCode.Ecc.Quartile, QrCode.Ecc.High
    ];

    private static Encoder CreateQrCodeGeneratorEncoder()
    {
        return (payload, ecc) => QrCode.EncodeText(payload, OwnEccLevels[ecc]).Version;
    }

    private static readonly QRCoderGenerator.ECCLevel[] QRCoderEccLevels =
    [
        QRCoderGenerator.ECCLevel.L, QRCoderGenerator.ECCLevel.M, QRCoderGenerator.ECCLevel.Q, QRCoderGenerator.ECCLevel.H
    ];

    private static Encoder CreateQRCoderEncoder()
    {
        var generator = new QRCoderGenerator();
        return (payload, ecc) =>
        {
            using var qr = generator.CreateQrCode(payload, QRCoderEccLevels[ecc]);
            return qr.Version;
        };
    }

    private static readonly SkiaSharp.QrCode.ECCLevel[] SkiaEccLevels =
    [
        SkiaSharp.QrCode.ECCLevel.L, SkiaSharp.QrCode.ECCLevel.M, SkiaSharp.QrCode.ECCLevel.Q, SkiaSharp.QrCode.ECCLevel.H
    ];

    private static Encoder CreateSkiaSharpQrCodeEncoder()
    {
        return (payload, ecc) => SkiaQrGenerator.CreateQrCode(payload, SkiaEccLevels[ecc]).Version;
    }

    private static readonly ErrorCorrectionLevel[] ZXingEccLevels =
    [
        ErrorCorrectionLevel.L, ErrorCorrectionLevel.M, ErrorCorrectionLevel.Q, ErrorCorrectionLevel.H
    ];

    private static Encoder CreateZXingNetEncoder()
    {
        var hints = new Dictionary<EncodeHintType, object> { [EncodeHintType.CHARACTER_SET] = "UTF-8" };
        return (payload, ecc) => ZXingEncoder.encode(payload, ZXingEccLevels[ecc], hints).Version.VersionNumber;
    }

    /// <summary>
    /// Encodes all sample payloads at all ECC levels with each library and prints the
    /// average QR code version per library as a Markdown table.
    /// </summary>
    /// <remarks>
    /// The version is compared rather than the size because some libraries include the
    /// quiet zone in the reported size.
    /// </remarks>
    public static void PrintAverageVersions()
    {
        var payloads = SampleData.Payloads;
        var count = payloads.Count * 4;

        Console.WriteLine();
        Console.WriteLine($"Average QR code version (samples={count:N0})");
        Console.WriteLine();
        Console.WriteLine("| Library          | Avg. version |");
        Console.WriteLine("|----------------- |-------------:|");

        foreach (var (name, createEncoder) in Libraries)
        {
            var encode = createEncoder();
            var sum = 0L;
            foreach (var payload in payloads)
            {
                for (var ecc = 0; ecc < 4; ecc++)
                {
                    sum += encode(payload, ecc);
                }
            }
            Console.WriteLine($"| {name,-16} | {(double)sum / count,12:F2} |");
        }
    }
}
