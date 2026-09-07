# Benchmark Log

The benchmark history behind the [profiling project](README.md): one entry per optimization step,
recorded when the step was made.

Entries are in chronological order, oldest first. Each one shows the state of the library after that
change and is left as it was written; the log is not updated retroactively.

Measurements are comparable only within a sample data era. The sample data changed once, at
[Sample Data Change](#sample-data-change). Numbers on either side of that point mean different
things and must not be compared.

## Baseline

MacBook M5 Pro

### Profiling

```
Profile loop: 500 iterations × 200 payloads × 4 ECC levels
Total EncodeText calls: 400'000
Elapsed: 00:01:41.0574210 (checksum=14696000)
```

### Benchmark

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
`

## Introduction of BitMatrix

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


## Optimized bit count (color balance)

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



## Optimized horizontal finder pattern

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


## Use BitMatrix operations for patterns

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



## Pattern caching

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



## Penalty Calculation with Transposed Matrix

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


## Improved 2x2 block penalty

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


## Improved Calc Strides of Same Color

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


## Evaluate penalty for likely patterns first

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


## Data Segment Compaction

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



## Reed-Solomon Product Table

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


## Row Layouts

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



## Sample Data Change

**The sample data changed at this point**, from 200 short payloads to 400 payloads that include
long texts. Every measurement below was taken on the new data, every measurement above on the old.

The entry that follows is therefore slower than the one before it even though nothing got slower:
it encodes twice as many payloads, and the added ones are much larger.

## Larger Sample Data

The sample data covered versions 1 to 13 only, so the two- and three-word row layouts of
`BitMatrix` were barely measured. It now holds 400 payloads instead of 200, and one in five of
them is a long text of 400 to 900 characters assembled from the same fragments (sentences, names,
towns, URLs, numbers, messages). Long payloads reach versions 12 to 36, the short ones stay in
versions 1 to 11.

The measurements below are the new baseline; they are not comparable to the sections above, which
all used the 200 short payloads.

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


## Optimal Segment Compaction

The segment compaction now assigns the segment modes by a dynamic programme over the blocks
instead of two greedy merge passes, and the compaction runs per version group (1–9, 10–26, 27–40)
instead of once for the maximum version. The result is the shortest possible bit stream for the
chosen version; the `compaction` mode reports no case where QRCoder's segments are shorter.
The checksum differs from the sections above because some QR codes got smaller.

### MacBook Pro M5

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


## Precomputed payload filling

The target for each payload bit is computed once and cached.

### MacBook Pro M5

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


## Modified pattern evaluation order

### MacBook Pro M5

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
| EncodeAll | 19.57 ms | 0.050 ms | 0.047 ms | 937.5000 |  11.25 MB |


## Reduced memory allocation

### MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 19.60 ms | 0.183 ms | 0.171 ms | 906.2500 |   7.38 MB |


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
| EncodeAll | 18.30 ms | 0.092 ms | 0.072 ms | 593.7500 |   7.22 MB |


## Modified segment compaction

### MacBook Pro M5

```
BenchmarkDotNet v0.15.8, macOS Tahoe 26.6.2 (25G83) [Darwin 25.6.0]
Apple M5 Pro, 1 CPU, 18 logical and 18 physical cores                                                                                                                                                                                                                                      
.NET SDK 10.0.203                                                                                                                                                                                                                                                                          
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a                                                                                                                                                                                                                
  DefaultJob : .NET 10.0.7 (10.0.7, 10.0.726.21808), Arm64 RyuJIT armv8.0-a                                                                                                                                                                                                                
```

| Method    | Mean     | Error    | StdDev   | Gen0     | Allocated |
|---------- |---------:|---------:|---------:|---------:|----------:|
| EncodeAll | 19.85 ms | 0.038 ms | 0.029 ms | 843.7500 |   6.93 MB |                                                                                                                                                                                                                      
