### PathHelpers

#### Purpose

`PathHelpers` is a static utility class that provides a safe path-combination primitive.
It guards against path-traversal attacks by verifying the resolved combined path remains
within the base directory, protecting the tool when it constructs file paths from
`file` fields read from externally supplied evidence index records.

#### Data Model

N/A — static utility class with no instance state.

#### Key Methods

**`PathHelpers.SafePathCombine(string basePath, string relativePath)`** → `string`

- *Parameters*:
  - `string basePath` — the root directory that the result must reside within; must not
    be null
  - `string relativePath` — the relative path to combine with `basePath`; must not be null
- *Returns*: The combined absolute path as a string
- *Preconditions*: Both parameters are non-null and the combined path resolves inside
  `basePath`
- *Postconditions*: Returned path is within `basePath`; throws otherwise

Algorithm:

1. Reject null inputs via `ArgumentNullException.ThrowIfNull`.
2. Combine the paths with `Path.Combine` to produce a candidate path.
3. Resolve both `basePath` and the candidate to absolute form via `Path.GetFullPath`.
4. Compute `Path.GetRelativePath(absoluteBase, absoluteCombined)`.
5. Reject the input if the relative result is `".."`, starts with `".."` followed by a
   directory-separator character, or is rooted (absolute) — any of these conditions
   indicates the combined path escapes `basePath`.

Using `Path.GetRelativePath()` after resolving to absolute form handles all traversal
patterns (`../`, embedded `/../`, absolute-path overrides) without fragile pre-combine
string inspection. The check treats `..` as an escaping segment only when it is the
complete relative result or is followed by a separator, avoiding false positives for
valid names such as `..data`.

Note: `Path.GetFullPath` normalizes `.`/`..` segments but does not resolve symlinks or
reparse points; this containment check guards against string-level traversal only.

#### Error Handling

| Exception | Condition |
| --------- | --------- |
| `ArgumentNullException` | `basePath` or `relativePath` is null |
| `ArgumentException` | Resolved path escapes `basePath` |
| `NotSupportedException` | Path contains an unsupported format (e.g. colon in a non-root position on Windows); propagated without wrapping |
| `PathTooLongException` | Combined path exceeds the platform path-length limit; propagated without wrapping |

#### Dependencies

- No dependencies on other ReviewMark units, subsystems, or OTS libraries — uses only
  `System.IO.Path` from the .NET runtime.

#### Callers

- **`ReviewIndex.Scan()`** (Indexing subsystem) — calls `SafePathCombine()` to validate
  each PDF file path constructed from index `file` fields before opening the file,
  preventing directory-traversal attacks from maliciously crafted evidence index records
