# QrCodeGenerator Context

Domain language for the QR code generation library. These are the terms specific
to QR code structure and this library's layout pipeline — use them consistently in
code, comments, and discussion.

## Language

**Module**:
A single pixel of a QR code (dark or light); the unit the standard counts by.
_Avoid_: pixel (reserve "pixel" for rendered output), cell.

**Fixed patterns**:
The modules placed identically for a given version regardless of payload — finder
patterns, separators, timing patterns, alignment patterns, and version information —
together with the reserved area for format information.
_Avoid_: function patterns (the ISO/IEC 18004 term; we say "fixed patterns").

**Footprint**:
The area a fixed pattern occupies, independent of which modules within it are dark.
A footprint is reserved even where its modules are light (e.g. separators, the light
rings of a finder, format/version `0` bits).

**Reserved modules**:
The union of all fixed-pattern footprints — every module the payload must not use.

**Payload-area map**:
The complement of the reserved modules: the modules the payload zig-zag is allowed
to fill. This is what `GetPayloadAreaMap` returns.
_Avoid_: "data mask" — see Flagged ambiguities.

**Mask pattern**:
One of the 8 XOR patterns (index 0–7) applied to the payload area and chosen by
lowest penalty score. Exposed as `QrCode.Mask`.

**Scoring matrix**:
A **module** matrix paired with its transpose, kept in sync, used while selecting
the **mask pattern**. Its `Rows` view is the matrix as stored; its `Columns` view is
the transpose. Penalty rules that scan rows read `Rows`; rules that scan columns read
`Columns`, so the column rules reuse the row algorithm instead of duplicating it.
Every mutation (mask XOR, format-information bit) updates both views together.
_Avoid_: treating the transpose as a separate "transposed copy" the caller keeps in
sync by hand — the two views are one value.

**Mask pair**:
The two views (a mask and its transpose) of a single **mask pattern**, cached per
(mask pattern, **version**) and XORed into a **scoring matrix** as a unit.

**Codewords**:
The 8-bit symbols the payload becomes after a version and error correction level are
chosen: data codewords (segment bits + terminator + padding) followed by Reed-Solomon
error correction codewords, interleaved per spec, ready to fill into the matrix.

**Data segment mode info**:
The per-mode rules of a `DataSegmentMode` — mode indicator, count-indicator widths, and
(for the data modes: numeric, alphanumeric, Kanji, binary) the bit-length/byte-count
formulas and the segment factory — held in one internal descriptor per mode and looked
up by mode value. The public `DataSegmentMode` enum stays a plain enum; the behavior
keyed off it lives in the descriptor.
_Avoid_: putting per-mode logic on the public enum, or re-deriving it via `switch`/
`(int)mode` arithmetic at each call site.

## Relationships

- The encode pipeline is: text → data segments → **codewords** → **module** matrix.
  A **version**/error-correction level is planned first, then the **codewords** are
  built, then filled into the matrix and a **mask pattern** is chosen.
- A **version** (1–40) fully determines the **fixed patterns** and therefore the
  **reserved modules** and the **payload-area map**.
- The **reserved modules** are the union of every fixed-pattern **footprint**;
  the **payload-area map** is their complement.
- Every dark **module** drawn by a fixed pattern lies within the **reserved modules**
  (the load-bearing invariant: drawn ⊆ reserved).
- A **mask pattern** is XORed only over the **payload-area map**, never over
  **reserved modules**.
- Choosing the **mask pattern** scores one **scoring matrix** per candidate; the
  column penalty rules read its `Columns` view so they reuse the row algorithm, and
  each candidate mask is applied as a **mask pair**.

## Example dialogue

> **Dev:** "If I move an alignment pattern, do I update the drawn matrix and the
> reserved modules separately?"
> **Domain expert:** "No — they're two views of the same **fixed patterns**. One walk
> emits both: it stamps the dark **modules** and reserves the **footprint** in the same
> place. You can't infer reserved from drawn, because a footprint reserves light modules
> too."
