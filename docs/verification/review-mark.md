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
| `Runner.Run`           | Launches DLL as subprocess; captures stdout/stderr for assertion |

## Test Scenarios (System-Level)

### IntegrationTest_VersionFlag_OutputsVersion

**Scenario**: The tool is invoked with `--version`.

**Expected**: Exit code is 0; output is non-empty; output does not contain "Error" or
"Copyright".

**Requirement coverage**: `ReviewMark-System-Version`

### IntegrationTest_HelpFlag_OutputsUsageInformation

**Scenario**: The tool is invoked with `--help`.

**Expected**: Exit code is 0; output contains "Usage:", "Options:", and "--version".

**Requirement coverage**: `ReviewMark-System-Help`

### IntegrationTest_ValidateFlag_RunsValidation

**Scenario**: The tool is invoked with `--validate`.

**Expected**: Exit code is 0; output contains "Total Tests:" and "Passed:".

**Requirement coverage**: `ReviewMark-System-Validate`

### IntegrationTest_ValidateWithResults_GeneratesTrxFile

**Scenario**: The tool is invoked with `--validate --results <file>.trx`.

**Expected**: Exit code is 0; results file is created; file contains `<TestRun` and
`</TestRun>`.

**Requirement coverage**: `ReviewMark-System-Results`

### IntegrationTest_ValidateWithResults_GeneratesJUnitFile

**Scenario**: The tool is invoked with `--validate --results <file>.xml`.

**Expected**: Exit code is 0; results file is created; file contains `<testsuites`.

**Requirement coverage**: `ReviewMark-System-Results`

### IntegrationTest_SilentFlag_SuppressesOutput

**Scenario**: The tool is invoked with `--silent`.

**Expected**: Exit code is 0; console output is empty.

**Requirement coverage**: `ReviewMark-System-Silent`

### IntegrationTest_LogFlag_WritesOutputToFile

**Scenario**: The tool is invoked with `--log <file>`.

**Expected**: Exit code is 0; log file is created; log file contains "ReviewMark version".

**Requirement coverage**: `ReviewMark-System-Log`

### IntegrationTest_UnknownArgument_ReturnsError

**Scenario**: The tool is invoked with `--unknown`.

**Expected**: Exit code is non-zero; output contains "Error".

**Requirement coverage**: `ReviewMark-System-InvalidArgs`

### IntegrationTest_ReviewPlanGeneration

**Scenario**: The tool is invoked with `--definition <file> --plan <planfile>` using
a temporary definition file with one review set.

**Expected**: Exit code is 0; plan file is created; plan file contains the review set ID.

**Requirement coverage**: `ReviewMark-System-ReviewPlan`, `ReviewMark-System-Definition`

## Requirements Coverage

- **ReviewMark-System-Version**: IntegrationTest_VersionFlag_OutputsVersion
- **ReviewMark-System-Help**: IntegrationTest_HelpFlag_OutputsUsageInformation
- **ReviewMark-System-Validate**: IntegrationTest_ValidateFlag_RunsValidation
- **ReviewMark-System-Results**: IntegrationTest_ValidateWithResults_GeneratesTrxFile,
  IntegrationTest_ValidateWithResults_GeneratesJUnitFile
- **ReviewMark-System-Silent**: IntegrationTest_SilentFlag_SuppressesOutput
- **ReviewMark-System-Log**: IntegrationTest_LogFlag_WritesOutputToFile
- **ReviewMark-System-InvalidArgs**: IntegrationTest_UnknownArgument_ReturnsError
- **ReviewMark-System-ReviewPlan**: IntegrationTest_ReviewPlanGeneration
- **ReviewMark-System-Definition**: IntegrationTest_ReviewPlanGeneration
