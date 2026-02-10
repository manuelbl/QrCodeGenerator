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


# Penalty Contribution

Penalty contribution statistics (samples=3,200,000)

| Bucket        | Min | Max  |   Mean | StdDev | Share% |
|---------------|----:|-----:|-------:|-------:|-------:|
| 2x2Blocks     |  81 | 1797 | 480.56 | 270.35 |  48.25 |
| SameColorCols |  74 |  751 | 226.60 | 106.04 |  22.75 |
| SameColorRows |  70 |  688 | 218.13 |  99.71 |  21.90 |
| FinderRows    |   0 |  280 |  36.25 |  41.23 |   3.64 |
| FinderCols    |   0 |  320 |  34.44 |  40.11 |   3.46 |
| ColorBalance  |   0 |   10 |   0.05 |   0.71 |   0.01 |


# Mask Pattern Selection

Mask pattern selection (samples=400,000)

|  Pattern   |    Count |   Share% |
|------------|---------:|---------:|
|  2         |  125,000 |    31.25 |
|  3         |   58,500 |    14.62 |
|  7         |   53,500 |    13.38 |
|  4         |   49,500 |    12.38 |
|  6         |   43,500 |    10.88 |
|  5         |   32,500 |     8.12 |
|  0         |   20,000 |     5.00 |
|  1         |   17,500 |     4.38 |
