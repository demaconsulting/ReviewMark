### Context

#### Verification Approach

Context unit verification uses `ContextTests.cs` to construct real contexts from controlled argument arrays and inspect parsed properties, exit-code behavior, console output, and log-file output. Dependencies remain real .NET BCL types, so no subsystem mocks are required; the tests validate the unit directly through `Context.Create`, `WriteLine`, and `WriteError`.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10, capture console streams with `StringWriter`, and create temporary log files only for the logging scenarios.

#### Acceptance Criteria

- All Context unit tests pass with zero failures.
- Each `ReviewMark-Context-*` requirement is traced to at least one scenario and test method.
- Flag parsing, value validation, output routing, and log-file error handling all produce the documented state changes and exceptions.

#### Test Scenarios

**Context_Create_NoArguments_ReturnsDefaultContext**: `Context.Create` is called with an empty argument array. Expected outcome: All boolean flags are false; exit code is 0. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_ReturnsDefaultContext`.

**Context_Create_VersionFlag_SetsVersionTrue**: `Context.Create` is called with `["--version"]`. Expected outcome: `Version` property is true; `Help` is false; exit code is 0. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_VersionFlag_SetsVersionTrue`.

**Context_Create_ShortVersionFlag_SetsVersionTrue**: `Context.Create` is called with `["-v"]`. Expected outcome: `Version` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ShortVersionFlag_SetsVersionTrue`.

**Context_Create_HelpFlag_SetsHelpTrue**: `Context.Create` is called with `["--help"]`. Expected outcome: `Help` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_HelpFlag_SetsHelpTrue`.

**Context_Create_SilentFlag_SetsSilentTrue**: `Context.Create` is called with `["--silent"]`. Expected outcome: `Silent` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_SilentFlag_SetsSilentTrue`.

**Context_Create_ValidateFlag_SetsValidateTrue**: `Context.Create` is called with `["--validate"]`. Expected outcome: `Validate` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ValidateFlag_SetsValidateTrue`.

**Context_Create_UnknownArgument_ThrowsArgumentException**: `Context.Create` is called with `["--unknown"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Unknown argument rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_UnknownArgument_ThrowsArgumentException`.

**Context_WriteLine_NotSilent_WritesToConsole**: A non-silent `Context` calls `WriteLine`. Expected outcome: The message appears on standard output. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteLine_NotSilent_WritesToConsole`.

**Context_WriteLine_Silent_DoesNotWriteToConsole**: A silent `Context` calls `WriteLine`. Expected outcome: Standard output receives nothing. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteLine_Silent_DoesNotWriteToConsole`.

**Context_WriteError_NotSilent_WritesToConsole**: A non-silent `Context` calls `WriteError`. Expected outcome: The message appears on standard error. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteError_NotSilent_WritesToConsole`.

**Context_WriteError_SetsErrorExitCode**: A `Context` calls `WriteError`. Expected outcome: `ExitCode` is 1 after the call. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteError_SetsErrorExitCode`.

**Context_Create_ShortHelpFlag_H_SetsHelpTrue**: `Context.Create` is called with `["-h"]`. Expected outcome: `Help` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ShortHelpFlag_H_SetsHelpTrue`.

**Context_Create_ShortHelpFlag_Question_SetsHelpTrue**: `Context.Create` is called with `["-?"]`. Expected outcome: `Help` property is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ShortHelpFlag_Question_SetsHelpTrue`.

**Context_Create_ResultsFlag_SetsResultsFile**: `Context.Create` is called with `["--results", "test.trx"]`. Expected outcome: `ResultsFile` is set to `"test.trx"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ResultsFlag_SetsResultsFile`.

**Context_Create_LogFlag_OpensLogFile**: `Context.Create` is called with `["--log", "<file>"]` and `WriteLine` is called. Expected outcome: Log file exists and contains the written message. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_LogFlag_OpensLogFile`.

**Context_Create_LogFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--log"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--log"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_LogFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--results"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--results"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_ResultAlias_SetsResultsFile**: `Context.Create` is called with `["--result", "test.trx"]`. Expected outcome: `ResultsFile` is set to `"test.trx"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ResultAlias_SetsResultsFile`.

**Context_Create_ResultAlias_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--result"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--result"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ResultAlias_WithoutValue_ThrowsArgumentException`.

