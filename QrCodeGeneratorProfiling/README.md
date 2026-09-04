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

## MacBook Pro M5

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

## Dell Core Ultra 5

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


# Optimal Segment Compaction

The segment compaction now assigns the segment modes by a dynamic programme over the blocks
instead of two greedy merge passes, and the compaction runs per version group (1–9, 10–26, 27–40)
instead of once for the maximum version. The result is the shortest possible bit stream for the
chosen version; the `compaction` mode reports no case where QRCoder's segments are shorter.
The checksum differs from the sections above because some QR codes got smaller.

## MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method    | Mean     | Error    | StdDev   | Gen0      | Allocated |
|---------- |---------:|---------:|---------:|----------:|----------:|
| EncodeAll | 32.17 ms | 0.051 ms | 0.040 ms | 1312.5000 |  10.68 MB |


# Precomputed payload filling

The target for each payload bit is computed once and cached.

## MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method    | Mean     | Error    | StdDev   | Gen0      | Allocated |
|---------- |---------:|---------:|---------:|----------:|----------:|
| EncodeAll | 20.79 ms | 0.037 ms | 0.031 ms | 1406.2500 |  11.25 MB |


# Modified pattern evaluation order

## MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method    | Mean     | Error    | StdDev   | Gen0      | Allocated |
|---------- |---------:|---------:|---------:|----------:|----------:|
| EncodeAll | 20.66 ms | 0.065 ms | 0.051 ms | 1406.2500 |  11.25 MB |

## Dell Core Ultra 5

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
Intel Core Ultra 5 235T 2.20GHz, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 19.57 ms | 0.050 ms | 0.047 ms | 937.5000 |  11.25 MB |

# Penalty Contribution

Penalty contribution statistics (samples=12,800)

| Bucket        | Min |  Max |    Mean |  StdDev | Share% |
|---------------|----:|-----:|--------:|--------:|-------:|
| 2x2Blocks     |  36 | 9237 | 1306.61 | 1824.02 |  54.45 |
| SameColorCols |  10 | 3687 |  476.31 |  672.00 |  19.85 |
| SameColorRows |   8 | 3223 |  457.02 |  644.43 |  19.05 |
| FinderRows    |   0 |  800 |   81.70 |  110.78 |   3.40 |
| FinderCols    |   0 |  760 |   78.03 |  106.70 |   3.25 |
| ColorBalance  |   0 |   10 |    0.03 |    0.51 |   0.00 |

# Mask Pattern Selection

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

# Version Distribution

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


# Comparison with other libraries

`compare` mode: QR code generation only (no rendering), 400 payloads × 4 ECC levels per invocation,
library defaults (except ZXing.Net, see below).

## Differences between libraries

- *QrCodeGenerator* (this library) will always compact the data segments and generate to smallest possible QR code.
- *ZXing.NET* is run without segment compaction. So several generated QR codes are bigger than they need to be.
Segment compaction could be enabled but the ZXing.NET implementation is expensive.
The library is told to use UTF-8. Otherwise, it uses ISO-8859-1 and replaces characters that cannot be represented with `?`.
The other libraries prefer ISO-8859-1 but automatically fall back to UTF-8 if needed. 
- *SkiaSharp.QrCode* does not compact data segments and produces bigger QR codes than needed.
The library also depends on *SkiaSharp*. Thus, it is big and depends on native architecture specific DLLs.
- *QrCoder* compacts data segments but does not generate the smallest QR code in all cases. But it is close.

The libraries have also different strategies determining if they insert an ECI segment to indicate the character set.


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
| QrCodeGenerator |    22.02 ms | 0.153 ms | 0.143 ms |  1.00 |    0.01 |  1406.2500 |         - |  11718.46 KB |        1.00 |
| QRCoder         | 1,806.44 ms | 3.816 ms | 3.569 ms | 82.03 |    0.54 |  1000.0000 |         - |   15708.1 KB |        1.34 |
| SkiaSharpQrCode |    24.29 ms | 0.072 ms | 0.060 ms |  1.10 |    0.01 |    93.7500 |         - |    865.32 KB |        0.07 |
| ZXingNet        | 1,169.48 ms | 3.126 ms | 2.924 ms | 53.11 |    0.36 | 58000.0000 | 1000.0000 | 476209.76 KB |       40.64 |

### Dell Core Ultra 5

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
Intel Core Ultra 5 235T 2.20GHz, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.400
  [Host]     : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
```

| Method          | Mean        | Error    | StdDev   | Ratio  | RatioSD | Gen0       | Allocated    | Alloc Ratio |
|---------------- |------------:|---------:|---------:|-------:|--------:|-----------:|-------------:|------------:|
| QrCodeGenerator |    20.94 ms | 0.060 ms | 0.053 ms |   1.00 |    0.00 |   937.5000 |  11718.46 KB |        1.00 |
| QRCoder         | 2,292.55 ms | 4.503 ms | 3.992 ms | 109.49 |    0.33 |  1000.0000 |   15708.1 KB |        1.34 |
| SkiaSharpQrCode |    22.21 ms | 0.019 ms | 0.016 ms |   1.06 |    0.00 |    62.5000 |    866.16 KB |        0.07 |
| ZXingNet        | 1,328.42 ms | 1.902 ms | 1.686 ms |  63.44 |    0.17 | 38000.0000 | 476209.76 KB |       40.64 |


## Versions

The average version shows how compactly each library encodes the payloads (data segment compaction).

Average QR code version (samples=1'600)

| Library          | Avg. version |
|----------------- |-------------:|
| QrCodeGenerator  |         8.93 |
| QRCoder          |         8.98 |
| SkiaSharp.QrCode |         9.12 |
| ZXing.Net        |         9.16 |