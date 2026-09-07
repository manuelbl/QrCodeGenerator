# Profiling

`QrCodeGeneratorProfiling` measures the performance of the QR code generator. It is a .NET 10
console application that combines BenchmarkDotNet benchmarks, a plain encoding loop for use with an
external profiler, and a statistics collector. A single executable serves all of them; the first
argument selects the mode.

## Usage

| Command | Purpose |
| ------- | ------- |
| `dotnet run -c Release -- benchmark` | Measure `QrCode.EncodeText` over the sample data with BenchmarkDotNet. |
| `dotnet run -c Release -- compare` | Compare generation speed and QR code size with other .NET libraries. |
| `dotnet run -c Release -- profile [N]` | Run a plain encoding loop, `N` iterations (default 500). |
| `dotnet run -c Release -- stats` | Collect penalty, mask pattern and version statistics as Markdown tables. |
| `dotnet run -c Release -- help` | Print the same list. |

Run the commands from this directory, and always in the `Release` configuration. A `Debug` build
measures unoptimized code, and BenchmarkDotNet refuses to run in it.

`profile` is the mode to attach a profiler to, such as JetBrains dotTrace or dotMemory. It has no
measurement harness of its own — just a warm-up pass and a timed loop — so the profiler sees the
encoding work and little else. The loop prints a checksum, both to keep the compiler from eliminating
it and to detect when a change alters the generated QR codes.

`stats` produces the tables in the last three sections of this file, in the same order. To refresh
them, run it and replace the tables with its output.

`benchmark` and `compare` write their reports (Markdown, CSV and HTML) to
`BenchmarkDotNet.Artifacts/results/`. That directory is not checked in.

All modes use the same sample data: 400 deterministic payloads assembled from sentences, names,
towns, URLs, numbers and messages, each encoded at all four error correction levels. One payload in
five is a long text of 400 to 900 characters, so the larger QR code versions are covered as well.

[LOG.md](LOG.md) records the benchmark results of every optimization step of the library.

## Comparison with Other Libraries

The `compare` mode measures QR code generation only, with no rendering: 400 payloads × 4 error
correction levels per invocation, each library with its default settings except ZXing.Net.

### Differences Between the Libraries

- *QrCodeGenerator* (this library) always compacts the data segments and generates the smallest
  possible QR codes.