**Context_Create_DefinitionFlag_SetsDefinitionFile**: `Context.Create` is called with `["--definition", "spec.yaml"]`. Expected outcome: `DefinitionFile` is set to `"spec.yaml"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DefinitionFlag_SetsDefinitionFile`.

**Context_Create_DefinitionFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--definition"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--definition"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DefinitionFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_PlanFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--plan"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--plan"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_ReportFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--report"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--report"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_IndexFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--index"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--index"`. Boundary or error path: Missing value rejection. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_IndexFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_PlanFlag_SetsPlanFile**: `Context.Create` is called with `["--plan", "plan.yaml"]`. Expected outcome: `PlanFile` is set to `"plan.yaml"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanFlag_SetsPlanFile`.

**Context_Create_PlanDepthFlag_SetsPlanDepth**: `Context.Create` is called with `["--plan-depth", "3"]`. Expected outcome: `PlanDepth` is 3. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanDepthFlag_SetsPlanDepth`.

**Context_Create_PlanDepthFlag_WithInvalidValue_ThrowsArgumentException**: `Context.Create` is called with `["--plan-depth", "not-a-number"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Non-numeric depth value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanDepthFlag_WithInvalidValue_ThrowsArgumentException`.

**Context_Create_PlanDepthFlag_WithZeroValue_ThrowsArgumentException**: `Context.Create` is called with `["--plan-depth", "0"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Zero depth value (must be >= 1). Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanDepthFlag_WithZeroValue_ThrowsArgumentException`.

**Context_Create_ReportFlag_SetsReportFile**: `Context.Create` is called with `["--report", "report.md"]`. Expected outcome: `ReportFile` is set to `"report.md"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportFlag_SetsReportFile`.

**Context_Create_ReportDepthFlag_SetsReportDepth**: `Context.Create` is called with `["--report-depth", "2"]`. Expected outcome: `ReportDepth` is 2. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportDepthFlag_SetsReportDepth`.

**Context_Create_ReportDepthFlag_NonNumeric_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth", "abc"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Non-numeric depth value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportDepthFlag_NonNumeric_ThrowsArgumentException`.

**Context_Create_ReportDepthFlag_Zero_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth", "0"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Zero depth value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportDepthFlag_Zero_ThrowsArgumentException`.

**Context_Create_ReportDepthFlag_MissingValue_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth"]` (no value). Expected outcome: `ArgumentException` is thrown. Boundary or error path: Missing value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportDepthFlag_MissingValue_ThrowsArgumentException`.

**Context_Create_IndexFlag_AddsIndexPath**: `Context.Create` is called with `["--index", "*.pdf"]`. Expected outcome: `IndexPaths` contains `"*.pdf"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_IndexFlag_AddsIndexPath`.

**Context_Create_IndexFlag_MultipleTimes_AddsAllPaths**: `Context.Create` is called with two `--index` flags. Expected outcome: `IndexPaths` contains both patterns. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_IndexFlag_MultipleTimes_AddsAllPaths`.

**Context_Create_NoArguments_IndexPathsEmpty**: `Context.Create` is called with no arguments. Expected outcome: `IndexPaths` is empty. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_IndexPathsEmpty`.

**Context_Create_NoArguments_PlanDepthDefaultsToOne**: `Context.Create` is called with no arguments. Expected outcome: `PlanDepth` is 1. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_PlanDepthDefaultsToOne`.

**Context_Create_NoArguments_ReportDepthDefaultsToOne**: `Context.Create` is called with no arguments. Expected outcome: `ReportDepth` is 1. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_ReportDepthDefaultsToOne`.

**Context_Create_EnforceFlag_SetsEnforceTrue**: `Context.Create` is called with `["--enforce"]`. Expected outcome: `Enforce` is true. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_EnforceFlag_SetsEnforceTrue`.

**Context_Create_NoArguments_EnforceFalse**: `Context.Create` is called with no arguments. Expected outcome: `Enforce` is false. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_EnforceFalse`.

**Context_Create_PlanDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException**: `Context.Create` is called with `["--plan-depth", "6"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Depth exceeds maximum of 5. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_PlanDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException`.

**Context_Create_ReportDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException**: `Context.Create` is called with `["--report-depth", "6"]`. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Depth exceeds maximum of 5. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ReportDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException`.

**Context_Create_DirFlag_SetsWorkingDirectory**: `Context.Create` is called with `["--dir", "/evidence"]`. Expected outcome: `WorkingDirectory` is `"/evidence"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DirFlag_SetsWorkingDirectory`.

**Context_Create_NoArguments_WorkingDirectoryIsNull**: `Context.Create` is called with no arguments. Expected outcome: `WorkingDirectory` is null. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_WorkingDirectoryIsNull`.

