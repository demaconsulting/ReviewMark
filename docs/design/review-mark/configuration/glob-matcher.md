### GlobMatcher

#### Purpose

`GlobMatcher` is a static utility class that resolves an ordered list of glob patterns
into a concrete, sorted list of file paths relative to a base directory. It provides the
file enumeration primitive used by the Configuration subsystem to expand the
`needs-review` and review-set file lists defined in `.reviewmark.yaml`, supporting both
inclusion patterns and `!`-prefixed exclusion patterns with recursive `**` matching.

#### Data Model

N/A — static utility class with no instance state.

#### Key Methods

**`GlobMatcher.GetMatchingFiles(string baseDirectory, IReadOnlyList<string> patterns)`**
→ `IReadOnlyList<string>`

- *Parameters*:
  - `string baseDirectory` — root directory to search within; must not be null, empty,
    or whitespace
  - `IReadOnlyList<string> patterns` — ordered list of glob patterns; patterns prefixed
    with `!` are exclusions, all others are inclusions; must not be null
- *Returns*: Sorted, deduplicated `IReadOnlyList<string>` of relative file paths with
  forward-slash separators
- *Preconditions*: `baseDirectory` is not null, empty, or whitespace; `patterns` is not null
- *Postconditions*: Returned list has no duplicates, is sorted by `StringComparer.Ordinal`,
  and all separators are normalized to forward slashes

Processes patterns in declaration order. Each inclusion pattern adds matching paths to a
`HashSet<string>` accumulator; each exclusion pattern removes matching paths. Because
patterns are applied in order, a later pattern can re-include files excluded by an earlier
one, or vice versa.

Per-pattern `Matcher` instances are used (not a single combined matcher) to preserve
declaration-order semantics. A `HashSet<string>` accumulator deduplicates files matched
by more than one include pattern. After all patterns are processed, paths are normalized
to forward slashes and sorted using `StringComparer.Ordinal`.

#### Error Handling

- `ArgumentNullException` — when `baseDirectory` or `patterns` is null
- `ArgumentException` — when `baseDirectory` is empty or whitespace
- `IOException` / `UnauthorizedAccessException` — file-system exceptions during enumeration
  are not caught and propagate to the caller

#### Dependencies

- **`Microsoft.Extensions.FileSystemGlobbing`** (OTS) — provides the `Matcher` class used
  for glob pattern matching and file enumeration via `AddInclude()` and
  `GetResultsInFullPath()`

#### Callers

- **`ReviewMarkConfiguration`** (Configuration subsystem) — calls `GetMatchingFiles()` to
  resolve `needs-review` and review-set glob patterns into file lists
- **`ReviewIndex.Scan()`** (Indexing subsystem) — calls `GetMatchingFiles()` to resolve
  PDF evidence glob patterns into file paths
