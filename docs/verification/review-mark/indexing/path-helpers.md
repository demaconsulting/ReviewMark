### PathHelpers

#### Verification Approach

PathHelpers unit verification uses `PathHelpersTests.cs` to call the pure `SafePathCombine` function directly with valid paths, traversal attempts, absolute-path injections, and null values. The tests use no mocks or file-system setup because the unit's behavior is completely determined by its string inputs.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10 and require no external services, file fixtures, or environment setup beyond the test runner.

#### Acceptance Criteria

- All PathHelpers unit tests pass with zero failures.
- Each `ReviewMark-PathHelpers-*` requirement is traced to at least one scenario and test method.
- Valid combinations succeed, traversal attempts are rejected, and null inputs raise the documented exceptions.

#### Test Scenarios

**PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly**: `SafePathCombine("/home/user/project", "subfolder/file.txt")` is called. Expected outcome: Returns the result of `Path.Combine(basePath, relativePath)`. Requirement coverage: `ReviewMark-PathHelpers-SafeCombine`. This scenario is tested by `PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly`.

**PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly**: `SafePathCombine` is called with a multi-level relative path (`"level1/level2/level3/file.txt"`). Expected outcome: Returns the correctly combined nested path. Requirement coverage: `ReviewMark-PathHelpers-SafeCombine`. This scenario is tested by `PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly`.

**PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly**: `SafePathCombine` is called with a relative path that begins with `./`. Expected outcome: Returns the correctly combined path equivalent to `Path.Combine(basePath, relativePath)`, where `./` is consumed by the combination. Requirement coverage: `ReviewMark-PathHelpers-SafeCombine`. This scenario is tested by `PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly`.

**PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath**: `SafePathCombine` is called with an empty relative path (`""`). Expected outcome: Returns the base path unchanged. Requirement coverage: `ReviewMark-PathHelpers-SafeCombine`. This scenario is tested by `PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath`.

**PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException**: `SafePathCombine("/home/user/project", "../etc/passwd")` is called. Expected outcome: `ArgumentException` is thrown with a message containing "Invalid path component". Boundary or error path: Path traversal via leading `..` segment. Requirement coverage: `ReviewMark-PathHelpers-TraversalRejection`. This scenario is tested by `PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException**: `SafePathCombine` is called with a relative path containing `..` embedded in the middle (e.g. `"subfolder/../../../etc/passwd"`). Expected outcome: `ArgumentException` is thrown with a message containing "Invalid path component". Boundary or error path: Path traversal via embedded `..` segments. Requirement coverage: `ReviewMark-PathHelpers-TraversalRejection`. This scenario is tested by `PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_AbsoluteUnixPath_ThrowsArgumentException**: `SafePathCombine` is called where the relative path is a Unix absolute path (`/etc/passwd`). Runs on all platforms. Expected outcome: `ArgumentException` is thrown with a message containing "Invalid path component". Boundary or error path: Unix absolute path injection. Requirement coverage: `ReviewMark-PathHelpers-TraversalRejection`. This scenario is tested by `PathHelpers_SafePathCombine_AbsoluteUnixPath_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_AbsoluteWindowsPath_ThrowsArgumentException**: `SafePathCombine` is called where the relative path is a Windows absolute path (`C:\Windows\System32\file.txt`). Runs on Windows only. Expected outcome: `ArgumentException` is thrown with a message containing "Invalid path component". Boundary or error path: Windows absolute path injection. Requirement coverage: `ReviewMark-PathHelpers-TraversalRejection`. This scenario is tested by `PathHelpers_SafePathCombine_AbsoluteWindowsPath_ThrowsArgumentException`.

**PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException**: `SafePathCombine(null, "relative")` is called. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null base path rejection. Requirement coverage: `ReviewMark-PathHelpers-NullRejection`. This scenario is tested by `PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException`.

**PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException**: `SafePathCombine("base", null)` is called. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null relative path rejection. Requirement coverage: `ReviewMark-PathHelpers-NullRejection`. This scenario is tested by `PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException`.

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
