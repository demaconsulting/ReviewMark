### PathHelpers Verification

This document describes the unit-level verification design for the `PathHelpers` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`Indexing/PathHelpers.cs`.

#### Verification Approach

`PathHelpers` is verified with unit tests in `PathHelpersTests.cs`. All methods are
pure functions, so tests pass string arguments directly and assert on return values.
No file system access or mocking is required.

#### Dependencies

`PathHelpers` has no runtime dependencies on other tool units and no I/O operations.

#### Test Environment

N/A - standard test environment. All methods under test are pure functions with no file-
system access, I/O operations, or external dependencies. Tests pass string arguments
directly and assert on return values.

#### Acceptance Criteria

All PathHelpers unit tests pass with zero failures. Every `ReviewMark-PathHelpers-*`
requirement is covered by at least one passing test scenario. Null inputs and path
traversal attempts (relative `..` segments and absolute path injection) produce the
correct exception types.

#### Test Scenarios

##### PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly

**Scenario**: `SafePathCombine("/home/user/project", "subfolder/file.txt")` is called.

**Expected**: Returns the result of `Path.Combine(basePath, relativePath)`.

**Requirement coverage**: `ReviewMark-PathHelpers-SafeCombine`

##### PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly

**Scenario**: `SafePathCombine` is called with a multi-level relative path
(`"level1/level2/level3/file.txt"`).

**Expected**: Returns the correctly combined nested path.

**Requirement coverage**: `ReviewMark-PathHelpers-SafeCombine`

##### PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly

**Scenario**: `SafePathCombine` is called with a relative path that begins with `./`.

**Expected**: Returns the combined path with the current-directory prefix preserved.

**Requirement coverage**: `ReviewMark-PathHelpers-SafeCombine`

##### PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath

**Scenario**: `SafePathCombine` is called with an empty relative path (`""`).

**Expected**: Returns the base path unchanged.

**Requirement coverage**: `ReviewMark-PathHelpers-SafeCombine`

##### PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException

**Scenario**: `SafePathCombine("/home/user/project", "../etc/passwd")` is called.

**Expected**: `ArgumentException` is thrown with a message containing "Invalid path component".

**Boundary / error path**: Path traversal via leading `..` segment.

**Requirement coverage**: `ReviewMark-PathHelpers-TraversalRejection`

##### PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException

**Scenario**: `SafePathCombine` is called with a relative path containing `..` embedded
in the middle (e.g. `"subfolder/../../../etc/passwd"`).

**Expected**: `ArgumentException` is thrown with a message containing "Invalid path component".

**Boundary / error path**: Path traversal via embedded `..` segments.

**Requirement coverage**: `ReviewMark-PathHelpers-TraversalRejection`

##### PathHelpers_SafePathCombine_AbsoluteUnixPath_ThrowsArgumentException

**Scenario**: `SafePathCombine` is called where the relative path is a Unix absolute path
(`/etc/passwd`). Runs on all platforms.

**Expected**: `ArgumentException` is thrown with a message containing "Invalid path component".

**Boundary / error path**: Unix absolute path injection.

**Requirement coverage**: `ReviewMark-PathHelpers-TraversalRejection`

##### PathHelpers_SafePathCombine_AbsoluteWindowsPath_ThrowsArgumentException

**Scenario**: `SafePathCombine` is called where the relative path is a Windows absolute path
(`C:\Windows\System32\file.txt`). Runs on Windows only.

**Expected**: `ArgumentException` is thrown with a message containing "Invalid path component".

**Boundary / error path**: Windows absolute path injection.

**Requirement coverage**: `ReviewMark-PathHelpers-TraversalRejection`

##### PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException

**Scenario**: `SafePathCombine(null, "relative")` is called.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null base path rejection.

**Requirement coverage**: `ReviewMark-PathHelpers-NullRejection`

##### PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException

**Scenario**: `SafePathCombine("base", null)` is called.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null relative path rejection.

**Requirement coverage**: `ReviewMark-PathHelpers-NullRejection`

#### Requirements Coverage

- **ReviewMark-PathHelpers-SafeCombine**: PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly,
  PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly,
  PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly,
  PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath
- **ReviewMark-PathHelpers-TraversalRejection**: PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException,
  PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException,
  PathHelpers_SafePathCombine_AbsoluteUnixPath_ThrowsArgumentException,
  PathHelpers_SafePathCombine_AbsoluteWindowsPath_ThrowsArgumentException (Windows only)
- **ReviewMark-PathHelpers-NullRejection**: PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException
