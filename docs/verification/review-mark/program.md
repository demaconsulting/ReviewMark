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

**Scenario**: `Program.Run` is called with `--lint --definition <valid-file>` and `--silent`.

**Expected**: Console output is empty; exit code is 0.

**Requirement coverage**: `ReviewMark-Program-LintVerbosity`

#### Program_HandleIssues_WithEnforce_SetsExitCode1

**Scenario**: `Program.HandleIssues` is called with enforce=true and a non-empty issue list.

**Expected**: Context exit code is set to 1.

**Requirement coverage**: `ReviewMark-Program-HandleIssues`

#### Program_HandleIssues_WithoutEnforce_EmitsWarning

**Scenario**: `Program.HandleIssues` is called with enforce=false and a non-empty issue list.

**Expected**: A warning is written to output; exit code remains 0.

**Requirement coverage**: `ReviewMark-Program-HandleIssues`

### Requirements Coverage

- **ReviewMark-Program-EntryPoint**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Version_ReturnsNonEmptyString, Program_Run_WithHelpFlag_DisplaysUsageInformation
- **ReviewMark-Program-Dispatch**: Program_Run_WithVersionFlag_DisplaysVersionOnly,
  Program_Run_WithHelpFlag_DisplaysUsageInformation, Program_Run_WithValidateFlag_RunsValidation,
  Program_Run_NoArguments_DisplaysDefaultBehavior, Program_Run_WithHelpFlag_IncludesElaborateOption,
  Program_Run_WithHelpFlag_IncludesLintOption, Program_Run_WithElaborateFlag_OutputsElaboration,
  Program_Run_WithElaborateFlag_UnknownId_ReportsError, Program_Run_WithLintFlag_ValidConfig_ReportsSuccess
- **ReviewMark-Program-LintVerbosity**: Program_Run_WithHelpFlag_IncludesLintOption,
  Program_Run_WithLintFlag_ValidConfig_ReportsSuccess,
  Program_Run_WithLintFlag_ValidConfig_SuppressesBanner
- **ReviewMark-Program-HandleIssues**: Program_HandleIssues_WithEnforce_SetsExitCode1,
  Program_HandleIssues_WithoutEnforce_EmitsWarning
