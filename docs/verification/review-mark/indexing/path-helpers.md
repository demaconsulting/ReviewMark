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

**PathHelpers_ParseChangedPaths_NewlineSeparated_PreservesWhitespaceRemovesCrAndSorts**: `ParseChangedPaths` is called with LF and CRLF line endings, blank lines, a padded entry, and a duplicate. Expected outcome: One trailing CR per entry is removed, padding is preserved, and the paths are de-duplicated and sorted ordinally. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Separator`, `ReviewMark-PathHelpers-ChangedPaths-Parse-Transport`. This scenario is tested by `PathHelpers_ParseChangedPaths_NewlineSeparated_PreservesWhitespaceRemovesCrAndSorts`.

**PathHelpers_ParseChangedPaths_NulSeparated_SplitsOnNul**: `ParseChangedPaths` is called with NUL-separated entries containing a space, an embedded LF, and a trailing CR. Expected outcome: Entries are split on NUL; the space, LF and CR are preserved. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Separator`. This scenario is tested by `PathHelpers_ParseChangedPaths_NulSeparated_SplitsOnNul`.

**PathHelpers_ParseChangedPaths_EmptyInput_ReturnsEmpty**: `ParseChangedPaths` is called with empty text and text containing only line breaks. Expected outcome: An empty list. Boundary or error path: Empty input. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Separator`. This scenario is tested by `PathHelpers_ParseChangedPaths_EmptyInput_ReturnsEmpty`.

**PathHelpers_ParseChangedPaths_BomAndCrOnly_Removed**: `ParseChangedPaths` is called with a leading byte-order mark and a CR-only blank line. Expected outcome: The BOM is removed once and the blank entry dropped. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Transport`. This scenario is tested by `PathHelpers_ParseChangedPaths_BomAndCrOnly_Removed`.

**PathHelpers_ParseChangedPaths_LeadingTrailingSpaces_Preserved**: `ParseChangedPaths` is called with an entry that has leading and trailing spaces. Expected outcome: The spaces are preserved. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Whitespace`. This scenario is tested by `PathHelpers_ParseChangedPaths_LeadingTrailingSpaces_Preserved`.

**PathHelpers_ParseChangedPaths_WhitespaceOnlyEntry_Kept**: `ParseChangedPaths` is called with an entry consisting of a single space. Expected outcome: The entry is kept as a name. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-Whitespace`. This scenario is tested by `PathHelpers_ParseChangedPaths_WhitespaceOnlyEntry_Kept`.

**PathHelpers_ParseChangedPaths_DuplicateForms_Deduplicated**: `ParseChangedPaths` is called with `a.cs`, `./a.cs` and `.\a.cs`. Expected outcome: A single `a.cs` entry. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Dedupe`. This scenario is tested by `PathHelpers_ParseChangedPaths_DuplicateForms_Deduplicated`.

**PathHelpers_NormalizeChangedPath_BackslashesAndDotSlash_Normalized**: `NormalizeChangedPath` is called with backslash separators, repeated leading `./`, and a whitespace-only entry. Expected outcome: Forward slashes, no leading `./`, and whitespace left untouched (no trimming). Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Normalize-Separators`. This scenario is tested by `PathHelpers_NormalizeChangedPath_BackslashesAndDotSlash_Normalized`.

**PathHelpers_NormalizeChangedPath_AbsolutePath_MadeRelativeToDirectory**: `NormalizeChangedPath` is called with an absolute path under the directory. Expected outcome: The path becomes relative to the directory. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Normalize-Absolute`. This scenario is tested by `PathHelpers_NormalizeChangedPath_AbsolutePath_MadeRelativeToDirectory`.

**PathHelpers_NormalizeChangedPath_AbsoluteOutsideDirectory_RetainedUnchanged**: `NormalizeChangedPath` is called with an absolute path outside the directory. Expected outcome: The path is retained unchanged except for forward slashes. Boundary or error path: Path outside the directory. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Normalize-Absolute`. This scenario is tested by `PathHelpers_NormalizeChangedPath_AbsoluteOutsideDirectory_RetainedUnchanged`.