**Context_Create_DirFlag_MissingValue_ThrowsArgumentException**: `Context.Create` is called with `["--dir"]` (no value). Expected outcome: `ArgumentException` is thrown. Boundary or error path: Missing value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DirFlag_MissingValue_ThrowsArgumentException`.

**Context_Create_ElaborateFlag_SetsElaborateId**: `Context.Create` is called with `["--elaborate", "Core-Logic"]`. Expected outcome: `ElaborateId` is `"Core-Logic"`. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ElaborateFlag_SetsElaborateId`.

**Context_Create_NoArguments_ElaborateIdIsNull**: `Context.Create` is called with no arguments. Expected outcome: `ElaborateId` is null. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_ElaborateIdIsNull`.

**Context_Create_ElaborateFlag_WithoutValue_ThrowsArgumentException**: `Context.Create` is called with `["--elaborate"]` (no value). Expected outcome: `ArgumentException` is thrown. Boundary or error path: Missing value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_ElaborateFlag_WithoutValue_ThrowsArgumentException`.

**Context_Create_LintFlag_SetsLintTrue**: `Context.Create` is called with `["--lint"]`. Expected outcome: `Lint` is true; `Version` and `Help` are false. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_LintFlag_SetsLintTrue`.

**Context_Create_NoArguments_LintIsFalse**: `Context.Create` is called with no arguments. Expected outcome: `Lint` is false. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_NoArguments_LintIsFalse`.

**Context_Create_DepthFlag_SetsDepth**: `Context.Create` is called with `["--depth", "3"]`. Expected outcome: `Depth`, `PlanDepth`, and `ReportDepth` are all 3. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_SetsDepth`.

**Context_Create_DepthFlag_PlanDepthOverride**: `Context.Create` is called with `["--depth", "2", "--plan-depth", "4"]`. Expected outcome: `Depth` is 2, `PlanDepth` is 4, `ReportDepth` is 2. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_PlanDepthOverride`.

**Context_Create_DepthFlag_WithInvalidValue_ThrowsArgumentException**: `Context.Create` is called with `["--depth", "not-a-number"]`. Expected outcome: `ArgumentException` is thrown with message containing `"--depth"`. Boundary or error path: Non-numeric depth. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_WithInvalidValue_ThrowsArgumentException`.

**Context_Create_DepthFlag_WithZeroValue_ThrowsArgumentException**: `Context.Create` is called with `["--depth", "0"]`. Expected outcome: `ArgumentException` is thrown with message containing `"--depth"`. Boundary or error path: Zero depth. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_WithZeroValue_ThrowsArgumentException`.

**Context_Create_DepthFlag_WithValueGreaterThanFive_ThrowsArgumentException**: `Context.Create` is called with `["--depth", "6"]`. Expected outcome: `ArgumentException` is thrown with message containing `"--depth"`. Boundary or error path: Depth exceeds maximum of 5. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_WithValueGreaterThanFive_ThrowsArgumentException`.

**Context_Create_DepthFlag_MissingValue_ThrowsArgumentException**: `Context.Create` is called with `["--depth"]` (no value). Expected outcome: `ArgumentException` is thrown with message containing `"--depth"`. Boundary or error path: Missing value. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_MissingValue_ThrowsArgumentException`.

**Context_Create_DepthFlag_ReportDepthOverride**: `Context.Create` is called with `["--depth", "2", "--report-depth", "4"]`. Expected outcome: `Depth` is 2, `PlanDepth` is 2, `ReportDepth` is 4. Requirement coverage: `ReviewMark-Context-Parsing`. This scenario is tested by `Context_Create_DepthFlag_ReportDepthOverride`.

**Context_Create_LogFlag_InvalidPath_ThrowsInvalidOperationException**: `Context.Create` is called with `["--log", "<path-with-nonexistent-parent-dir>"]`. Expected outcome: `InvalidOperationException` is thrown. Boundary or error path: Log file path whose parent directory does not exist. Requirement coverage: `ReviewMark-Context-LogFileError`. This scenario is tested by `Context_Create_LogFlag_InvalidPath_ThrowsInvalidOperationException`.

