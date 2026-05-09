### Context Verification

This document describes the unit-level verification design for the `Context` unit. It
defines the test scenarios, dependency usage, and requirement coverage for `Cli/Context.cs`.

#### Verification Approach

`Context` is verified with unit tests in `ContextTests.cs`. Because `Context` depends
only on .NET base class library types (`Console`, `StreamWriter`, `Path`), no mocking or
test doubles are required. Tests call `Context.Create` with controlled argument arrays,
inspect the resulting properties and exit codes, and verify output written to captured streams.

#### Dependencies

`Context` has no dependencies on other tool units. All dependencies are real .NET BCL
types; no mocking is needed at this level.

#### Test Scenarios

##### Context_Create_NoArguments_ReturnsDefaultContext

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: All boolean flags are false; exit code is 0.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_VersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["--version"]`.

**Expected**: `Version` property is true; `Help` is false; exit code is 0.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ShortVersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["-v"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_HelpFlag_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["--help"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_SilentFlag_SetsSilentTrue

**Scenario**: `Context.Create` is called with `["--silent"]`.

**Expected**: `Silent` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ValidateFlag_SetsValidateTrue

**Scenario**: `Context.Create` is called with `["--validate"]`.

**Expected**: `Validate` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_UnknownArgument_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--unknown"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Unknown argument rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteLine`.

**Expected**: The message appears on standard output.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteLine_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteLine`.

**Expected**: Standard output receives nothing.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteError_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteError`.

**Expected**: The message appears on standard error.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteError_SetsErrorExitCode

**Scenario**: A `Context` calls `WriteError`.

**Expected**: `ExitCode` is 1 after the call.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_Create_ShortHelpFlag_H_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-h"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ShortHelpFlag_Question_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["-?"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ResultsFlag_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--results", "test.trx"]`.

**Expected**: `ResultsFile` is set to `"test.trx"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_LogFlag_OpensLogFile

**Scenario**: `Context.Create` is called with `["--log", "<file>"]` and `WriteLine` is called.

**Expected**: Log file exists and contains the written message.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_LogFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--log"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--log"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--results"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--results"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ResultAlias_SetsResultsFile

**Scenario**: `Context.Create` is called with `["--result", "test.trx"]`.

**Expected**: `ResultsFile` is set to `"test.trx"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ResultAlias_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--result"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--result"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DefinitionFlag_SetsDefinitionFile

**Scenario**: `Context.Create` is called with `["--definition", "spec.yaml"]`.

**Expected**: `DefinitionFile` is set to `"spec.yaml"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DefinitionFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--definition"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--definition"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--plan"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--plan"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--report"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_IndexFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--index"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--index"`.

**Boundary / error path**: Missing value rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanFlag_SetsPlanFile

**Scenario**: `Context.Create` is called with `["--plan", "plan.yaml"]`.

**Expected**: `PlanFile` is set to `"plan.yaml"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanDepthFlag_SetsPlanDepth

**Scenario**: `Context.Create` is called with `["--plan-depth", "3"]`.

**Expected**: `PlanDepth` is 3.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanDepthFlag_WithInvalidValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--plan-depth", "not-a-number"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Non-numeric depth value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanDepthFlag_WithZeroValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--plan-depth", "0"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Zero depth value (must be >= 1).

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportFlag_SetsReportFile

**Scenario**: `Context.Create` is called with `["--report", "report.md"]`.

**Expected**: `ReportFile` is set to `"report.md"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportDepthFlag_SetsReportDepth

**Scenario**: `Context.Create` is called with `["--report-depth", "2"]`.

**Expected**: `ReportDepth` is 2.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportDepthFlag_NonNumeric_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report-depth", "abc"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Non-numeric depth value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportDepthFlag_Zero_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report-depth", "0"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Zero depth value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportDepthFlag_MissingValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report-depth"]` (no value).

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Missing value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_IndexFlag_AddsIndexPath

