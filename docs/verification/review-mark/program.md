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
