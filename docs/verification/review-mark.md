# ReviewMark

## Verification Approach

ReviewMark is verified at the system level through a set of integration tests in
`IntegrationTests.cs` that exercise the full CLI pipeline by launching the ReviewMark
DLL as a subprocess via `dotnet` and asserting on exit codes and console output.
The `Runner.cs` helper captures combined stdout/stderr from the subprocess, allowing
tests to assert on both normal output and error messages.

The integration tests exercise all major system-level operations: version display,
help display, self-validation, silent mode, logging, review plan generation, review
report generation, enforce mode, index scanning, working directory override, review
set elaboration, lint mode, depth flags, results file generation, and error handling
for unknown arguments.

## Dependencies

| Mock / Stub            | Reason                                                          |
| ---------------------- | --------------------------------------------------------------- |
| Temporary YAML files   | Created in-process to provide controlled definition inputs      |
| Temporary directories  | Isolated filesystem state prevents test interference            |
| `Runner.Run`           | Runs DLL as subprocess; captures stdout/stderr for assertion    |

## Test Scenarios (System-Level)

### ReviewMark_VersionFlag_Invoked_OutputsVersion

**Scenario**: The tool is invoked with `--version`.

**Expected**: Exit code is 0; output is non-empty; output does not contain "Error" or
"Copyright".

**Requirement coverage**: `ReviewMark-System-Version`

### ReviewMark_HelpFlag_Invoked_OutputsUsageInformation

**Scenario**: The tool is invoked with `--help`.

**Expected**: Exit code is 0; output contains "Usage:", "Options:", and "--version".

**Requirement coverage**: `ReviewMark-System-Help`

### ReviewMark_ValidateFlag_Invoked_RunsValidation

**Scenario**: The tool is invoked with `--validate`.

**Expected**: Exit code is 0; output contains "Total Tests:" and "Passed:".

**Requirement coverage**: `ReviewMark-System-Validate`

### ReviewMark_ValidateFlag_WithTrxResultsPath_GeneratesTrxFile

**Scenario**: The tool is invoked with `--validate --results <file>.trx`.

**Expected**: Exit code is 0; results file is created; file contains `<TestRun` and
`</TestRun>`.

**Requirement coverage**: `ReviewMark-System-Results`

### ReviewMark_ValidateFlag_WithXmlResultsPath_GeneratesJUnitFile

**Scenario**: The tool is invoked with `--validate --results <file>.xml`.

**Expected**: Exit code is 0; results file is created; file contains `<testsuites`.

**Requirement coverage**: `ReviewMark-System-Results`

### ReviewMark_SilentFlag_Invoked_SuppressesOutput

**Scenario**: The tool is invoked with `--silent`.

**Expected**: Exit code is 0; console output is empty.

**Requirement coverage**: `ReviewMark-System-Silent`

### ReviewMark_LogFlag_Invoked_WritesOutputToFile

**Scenario**: The tool is invoked with `--log <file>`.

**Expected**: Exit code is 0; log file is created; log file contains "ReviewMark version".

**Requirement coverage**: `ReviewMark-System-Log`

### ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError

**Scenario**: The tool is invoked with `--unknown`.

**Expected**: Exit code is non-zero; output contains "Error".

**Requirement coverage**: `ReviewMark-System-InvalidArgs`

### ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan

**Scenario**: The tool is invoked with `--definition <file> --plan <planfile>` using
a temporary definition file with one review set.

**Expected**: Exit code is 0; plan file is created; plan file contains the review set ID.

**Requirement coverage**: `ReviewMark-System-ReviewPlan`, `ReviewMark-System-Definition`

### ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport

**Scenario**: The tool is invoked with `--definition <file> --report <reportfile>` using
a temporary definition file with one review set.

**Expected**: Exit code is 0; report file is created; report file contains the review set ID.

**Requirement coverage**: `ReviewMark-System-ReviewReport`, `ReviewMark-System-Definition`

### ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero

**Scenario**: The tool is invoked with `--definition <file> --report <reportfile> --enforce`
where the evidence source is `type: none`.

**Expected**: Exit code is non-zero because no reviews are current against a `none` evidence source.

**Requirement coverage**: `ReviewMark-System-Enforce`

### ReviewMark_IndexFlag_OnEmptyDirectory_CreatesIndexJson

**Scenario**: The tool is invoked with `--dir <tmpdir> --index <tmpdir>/**/*.pdf` against
an empty temporary directory.

**Expected**: Exit code is 0; `index.json` is created in the temporary directory.

**Requirement coverage**: `ReviewMark-System-IndexScan`

### ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory

**Scenario**: The tool is invoked with `--dir <tmpdir> --plan <planfile>` where `<tmpdir>`
contains a `.reviewmark.yaml` definition file.

**Expected**: Exit code is 0; plan file is created; ReviewMark resolves the definition file
relative to the overridden working directory.

**Requirement coverage**: `ReviewMark-System-WorkingDirectory`, `ReviewMark-System-ReviewPlan`

### ReviewMark_ElaborateFlag_WithValidId_OutputsElaboration

**Scenario**: The tool is invoked with `--definition <file> --elaborate Test-Review` where
the definition file defines a review set named `Test-Review`.

**Expected**: Exit code is 0; output contains `Test-Review`.

**Requirement coverage**: `ReviewMark-System-Elaborate`

### ReviewMark_DepthFlag_Invoked_SetsDefaultHeadingDepth

**Scenario**: The tool is invoked with `--definition <file> --plan <planfile> --report <reportfile> --depth 2`.

**Expected**: Exit code is 0; plan file contains `## Review Coverage`; report file contains `## Review Status`.

**Requirement coverage**: `ReviewMark-System-Depth`

### ReviewMark_DepthFlag_WithValidate_SetsValidationHeadingDepth

**Scenario**: The tool is invoked with `--validate --depth 2`.

**Expected**: Exit code is 0; output contains `## DEMA Consulting ReviewMark`.

**Requirement coverage**: `ReviewMark-System-Depth`

### ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput

**Scenario**: The tool is invoked with `--definition <file> --lint` using a valid definition file.

**Expected**: Exit code is 0; output is empty (no issues, no banner in lint mode).

**Requirement coverage**: `ReviewMark-System-LintValidation`, `ReviewMark-System-LintSilenceOnSuccess`

## Requirements Coverage

- **ReviewMark-System-Version**: ReviewMark_VersionFlag_Invoked_OutputsVersion
- **ReviewMark-System-Help**: ReviewMark_HelpFlag_Invoked_OutputsUsageInformation
- **ReviewMark-System-Validate**: ReviewMark_ValidateFlag_Invoked_RunsValidation
- **ReviewMark-System-Results**: ReviewMark_ValidateFlag_WithTrxResultsPath_GeneratesTrxFile,
  ReviewMark_ValidateFlag_WithXmlResultsPath_GeneratesJUnitFile
- **ReviewMark-System-Silent**: ReviewMark_SilentFlag_Invoked_SuppressesOutput
- **ReviewMark-System-Log**: ReviewMark_LogFlag_Invoked_WritesOutputToFile
- **ReviewMark-System-InvalidArgs**: ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError
- **ReviewMark-System-ReviewPlan**: ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan, ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory
- **ReviewMark-System-ReviewReport**: ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport
- **ReviewMark-System-Enforce**: ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero
- **ReviewMark-System-IndexScan**: ReviewMark_IndexFlag_OnEmptyDirectory_CreatesIndexJson
- **ReviewMark-System-WorkingDirectory**: ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory
- **ReviewMark-System-Elaborate**: ReviewMark_ElaborateFlag_WithValidId_OutputsElaboration
- **ReviewMark-System-Depth**: ReviewMark_DepthFlag_Invoked_SetsDefaultHeadingDepth, ReviewMark_DepthFlag_WithValidate_SetsValidationHeadingDepth
- **ReviewMark-System-LintValidation**: ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput
- **ReviewMark-System-LintSilenceOnSuccess**: ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput
- **ReviewMark-System-Definition**: ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan, ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport
