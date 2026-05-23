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

## Relationships

- A **version** (1–40) fully determines the **fixed patterns** and therefore the
  **reserved modules** and the **payload-area map**.
- The **reserved modules** are the union of every fixed-pattern **footprint**;
  the **payload-area map** is their complement.
- Every dark **module** drawn by a fixed pattern lies within the **reserved modules**
  (the load-bearing invariant: drawn ⊆ reserved).
- A **mask pattern** is XORed only over the **payload-area map**, never over
  **reserved modules**.

## Example dialogue

> **Dev:** "If I move an alignment pattern, do I update the drawn matrix and the
> reserved modules separately?"
> **Domain expert:** "No — they're two views of the same **fixed patterns**. One walk
> emits both: it stamps the dark **modules** and reserves the **footprint** in the same
> place. You can't infer reserved from drawn, because a footprint reserves light modules
> too."