- [*ZXing.Net*](https://github.com/micjahn/zxing.net) runs without segment compaction, so several of
  its QR codes are bigger than they need to be. Compaction can be enabled, but ZXing.Net's
  implementation is expensive. The library is also told to use UTF-8; otherwise it uses
  ISO-8859-1 and replaces characters it cannot represent with `?`. The other libraries prefer
  ISO-8859-1 but fall back to UTF-8 when needed.
- A pre-release version of [*FeatherQR*](https://github.com/guitarrapc/FeatherQR) (the successor of *SkiaSharp.QrCode*, with
  the rendering split off into a separate *FeatherQR.SkiaSharp* package) does not compact data
  segments by default and produces bigger QR codes than needed. Compaction can be enabled per call.
- [*QRCoder*](https://github.com/Shane32/QRCoder) compacts data segments but does not produce the
  smallest QR code in every case. It comes close though.

The libraries also differ in when they insert an ECI segment to declare the character set.

### Speed and Memory on a MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method          | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0       | Gen1      | Allocated    | Alloc Ratio |
|---------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|-------------:|------------:|
| QrCodeGenerator |    19.77 ms | 0.175 ms | 0.155 ms |  1.00 |    0.01 |   843.7500 |         - |   7082.33 KB |        1.00 |
| QRCoder         | 1,809.08 ms | 1.076 ms | 0.840 ms | 91.50 |    0.69 |  1000.0000 |         - |   15708.1 KB |        2.22 |
| FeatherQr       |    21.06 ms | 0.049 ms | 0.043 ms |  1.06 |    0.01 |    93.7500 |         - |    865.32 KB |        0.12 |
| ZXingNet        | 1,195.85 ms | 0.756 ms | 0.631 ms | 60.48 |    0.46 | 58000.0000 | 1000.0000 | 476209.76 KB |       67.24 |


### Speed and Memory on a Dell Core Ultra 5

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
Intel Core Ultra 5 235T 2.20GHz, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
```

| Method          | Mean        | Error    | StdDev   | Ratio  | RatioSD | Gen0       | Allocated    | Alloc Ratio |
|---------------- |------------:|---------:|---------:|-------:|--------:|-----------:|-------------:|------------:|
| QrCodeGenerator |    18.85 ms | 0.050 ms | 0.042 ms |   1.00 |    0.00 |   562.5000 |   7082.33 KB |        1.00 |
| QRCoder         | 2,357.46 ms | 2.547 ms | 2.382 ms | 125.04 |    0.29 |  1000.0000 |   15708.1 KB |        2.22 |
| FeatherQr       |    14.20 ms | 0.020 ms | 0.017 ms |   0.75 |    0.00 |    62.5000 |    865.32 KB |        0.12 |
| ZXingNet        | 1,339.92 ms | 4.576 ms | 4.056 ms |  71.07 |    0.26 | 38000.0000 | 476209.76 KB |       67.24 |


### QR Code Size

Average QR code version (samples=1'600)

| Library          | Avg. version |
|----------------- |-------------:|
| QrCodeGenerator  |         8.93 |
| QRCoder          |         8.98 |
| FeatherQR        |         9.12 |
| ZXing.Net        |         9.16 |

The version says how compactly a library encodes the payload: for the same text and the same error
correction level, a lower version is a physically smaller QR code. The average is taken over all 400
payloads at all four error correction levels. The version is compared instead of the module size
because some libraries include the quiet zone in the size they report.

The spread is small, from 8.93 to 9.16, because all four libraries compact data segments to some
degree. Segment compaction is a tiebreaker between these libraries, not a decisive difference.

## Penalty Contribution

The `stats` mode reports how much each penalty rule contributes to the total penalty score. The
buckets are the rules of the standard; `Share%` is the share of the total score. `Penalty.Calculate`
evaluates the rules in exactly this order, from the largest share to the smallest, so its early-stop
path reaches the cut-off as soon as possible.

Penalty contribution statistics (samples=12,800)

| Bucket        | Min |  Max |    Mean |  StdDev | Share% |
|---------------|----:|-----:|--------:|--------:|-------:|
| 2x2Blocks     |  36 | 9237 | 1306.61 | 1824.02 |  54.45 |
| SameColorCols |  10 | 3687 |  476.31 |  672.00 |  19.85 |
| SameColorRows |   8 | 3223 |  457.02 |  644.43 |  19.05 |
| FinderRows    |   0 |  800 |   81.70 |  110.78 |   3.40 |
| FinderCols    |   0 |  760 |   78.03 |  106.70 |   3.25 |
| ColorBalance  |   0 |   10 |    0.03 |    0.51 |   0.00 |

## Mask Pattern Selection

How often each of the eight data mask patterns wins, that is, scores the lowest penalty. Pattern 2
wins in more than 40% of all cases. `MatrixEncoder.PatternEvaluationOrder` evaluates the patterns in
this order (2, 4, 6, 3, 7, 5, 1, 0), so a low penalty score is usually found early and the
early-stop path can discard the remaining patterns sooner.

Mask pattern selection (samples=1,600)

| Pattern | Count | Share% |
|--------:|------:|-------:|
|       2 |   667 |  41.69 |
|       4 |   175 |  10.94 |
|       6 |   174 |  10.88 |
|       3 |   166 |  10.38 |
|       7 |   163 |  10.19 |
|       1 |    91 |   5.69 |
|       5 |    87 |   5.44 |
|       0 |    77 |   4.81 |

## Version Distribution

The versions of the QR codes generated from the sample data. The grouping at the end matters for
performance: it is the share of each `BitMatrix` row layout, and the narrower the row, the fewer
words the penalty rules scan per row. Most QR codes fit into a single word per row.

Version distribution (samples=1,600)

| Version | Count | Share% |
|--------:|------:|-------:|
|       1 |    50 |   3.12 |
|       2 |    89 |   5.56 |
|       3 |   189 |  11.81 |
|       4 |   250 |  15.62 |
|       5 |   205 |  12.81 |
|       6 |   174 |  10.88 |
|       7 |    77 |   4.81 |
|       8 |   109 |   6.81 |
|       9 |    55 |   3.44 |
|      10 |    38 |   2.38 |
|      11 |    14 |   0.88 |
|      12 |    11 |   0.69 |
|      13 |     4 |   0.25 |
|      14 |     1 |   0.06 |
|      15 |     7 |   0.44 |
|      16 |    12 |   0.75 |
|      17 |    22 |   1.38 |
|      18 |    21 |   1.31 |
|      19 |    28 |   1.75 |
|      20 |    28 |   1.75 |
|      21 |    18 |   1.12 |
|      22 |    22 |   1.38 |
|      23 |    25 |   1.56 |
|      24 |    28 |   1.75 |
|      25 |    15 |   0.94 |
|      26 |    13 |   0.81 |
|      27 |    19 |   1.19 |
|      28 |    21 |   1.31 |
|      29 |     9 |   0.56 |
|      30 |    15 |   0.94 |
|      31 |     8 |   0.50 |
|      32 |     8 |   0.50 |
|      33 |    10 |   0.62 |
|      34 |     5 |   0.31 |

- Versions 1-11 (one word per row): 1,250 (78.12%)
- Versions 12-27 (two words per row): 274 (17.12%)
- Versions 28-40 (three words per row): 76 (4.75%)
