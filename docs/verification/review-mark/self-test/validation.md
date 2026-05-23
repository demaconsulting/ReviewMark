### Validation

#### Verification Approach

Validation unit verification uses `ValidationTests.cs` to call `Validation.Run` with real `Context` instances, captured console and error streams, and temporary results-file paths. The tests verify the validation header, summary output, supported TRX and JUnit serialization, parent-directory creation, and error handling for unsupported results-file extensions.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10, capture console output with `StringWriter`, create temporary results paths in-process, and require the built ReviewMark assembly so the internal validation suite can execute.

#### Acceptance Criteria

- All Validation unit tests pass with zero failures.
- Each `ReviewMark-Validation-*` requirement is traced to at least one scenario and test method.
- Validation output, results-file generation, and error handling all produce the documented exit-code and artifact behavior.

#### Test Scenarios

**Validation_Run_NullContext_ThrowsArgumentNullException**: `Validation.Run` is called with a `null` context. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-Validation-Run`. This scenario is tested by `Validation_Run_NullContext_ThrowsArgumentNullException`.

**Validation_Run_WritesValidationHeader**: `Validation.Run` is called with `["--validate"]`; console output is captured. Expected outcome: Output contains `DEMA Consulting ReviewMark`, `Tool Version`, and `Machine Name`. Requirement coverage: `ReviewMark-Validation-Run`. This scenario is tested by `Validation_Run_WritesValidationHeader`.

**Validation_Run_WritesSummaryWithTotalTests**: `Validation.Run` is called with `["--validate"]`; console output is captured. Expected outcome: Output contains `Total Tests:`, `Passed:`, and `Failed:`. Requirement coverage: `ReviewMark-Validation-Run`. This scenario is tested by `Validation_Run_WritesSummaryWithTotalTests`.

**Validation_Run_AllTestsPass_ExitCodeIsZero**: `Validation.Run` is called with `["--validate"]` in a correctly functioning build environment. Expected outcome: `context.ExitCode` is 0 after the run completes. Requirement coverage: `ReviewMark-Validation-Run`. This scenario is tested by `Validation_Run_AllTestsPass_ExitCodeIsZero`.

**Validation_Run_WithTrxResultsFile_WritesFile**: `Validation.Run` is called with `["--validate", "--results", "<path>.trx"]`. Expected outcome: The TRX file is created, is non-empty, and contains the text `TestRun`. Requirement coverage: `ReviewMark-Validation-ResultsFile`. This scenario is tested by `Validation_Run_WithTrxResultsFile_WritesFile`.

**Validation_Run_WithXmlResultsFile_WritesFile**: `Validation.Run` is called with `["--validate", "--results", "<path>.xml"]`. Expected outcome: The JUnit XML file is created, is non-empty, and contains the text `testsuites`. Requirement coverage: `ReviewMark-Validation-ResultsFile`. This scenario is tested by `Validation_Run_WithXmlResultsFile_WritesFile`.

**Validation_Run_WithResultsFileInNewDirectory_CreatesDirectory**: `Validation.Run` is called with a results path whose parent directory does not exist yet (e.g. `<tempDir>/output/results.trx`). Expected outcome: The parent directory is created and the results file is written successfully. Boundary or error path: Parent directory creation. Requirement coverage: `ReviewMark-Validation-ResultsFile`. This scenario is tested by `Validation_Run_WithResultsFileInNewDirectory_CreatesDirectory`.

**Validation_Run_WithUnsupportedResultsFileExtension_WritesError**: `Validation.Run` is called with `["--validate", "--results", "results.csv"]`. The `.csv` extension is not supported. Expected outcome: No results file is created; `context.ExitCode` is non-zero; error output contains a message about the unsupported extension. Boundary or error path: Unsupported results file extension. Requirement coverage: `ReviewMark-Validation-ResultsFile`. This scenario is tested by `Validation_Run_WithUnsupportedResultsFileExtension_WritesError`.

#### Requirements Coverage

- **ReviewMark-Validation-Run**: Validation_Run_NullContext_ThrowsArgumentNullException,
  Validation_Run_WritesValidationHeader,
  Validation_Run_WritesSummaryWithTotalTests,
  Validation_Run_AllTestsPass_ExitCodeIsZero
- **ReviewMark-Validation-ResultsFile**: Validation_Run_WithTrxResultsFile_WritesFile,
  Validation_Run_WithXmlResultsFile_WritesFile,
  Validation_Run_WithResultsFileInNewDirectory_CreatesDirectory,
  Validation_Run_WithUnsupportedResultsFileExtension_WritesError
