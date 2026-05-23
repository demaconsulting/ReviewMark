## Microsoft.Extensions.FileSystemGlobbing

### Verification Approach

ReviewMark uses Microsoft.Extensions.FileSystemGlobbing 10.0.8, referenced from
`DemaConsulting.ReviewMark.csproj`, to resolve ordered include and exclude patterns from
`.reviewmark.yaml`. The integration surface is `Matcher.AddInclude`, `Matcher.AddExclude`, and
`Matcher.GetResultsInFullPath`, exercised through `GlobMatcher.GetMatchingFiles()`, which applies
patterns one at a time so later includes can re-add files removed by earlier excludes and then
normalizes returned paths to forward slashes. Fitness for intended use is verified by dedicated OTS
tests in `test/OtsSoftwareTests/MicrosoftExtensionsFileSystemGlobbingTests.cs`, companion globbing
tests in `test/DemaConsulting.ReviewMark.Tests/Configuration/GlobMatcherTests.cs`, and the
`dotnet test` step in the `build` matrix job of `build.yaml`, which publishes TRX evidence to
`artifacts/`. No project-specific issues have been observed in this validated matching surface.

### Test Scenarios

**FileSystemGlobbingWildcardMatching**: Include patterns correctly resolve files in the root
directory and nested directories, support `**` traversal, combine multiple include patterns, and
return normalized relative paths suitable for deterministic review-set processing. This scenario is
tested by `Matcher_GetResultsInFullPath_DoubleWildcard_MatchesFilesInSubdirectories`,
`Matcher_GetResultsInFullPath_SingleWildcard_MatchesFilesInDirectory`,
`GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles`,
`GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching`,
`GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`,
`GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`,
`GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList`, and
`GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator`.

**FileSystemGlobbingExclusionPrefix**: Exclusion patterns remove unwanted matches from the active
file set while preserving ReviewMark's ordered semantics, including the ability to re-include a
specific path later in the pattern list. This scenario is tested by
`Matcher_GetResultsInFullPath_ExcludePattern_OmitsMatchingFiles`,
`GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles`,
`GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`, and
`GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`.

### Requirements Coverage

- **ReviewMark-OTS-FileSystemGlobbing-WildcardMatching**:
  Microsoft.Extensions.FileSystemGlobbing shall match files using `**` double-wildcard patterns.
  - *FileSystemGlobbingWildcardMatching*
    - `Matcher_GetResultsInFullPath_DoubleWildcard_MatchesFilesInSubdirectories`
    - `Matcher_GetResultsInFullPath_SingleWildcard_MatchesFilesInDirectory`
    - `GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching`
    - `GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`
    - `GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`
    - `GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList`
    - `GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator`
- **ReviewMark-OTS-FileSystemGlobbing-ExclusionPrefix**:
  Microsoft.Extensions.FileSystemGlobbing shall exclude files matching a `!`-prefixed pattern.
  - *FileSystemGlobbingExclusionPrefix*
    - `Matcher_GetResultsInFullPath_ExcludePattern_OmitsMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles`
    - `GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`
    - `GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`
