# PathHelpers

## Overview

`PathHelpers` is a static utility class that provides a safe path-combination method. It
protects callers against path-traversal attacks by verifying the resolved combined path stays
within the base directory. Note that `Path.GetFullPath` normalizes `.`/`..` segments but does
not resolve symlinks or reparse points, so this check guards against string-level traversal
only.

## Class Structure

### SafePathCombine Method

```csharp
internal static string SafePathCombine(string basePath, string relativePath)
```

Combines `basePath` and `relativePath` safely, ensuring the resulting path remains within
the base directory.

**Validation steps:**

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Combine the paths with `Path.Combine` to produce the candidate path (preserving the
   caller's relative/absolute style).
3. Resolve both `basePath` and the candidate to absolute form with `Path.GetFullPath`.
4. Compute `Path.GetRelativePath(absoluteBase, absoluteCombined)` and reject the input if
   the result is exactly `".."`, starts with `".."` followed by `Path.DirectorySeparatorChar`
   or `Path.AltDirectorySeparatorChar`, or is itself rooted (absolute), which would indicate
   the combined path escapes the base directory.

## Design Decisions

- **`Path.GetRelativePath` for containment check**: Using `GetRelativePath` to verify
  containment handles root paths (e.g. `/`, `C:\`), platform case-sensitivity, and
  directory-separator normalization natively. The containment test should treat `..` as an
  escaping segment only when it is the entire relative result or is followed by a directory
  separator, avoiding false positives for valid in-base names such as `..data`.
- **Post-combine canonical-path check**: Resolving paths after combining handles all traversal
  patterns — `../`, embedded `/../`, absolute-path overrides, and platform edge cases —
  without fragile pre-combine string inspection of `relativePath`.
- **ArgumentException on invalid input**: Callers receive a specific `ArgumentException`
  identifying `relativePath` as the problematic parameter, making debugging straightforward.
- **No logging or error accumulation**: `SafePathCombine` is a pure utility method that throws
  on invalid input; it does not interact with the `Context` or any output mechanism.

## Security Rationale

Evidence index files may be loaded from external sources (file shares or URLs).
The `file` field in each index record is supplied by the evidence store and must
be treated as untrusted input. Without path validation, a maliciously crafted
index could direct the tool to read or reference files outside the intended
evidence directory. `SafePathCombine` eliminates this attack surface.
