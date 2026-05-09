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

#### Test Scenarios

##### PathHelpers_SafePathCombine_SimpleRelativePath_ReturnsCombinedPath

**Scenario**: `SafePathCombine("base", "relative/file.txt")` is called.

**Expected**: Returns `"base/relative/file.txt"` using forward slashes.

**Requirement coverage**: `ReviewMark-PathHelpers-SafePathCombine`

##### PathHelpers_SafePathCombine_AbsoluteRelative_ReturnsRelativeOnly

**Scenario**: `SafePathCombine` is called where `relative` begins with a drive letter or
absolute path segment.

**Expected**: Returns the relative path without the base; path traversal is prevented.

**Boundary / error path**: Absolute relative injection.

**Requirement coverage**: `ReviewMark-PathHelpers-SafePathCombine`

##### PathHelpers_SafePathCombine_DotDotRelative_ReturnsSafeBase

**Scenario**: `SafePathCombine("base", "../../etc/passwd")` is called.

**Expected**: Returns only the base path; directory traversal is blocked.

**Boundary / error path**: Path traversal attempt.

**Requirement coverage**: `ReviewMark-PathHelpers-SafePathCombine`

##### PathHelpers_ToForwardSlash_BackslashPath_ReturnsForwardSlashPath

**Scenario**: `ToForwardSlash(@"a\b\c")` is called.

**Expected**: Returns `"a/b/c"`.

**Requirement coverage**: `ReviewMark-PathHelpers-Normalization`

##### PathHelpers_ToForwardSlash_AlreadyForwardSlash_ReturnsUnchanged

**Scenario**: `ToForwardSlash("a/b/c")` is called.

**Expected**: Returns `"a/b/c"` unchanged.

**Requirement coverage**: `ReviewMark-PathHelpers-Normalization`

##### PathHelpers_SafePathCombine_NullBase_ThrowsArgumentNullException

**Scenario**: `SafePathCombine(null, "relative")` is called.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-PathHelpers-NullRejection`

##### PathHelpers_SafePathCombine_NullRelative_ThrowsArgumentNullException

**Scenario**: `SafePathCombine("base", null)` is called.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-PathHelpers-NullRejection`

#### Requirements Coverage

- **ReviewMark-PathHelpers-SafePathCombine**: PathHelpers_SafePathCombine_SimpleRelativePath_ReturnsCombinedPath,
  PathHelpers_SafePathCombine_AbsoluteRelative_ReturnsRelativeOnly,
  PathHelpers_SafePathCombine_DotDotRelative_ReturnsSafeBase
- **ReviewMark-PathHelpers-Normalization**: PathHelpers_ToForwardSlash_BackslashPath_ReturnsForwardSlashPath,
  PathHelpers_ToForwardSlash_AlreadyForwardSlash_ReturnsUnchanged
- **ReviewMark-PathHelpers-NullRejection**: PathHelpers_SafePathCombine_NullBase_ThrowsArgumentNullException,
  PathHelpers_SafePathCombine_NullRelative_ThrowsArgumentNullException
