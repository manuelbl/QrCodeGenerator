/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Net.Codecrete.QrCodeGenerator.Profiling;

/// <summary>
/// BenchmarkDotNet benchmarks for <see cref="QrCode.EncodeText"/>.
/// </summary>
[MemoryDiagnoser]
public class EncodeTextBenchmarks
{
    private IReadOnlyList<string> _payloads = null!;

    [GlobalSetup]
    public void Setup()
    {
        _payloads = SampleData.Payloads;
    }

    /// <summary>
    /// Encodes every sample payload once for each error correction level.
    /// <para>
    /// The result is summed so the JIT cannot eliminate the calls as dead code.
    /// </para>
    /// </summary>
    [Benchmark]
    public int EncodeAll()
    {
        var checksum = 0;
        foreach (var payload in _payloads)
        {
            foreach (var eccLevel in Enum.GetValues<QrCode.Ecc>())
            {
                var qr = QrCode.EncodeText(payload, eccLevel);
                checksum += qr.Size;
                
            }
        }
        return checksum;
    }
}
