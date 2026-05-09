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

#### Requirements Coverage

- **ReviewMark-GlobMatcher-IncludeExclude**: GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles,
  GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles,
  GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList
- **ReviewMark-GlobMatcher-NullBaseDirectoryRejection**: GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException
- **ReviewMark-GlobMatcher-EmptyBaseDirectoryRejection**: GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException
- **ReviewMark-GlobMatcher-PathNormalization**: GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator
