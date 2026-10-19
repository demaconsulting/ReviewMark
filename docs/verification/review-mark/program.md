## Program

### Verification Approach

Program unit verification uses `ProgramTests.cs` to call `Program.Run` with real `Context` instances, captured console streams, and temporary definition, evidence, and output fixtures. The tests verify dispatch priority, banner and help behavior, lint-mode silence, elaboration, and enforce and warning handling without mocking downstream units.

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. Console streams are redirected with `StringWriter`, temporary YAML and Markdown files are created as needed, and no external services or network access are required.

### Acceptance Criteria

- All Program unit tests pass with zero failures.
- Each `ReviewMark-Program-*` requirement is traced to at least one scenario and test method.
- Dispatch, lint behavior, elaboration, and issue handling yield the documented output and exit-code behavior for both success and failure paths.

### Test Scenarios

**Program_Run_WithVersionFlag_DisplaysVersionOnly**: `Program.Run` is called with `["--version"]`. Expected outcome: Output equals the trimmed version string; "Copyright" and "ReviewMark version" are absent; exit code is 0. Requirement coverage: `ReviewMark-Program-ParseArguments`, `ReviewMark-Program-ExecuteOperation`, `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithVersionFlag_DisplaysVersionOnly`.

**Program_Version_ReturnsNonEmptyString**: `Program.Version` property is accessed directly. Expected outcome: Returns a non-null, non-empty, non-whitespace string. Requirement coverage: `ReviewMark-Program-ParseArguments`. This scenario is tested by `Program_Version_ReturnsNonEmptyString`.

**Program_Run_WithHelpFlag_DisplaysUsageInformation**: `Program.Run` is called with `["--help"]`. Expected outcome: Output contains "Usage:", "Options:", "--version", and "--help"; exit code is 0. Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithHelpFlag_DisplaysUsageInformation`.

**Program_Run_WithValidateFlag_RunsValidation**: `Program.Run` is called with `["--validate"]`. Expected outcome: Output contains "Total Tests:"; exit code is 0. Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithValidateFlag_RunsValidation`.

**Program_Run_NoArguments_DisplaysDefaultBehavior**: `Program.Run` is called with `[]`. Expected outcome: Output contains "ReviewMark version" and "Copyright". Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_NoArguments_DisplaysDefaultBehavior`.

**Program_Run_WithHelpFlag_IncludesElaborateOption**: `Program.Run` is called with `["--help"]`. Expected outcome: Help text includes "--elaborate". Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithHelpFlag_IncludesElaborateOption`.

**Program_Run_WithHelpFlag_IncludesLintOption**: `Program.Run` is called with `["--help"]`. Expected outcome: Help text includes "--lint". Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithHelpFlag_IncludesLintOption`.

**Program_Run_WithElaborateFlag_OutputsElaboration**: `Program.Run` is called with `--definition`, `--dir`, and `--elaborate Core-Logic`. Expected outcome: Output contains "Core-Logic", "Fingerprint", and "Files"; exit code is 0. Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithElaborateFlag_OutputsElaboration`.

**Program_Run_WithElaborateFlag_UnknownId_ReportsError**: `Program.Run` is called with `--elaborate Unknown-Id` against a definition that does not contain that ID. Expected outcome: Exit code is 1. Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithElaborateFlag_UnknownId_ReportsError`.

**Program_Run_WithLintFlag_ValidConfig_ReportsSuccess**: `Program.Run` is called with `--lint --definition <valid-file>`. Expected outcome: Exit code is 0; log file contains no error text. Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_ValidConfig_ReportsSuccess`.

**Program_Run_WithLintFlag_ValidConfig_SuppressesBanner**: `Program.Run` is called with `--lint --definition <valid-file>`. Expected outcome: Console output is empty; exit code is 0. The banner is suppressed because lint mode itself suppresses the application banner, not because of a `--silent` flag. Requirement coverage: `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_ValidConfig_SuppressesBanner`.

**Program_Run_WithLintFlag_MissingConfig_ReportsError**: `Program.Run` is called with `--lint --definition <nonexistent-file>`. Expected outcome: Exit code is 1; log output contains "error:" and the name of the missing file. Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_MissingConfig_ReportsError`.

**Program_Run_WithLintFlag_DuplicateIds_ReportsError**: `Program.Run` is called with `--lint --definition <file>` where the definition contains two review sets with the same ID `Core-Logic`. Expected outcome: Exit code is 1; log output contains "error:", "duplicate ID", and "Core-Logic". Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_DuplicateIds_ReportsError`.

**Program_Run_WithLintFlag_UnknownSourceType_ReportsError**: `Program.Run` is called with `--lint --definition <file>` where the definition has `evidence-source.type: ftp`. Expected outcome: Exit code is 1; log output contains "error:", "ftp", and "not supported". Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_UnknownSourceType_ReportsError`.

**Program_Run_WithLintFlag_CorruptedYaml_ReportsError**: `Program.Run` is called with `--lint --definition <file>` where the definition file contains invalid YAML syntax. Expected outcome: Exit code is 1; log output contains "error:" and the definition file name with a line number. Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_CorruptedYaml_ReportsError`.

**Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError**: `Program.Run` is called with `--lint --definition <file>` where the definition has no `evidence-source` block. Expected outcome: Exit code is 1; log output contains "error:", the definition file name, and "evidence-source". Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError`.

**Program_Run_WithLintFlag_MultipleErrors_ReportsAll**: `Program.Run` is called with `--lint --definition <file>` where the definition is missing `evidence-source` AND has duplicate review-set IDs. Expected outcome: Exit code is 1; log output contains BOTH "evidence-source" AND "duplicate ID", proving all errors are accumulated in a single pass. Requirement coverage: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`. This scenario is tested by `Program_Run_WithLintFlag_MultipleErrors_ReportsAll`.