**Context_WriteError_Silent_DoesNotWriteToConsole**: A silent `Context` calls `WriteError`. Expected outcome: Standard error receives nothing. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteError_Silent_DoesNotWriteToConsole`.

**Context_WriteError_WritesToLogFile**: A `Context` with `--silent --log <file>` calls `WriteError`. Expected outcome: The error message appears in the log file. Requirement coverage: `ReviewMark-Context-Output`. This scenario is tested by `Context_WriteError_WritesToLogFile`.

#### Requirements Coverage

- **`ReviewMark-Context-Parsing`**:
  Context_Create_NoArguments_ReturnsDefaultContext,
  Context_Create_VersionFlag_SetsVersionTrue,
  Context_Create_ShortVersionFlag_SetsVersionTrue,
  Context_Create_HelpFlag_SetsHelpTrue,
  Context_Create_ShortHelpFlag_H_SetsHelpTrue,
  Context_Create_ShortHelpFlag_Question_SetsHelpTrue,
  Context_Create_SilentFlag_SetsSilentTrue,
  Context_Create_ValidateFlag_SetsValidateTrue,
  Context_Create_ResultsFlag_SetsResultsFile,
  Context_Create_LogFlag_OpensLogFile,
  Context_Create_UnknownArgument_ThrowsArgumentException,
  Context_Create_LogFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_ResultAlias_SetsResultsFile,
  Context_Create_ResultAlias_WithoutValue_ThrowsArgumentException,
  Context_Create_DefinitionFlag_SetsDefinitionFile,
  Context_Create_DefinitionFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_PlanFlag_SetsPlanFile,
  Context_Create_PlanDepthFlag_SetsPlanDepth,
  Context_Create_PlanDepthFlag_WithInvalidValue_ThrowsArgumentException,
  Context_Create_PlanDepthFlag_WithZeroValue_ThrowsArgumentException,
  Context_Create_ReportFlag_SetsReportFile,
  Context_Create_ReportDepthFlag_SetsReportDepth,
  Context_Create_ReportDepthFlag_NonNumeric_ThrowsArgumentException,
  Context_Create_ReportDepthFlag_Zero_ThrowsArgumentException,
  Context_Create_ReportDepthFlag_MissingValue_ThrowsArgumentException,
  Context_Create_IndexFlag_AddsIndexPath,
  Context_Create_IndexFlag_MultipleTimes_AddsAllPaths,
  Context_Create_NoArguments_IndexPathsEmpty,
  Context_Create_NoArguments_PlanDepthDefaultsToOne,
  Context_Create_NoArguments_ReportDepthDefaultsToOne,
  Context_Create_EnforceFlag_SetsEnforceTrue,
  Context_Create_NoArguments_EnforceFalse,
  Context_Create_PlanDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException,
  Context_Create_ReportDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException,
  Context_Create_DirFlag_SetsWorkingDirectory,
  Context_Create_NoArguments_WorkingDirectoryIsNull,
  Context_Create_DirFlag_MissingValue_ThrowsArgumentException,
  Context_Create_ElaborateFlag_SetsElaborateId,
  Context_Create_NoArguments_ElaborateIdIsNull,
  Context_Create_ElaborateFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_LintFlag_SetsLintTrue,
  Context_Create_NoArguments_LintIsFalse,
  Context_Create_DepthFlag_SetsDepth,
  Context_Create_DepthFlag_PlanDepthOverride,
  Context_Create_DepthFlag_WithInvalidValue_ThrowsArgumentException,
  Context_Create_DepthFlag_WithZeroValue_ThrowsArgumentException,
  Context_Create_DepthFlag_WithValueGreaterThanFive_ThrowsArgumentException,
  Context_Create_DepthFlag_MissingValue_ThrowsArgumentException,
  Context_Create_DepthFlag_ReportDepthOverride,
  Context_Create_PlanFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_ReportFlag_WithoutValue_ThrowsArgumentException,
  Context_Create_IndexFlag_WithoutValue_ThrowsArgumentException
- **`ReviewMark-Context-LogFileError`**: Context_Create_LogFlag_InvalidPath_ThrowsInvalidOperationException
- **`ReviewMark-Context-Output`**:
  Context_WriteLine_NotSilent_WritesToConsole,
  Context_WriteLine_Silent_DoesNotWriteToConsole,
  Context_WriteError_Silent_DoesNotWriteToConsole,
  Context_WriteError_SetsErrorExitCode,
  Context_WriteError_NotSilent_WritesToConsole,
  Context_WriteError_WritesToLogFile
