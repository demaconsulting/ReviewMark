### GlobMatcher

#### Verification Approach

GlobMatcher unit verification uses `GlobMatcherTests.cs` to create temporary directory layouts, execute ordered include and exclude pattern sets, and inspect the returned relative file lists. The tests use real file-system state and no mocks so declaration-order semantics, empty results, validation behavior, and path normalization are verified directly.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10 and create temporary directories and files in-process without any external services.

#### Acceptance Criteria

- All GlobMatcher unit tests pass with zero failures.
- Each `ReviewMark-GlobMatcher-*` requirement is traced to at least one scenario and test method.
- Ordered include and exclude semantics, argument validation, and forward-slash path normalization all behave as documented.

#### Test Scenarios

**GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles**: `GetMatchingFiles` is called with a single include pattern that matches several files. Expected outcome: All matching files are returned. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles`.

**GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles**: `GetMatchingFiles` is called with an include pattern followed by an exclude pattern. Expected outcome: Files matching the exclude pattern are absent from the result. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles`.

**GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList**: `GetMatchingFiles` is called with a pattern that matches nothing. Expected outcome: Returns an empty list. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList`.

**GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException**: `GetMatchingFiles` is called with null as the base directory. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-GlobMatcher-NullBaseDirectoryRejection`. This scenario is tested by `GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException`.

**GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException**: `GetMatchingFiles` is called with an empty string as the base directory. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Empty input rejection. Requirement coverage: `ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection`. This scenario is tested by `GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException`.

**GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator**: `GetMatchingFiles` returns a file from a subdirectory. Expected outcome: The returned path uses forward slashes regardless of OS. Requirement coverage: `ReviewMark-GlobMatcher-PathNormalization`. This scenario is tested by `GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator`.

**GlobMatcher_GetMatchingFiles_NullPatterns_ThrowsArgumentNullException**: `GetMatchingFiles` is called with null as the patterns list. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null patterns rejection. Requirement coverage: `ReviewMark-GlobMatcher-NullPatternsRejection`. This scenario is tested by `GlobMatcher_GetMatchingFiles_NullPatterns_ThrowsArgumentNullException`.

**GlobMatcher_GetMatchingFiles_WhitespaceBaseDirectory_ThrowsArgumentException**: `GetMatchingFiles` is called with a whitespace-only string as the base directory. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Whitespace base directory rejection. Requirement coverage: `ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection`. This scenario is tested by `GlobMatcher_GetMatchingFiles_WhitespaceBaseDirectory_ThrowsArgumentException`.

**GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles**: `GetMatchingFiles` is called with patterns that include all `.cs` files, exclude a subdirectory, then re-include a specific file in that subdirectory. Expected outcome: The re-included file and all other non-excluded files are in the result; the other excluded files are absent. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles`.

**GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles**: `GetMatchingFiles` is called with an include pattern for all `.cs` files and an exclude pattern for the `obj/` directory. Expected outcome: Only files outside the excluded directory are returned. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles`.

**GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList**: `GetMatchingFiles` is called with an empty patterns list. Expected outcome: An empty list is returned. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList`.

**GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching**: `GetMatchingFiles` is called with two include patterns (e.g., `**/*.cs` and `**/*.yaml`). Expected outcome: Files matching either pattern are returned; files matching neither are absent. Requirement coverage: `ReviewMark-GlobMatcher-IncludeExclude`. This scenario is tested by `GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching`.

#### Requirements Coverage

- **ReviewMark-GlobMatcher-IncludeExclude**:
  GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles,
  GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles,
  GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList,
  GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles,
  GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles,
  GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList,
  GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching
- **ReviewMark-GlobMatcher-NullBaseDirectoryRejection**: GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException
- **ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection**:
  GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException,
  GlobMatcher_GetMatchingFiles_WhitespaceBaseDirectory_ThrowsArgumentException
- **ReviewMark-GlobMatcher-NullPatternsRejection**: GlobMatcher_GetMatchingFiles_NullPatterns_ThrowsArgumentNullException
- **ReviewMark-GlobMatcher-PathNormalization**: GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator
