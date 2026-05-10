## Cli

### Verification Approach

The Cli subsystem is verified through `CliTests.cs`, which exercises the `Context`
class and `Program.Run` together with controlled argument arrays and output capture.
Each test targets a specific flag or argument combination and validates the correct
end-to-end behavior including parsing, dispatching, output, and exit code.

### Dependencies

| Mock / Stub     | Reason                                                              |
| --------------- | ------------------------------------------------------------------- |
| `StringWriter`  | Captures context output for assertion without console side effects  |
| Temporary files | Provide controlled configuration inputs for plan/report operations  |

### Test Scenarios

#### Cli_VersionFlag_FlagSupplied_OutputsVersionOnly

**Scenario**: CLI is invoked via `Context.Create(["--version"])` and `Program.Run`.

**Expected**: Output equals the version string only; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Version`

#### Cli_HelpFlag_FlagSupplied_OutputsUsageInformation

**Scenario**: CLI is invoked with `--help`.

**Expected**: Output contains "Usage:", "Options:", "--version"; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Help`

#### Cli_SilentFlag_FlagSupplied_SuppressesOutput

**Scenario**: CLI is invoked with `--silent`.

**Expected**: Console output is empty; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Silent`

#### Cli_ValidateFlag_FlagSupplied_RunsValidation

**Scenario**: CLI is invoked with `--validate`.

**Expected**: Output contains validation summary; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Validate`

#### Cli_ResultsFlag_FlagSupplied_GeneratesTrxFile

**Scenario**: CLI is invoked with `--validate --results <file>.trx`.

**Expected**: TRX results file is created; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Results`

#### Cli_LogFlag_FlagSupplied_WritesOutputToFile

**Scenario**: CLI is invoked with `--log <file>`.

**Expected**: Log file is created; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Log`

#### Cli_DepthFlag_FlagSupplied_SetsDefaultHeadingDepth

**Scenario**: CLI is invoked with `--depth 2 --plan <file>`.

**Expected**: Generated plan uses level-2 headings; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Depth`

#### Cli_DepthFlag_BelowMinimum_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "0"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Depth value below minimum of 1.

**Requirement coverage**: `ReviewMark-Cmd-Depth`, `ReviewMark-Cmd-PlanDepth`, `ReviewMark-Cmd-ReportDepth`

#### Cli_DepthFlag_AboveMaximum_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "6"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Depth value above maximum of 5.

**Requirement coverage**: `ReviewMark-Cmd-Depth`, `ReviewMark-Cmd-PlanDepth`, `ReviewMark-Cmd-ReportDepth`

#### Cli_ErrorOutput_UnknownArg_WritesToStderr

**Scenario**: CLI is invoked with `--unknown-arg-xyz`.

**Expected**: Error message appears on stderr; exit code is non-zero.

**Requirement coverage**: `ReviewMark-Cmd-ErrorOutput`

#### Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode

**Scenario**: CLI is invoked with an unknown argument.

**Expected**: Exit code is non-zero.

**Requirement coverage**: `ReviewMark-Cmd-InvalidArgs`

#### Cli_DefinitionFlag_FlagSupplied_LoadsSpecifiedFile

**Scenario**: CLI is invoked with `--definition <file> --plan <file>`.

**Expected**: Plan file is created using the specified definition; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Definition`

#### Cli_PlanFlag_FlagSupplied_GeneratesReviewPlan

**Scenario**: CLI is invoked with `--definition <file> --plan <file>`.

**Expected**: Plan file exists and contains review-set ID; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Plan`

#### Cli_PlanDepthFlag_FlagSupplied_SetsHeadingDepth

**Scenario**: CLI is invoked with `--plan-depth 2` along with `--plan <file>`.

**Expected**: Plan file contains `## Review Coverage` (depth 2 heading); exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-PlanDepth`

#### Cli_ReportFlag_FlagSupplied_GeneratesReviewReport

**Scenario**: CLI is invoked with `--definition <file> --report <file>`.

**Expected**: Report file exists and contains review-set ID; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Report`

#### Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth

**Scenario**: CLI is invoked with `--report-depth 2` along with `--report <file>`.

**Expected**: Report file contains `## Review Status` (depth 2 heading); exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-ReportDepth`

#### Cli_IndexFlag_FlagSupplied_CreatesIndexJson

**Scenario**: CLI is invoked with `--dir <tmpDir> --index <glob>` where tmpDir contains a valid config.

