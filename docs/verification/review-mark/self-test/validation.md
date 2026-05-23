### Validation Verification

This document describes the unit-level verification design for the `Validation` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`SelfTest/Validation.cs`.

#### Verification Approach

`Validation` is verified with unit tests in `ValidationTests.cs`. Tests call
`Validation.Run(Context)` with controlled `Context` instances created via
`Context.Create` with specific argument arrays, capture console output using
`StringWriter`, and assert on exit codes, output content, and results file presence.

#### Dependencies

| Dependency                   | Reason                                                        |
| ---------------------------- | ------------------------------------------------------------- |
| `Context` (real)             | Parsing and state are exercised via the real `Context` class  |
| Captured `Console.Out`       | Allows tests to assert on human-readable output               |
| Temporary files/directories  | Results file tests need a real writable path                  |

#### Test Environment

N/A - standard test environment. Tests capture `Console.Out` via `StringWriter` and create
temporary results files in-process. The real built ReviewMark assembly must be present at
the output path for `Validation.Run` to launch its internal test suite. Tests run on any
platform supporting .NET 8, 9, or 10.

#### Acceptance Criteria

All Validation unit tests pass with zero failures. Every `ReviewMark-Validation-*`
requirement is covered by at least one passing test scenario. Unsupported results file
extensions produce a non-zero exit code and an appropriate error message. The parent
directory of a results path is created automatically when it does not already exist.

#### Test Scenarios

##### Validation_Run_NullContext_ThrowsArgumentNullException

**Scenario**: `Validation.Run` is called with a `null` context.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-Validation-Run`

##### Validation_Run_WritesValidationHeader

**Scenario**: `Validation.Run` is called with `["--validate"]`; console output is captured.

**Expected**: Output contains `DEMA Consulting ReviewMark`, `Tool Version`, and `Machine Name`.

**Requirement coverage**: `ReviewMark-Validation-Run`

##### Validation_Run_WritesSummaryWithTotalTests

**Scenario**: `Validation.Run` is called with `["--validate"]`; console output is captured.

**Expected**: Output contains `Total Tests:`, `Passed:`, and `Failed:`.

**Requirement coverage**: `ReviewMark-Validation-Run`

##### Validation_Run_AllTestsPass_ExitCodeIsZero

**Scenario**: `Validation.Run` is called with `["--validate"]` in a correctly functioning
build environment.

**Expected**: `context.ExitCode` is 0 after the run completes.

**Requirement coverage**: `ReviewMark-Validation-Run`

##### Validation_Run_WithTrxResultsFile_WritesFile

**Scenario**: `Validation.Run` is called with `["--validate", "--results", "<path>.trx"]`.

**Expected**: The TRX file is created, is non-empty, and contains the text `TestRun`.

**Requirement coverage**: `ReviewMark-Validation-ResultsFile`

##### Validation_Run_WithXmlResultsFile_WritesFile

**Scenario**: `Validation.Run` is called with `["--validate", "--results", "<path>.xml"]`.

**Expected**: The JUnit XML file is created, is non-empty, and contains the text `testsuites`.

**Requirement coverage**: `ReviewMark-Validation-ResultsFile`

##### Validation_Run_WithResultsFileInNewDirectory_CreatesDirectory

**Scenario**: `Validation.Run` is called with a results path whose parent directory does
not exist yet (e.g. `<tempDir>/output/results.trx`).

**Expected**: The parent directory is created and the results file is written successfully.

**Boundary / error path**: Parent directory creation.

**Requirement coverage**: `ReviewMark-Validation-ResultsFile`

##### Validation_Run_WithUnsupportedResultsFileExtension_WritesError

**Scenario**: `Validation.Run` is called with `["--validate", "--results", "results.csv"]`.
The `.csv` extension is not supported.

**Expected**: No results file is created; `context.ExitCode` is non-zero; error output
contains a message about the unsupported extension.

**Boundary / error path**: Unsupported results file extension.

**Requirement coverage**: `ReviewMark-Validation-ResultsFile`

#### Requirements Coverage

- **ReviewMark-Validation-Run**: Validation_Run_NullContext_ThrowsArgumentNullException,
  Validation_Run_WritesValidationHeader,
  Validation_Run_WritesSummaryWithTotalTests,
  Validation_Run_AllTestsPass_ExitCodeIsZero
- **ReviewMark-Validation-ResultsFile**: Validation_Run_WithTrxResultsFile_WritesFile,
  Validation_Run_WithXmlResultsFile_WritesFile,
  Validation_Run_WithResultsFileInNewDirectory_CreatesDirectory,
  Validation_Run_WithUnsupportedResultsFileExtension_WritesError
