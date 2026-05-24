## SelfTest

### Verification Approach

SelfTest subsystem verification uses `SelfTestTests.cs` to exercise `Validation.Run` through real `Context` instances, captured console output, and generated TRX and JUnit results files. The tests rely on the built ReviewMark assembly and verify the subsystem's qualification behavior, results-output formats, console summary, and exit-code handling without replacing the underlying validation runner.

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. They capture console output in-process, create temporary results paths on demand, and require the built ReviewMark assembly to be present so self-validation can execute its internal suite.

### Acceptance Criteria

- All SelfTest subsystem integration tests pass with zero failures.
- Each `ReviewMark-SelfTest-*` requirement is traced to at least one scenario and test method.
- Self-validation writes the expected console summary and supported results-file formats, and it drives a non-zero exit code when validation errors occur.

### Test Scenarios

**SelfTest_Run_AllTestsPass_ExitCodeIsZero**: `Validation.Run` is called with `--validate`; all built-in validation tests pass in the correctly functioning environment. Expected outcome: Exit code is 0; console output contains `Total Tests:`, `Passed:`, and `Failed:`. Requirement coverage: `ReviewMark-SelfTest-Qualification`, `ReviewMark-SelfTest-ConsoleSummary`. This scenario is tested by `SelfTest_Run_AllTestsPass_ExitCodeIsZero`.

**SelfTest_Run_WithTrxResultsFile_WritesFile**: `Validation.Run` is called with `--validate --results <path>.trx`; the specified TRX results file path does not exist before the run. Expected outcome: The file is created; its root XML element is `TestRun` (TRX format). Requirement coverage: `ReviewMark-SelfTest-ResultsOutput-Trx`. This scenario is tested by `SelfTest_Run_WithTrxResultsFile_WritesFile`.

**SelfTest_Run_WithJUnitResultsFile_WritesFile**: `Validation.Run` is called with `--validate --results <path>.xml`; the specified JUnit XML results file path does not exist before the run. Expected outcome: The file is created; its content contains `testsuites` (JUnit format). Requirement coverage: `ReviewMark-SelfTest-ResultsOutput-Junit`. This scenario is tested by `SelfTest_Run_WithJUnitResultsFile_WritesFile`.

**SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero**: `Validation.Run` is called with `--validate --results unsupported-format.csv`; the `.csv` extension is not a supported results format. Expected outcome: Exit code is non-zero; the unsupported extension triggers a `WriteError` call via the same exit-code path used for test failures. Boundary or error path: Unsupported results file extension. Requirement coverage: `ReviewMark-SelfTest-ExitCodeOnFailure`. This scenario is tested by `SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero`.

### Requirements Coverage

- **ReviewMark-SelfTest-Qualification**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
- **ReviewMark-SelfTest-ResultsOutput-Trx**: SelfTest_Run_WithTrxResultsFile_WritesFile
- **ReviewMark-SelfTest-ResultsOutput-Junit**: SelfTest_Run_WithJUnitResultsFile_WritesFile
- **ReviewMark-SelfTest-ExitCodeOnFailure**: SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero
- **ReviewMark-SelfTest-ConsoleSummary**: SelfTest_Run_AllTestsPass_ExitCodeIsZero
