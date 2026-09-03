# Baseline

MacBook M5 Pro

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:41.0574210 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 201.7 ms | 0.23 ms | 0.19 ms | 333.3333 |   3.85 MB |


# Introduction of BitMatrix

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:42.6702940 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 205.0 ms | 0.32 ms | 0.28 ms | 333.3333 |   3.63 MB |


# Optimized bit count (color balance)

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:30.7018893 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 181.9 ms | 0.32 ms | 0.28 ms | 333.3333 |   3.63 MB |



# Optimized horizontal finder pattern

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:20.0699378 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 160.5 ms | 0.80 ms | 0.75 ms | 250.0000 |   3.62 MB |


# Use BitMatrix operations for patterns

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:01.5014088 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0      | Allocated |
|---------- |---------:|--------:|--------:|----------:|----------:|
| EncodeAll | 124.7 ms | 0.14 ms | 0.11 ms | 2250.0000 |  19.88 MB |



# Pattern caching

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:00.5289081 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 119.9 ms | 0.10 ms | 0.10 ms | 200.0000 |   3.08 MB |



# Penalty Calculation with Transposed Matrix

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:51.6643934 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error   | StdDev  | Gen0     | Allocated |
|---------- |---------:|--------:|--------:|---------:|----------:|
| EncodeAll | 103.2 ms | 0.08 ms | 0.07 ms | 600.0000 |    5.2 MB |


# Improved 2x2 block penalty

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:33.9272824 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 67.76 ms | 0.577 ms | 0.512 ms | 625.0000 |    5.2 MB |


# Improved Calc Strides of Same Color

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:12.0364860 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 23.53 ms | 0.142 ms | 0.126 ms | 625.0000 |    5.2 MB |


# Evaluate penalty for likely patterns first

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:08.9226285 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.14.0, macOS 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.5 (10.0.526.15411), Arm64 RyuJIT AdvSIMD
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 17.08 ms | 0.082 ms | 0.072 ms | 625.0000 |    5.2 MB |


# Data Segment Compaction

Use a fixed array for the blocks and merge them in-place in order
to reduce the multiple memory allocations required for a dynamically
growing list.


## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:08.1933755 (checksum=14696000)
```

## Benchmark

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.4.1 (25E253) [Darwin 25.4.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
[Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 15.98 ms | 0.131 ms | 0.109 ms | 593.7500 |   4.77 MB |




# Reed-Solomon Product Table

Cache the generator polynomial multiplied by every element of the field instead of the polynomial
alone. The division is then a shift and an exclusive or per data codeword, eight coefficients at a
time, with no field arithmetic left in the loop. The codewords are written straight into the
interleaved result, at a stride, so no block needs a buffer of its own.

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:07.4715698 (checksum=14696000)   [before: 00:00:08.1384139]
```

