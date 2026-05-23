### GlobMatcher Verification

This document describes the unit-level verification design for the `GlobMatcher` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`Configuration/GlobMatcher.cs`.

#### Verification Approach

`GlobMatcher` is verified with unit tests in `GlobMatcherTests.cs`. Tests create temporary
directories with controlled file layouts, call `GlobMatcher.GetMatchingFiles` with various
pattern combinations, and assert on the returned file lists.

#### Dependencies

`GlobMatcher` has no dependencies on other tool units. All file system operations use
real temporary directories; no mocking is required.

#### Test Environment

N/A - standard test environment. Tests create temporary directories with controlled file
layouts in-process using the OS temporary directory; no external tools, services, or
network access are required. Temporary directories are deleted after each test.

#### Acceptance Criteria

All GlobMatcher unit tests pass with zero failures. Every `ReviewMark-GlobMatcher-*`
requirement is covered by at least one passing test scenario. Null inputs, empty strings,
and whitespace base directories produce the correct exception types.

#### Test Scenarios

##### GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles

**Scenario**: `GetMatchingFiles` is called with a single include pattern that matches
several files.

**Expected**: All matching files are returned.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles

**Scenario**: `GetMatchingFiles` is called with an include pattern followed by an exclude
pattern.

**Expected**: Files matching the exclude pattern are absent from the result.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList

**Scenario**: `GetMatchingFiles` is called with a pattern that matches nothing.

**Expected**: Returns an empty list.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException

**Scenario**: `GetMatchingFiles` is called with null as the base directory.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-GlobMatcher-NullBaseDirectoryRejection`

##### GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException

**Scenario**: `GetMatchingFiles` is called with an empty string as the base directory.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Empty input rejection.

**Requirement coverage**: `ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection`

##### GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator

**Scenario**: `GetMatchingFiles` returns a file from a subdirectory.

**Expected**: The returned path uses forward slashes regardless of OS.

**Requirement coverage**: `ReviewMark-GlobMatcher-PathNormalization`

##### GlobMatcher_GetMatchingFiles_NullPatterns_ThrowsArgumentNullException

**Scenario**: `GetMatchingFiles` is called with null as the patterns list.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null patterns rejection.

**Requirement coverage**: `ReviewMark-GlobMatcher-NullPatternsRejection`

##### GlobMatcher_GetMatchingFiles_WhitespaceBaseDirectory_ThrowsArgumentException

**Scenario**: `GetMatchingFiles` is called with a whitespace-only string as the base directory.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Whitespace base directory rejection.

**Requirement coverage**: `ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection`

##### GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles

**Scenario**: `GetMatchingFiles` is called with patterns that include all `.cs` files,
exclude a subdirectory, then re-include a specific file in that subdirectory.

**Expected**: The re-included file and all other non-excluded files are in the result; the other excluded files are absent.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles

**Scenario**: `GetMatchingFiles` is called with an include pattern for all `.cs` files
and an exclude pattern for the `obj/` directory.

**Expected**: Only files outside the excluded directory are returned.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList

**Scenario**: `GetMatchingFiles` is called with an empty patterns list.

**Expected**: An empty list is returned.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

##### GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching

**Scenario**: `GetMatchingFiles` is called with two include patterns (e.g., `**/*.cs` and `**/*.yaml`).

**Expected**: Files matching either pattern are returned; files matching neither are absent.

**Requirement coverage**: `ReviewMark-GlobMatcher-IncludeExclude`

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
