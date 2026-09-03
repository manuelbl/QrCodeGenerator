# CLAUDE.md

## Purpose

QrCodeGenerator is a library to generate QR code. It is designed to be easy to use and performant.

The library only supports limited graphics types and options in order to work without
any graphics libraries, which might not run all platforms.
The library can provide the QR code as a list of rectangles or as a two-dimensional array of pixels
(called modules by the QR code standard). It is then up to the application to display the QR code.
Many demo projects show how to use this approach for different graphics libraries and UI frameworks.

## Build targets

- **`QrCodeGenerator/`** (the library) targets `netstandard2.0;net6.0`. The `net6.0` target exists only to enable trimming (`IsTrimmable`). Keep public API and language usage compatible with netstandard2.0 — don't reach for newer BCL/`Span` APIs that aren't available there.
- **`QrCodeGeneratorTest/`** (the unit tests) targets `net8.0;net10.0`, plus `net481` on Windows. `dotnet test` runs every target framework.
- **`QrCodeGeneratorProfiling/`** targets `net10.0` (BenchmarkDotNet).
- **`QrCodeAnalyzer/`** is a separate WPF tool in its own solution (`QrCodeAnalyzer/QrCodeAnalyzer.sln`), not part of `QrCodeGenerator.sln`.


## Architecture

`QrCode` is the only substantial public surface: factory methods create immutable `QrCode` instances. They can be rendered to
sVG, PNG, BMP or a list of rectangles. It holds a single `BitMatrix` of modules. Almost all real work lives in `internal` types.


### Encoding pipeline

Text/bytes → segments → codewords → matrix, in this order:

1. **`DataSegment.FromText` / `FromBinaryData`** — chooses the text encoding. For automatic ECI: tries Latin-1 (ISO-8859-1) and adds no ECI; falls back to UTF-8 with an ECI designator if Latin-1 is lossy. Segments retain the *original unencoded bytes* as an `ArraySegment` over the caller's array until the bit stream is built — **the source array must not be mutated** in the meantime.
2. **`SegmentCompaction`** — picks the cheapest per-byte mode (numeric > alphanumeric > Kanji > binary), groups consecutive bytes into blocks, then assigns each block the mode of the segment it ends up in by a dynamic programme over the blocks (costs in sixths of a bit, rounded up at segment ends), so the bit stream is the shortest possible. Header cost depends on `version` only through the count-indicator widths, which are constant within the version groups 1–9 / 10–26 / 27–40. `QrCode.EncodeTextAdvanced` and `EncodeBinary` therefore build the blocks once (`SegmentSource`), derive the segments per group in ascending order, and take the first group whose segments fit; groups the data cannot fit into are skipped by a lower bound. This is what produces the "smallest possible QR code" claim.
3. **`QrCodeBuilder.Build`** is a thin orchestrator that wires the remaining stages, each its own `internal static` module:
   - **`VersionPlanner.Plan`** — smallest version that fits, then boost ECC level for free if it doesn't grow the version. Returns a named `(int Version, int Ecc)`.
   - **`Codewords.BuildData`** — segments → `BitStream` → byte codewords + terminator + 0xEC/0x11 padding.
   - **`Codewords.AddErrorCorrection`** — splits into blocks, computes Reed-Solomon ECC (`ReedSolomon`), interleaves data and ECC codewords per spec.
   - **`MatrixEncoder.Encode`** — matrix layout then mask selection:
     - `FixedPatterns.BuildFixedPatterns` deals with the fixed-pattern geometry of a version: one walk emits both the *drawn* matrix (finder/timing/alignment/version) and the *reserved-module* mask. The reserved mask, inverted, is the *payload-area map* (`GetPayloadAreaMap`). Then `FillPayload` zig-zags the codewords into the free modules.
     - `ApplyBestPattern` — XORs each of the 8 mask patterns, scores it (`Penalty`), keeps the lowest, and draws the format information. `EncodingInfo.ForcedDataMask` can override the choice.

   The ISO/IEC 18004 lookup tables these stages share live in **`QrCodeParameters`**.

### `BitMatrix` and the transpose trick

`BitMatrix` (`internal readonly struct`) is the central data structure: a square bit grid stored in `ulong`s, row-major, with every row starting at a word boundary. It exposes fast whole-matrix `And`/`Xor`/`Invert`/`PopCount` and an in-place **64×64 block transpose**.

