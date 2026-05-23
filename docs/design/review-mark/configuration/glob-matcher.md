### GlobMatcher

#### Purpose

`GlobMatcher` is a static utility class that resolves an ordered list of glob patterns
into a concrete, sorted list of file paths relative to a base directory. It provides the
file enumeration primitive used by the Configuration subsystem to expand the
`needs-review` and `review-set` file lists defined in `.reviewmark.yaml`, supporting
both inclusion and `!`-prefixed exclusion patterns with recursive `**` matching.

#### Data Model

N/A — static utility class with no instance state.

#### Key Methods

##### Parameters

| Parameter | Type | Description |
| --------- | ---- | ----------- |
| `baseDirectory` | `string` | The root directory to search within. Must not be null, empty, or whitespace. |
| `patterns` | `IReadOnlyList<string>` | Ordered list of glob patterns. Must not be null. Patterns prefixed with `!` are treated as exclusions; all other patterns are inclusions. |

##### Preconditions

- `baseDirectory` must not be `null` (throws `ArgumentNullException`)
- `baseDirectory` must not be empty or whitespace (throws `ArgumentException`)
- `patterns` must not be `null` (throws `ArgumentNullException`)

##### Postconditions

- Returns a non-null `IReadOnlyList<string>` of relative file paths
- All path separators are normalized to forward slashes
- The list contains no duplicates
- The list is sorted in ordinal order

##### Algorithm

`GlobMatcher.GetMatchingFiles(baseDirectory, patterns)` processes patterns in the
order they are declared. Patterns prefixed with `!` are exclusion patterns; all
others are inclusion patterns. Each inclusion pattern adds matching paths to the
result set; each exclusion pattern removes matching paths from the result set.
Because patterns are applied in declaration order, a later pattern can re-include
files excluded by an earlier one, or exclude files included by an earlier one. The
`**` wildcard matches any number of path segments, enabling recursive matching.
After all patterns are processed, the result set is sorted and returned.

Per-pattern `Matcher` instances are used (rather than a single combined matcher) to
preserve pattern order and allow independent include/exclude matching — a single
combined matcher cannot enforce declaration-order semantics. A `HashSet` accumulator
is used to collect matched paths across multiple patterns and deduplicate results,
so that files matched by more than one include pattern appear only once in the result.
Path separators are normalized to forward slashes before returning, ensuring consistent
fingerprints across platforms.

##### Return Value

The method returns a sorted `IReadOnlyList<string>` of relative file paths. Path
separators are normalized to forward slashes regardless of the host operating system,
ensuring consistent fingerprint computation across platforms.

##### Usage

`GlobMatcher.GetMatchingFiles()` is called by `ReviewMarkConfiguration` to resolve:

- The `needs-review` file list, which represents all files subject to review
- Each `review-set` file list, which represents the files covered by a specific review record

#### Error Handling

`GlobMatcher.GetMatchingFiles()` throws the following exceptions for invalid inputs:

- `ArgumentNullException` — when `baseDirectory` or `patterns` is `null`
- `ArgumentException` — when `baseDirectory` is empty or whitespace

File-system exceptions (`IOException`, `UnauthorizedAccessException`) are not caught
and propagate to the caller when the base directory is inaccessible or the filesystem
returns an error during enumeration.

#### Interactions

**Called by:**

- `ReviewMarkConfiguration` (Configuration subsystem) — calls `GetMatchingFiles()` to
  resolve `needs-review` and review-set glob patterns into file lists
- `ReviewIndex.Scan()` (Indexing subsystem) — calls `GetMatchingFiles()` to resolve PDF
  evidence glob patterns into file paths

**Dependencies:**

- No dependencies on other ReviewMark units or subsystems
- `Microsoft.Extensions.FileSystemGlobbing` (OTS) — provides the `Matcher` class used
  for glob pattern matching and file enumeration