**PathHelpers_ParseChangedPaths_NullText_ThrowsArgumentNullException**: `ParseChangedPaths` is called with null text. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-NullRejected`. This scenario is tested by `PathHelpers_ParseChangedPaths_NullText_ThrowsArgumentNullException`.

**PathHelpers_ParseChangedPaths_NullDirectory_ThrowsArgumentNullException**: `ParseChangedPaths` is called with a null directory. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-NullRejected`. This scenario is tested by `PathHelpers_ParseChangedPaths_NullDirectory_ThrowsArgumentNullException`.

**PathHelpers_NormalizeChangedPath_NullPath_ThrowsArgumentNullException**: `NormalizeChangedPath` is called with a null path. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-NullRejected`. This scenario is tested by `PathHelpers_NormalizeChangedPath_NullPath_ThrowsArgumentNullException`.

**PathHelpers_NormalizeChangedPath_NullDirectory_ThrowsArgumentNullException**: `NormalizeChangedPath` is called with a null directory. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Parse-NullRejected`. This scenario is tested by `PathHelpers_NormalizeChangedPath_NullDirectory_ThrowsArgumentNullException`.

**PathHelpers_EscapeRecordField_SpecialCharacters_EscapedAndReversible**: `EscapeRecordField` is called with text containing backslash, TAB, LF and CR. Expected outcome: They become `\\`, `\t`, `\n` and `\r`, and a left-to-right unescape restores the original. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Escape`. This scenario is tested by `PathHelpers_EscapeRecordField_SpecialCharacters_EscapedAndReversible`.

**PathHelpers_EscapeRecordField_Null_ThrowsArgumentNullException**: `EscapeRecordField` is called with null. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-PathHelpers-ChangedPaths-Escape`. This scenario is tested by `PathHelpers_EscapeRecordField_Null_ThrowsArgumentNullException`.

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
- **ReviewMark-PathHelpers-ChangedPaths-Parse-Separator**:
  PathHelpers_ParseChangedPaths_NewlineSeparated_PreservesWhitespaceRemovesCrAndSorts,
  PathHelpers_ParseChangedPaths_NulSeparated_SplitsOnNul, PathHelpers_ParseChangedPaths_EmptyInput_ReturnsEmpty
- **ReviewMark-PathHelpers-ChangedPaths-Parse-Transport**:
  PathHelpers_ParseChangedPaths_BomAndCrOnly_Removed,
  PathHelpers_ParseChangedPaths_NewlineSeparated_PreservesWhitespaceRemovesCrAndSorts
- **ReviewMark-PathHelpers-ChangedPaths-Parse-Whitespace**:
  PathHelpers_ParseChangedPaths_LeadingTrailingSpaces_Preserved,
  PathHelpers_ParseChangedPaths_WhitespaceOnlyEntry_Kept
- **ReviewMark-PathHelpers-ChangedPaths-Parse-NullRejected**:
  PathHelpers_ParseChangedPaths_NullText_ThrowsArgumentNullException,
  PathHelpers_ParseChangedPaths_NullDirectory_ThrowsArgumentNullException,
  PathHelpers_NormalizeChangedPath_NullPath_ThrowsArgumentNullException,
  PathHelpers_NormalizeChangedPath_NullDirectory_ThrowsArgumentNullException
- **ReviewMark-PathHelpers-ChangedPaths-Normalize-Separators**:
  PathHelpers_NormalizeChangedPath_BackslashesAndDotSlash_Normalized
- **ReviewMark-PathHelpers-ChangedPaths-Normalize-Absolute**:
  PathHelpers_NormalizeChangedPath_AbsolutePath_MadeRelativeToDirectory,
  PathHelpers_NormalizeChangedPath_AbsoluteOutsideDirectory_RetainedUnchanged
- **ReviewMark-PathHelpers-ChangedPaths-Dedupe**: PathHelpers_ParseChangedPaths_DuplicateForms_Deduplicated
- **ReviewMark-PathHelpers-ChangedPaths-Escape**:
  PathHelpers_EscapeRecordField_SpecialCharacters_EscapedAndReversible,
  PathHelpers_EscapeRecordField_Null_ThrowsArgumentNullException