**Program_Run_WithDefinitionFlag_InvalidConfig_ReportsLintError**: `Program.Run` is called with `--definition <invalid-file> --plan <planfile>` where the definition is missing `evidence-source`. Expected outcome: Exit code is 1; log output contains "error:" and "evidence-source". Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithDefinitionFlag_InvalidConfig_ReportsLintError`.

**Program_HandleIssues_WithEnforce_SetsExitCode1**: `Program.Run` is called with `--report` and an empty evidence index, which triggers HandleIssues via the report path with enforce=true. Expected outcome: Context exit code is set to 1. Requirement coverage: `ReviewMark-Program-HandleIssues-Enforce`. This scenario is tested by `Program_HandleIssues_WithEnforce_SetsExitCode1`.

**Program_HandleIssues_WithoutEnforce_EmitsWarning**: `Program.Run` is called with `--report` and an empty evidence index, which triggers HandleIssues via the report path with enforce=false. Expected outcome: A warning is written to output; exit code remains 0. Requirement coverage: `ReviewMark-Program-HandleIssues-Warn`. This scenario is tested by `Program_HandleIssues_WithoutEnforce_EmitsWarning`.

**Program_Run_WithHelpFlag_IncludesAffectedOption**: `Program.Run` is called with `["--help"]`. Expected outcome: Help text includes "--affected". Requirement coverage: `ReviewMark-Program-Dispatch`. This scenario is tested by `Program_Run_WithHelpFlag_IncludesAffectedOption`.

**Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed**: `Program.Run` is called with `--definition`, `--dir`, and `--affected -` with standard input containing CRLF line endings, a leading `./` (and backslashes on Windows), a blank line, a deleted file under a review-set path, a needs-review file in no review set, and a file outside needs-review. Expected outcome: Output is exactly one `affected<TAB>Core Logic<TAB>src/Core/Gone.cs` line and one `unreviewed<TAB>src/Other/O.cs` line, with no banner; exit code is 0. Requirement coverage: `ReviewMark-Program-Affected-Records-Format`, `ReviewMark-Program-Affected-Input-Source`, `ReviewMark-Program-Affected-Input-Separators`, `ReviewMark-Program-Affected-Banner`. This scenario is tested by `Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed`.

**Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets**: `Program.Run` is called with `--affected <empty-file>`. Expected outcome: Output is exactly `No affected review-sets`; exit code is 0. Boundary or error path: empty input. Requirement coverage: `ReviewMark-Program-Affected-Input-Source`, `ReviewMark-Program-Affected-Input-Empty`, `ReviewMark-Program-Affected-NoMatch`. This scenario is tested by `Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets`.

**Program_Run_WithAffectedFlag_NulInputAndSilent_SuppressesOutput**: `Program.Run` is called with `--affected <file>` where the file is NUL-separated, once without and once with `--silent`. Expected outcome: Without `--silent` one `affected` line per file is written in sorted order; with `--silent` nothing is written; both exit 0. Requirement coverage: `ReviewMark-Program-Affected-Input-Separators`, `ReviewMark-Program-Affected-Silent`. This scenario is tested by `Program_Run_WithAffectedFlag_NulInputAndSilent_SuppressesOutput`.

**Program_Run_WithAffectedFlag_MissingDefinition_ReportsError**: `Program.Run` is called with `--definition <nonexistent-file> --affected -`. Expected outcome: Exit code is 1. Boundary or error path: missing definition. Requirement coverage: `ReviewMark-Program-Affected-Errors-MissingDefinition`. This scenario is tested by `Program_Run_WithAffectedFlag_MissingDefinition_ReportsError`.

**Program_Run_WithAffectedFlag_UnreadableInput_ThrowsInvalidOperationException**: `Program.Run` is called with `--affected <nonexistent-file>`. Expected outcome: `InvalidOperationException` is thrown (reported by `Main` as exit code 1). Boundary or error path: unreadable input. Requirement coverage: `ReviewMark-Program-Affected-Errors-ReadFailure`. This scenario is tested by `Program_Run_WithAffectedFlag_UnreadableInput_ThrowsInvalidOperationException`.

**Program_Run_WithAffectedFlag_InvalidPath_ThrowsInvalidOperationException**: `Program.Run` is called with `--affected` and a path containing a NUL character. Expected outcome: `InvalidOperationException` is thrown (reported by `Main` as exit code 1). Boundary or error path: invalid path. Requirement coverage: `ReviewMark-Program-Affected-Errors-ReadFailure`. This scenario is tested by `Program_Run_WithAffectedFlag_InvalidPath_ThrowsInvalidOperationException`.

**Program_Run_WithAffectedFlag_StdinReadFailure_ThrowsInvalidOperationException**: `Program.Run` is called with `--affected -` while standard input throws `IOException`. Expected outcome: `InvalidOperationException` whose message names `standard input` is thrown (reported by `Main` as exit code 1). Boundary or error path: stdin read failure. Requirement coverage: `ReviewMark-Program-Affected-Errors-StdinFailure`. This scenario is tested by `Program_Run_WithAffectedFlag_StdinReadFailure_ThrowsInvalidOperationException`.

**Program_Run_WithAffectedFlag_ConfigWarning_NotWrittenToOutput**: `Program.Run` is called with `--affected -` and a definition containing a whitespace-only `needs-review` entry (lint warning). Expected outcome: Output is exactly the record lines, error output is empty, exit code is 0. Requirement coverage: `ReviewMark-Program-Affected-Warnings`. This scenario is tested by `Program_Run_WithAffectedFlag_ConfigWarning_NotWrittenToOutput`.

**Program_Run_WithAffectedFlag_InvalidDefinition_ExitsOne**: `Program.Run` is called with `--affected -` and a malformed definition. Expected outcome: Exit code is 1 and no `affected` records are written. Boundary or error path: invalid definition. Requirement coverage: `ReviewMark-Program-Affected-Errors-Stderr`. This scenario is tested by `Program_Run_WithAffectedFlag_InvalidDefinition_ExitsOne`.

**Program_Run_WithAffectedFlag_SilentInvalidDefinition_WritesStderrExitsOne**: `Program.Run` is called with `--silent --affected -` and a malformed definition. Expected outcome: The error is written to standard error despite `--silent`, and exit code is 1. Boundary or error path: silent error reporting. Requirement coverage: `ReviewMark-Program-Affected-Errors-Silent`. This scenario is tested by `Program_Run_WithAffectedFlag_SilentInvalidDefinition_WritesStderrExitsOne`.

**Program_Run_WithAffectedFlag_LogFile_WritesRecords**: `Program.Run` is called with `--log <file> --affected -`. Expected outcome: The records are also written to the log file. Requirement coverage: `ReviewMark-Program-Affected-Log`. This scenario is tested by `Program_Run_WithAffectedFlag_LogFile_WritesRecords`.

**Program_Run_WithAffectedFlag_EscapesSpecialCharacters**: `Program.Run` is called with NUL-separated input containing a path with TAB and LF and a review-set ID containing a backslash. Expected outcome: Backslash, TAB, LF and CR are written as `\\`, `\t`, `\n` and `\r`. Requirement coverage: `ReviewMark-Program-Affected-Records-Escaping`. This scenario is tested by `Program_Run_WithAffectedFlag_EscapesSpecialCharacters`.

**Program_Run_WithAffectedFlag_DuplicatePaths_ListedOnce**: `Program.Run` is called with `src/Core/A.cs` listed as `a`, `./a` and `a` again. Expected outcome: A single `affected` record. Requirement coverage: `ReviewMark-Program-Affected-Records-Dedupe`. This scenario is tested by `Program_Run_WithAffectedFlag_DuplicatePaths_ListedOnce`.

**Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed**: `Program.Run` is called with a single needs-review path in no review set. Expected outcome: The sentinel line followed by one `unreviewed` record; exit code is 0. Requirement coverage: `ReviewMark-Program-Affected-Unreviewed-Records`, `ReviewMark-Program-Affected-Unreviewed-ExitZero`, `ReviewMark-Program-Affected-NoMatch`. This scenario is tested by `Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed`.

**Program_Run_WithAffectedFlag_AbsolutePathOutsideDir_Ignored**: `Program.Run` is called with an absolute path outside `--dir` and a definition with an empty `needs-review` list. Expected outcome: The path is neither affected nor unreviewed; output is `No affected review-sets`. Requirement coverage: `ReviewMark-Program-Affected-Unreviewed-OutsideDir`. This scenario is tested by `Program_Run_WithAffectedFlag_AbsolutePathOutsideDir_Ignored`.

**Program_Run_WithAffectedFlag_Help_PrintsBanner**: `Program.Run` is called with `--affected changed.txt --help`. Expected outcome: The banner and help are printed; exit code is 0. Requirement coverage: `ReviewMark-Program-Affected-Banner`. This scenario is tested by `Program_Run_WithAffectedFlag_Help_PrintsBanner`.

**Program_Run_WithAffectedFlag_Validate_PrintsBanner**: `Program.Run` is called with `--affected changed.txt --validate`. Expected outcome: The banner is printed because affected mode does not run. Requirement coverage: `ReviewMark-Program-Affected-Banner`. This scenario is tested by `Program_Run_WithAffectedFlag_Validate_PrintsBanner`.

**Program_Run_WithAffectedFlag_BackslashInPath_EscapedOrConvertedByPlatform**: `Program.Run` is called with a changed path containing a backslash. Expected outcome: On Windows the backslash is a separator; on other platforms it is preserved and escaped as `\\`. Requirement coverage: `ReviewMark-Program-Affected-Records-Escaping`. This scenario is tested by `Program_Run_WithAffectedFlag_BackslashInPath_EscapedOrConvertedByPlatform`.

**Program_Run_WithIndexFlag_ScansAndWritesIndexFile**: `Program.Run` is called with `["--index", <tempDir>]`. Expected outcome: An `index.json` file is written to the current directory; exit code is 0. Requirement coverage: `ReviewMark-Program-Index`. This scenario is tested by `Program_Run_WithIndexFlag_ScansAndWritesIndexFile`.

### Requirements Coverage

- **ReviewMark-Program-ParseArguments**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Version_ReturnsNonEmptyString
- **ReviewMark-Program-ExecuteOperation**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Run_WithHelpFlag_DisplaysUsageInformation
- **ReviewMark-Program-ExitCode**: Program_HandleIssues_WithEnforce_SetsExitCode1
- **ReviewMark-Program-Dispatch**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Run_WithHelpFlag_DisplaysUsageInformation, Program_Run_WithValidateFlag_RunsValidation,
  Program_Run_NoArguments_DisplaysDefaultBehavior, Program_Run_WithHelpFlag_IncludesElaborateOption,
  Program_Run_WithHelpFlag_IncludesLintOption, Program_Run_WithElaborateFlag_OutputsElaboration,
  Program_Run_WithHelpFlag_IncludesAffectedOption,
  Program_Run_WithElaborateFlag_UnknownId_ReportsError, Program_Run_WithLintFlag_ValidConfig_ReportsSuccess,
  Program_Run_WithLintFlag_MissingConfig_ReportsError, Program_Run_WithLintFlag_DuplicateIds_ReportsError,
  Program_Run_WithLintFlag_UnknownSourceType_ReportsError, Program_Run_WithLintFlag_CorruptedYaml_ReportsError,
  Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError, Program_Run_WithLintFlag_MultipleErrors_ReportsAll,
  Program_Run_WithDefinitionFlag_InvalidConfig_ReportsLintError
- **ReviewMark-Program-LintVerbosity**: Program_Run_WithHelpFlag_IncludesLintOption,
  Program_Run_WithLintFlag_ValidConfig_ReportsSuccess,
  Program_Run_WithLintFlag_ValidConfig_SuppressesBanner,
  Program_Run_WithLintFlag_MissingConfig_ReportsError,
  Program_Run_WithLintFlag_DuplicateIds_ReportsError,
  Program_Run_WithLintFlag_UnknownSourceType_ReportsError,
  Program_Run_WithLintFlag_CorruptedYaml_ReportsError,
  Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError,
  Program_Run_WithLintFlag_MultipleErrors_ReportsAll
- **ReviewMark-Program-HandleIssues-Enforce**: Program_HandleIssues_WithEnforce_SetsExitCode1
- **ReviewMark-Program-HandleIssues-Warn**: Program_HandleIssues_WithoutEnforce_EmitsWarning
- **ReviewMark-Program-Index**: Program_Run_WithIndexFlag_ScansAndWritesIndexFile
- **ReviewMark-Program-Affected-Input-Source**: Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed,
  Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets
- **ReviewMark-Program-Affected-Input-Separators**: Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed,
  Program_Run_WithAffectedFlag_NulInputAndSilent_SuppressesOutput
- **ReviewMark-Program-Affected-Input-Empty**: Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets
- **ReviewMark-Program-Affected-Records-Format**: Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed
- **ReviewMark-Program-Affected-Records-Escaping**: Program_Run_WithAffectedFlag_EscapesSpecialCharacters,
  Program_Run_WithAffectedFlag_BackslashInPath_EscapedOrConvertedByPlatform
- **ReviewMark-Program-Affected-Records-Dedupe**: Program_Run_WithAffectedFlag_DuplicatePaths_ListedOnce
- **ReviewMark-Program-Affected-Unreviewed-Records**: Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed
- **ReviewMark-Program-Affected-Unreviewed-OutsideDir**: Program_Run_WithAffectedFlag_AbsolutePathOutsideDir_Ignored
- **ReviewMark-Program-Affected-Unreviewed-ExitZero**: Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed
- **ReviewMark-Program-Affected-NoMatch**: Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets,
  Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed
- **ReviewMark-Program-Affected-Banner**: Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed,
  Program_Run_WithAffectedFlag_Help_PrintsBanner, Program_Run_WithAffectedFlag_Validate_PrintsBanner
- **ReviewMark-Program-Affected-Silent**: Program_Run_WithAffectedFlag_NulInputAndSilent_SuppressesOutput
- **ReviewMark-Program-Affected-Log**: Program_Run_WithAffectedFlag_LogFile_WritesRecords
- **ReviewMark-Program-Affected-Warnings**: Program_Run_WithAffectedFlag_ConfigWarning_NotWrittenToOutput,
  ReviewMarkLoadResult_ReportIssues_WarningsSuppressed_ReportsErrorsOnly
- **ReviewMark-Program-Affected-Errors-Stderr**: Program_Run_WithAffectedFlag_InvalidDefinition_ExitsOne
- **ReviewMark-Program-Affected-Errors-Silent**: Program_Run_WithAffectedFlag_SilentInvalidDefinition_WritesStderrExitsOne,
  ReviewMarkLoadResult_ReportIssues_AlwaysWriteErrors_Silent_WritesStderr
- **ReviewMark-Program-Affected-Errors-ReadFailure**: Program_Run_WithAffectedFlag_UnreadableInput_ThrowsInvalidOperationException,
  Program_Run_WithAffectedFlag_InvalidPath_ThrowsInvalidOperationException
- **ReviewMark-Program-Affected-Errors-StdinFailure**: Program_Run_WithAffectedFlag_StdinReadFailure_ThrowsInvalidOperationException
- **ReviewMark-Program-Affected-Errors-MissingDefinition**: Program_Run_WithAffectedFlag_MissingDefinition_ReportsError
