# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build
dotnet build --configuration Release

# Test (all)
dotnet test

# Test (single class)
dotnet test --filter "FullyQualifiedName~QrCodeTest"

# Test (single method)
dotnet test --filter "FullyQualifiedName=Net.Codecrete.QrCodeGenerator.Test.QrCodeTest.Constants"

# Pack NuGet
dotnet pack --no-build
```

## Architecture

The library (`QrCodeGenerator/`) targets `netstandard2.0`. Additionally, there is a `net6.0` target to enable trimming. The test project (`QrCodeGeneratorTest/`) and the profiling project (`QrCodeGeneratorProfiling/`) target `net10.0`.

## Tests

Uses **xUnit v3** with **Verify.XunitV3** for snapshot/approval testing and **Xunit.Combinatorial** for parameterized matrices.

Snapshot files live alongside the test source in `QrCodeGeneratorTest/`. When a snapshot changes intentionally, run `dotnet test` once — Verify will fail and write the new `.verified.*` file; accept it by deleting the old one or using the Verify tooling.
