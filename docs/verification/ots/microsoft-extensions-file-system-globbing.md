## Microsoft.Extensions.FileSystemGlobbing

### Verification Approach

**Component**: Microsoft.Extensions.FileSystemGlobbing
(<https://www.nuget.org/packages/Microsoft.Extensions.FileSystemGlobbing>)
**Role**: Glob-pattern file matching library used by the Configuration subsystem's `GlobMatcher`
unit to resolve file lists from `.reviewmark.yaml` pattern configurations.
**Acceptance approach**: Automated test coverage.

The integration surface is `Matcher.AddInclude`, `Matcher.AddExclude`, and
`Matcher.GetResultsInFullPath`, called by `GlobMatcher.GetMatchingFiles()` for each include and
exclude pattern. All key behaviors — single includes, excludes, multiple includes, re-include
after exclude, and cross-platform path normalization — are exercised by
`DemaConsulting.ReviewMark.OtsSoftwareTests`, with additional coverage through `GlobMatcherTests.cs`.

### Test Scenarios

#### FileSystemGlobbingWildcardMatching

Evidence that the library resolves include glob patterns correctly against the file system,
including `**` double-wildcard semantics.

- **`Matcher_GetResultsInFullPath_DoubleWildcard_MatchesFilesInSubdirectories`** — a file in a
  subdirectory is matched by a `**/*.cs` pattern added with `AddInclude`, confirming `**`
  double-wildcard traversal.
- **`Matcher_GetResultsInFullPath_SingleWildcard_MatchesFilesInDirectory`** — a `*.cs` pattern
  added with `AddInclude` matches only files in the root directory, not subdirectories.
- **`GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles`** — a single
  `**/*.cs` include pattern returns all matching files and excludes non-matching files.
- **`GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching`** — multiple include
  patterns return the union of all matching files.
- **`GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`** — combining include
  and exclude patterns returns only the intended files.
- **`GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`** — a subsequent
  include pattern re-adds files previously removed by an exclude pattern.
- **`GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList`** — a pattern matching no
  files returns an empty list.
- **`GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator`** — returned paths
  for files in subdirectories use forward-slash separators regardless of the host OS.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

#### FileSystemGlobbingExclusionPrefix

Evidence that the library correctly excludes files matching an exclusion pattern.

- **`Matcher_GetResultsInFullPath_ExcludePattern_OmitsMatchingFiles`** — files matching an
  exclusion pattern added with `AddExclude` are absent from the results, confirming exclusion
  behavior.
- **`GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles`** — an exclude pattern
  (prefixed with `!`) removes matching files from the result set.
- **`GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`** — combining include
  and exclude patterns returns only the intended files.
- **`GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`** — a subsequent
  include pattern re-adds files previously removed by an exclude pattern.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

### Requirements Coverage

- **ReviewMark-OTS-FileSystemGlobbing-WildcardMatching**: Microsoft.Extensions.FileSystemGlobbing
  shall match files using `**` double-wildcard patterns.
  - *FileSystemGlobbingWildcardMatching*: verifies `**` wildcard traversal and cross-platform path
    normalization.
    - `Matcher_GetResultsInFullPath_DoubleWildcard_MatchesFilesInSubdirectories`
    - `Matcher_GetResultsInFullPath_SingleWildcard_MatchesFilesInDirectory`
    - `GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching`
    - `GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`
    - `GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`
    - `GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList`
    - `GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator`
- **ReviewMark-OTS-FileSystemGlobbing-ExclusionPrefix**: Microsoft.Extensions.FileSystemGlobbing
  shall exclude files matching a `!`-prefixed pattern.
  - *FileSystemGlobbingExclusionPrefix*: verifies exclusion pattern behavior.
    - `Matcher_GetResultsInFullPath_ExcludePattern_OmitsMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`
    - `GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`