**Scenario**: `Context.Create` is called with `["--index", "*.pdf"]`.

**Expected**: `IndexPaths` contains `"*.pdf"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_IndexFlag_MultipleTimes_AddsAllPaths

**Scenario**: `Context.Create` is called with two `--index` flags.

**Expected**: `IndexPaths` contains both patterns.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_IndexPathsEmpty

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `IndexPaths` is empty.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_PlanDepthDefaultsToOne

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `PlanDepth` is 1.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_ReportDepthDefaultsToOne

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `ReportDepth` is 1.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_EnforceFlag_SetsEnforceTrue

**Scenario**: `Context.Create` is called with `["--enforce"]`.

**Expected**: `Enforce` is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_EnforceFalse

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `Enforce` is false.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_PlanDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--plan-depth", "6"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Depth exceeds maximum of 5.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ReportDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--report-depth", "6"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Depth exceeds maximum of 5.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DirFlag_SetsWorkingDirectory

**Scenario**: `Context.Create` is called with `["--dir", "/evidence"]`.

**Expected**: `WorkingDirectory` is `"/evidence"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_WorkingDirectoryIsNull

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `WorkingDirectory` is null.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DirFlag_MissingValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--dir"]` (no value).

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Missing value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ElaborateFlag_SetsElaborateId

**Scenario**: `Context.Create` is called with `["--elaborate", "Core-Logic"]`.

**Expected**: `ElaborateId` is `"Core-Logic"`.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_ElaborateIdIsNull

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `ElaborateId` is null.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ElaborateFlag_WithoutValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--elaborate"]` (no value).

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Missing value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_LintFlag_SetsLintTrue

**Scenario**: `Context.Create` is called with `["--lint"]`.

**Expected**: `Lint` is true; `Version` and `Help` are false.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_NoArguments_LintIsFalse

**Scenario**: `Context.Create` is called with no arguments.

**Expected**: `Lint` is false.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_SetsDepth

**Scenario**: `Context.Create` is called with `["--depth", "3"]`.

**Expected**: `Depth`, `PlanDepth`, and `ReportDepth` are all 3.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_PlanDepthOverride

**Scenario**: `Context.Create` is called with `["--depth", "2", "--plan-depth", "4"]`.

**Expected**: `Depth` is 2, `PlanDepth` is 4, `ReportDepth` is 2.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_WithInvalidValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "not-a-number"]`.

**Expected**: `ArgumentException` is thrown with message containing `"--depth"`.

**Boundary / error path**: Non-numeric depth.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_WithZeroValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "0"]`.

**Expected**: `ArgumentException` is thrown with message containing `"--depth"`.

**Boundary / error path**: Zero depth.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_WithValueGreaterThanFive_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth", "6"]`.

**Expected**: `ArgumentException` is thrown with message containing `"--depth"`.

**Boundary / error path**: Depth exceeds maximum of 5.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_MissingValue_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--depth"]` (no value).

**Expected**: `ArgumentException` is thrown with message containing `"--depth"`.

**Boundary / error path**: Missing value.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_DepthFlag_ReportDepthOverride

**Scenario**: `Context.Create` is called with `["--depth", "2", "--report-depth", "4"]`.

**Expected**: `Depth` is 2, `PlanDepth` is 2, `ReportDepth` is 4.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_LogFlag_InvalidPath_ThrowsInvalidOperationException

**Scenario**: `Context.Create` is called with `["--log", "<path-with-nonexistent-parent-dir>"]`.

**Expected**: `InvalidOperationException` is thrown.

**Boundary / error path**: Log file path whose parent directory does not exist.

**Requirement coverage**: `ReviewMark-Context-LogFileError`

##### Context_WriteError_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteError`.

**Expected**: Standard error receives nothing.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteError_WritesToLogFile

**Scenario**: A `Context` with `--silent --log <file>` calls `WriteError`.

**Expected**: The error message appears in the log file.

**Requirement coverage**: `ReviewMark-Context-Output`

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