A row holds its modules in 1, 2 or 3 words (one per 64 columns) — `UsedWordsPerRow`, the matrix's **row layout**. The three layouts cover sizes 1–64 / 65–128 / 129–192, i.e. versions 1–11 / 12–27 / 28–40. The *stride* between rows (`WordsPerRow`, `RowShift`) is that count rounded up to a power of two — 1, 2 or 4 — so a row index is a shift; a 3-word row gets a 4th, always-zero padding word, which is why whole-matrix operations can run flat over `Raw`. The layout follows from the size alone, so two matrices of the same size always agree on it. It exists for `Penalty`: the narrower the row, the fewer words each rule scans, and versions 1–11 are most QR codes. Maximum size is 192 (version 40 is 177).

The transpose is load-bearing, not a convenience: every penalty/format rule that operates on *columns* is implemented by transposing the matrix and reusing the *row*-wise algorithm. `ApplyBestPattern` keeps a `modules` and a `transposed` copy in lock-step, and `Penalty` takes both. `Penalty` itself is bit-parallel (operates on whole `ulong` words, not per-module) and has an early-stop path that bails once the running score exceeds the best-so-far. Each row-scanning rule has one implementation per row layout, with its loop over the words unrolled; all three compute the same score, so which one runs never changes the output.

### Performance-tuned constants

Two orderings are deliberately ordered by profiling data (see `QrCodeGeneratorProfiling/README.md`), not arbitrary: `MatrixEncoder.PatternEvaluationOrder` (evaluate likely-best masks first to tighten the early-stop bound) and the rule order inside `Penalty.CalculatePenalty`. Reordering them changes performance, not output. Per-version `BitMatrix` results are cached in `ConcurrentDictionary` instances — `FixedPatterns` caches the drawn fixed patterns and the payload-area map; `MatrixEncoder` caches the data-mask patterns. Cached `BitMatrix` instances are shared and must not be mutated (callers `Copy()` first).

Everything is keyed by `version` (1–40) and `ecc` (0–3 = L/M/Q/H). The large `static readonly` lookup tables in `QrCodeParameters` (codeword capacity, block counts, alignment positions, format/version info bits) come straight from ISO/IEC 18004 tables — comments cite the table numbers.

### Rendering and diagnostics

- `RectangleBuilder` merges adjacent dark modules into the largest rectangles (greedy, non-overlapping, union == dark modules) to shrink output. It is the single source of truth for that geometry: `QrCode.ToRectangles` exposes the list publicly (as `QrRectangle`s, in `GetModule` coordinates with no border), and `SvgBuilder` (SVG document + SVG/XAML path) consumes the same list, adding the border at emit time. `BmpBuilder` (BMP) and `PngBuilder` (PNG) take the finished modules directly. `QrCode` delegates to all of them.
- `StructuredAppend` splits long text across up to 16 linked QR codes (used by `EncodeTextInMultipleCodes`).
- `EncodingInfo` / `PenaltyScore` are opt-in diagnostics: pass an `EncodingInfo` to capture per-mask penalty breakdowns and the chosen segments. This forces *full* penalty evaluation (disables early-stop), so it is slower — it exists for the `QrCodeAnalyzer` tool, not normal use.

## Tests

Uses **xUnit v3** with **Verify.XunitV3** for snapshot/approval testing and **Xunit.Combinatorial** for parameterized matrices.

The test strategy is characterization + cross-validation, not just unit tests:

- **`QrCodeDataProvider.cs`** is a large generated `[ClassData]` source feeding `QrCodeTest`. `TestQrCode` asserts the exact module layout, version, ECC, and mask for each case (golden tests).
- **`VerifyWithZXing`** decodes every generated QR code with **ZXing.Net** and asserts the text round-trips with *zero* errors corrected. `CorruptedModule_IsCorrected_AndReportsErrorsCorrected` is a negative control that flips one module to prove that assertion has teeth.
- **`ReedSolomonTest`** cross-checks ECC against the **STH1123.ReedSolomon** package.
- **Verify** snapshot tests cover rendered output (SVG/PNG/BMP). Snapshot `.verified.*` files live alongside the test source in `QrCodeGeneratorTest/`. When a snapshot changes intentionally, run `dotnet test` once — Verify fails and writes the new `.verified.*` file; accept it by replacing the old one (or via the Verify tooling).