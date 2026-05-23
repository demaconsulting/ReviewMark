## Cli

### Verification Approach

Cli subsystem verification uses `CliTests.cs` to exercise `Context.Create`, `Program.Run`, and, for invalid-argument behavior, reflection-based calls to the internal `Program.Main` entry point. The tests use real configuration loading, review plan and report generation, index creation, captured console streams, and temporary files and directories so the subsystem boundary is verified end to end rather than through mocks.

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. `StringWriter` instances capture stdout and stderr in-process, temporary YAML files and output files are created for definition-based workflows, and no external services or network access are required.

### Acceptance Criteria

- All Cli subsystem integration tests pass with zero failures.
- Each `ReviewMark-Cmd-*` requirement is traced to at least one scenario and test method.
- Normal, boundary, and error-path invocations produce the expected output, generated artifacts, and exit codes.

### Test Scenarios

**Cli_VersionFlag_FlagSupplied_OutputsVersionOnly**: CLI is invoked via `Context.Create(["--version"])` and `Program.Run`. Expected outcome: Output equals the version string only; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Version`. This scenario is tested by `Cli_VersionFlag_FlagSupplied_OutputsVersionOnly`.

**Cli_HelpFlag_FlagSupplied_OutputsUsageInformation**: CLI is invoked with `--help`. Expected outcome: Output contains "Usage:", "Options:", "--version"; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Help`. This scenario is tested by `Cli_HelpFlag_FlagSupplied_OutputsUsageInformation`.

**Cli_SilentFlag_FlagSupplied_SuppressesOutput**: CLI is invoked with `--silent`. Expected outcome: Console output is empty; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Silent`. This scenario is tested by `Cli_SilentFlag_FlagSupplied_SuppressesOutput`.

**Cli_ValidateFlag_FlagSupplied_RunsValidation**: CLI is invoked with `--validate`. Expected outcome: Output contains validation summary; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Validate`. This scenario is tested by `Cli_ValidateFlag_FlagSupplied_RunsValidation`.

**Cli_ResultsFlag_FlagSupplied_GeneratesTrxFile**: CLI is invoked with `--validate --results <file>.trx`. Expected outcome: TRX results file is created; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Results`. This scenario is tested by `Cli_ResultsFlag_FlagSupplied_GeneratesTrxFile`.

**Cli_LogFlag_FlagSupplied_WritesOutputToFile**: CLI is invoked with `--log <file>`. Expected outcome: Log file is created; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Log`. This scenario is tested by `Cli_LogFlag_FlagSupplied_WritesOutputToFile`.