## Benchmark

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
[Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method             | Mean     | Error    | StdDev   | Gen0     | Allocated |
|------------------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll          | 14.65 ms | 0.044 ms | 0.039 ms | 546.8750 |    4.4 MB |

The 0.02 MB that remain are the packed `ulong[]` holding the remainder, which is a little larger
than the `byte[]` it replaces.


# Row Layouts

Two changes to the penalty rules, which is where encoding spends most of its time.

The finder-pattern rule no longer slides a 15-bit window one column at a time. A whole word is
matched at once: shifting the row lines up the module at each fixed offset from a candidate start,
so one sequence of shifts, ands and a population count finds every match beginning in that word.

`BitMatrix` then gained three row layouts. A row holds its modules in one, two or three 64-bit
words instead of always four, so a rule scans one word per row for versions 1 to 11, two for
versions 12 to 27 and three for versions 28 to 40. Every row-scanning rule has an implementation
per layout with its loop over the words unrolled. The stride between rows stays a power of two —
1, 2 or 4 — so a row index is still a shift, and the three-word layout keeps a fourth, always-zero
padding word that lets whole-matrix operations run flat over the raw array.

Versions 1 to 11 are most QR codes, and the sample data is no exception. Of the 4.1 s saved on the
profile loop, the word-parallel finder rule accounts for 1.7 s and the row layouts for 2.5 s.

## Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:00:03.4998049 (checksum=14696000)   [before: 00:00:07.6418913]
```

## Benchmark

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
[Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method             | Mean     | Error     | StdDev    | Gen0     | Allocated |
|------------------- |---------:|----------:|----------:|---------:|----------:|
| EncodeAll          | 6.428 ms | 0.0225 ms | 0.0199 ms | 382.8125 |   3.07 MB |

The matrices of the smaller versions are a quarter of their former size, which is where the drop
in allocation comes from.


# Larger Sample Data

The sample data covered versions 1 to 13 only, so the two- and three-word row layouts of
`BitMatrix` were barely measured. It now holds 400 payloads instead of 200, and one in five of
them is a long text of 400 to 900 characters assembled from the same fragments (sentences, names,
towns, URLs, numbers, messages). Long payloads reach versions 12 to 36, the short ones stay in
versions 1 to 11.

The measurements below are the new baseline; they are not comparable to the sections above, which
all used the 200 short payloads.

## Profiling

### MacBook Pro M5

```
Profile loop: 500 iterations × 400 payloads × 4 ECC levels
Total EncodeText calls: 800'000
Elapsed: 00:00:15.9592118 (checksum=41052000)
```

### Dell Core Ultra 5

```
Profile loop: 500 iterations × 400 payloads × 4 ECC levels
Total EncodeText calls: 800’000
Elapsed: 00:00:18.1419330 (checksum=41052000)
```

## Benchmark

### MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
[Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method             | Mean     | Error     | StdDev    | Gen0      | Allocated |
|------------------- |---------:|----------:|----------:|----------:|----------:|
| EncodeAll          | 31.17 ms | 0.052 ms  | 0.043 ms  | 1187.5000 |   9.57 MB |

Twice the payloads, and the long ones cost far more than a short one: the penalty rules scan a
matrix that grows with the square of the version, and eight mask patterns are scored per code.

### Dell Core Ultra 5

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
Intel Core Ultra 5 235T 2.20GHz, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 35.19 ms | 0.103 ms | 0.092 ms | 800.0000 |   9.57 MB |


# Penalty Contribution

Penalty contribution statistics (samples=12,800)

| Bucket        | Min |  Max |    Mean |  StdDev | Share% |
|---------------|----:|-----:|--------:|--------:|-------:|
| 2x2Blocks     |  42 | 9996 | 1238.85 | 1778.31 |  54.55 |
| SameColorCols |   6 | 3800 |  451.46 |  652.02 |  19.88 |
| SameColorRows |   4 | 3470 |  430.62 |  625.21 |  18.96 |
| FinderRows    |   0 |  840 |   77.55 |  107.26 |   3.41 |
| FinderCols    |   0 |  800 |   72.62 |  102.40 |   3.20 |
| ColorBalance  |   0 |   10 |    0.02 |    0.47 |   0.00 |

# Mask Pattern Selection

Mask pattern selection (samples=1,600)

| Pattern | Count | Share% |
|--------:|------:|-------:|
|       2 |   724 |  45.25 |
|       4 |   175 |  10.94 |
|       6 |   164 |  10.25 |
|       3 |   162 |  10.12 |
|       7 |   145 |   9.06 |
|       5 |   103 |   6.44 |
|       1 |    77 |   4.81 |
|       0 |    50 |   3.12 |

# Version Distribution

Version distribution (samples=1,600)

| Version | Count | Share% |
|--------:|------:|-------:|
|       1 |    84 |   5.25 |
|       2 |   105 |   6.56 |
|       3 |   175 |  10.94 |
|       4 |   191 |  11.94 |
|       5 |   228 |  14.25 |
|       6 |   165 |  10.31 |
|       7 |    83 |   5.19 |
|       8 |   136 |   8.50 |
|       9 |    67 |   4.19 |
|      10 |    36 |   2.25 |
|      11 |    14 |   0.88 |
|      12 |     7 |   0.44 |
|      13 |     2 |   0.12 |
|      14 |     5 |   0.31 |
|      15 |    11 |   0.69 |
|      16 |    10 |   0.62 |
|      17 |    19 |   1.19 |
|      18 |    15 |   0.94 |
|      19 |    25 |   1.56 |
|      20 |    21 |   1.31 |
|      21 |    21 |   1.31 |
|      22 |    24 |   1.50 |
|      23 |    23 |   1.44 |
|      24 |    24 |   1.50 |
|      25 |    11 |   0.69 |
|      26 |    15 |   0.94 |
|      27 |    15 |   0.94 |
|      28 |    12 |   0.75 |
|      29 |    12 |   0.75 |
|      30 |    12 |   0.75 |
|      31 |     7 |   0.44 |
|      32 |     9 |   0.56 |
|      33 |     7 |   0.44 |
|      34 |     6 |   0.38 |
|      35 |     2 |   0.12 |
|      36 |     1 |   0.06 |

- Versions 1-11 (one word per row): 1,284 (80.25%)
- Versions 12-27 (two words per row): 248 (15.50%)
- Versions 28-40 (three words per row): 68 (4.25%)


# Comparison with other libraries

`compare` mode: QR code generation only (no rendering), 400 payloads × 4 ECC levels per invocation,
library defaults except that ZXing.Net is told to use UTF-8.

## Performance 

### MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method          | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0       | Gen1      | Allocated    | Alloc Ratio |
|---------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|-------------:|------------:|
| QrCodeGenerator |    31.92 ms | 0.029 ms | 0.023 ms |  1.00 |    0.00 |  1187.5000 |         - |  10052.68 KB |        1.00 |
| QRCoder         | 1,719.10 ms | 1.779 ms | 1.577 ms | 53.85 |    0.06 |  1000.0000 |         - |  15181.69 KB |        1.51 |
| SkiaSharpQrCode |    22.53 ms | 0.032 ms | 0.030 ms |  0.71 |    0.00 |    93.7500 |         - |     815.7 KB |        0.08 |
| ZXingNet        | 1,076.54 ms | 2.910 ms | 2.722 ms | 33.72 |    0.09 | 54000.0000 | 1000.0000 | 441372.72 KB |       43.91 |

### Dell Core Ultra 5

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
Intel Core Ultra 5 235T 2.20GHz, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
```

| Method          | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0       | Allocated    | Alloc Ratio |
|---------------- |------------:|---------:|---------:|------:|--------:|-----------:|-------------:|------------:|
| QrCodeGenerator |    40.95 ms | 0.434 ms | 0.406 ms |  1.00 |    0.01 |   769.2308 |  10052.68 KB |        1.00 |
| QRCoder         | 2,174.83 ms | 7.653 ms | 7.158 ms | 53.11 |    0.54 |  1000.0000 |  15181.69 KB |        1.51 |
| SkiaSharpQrCode |    20.44 ms | 0.027 ms | 0.025 ms |  0.50 |    0.00 |    62.5000 |    816.26 KB |        0.08 |
| ZXingNet        | 1,291.49 ms | 5.552 ms | 4.922 ms | 31.54 |    0.32 | 36000.0000 | 441372.72 KB |       43.91 |



## Versions

The average version shows how compactly each library encodes the payloads. The version is
compared instead of the size because QRCoder and SkiaSharp.QrCode include the quiet zone in
the reported size.

Average QR code version (samples=1'600)

| Library          | Avg. version |
|----------------- |-------------:|
| QrCodeGenerator  |         8.58 |
| QRCoder          |         8.57 |
| SkiaSharp.QrCode |         8.63 |
| ZXing.Net        |         8.67 |
