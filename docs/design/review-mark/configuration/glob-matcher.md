### GlobMatcher

![Configuration Structure](ConfigurationView.svg)

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

**`GlobMatcher.IsMatch(string relativePath, IReadOnlyList<string> patterns)`** → `bool`

- *Parameters*: `relativePath` — forward-slash relative path (not null); `patterns` — ordered
  glob patterns (not null)
- *Returns*: `true` if the path is included after applying all patterns in order
- *Postconditions*: No file-system access occurs, so paths of deleted files can be matched

Returns `false` immediately for paths outside the base directory: rooted paths, a leading `/`
or `\`, a drive-letter prefix (recognized on every operating system) or any `..` path segment
(leading, middle or trailing; even `src/../a.cs`, which would stay inside, is rejected for
safety). `/` always separates segments; `\` separates segments only on Windows (on POSIX it is a
literal filename character, so `a\..\b` is not a traversal). Names such as `..foo/a.cs` or
`a..b/x.cs` are not outside paths. Otherwise starts with `false`. For each pattern in declaration order, a single-pattern `Matcher`
evaluates the path string (`Matcher.Match(string)`); a matching include sets the result to
`true` and a matching `!` exclude sets it to `false`. This reproduces the ordered semantics
of `GetMatchingFiles()` (including re-include after exclude) and uses the same matching
engine, which is verified by an equivalence test against file-system resolution.

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
