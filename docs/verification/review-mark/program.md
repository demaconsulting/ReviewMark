## Program

### Verification Approach

`Program` unit tests are in `ProgramTests.cs`. Each test constructs a `Context` object
with controlled arguments, redirects `Console.Out` or `Console.Error` to a `StringWriter`
for output capture, calls `Program.Run`, and then asserts on captured output and exit code.

### Dependencies

| Mock / Stub       | Reason                                                         |
| ----------------- | -------------------------------------------------------------- |
| `Context`         | Constructed with controlled arguments and output capture       |
| `StringWriter`    | Replaces `Console.Out`/`Console.Error` for assertion           |

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. Console
streams are redirected to `StringWriter` instances in-process. Temporary YAML definition
files are created as needed and removed after each test. No external services or network
access are required.

### Acceptance Criteria

All Program unit tests pass with zero failures. Every `ReviewMark-Program-*` requirement
is covered by at least one passing test scenario. Correct dispatch is verified for every
supported flag; lint error paths produce non-zero exit codes and the expected error
messages.

### Test Scenarios

#### Program_Run_WithVersionFlag_DisplaysVersionOnly

**Scenario**: `Program.Run` is called with `["--version"]`.

**Expected**: Output equals the trimmed version string; "Copyright" and "ReviewMark version"
are absent; exit code is 0.

**Requirement coverage**: `ReviewMark-Program-EntryPoint`, `ReviewMark-Program-Dispatch`

#### Program_Version_ReturnsNonEmptyString

**Scenario**: `Program.Version` property is accessed directly.

**Expected**: Returns a non-null, non-empty, non-whitespace string.

**Requirement coverage**: `ReviewMark-Program-EntryPoint`

#### Program_Run_WithHelpFlag_DisplaysUsageInformation

**Scenario**: `Program.Run` is called with `["--help"]`.

**Expected**: Output contains "Usage:", "Options:", "--version", and "--help"; exit code is 0.

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_WithValidateFlag_RunsValidation

**Scenario**: `Program.Run` is called with `["--validate"]`.

**Expected**: Output contains "Total Tests:"; exit code is 0.

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_NoArguments_DisplaysDefaultBehavior

**Scenario**: `Program.Run` is called with `[]`.

**Expected**: Output contains "ReviewMark version" and "Copyright".

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_WithHelpFlag_IncludesElaborateOption

**Scenario**: `Program.Run` is called with `["--help"]`.

**Expected**: Help text includes "--elaborate".

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_WithHelpFlag_IncludesLintOption

**Scenario**: `Program.Run` is called with `["--help"]`.

**Expected**: Help text includes "--lint".

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithElaborateFlag_OutputsElaboration

**Scenario**: `Program.Run` is called with `--definition`, `--dir`, and `--elaborate Core-Logic`.

**Expected**: Output contains "Core-Logic", "Fingerprint", and "Files"; exit code is 0.

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_WithElaborateFlag_UnknownId_ReportsError

**Scenario**: `Program.Run` is called with `--elaborate Unknown-Id` against a definition
that does not contain that ID.

**Expected**: Exit code is 1.

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_Run_WithLintFlag_ValidConfig_ReportsSuccess

**Scenario**: `Program.Run` is called with `--lint --definition <valid-file>`.

**Expected**: Exit code is 0; log file contains no error text.

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_ValidConfig_SuppressesBanner

**Scenario**: `Program.Run` is called with `--lint --definition <valid-file>`.

**Expected**: Console output is empty; exit code is 0. The banner is suppressed because
lint mode itself suppresses the application banner, not because of a `--silent` flag.

**Requirement coverage**: `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_MissingConfig_ReportsError

**Scenario**: `Program.Run` is called with `--lint --definition <nonexistent-file>`.

**Expected**: Exit code is 1; log output contains "error:" and the name of the missing file.

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_DuplicateIds_ReportsError

**Scenario**: `Program.Run` is called with `--lint --definition <file>` where the definition
contains two review sets with the same ID `Core-Logic`.

**Expected**: Exit code is 1; log output contains "error:", "duplicate ID", and "Core-Logic".

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_UnknownSourceType_ReportsError

**Scenario**: `Program.Run` is called with `--lint --definition <file>` where the definition
has `evidence-source.type: ftp`.

**Expected**: Exit code is 1; log output contains "error:", "ftp", and "not supported".

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_CorruptedYaml_ReportsError

**Scenario**: `Program.Run` is called with `--lint --definition <file>` where the definition
file contains invalid YAML syntax.

**Expected**: Exit code is 1; log output contains "error:" and the definition file name with a line number.

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError

**Scenario**: `Program.Run` is called with `--lint --definition <file>` where the definition
has no `evidence-source` block.

**Expected**: Exit code is 1; log output contains "error:", the definition file name, and "evidence-source".

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithLintFlag_MultipleErrors_ReportsAll

**Scenario**: `Program.Run` is called with `--lint --definition <file>` where the definition
is missing `evidence-source` AND has duplicate review-set IDs.

**Expected**: Exit code is 1; log output contains BOTH "evidence-source" AND "duplicate ID",
proving all errors are accumulated in a single pass.

**Requirement coverage**: `ReviewMark-Program-Dispatch`, `ReviewMark-Program-LintVerbosity`

#### Program_Run_WithDefinitionFlag_InvalidConfig_ReportsLintError

**Scenario**: `Program.Run` is called with `--definition <invalid-file> --plan <planfile>` where
the definition is missing `evidence-source`.

**Expected**: Exit code is 1; log output contains "error:" and "evidence-source".

**Requirement coverage**: `ReviewMark-Program-Dispatch`

#### Program_HandleIssues_WithEnforce_SetsExitCode1

**Scenario**: `Program.Run` is called with `--report` and an empty evidence index, which triggers
HandleIssues via the report path with enforce=true.

**Expected**: Context exit code is set to 1.

**Requirement coverage**: `ReviewMark-Program-HandleIssues-Enforce`

#### Program_HandleIssues_WithoutEnforce_EmitsWarning

**Scenario**: `Program.Run` is called with `--report` and an empty evidence index, which triggers
HandleIssues via the report path with enforce=false.

**Expected**: A warning is written to output; exit code remains 0.

**Requirement coverage**: `ReviewMark-Program-HandleIssues-Warn`

### Requirements Coverage

- **ReviewMark-Program-EntryPoint**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Version_ReturnsNonEmptyString, Program_Run_WithHelpFlag_DisplaysUsageInformation
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