**Expected**: `index.json` is created in the directory; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Index`

#### Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent

**Scenario**: CLI is invoked with `--enforce` and the evidence source is `none`.

**Expected**: Exit code is non-zero because reviews are in Missing state.

**Requirement coverage**: `ReviewMark-Cmd-Enforce`

#### Cli_DirFlag_FlagSupplied_SetsWorkingDirectory

**Scenario**: CLI is invoked with `--dir <tmpDir>` where tmpDir contains `.reviewmark.yaml`, plus `--plan <file>`.

**Expected**: Plan is created from directory-relative config; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Dir`

#### Cli_ElaborateFlag_ValidId_OutputsElaboration

**Scenario**: CLI is invoked with `--elaborate <review-set-id>`.

**Expected**: Output contains the review-set ID; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Elaborate`

#### Cli_LintFlag_ValidConfig_ReportsSuccess

**Scenario**: CLI is invoked with `--lint` on a valid definition file.

**Expected**: No output (silence on success); exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Lint`

#### Cli_LintFlag_InvalidConfig_ReportsIssueMessages

**Scenario**: CLI is invoked with `--lint` on a definition file missing `evidence-source`.

**Expected**: Issue messages appear in error output; exit code is non-zero.

**Requirement coverage**: `ReviewMark-Cmd-Lint`

#### Cli_Context_NoArgs_Parsed

**Scenario**: Context is created with no arguments (default values).

**Expected**: All default values are set correctly (Help=false, Silent=false, etc.).

**Requirement coverage**: `ReviewMark-Cmd-Context`

#### Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode

**Scenario**: CLI is invoked with an invalid argument.

**Expected**: Exit code is 1.

**Requirement coverage**: `ReviewMark-Cmd-ExitCode`

### Requirements Coverage

- **`ReviewMark-Cmd-Context`**: `Cli_Context_NoArgs_Parsed`
- **`ReviewMark-Cmd-ExecutionState`**: `Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode`
- **`ReviewMark-Cmd-Version`**: `Cli_VersionFlag_FlagSupplied_OutputsVersionOnly`
- **`ReviewMark-Cmd-Help`**: `Cli_HelpFlag_FlagSupplied_OutputsUsageInformation`
- **`ReviewMark-Cmd-Silent`**: `Cli_SilentFlag_FlagSupplied_SuppressesOutput`
- **`ReviewMark-Cmd-Validate`**: `Cli_ValidateFlag_FlagSupplied_RunsValidation`
- **`ReviewMark-Cmd-Results`**: `Cli_ResultsFlag_FlagSupplied_GeneratesTrxFile`
- **`ReviewMark-Cmd-Log`**: `Cli_LogFlag_FlagSupplied_WritesOutputToFile`
- **`ReviewMark-Cmd-Depth`**:
  `Cli_DepthFlag_FlagSupplied_SetsDefaultHeadingDepth`,
  `Cli_DepthFlag_BelowMinimum_ThrowsArgumentException`,
  `Cli_DepthFlag_AboveMaximum_ThrowsArgumentException`
- **`ReviewMark-Cmd-ErrorOutput`**: `Cli_ErrorOutput_UnknownArg_WritesToStderr`
- **`ReviewMark-Cmd-InvalidArgs`**: `Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode`
- **`ReviewMark-Cmd-ExitCode`**: `Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode`
- **`ReviewMark-Cmd-Definition`**: `Cli_DefinitionFlag_FlagSupplied_LoadsSpecifiedFile`
- **`ReviewMark-Cmd-Plan`**: `Cli_PlanFlag_FlagSupplied_GeneratesReviewPlan`
- **`ReviewMark-Cmd-PlanDepth`**:
  `Cli_PlanDepthFlag_FlagSupplied_SetsHeadingDepth`,
  `Cli_DepthFlag_BelowMinimum_ThrowsArgumentException`,
  `Cli_DepthFlag_AboveMaximum_ThrowsArgumentException`
- **`ReviewMark-Cmd-Report`**: `Cli_ReportFlag_FlagSupplied_GeneratesReviewReport`
- **`ReviewMark-Cmd-ReportDepth`**:
  `Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth`,
  `Cli_DepthFlag_BelowMinimum_ThrowsArgumentException`,
  `Cli_DepthFlag_AboveMaximum_ThrowsArgumentException`
- **`ReviewMark-Cmd-Index`**: `Cli_IndexFlag_FlagSupplied_CreatesIndexJson`
- **`ReviewMark-Cmd-Enforce`**: `Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent`
- **`ReviewMark-Cmd-Dir`**: `Cli_DirFlag_FlagSupplied_SetsWorkingDirectory`
- **`ReviewMark-Cmd-Elaborate`**: `Cli_ElaborateFlag_ValidId_OutputsElaboration`
- **`ReviewMark-Cmd-Lint`**:
  `Cli_LintFlag_ValidConfig_ReportsSuccess`,
  `Cli_LintFlag_InvalidConfig_ReportsIssueMessages`
