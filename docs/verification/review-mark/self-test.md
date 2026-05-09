## SelfTest

### Verification Approach

The SelfTest subsystem is verified through `SelfTestTests.cs`, which exercises the
`Validation` class by running it against the built assembly's own definition and
checking that it returns a passing result, generates results files in both TRX and
JUnit XML formats, and sets a non-zero exit code when an error occurs.

All SelfTest tests are run sequentially (parallelisation is disabled at assembly
level) because they exercise real file system and process state.

### Dependencies

| Dependency            | Reason                                                     |
| --------------------- | ---------------------------------------------------------- |
| Built assembly output | Self-test is integration-level; requires a real build      |

### Test Scenarios

#### SelfTest_Run_AllTestsPass_ExitCodeIsZero

**Scenario**: `Validation.Run` is called with `--validate`; all built-in validation
tests pass in the correctly functioning environment.

**Expected**: Exit code is 0; console output contains `Total Tests:`, `Passed:`, and `Failed:`.

**Requirement coverage**: `ReviewMark-SelfTest-Qualification`, `ReviewMark-SelfTest-ConsoleSummary`

#### SelfTest_Run_GeneratesResultsFile

**Scenario**: `Validation.Run` is called with `--validate --results <path>.trx`; the
specified TRX results file path does not exist before the run.

**Expected**: The file is created; its root XML element is `TestRun` (TRX format).

**Requirement coverage**: `ReviewMark-SelfTest-ResultsOutput`

#### SelfTest_Run_GeneratesJUnitResultsFile

**Scenario**: `Validation.Run` is called with `--validate --results <path>.xml`; the
specified JUnit XML results file path does not exist before the run.

**Expected**: The file is created; its content contains `testsuites` (JUnit format).

**Requirement coverage**: `ReviewMark-SelfTest-ResultsOutput`

#### SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero

**Scenario**: `Validation.Run` is called with `--validate --results unsupported-format.csv`;
the `.csv` extension is not a supported results format.

**Expected**: Exit code is non-zero; the unsupported extension triggers a `WriteError` call
via the same exit-code path used for test failures.

**Boundary / error path**: Unsupported results file extension.

**Requirement coverage**: `ReviewMark-SelfTest-ExitCodeOnFailure`

### Requirements Coverage

- **ReviewMark-SelfTest-Qualification**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
- **ReviewMark-SelfTest-ResultsOutput**: SelfTest_Run_GeneratesResultsFile,
  SelfTest_Run_GeneratesJUnitResultsFile
- **ReviewMark-SelfTest-ExitCodeOnFailure**: SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero
- **ReviewMark-SelfTest-ConsoleSummary**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
