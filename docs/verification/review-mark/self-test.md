## SelfTest

### Verification Strategy

The SelfTest subsystem is verified through `SelfTestTests.cs`, which exercises the
`Validation` class by running it against the built assembly's own definition and
checking that it returns a passing result, generates results files in both TRX and
JUnit XML formats, and sets a non-zero exit code when an error occurs.

All SelfTest tests are run sequentially (parallelization is disabled at assembly
level) because they exercise real file system and process state.

### Dependencies

| Dependency            | Reason                                                     |
| --------------------- | ---------------------------------------------------------- |
| Built assembly output | Self-test is integration-level; requires a real build      |

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. Because
self-tests exercise real file-system and process state, test parallelization is disabled
at the assembly level. Temporary directories are created in-process for results file paths.
The real built ReviewMark assembly must be present for integration-level self-test
execution.

### Acceptance Criteria

All SelfTest subsystem tests pass with zero failures. Every `ReviewMark-SelfTest-*`
requirement is covered by at least one passing test scenario. Unsupported results file
extensions produce a non-zero exit code and the test runner must be available at the
assembly output path.

### Test Scenarios

#### SelfTest_Run_AllTestsPass_ExitCodeIsZero

**Scenario**: `Validation.Run` is called with `--validate`; all built-in validation
tests pass in the correctly functioning environment.

**Expected**: Exit code is 0; console output contains `Total Tests:`, `Passed:`, and `Failed:`.

**Requirement coverage**: `ReviewMark-SelfTest-Qualification`, `ReviewMark-SelfTest-ConsoleSummary`

#### SelfTest_Run_WithTrxResultsFile_WritesFile

**Scenario**: `Validation.Run` is called with `--validate --results <path>.trx`; the
specified TRX results file path does not exist before the run.

**Expected**: The file is created; its root XML element is `TestRun` (TRX format).

**Requirement coverage**: `ReviewMark-SelfTest-ResultsOutput-Trx`

#### SelfTest_Run_WithJUnitResultsFile_WritesFile

**Scenario**: `Validation.Run` is called with `--validate --results <path>.xml`; the
specified JUnit XML results file path does not exist before the run.

**Expected**: The file is created; its content contains `testsuites` (JUnit format).

**Requirement coverage**: `ReviewMark-SelfTest-ResultsOutput-Junit`

#### SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero

**Scenario**: `Validation.Run` is called with `--validate --results unsupported-format.csv`;
the `.csv` extension is not a supported results format.

**Expected**: Exit code is non-zero; the unsupported extension triggers a `WriteError` call
via the same exit-code path used for test failures.

**Boundary / error path**: Unsupported results file extension.

**Requirement coverage**: `ReviewMark-SelfTest-ExitCodeOnFailure`

### Requirements Coverage

- **ReviewMark-SelfTest-Qualification**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
- **ReviewMark-SelfTest-ResultsOutput-Trx**: SelfTest_Run_WithTrxResultsFile_WritesFile
- **ReviewMark-SelfTest-ResultsOutput-Junit**: SelfTest_Run_WithJUnitResultsFile_WritesFile
- **ReviewMark-SelfTest-ExitCodeOnFailure**: SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero
- **ReviewMark-SelfTest-ConsoleSummary**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