**Cli_DepthFlag_FlagSupplied_SetsDefaultHeadingDepth**: CLI is invoked with `--depth 2 --plan <file>`. Expected outcome: Generated plan uses level-2 headings; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Depth`. This scenario is tested by `Cli_DepthFlag_FlagSupplied_SetsDefaultHeadingDepth`.

**Cli_DepthFlag_BelowMinimum_ThrowsArgumentException**: `Context.Create` is called with `["--depth", "0"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Depth value below minimum of 1. Requirement coverage: `ReviewMark-Cmd-Depth`. This scenario is tested by `Cli_DepthFlag_BelowMinimum_ThrowsArgumentException`.

**Cli_DepthFlag_AboveMaximum_ThrowsArgumentException**: `Context.Create` is called with `["--depth", "6"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Depth value above maximum of 5. Requirement coverage: `ReviewMark-Cmd-Depth`. This scenario is tested by `Cli_DepthFlag_AboveMaximum_ThrowsArgumentException`.

**Cli_PlanDepthFlag_BelowMinimum_ThrowsArgumentException**: `Context.Create` is called with `["--plan-depth", "0"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Plan-depth value below minimum of 1. Requirement coverage: `ReviewMark-Cmd-PlanDepth`. This scenario is tested by `Cli_PlanDepthFlag_BelowMinimum_ThrowsArgumentException`.

**Cli_PlanDepthFlag_AboveMaximum_ThrowsArgumentException**: `Context.Create` is called with `["--plan-depth", "6"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Plan-depth value above maximum of 5. Requirement coverage: `ReviewMark-Cmd-PlanDepth`. This scenario is tested by `Cli_PlanDepthFlag_AboveMaximum_ThrowsArgumentException`.

**Cli_ReportDepthFlag_BelowMinimum_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth", "0"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Report-depth value below minimum of 1. Requirement coverage: `ReviewMark-Cmd-ReportDepth`. This scenario is tested by `Cli_ReportDepthFlag_BelowMinimum_ThrowsArgumentException`.

**Cli_ReportDepthFlag_AboveMaximum_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth", "6"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Report-depth value above maximum of 5. Requirement coverage: `ReviewMark-Cmd-ReportDepth`. This scenario is tested by `Cli_ReportDepthFlag_AboveMaximum_ThrowsArgumentException`.

**Cli_ErrorOutput_UnknownArg_WritesToStderr**: CLI is invoked with `--unknown-arg-xyz`. Expected outcome: Error message appears on stderr; exit code is non-zero. Requirement coverage: `ReviewMark-Cmd-ErrorOutput`. Note: `Program.Main` is invoked via reflection because it is the only code path that catches `ArgumentException` and writes the error message to stderr. This test and `Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode` share the same `UnknownArgArray` input but test distinct observable behaviors: stderr output vs exit code. This scenario is tested by `Cli_ErrorOutput_UnknownArg_WritesToStderr`.

**Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode**: CLI is invoked with an unknown argument. Expected outcome: Exit code is non-zero. Requirement coverage: `ReviewMark-Cmd-InvalidArgs`. Note: `Program.Main` is invoked via reflection because it is the only code path that catches `ArgumentException` and returns the non-zero exit code. This test shares the same `UnknownArgArray` input as `Cli_ErrorOutput_UnknownArg_WritesToStderr` but tests the distinct observable behavior of exit code rather than stderr output. This scenario is tested by `Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode`.

**Cli_DefinitionFlag_FlagSupplied_LoadsSpecifiedFile**: CLI is invoked with `--definition <file> --plan <file>`. Expected outcome: Plan file is created using the specified definition; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Definition`. This scenario is tested by `Cli_DefinitionFlag_FlagSupplied_LoadsSpecifiedFile`.

**Cli_PlanFlag_FlagSupplied_GeneratesReviewPlan**: CLI is invoked with `--definition <file> --plan <file>`. Expected outcome: Plan file exists and contains review-set ID; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Plan`. This scenario is tested by `Cli_PlanFlag_FlagSupplied_GeneratesReviewPlan`.

**Cli_PlanDepthFlag_FlagSupplied_SetsHeadingDepth**: CLI is invoked with `--plan-depth 2` along with `--plan <file>`. Expected outcome: Plan file contains `## Review Coverage` (depth 2 heading); exit code is 0. Requirement coverage: `ReviewMark-Cmd-PlanDepth`. This scenario is tested by `Cli_PlanDepthFlag_FlagSupplied_SetsHeadingDepth`.

**Cli_ReportFlag_FlagSupplied_GeneratesReviewReport**: CLI is invoked with `--definition <file> --report <file>`. Expected outcome: Report file exists and contains review-set ID; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Report`. This scenario is tested by `Cli_ReportFlag_FlagSupplied_GeneratesReviewReport`.

**Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth**: CLI is invoked with `--report-depth 2` along with `--report <file>`. Expected outcome: Report file contains `## Review Status` (depth 2 heading); exit code is 0. Requirement coverage: `ReviewMark-Cmd-ReportDepth`. This scenario is tested by `Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth`.

**Cli_IndexFlag_FlagSupplied_CreatesIndexJson**: CLI is invoked with `--dir <tmpDir> --index <glob>` where tmpDir contains a valid config. Expected outcome: `index.json` is created in the directory; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Index`. This scenario is tested by `Cli_IndexFlag_FlagSupplied_CreatesIndexJson`.

**Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent**: CLI is invoked with `--enforce` and the evidence source is `none`. Expected outcome: Exit code is non-zero because reviews are in Missing state. Requirement coverage: `ReviewMark-Cmd-Enforce`. This scenario is tested by `Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent`.

**Cli_DirFlag_FlagSupplied_SetsWorkingDirectory**: CLI is invoked with `--dir <tmpDir>` where tmpDir contains `.reviewmark.yaml`, plus `--plan <file>`. Expected outcome: Plan is created from directory-relative config; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Dir`. This scenario is tested by `Cli_DirFlag_FlagSupplied_SetsWorkingDirectory`.

**Cli_ElaborateFlag_ValidId_OutputsElaboration**: CLI is invoked with `--elaborate <review-set-id>`. Expected outcome: Output contains the review-set ID; exit code is 0. Requirement coverage: `ReviewMark-Cmd-Elaborate`. This scenario is tested by `Cli_ElaborateFlag_ValidId_OutputsElaboration`.

**Cli_LintFlag_ValidConfig_ReportsSuccess**: CLI is invoked with `--lint` on a valid definition file. Expected outcome: No output (silence on success); exit code is 0. Requirement coverage: `ReviewMark-Cmd-LintSilence`. This scenario is tested by `Cli_LintFlag_ValidConfig_ReportsSuccess`.

**Cli_LintFlag_InvalidConfig_ReportsIssueMessages**: CLI is invoked with `--lint` on a definition file missing `evidence-source`. Expected outcome: Issue messages appear in error output; exit code is non-zero. Requirement coverage: `ReviewMark-Cmd-Lint`. This scenario is tested by `Cli_LintFlag_InvalidConfig_ReportsIssueMessages`.

**Cli_Context_NoArgs_Parsed**: Context is created with no arguments (default values). Expected outcome: All default values are set correctly (Help=false, Silent=false, etc.). Requirement coverage: `ReviewMark-Cmd-Context`. This scenario is tested by `Cli_Context_NoArgs_Parsed`.

**Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode**: A Context is created with no arguments; WriteError() is called directly. Expected: context.ExitCode is non-zero. Expected outcome: Exit code is 1. Requirement coverage: `ReviewMark-Cmd-ExitCode`. This scenario is tested by `Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode`.

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
  `Cli_PlanDepthFlag_BelowMinimum_ThrowsArgumentException`,
  `Cli_PlanDepthFlag_AboveMaximum_ThrowsArgumentException`
- **`ReviewMark-Cmd-Report`**: `Cli_ReportFlag_FlagSupplied_GeneratesReviewReport`
- **`ReviewMark-Cmd-ReportDepth`**:
  `Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth`,
  `Cli_ReportDepthFlag_BelowMinimum_ThrowsArgumentException`,
  `Cli_ReportDepthFlag_AboveMaximum_ThrowsArgumentException`
- **`ReviewMark-Cmd-Index`**: `Cli_IndexFlag_FlagSupplied_CreatesIndexJson`
- **`ReviewMark-Cmd-Enforce`**: `Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent`
- **`ReviewMark-Cmd-Dir`**: `Cli_DirFlag_FlagSupplied_SetsWorkingDirectory`
- **`ReviewMark-Cmd-Elaborate`**: `Cli_ElaborateFlag_ValidId_OutputsElaboration`
- **`ReviewMark-Cmd-Lint`**: `Cli_LintFlag_InvalidConfig_ReportsIssueMessages`
- **`ReviewMark-Cmd-LintSilence`**: `Cli_LintFlag_ValidConfig_ReportsSuccess`
