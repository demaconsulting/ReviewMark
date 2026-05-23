## DemaConsulting.TestResults

DemaConsulting.TestResults is a DEMA Consulting library that provides a language-neutral test
results object model together with serializers to MSTest TRX and JUnit XML formats. It is used by
the SelfTest subsystem to produce portable self-validation output.

### Purpose

DemaConsulting.TestResults was chosen because it provides a compact, well-typed model for
accumulating test results and a ready-made TRX serializer whose output format is accepted directly
by Azure DevOps, GitHub Actions, and the ReqStream `--enforce` pipeline. Introducing a bespoke TRX
serializer was avoided to keep the SelfTest subsystem focused on test logic rather than
XML serialization concerns.

### Features Used

- **`DemaConsulting.TestResults.TestResults`**: top-level container holding the test run name and
  a mutable list of `TestResult` records
- **`DemaConsulting.TestResults.TestResult`**: individual test-case record with `Name`,
  `Outcome`, `StartTime`, `EndTime`, and `Duration` properties
- **`DemaConsulting.TestResults.TestOutcome`**: enumeration providing `Passed` and `Failed` values
  used to record the per-test verdict
- **`DemaConsulting.TestResults.IO.TrxSerializer.Serialize(TestResults)`**: serializes the results
  object to an MSTest TRX XML string for `.trx` output files
- **`DemaConsulting.TestResults.IO.JUnitSerializer.Serialize(TestResults)`**: serializes the
  results object to a JUnit XML string for `.xml` output files

### Integration Pattern

`Validation.Run()` creates a single `TestResults` instance named `"ReviewMark Self-Validation"`.
Each test method in `Validation.cs` receives the shared `TestResults` collection.
`CreateTestResult(testName)` constructs a new `TestResult` and records the start timestamp. After
the test body executes, `FinalizeTestResult()` records either `TestOutcome.Passed` or
`TestOutcome.Failed`, computes the elapsed duration, and appends the result to the collection. When
`context.ResultsFile` is set, `WriteResultsFile()` selects `TrxSerializer` for `.trx` files or
`JUnitSerializer` for `.xml` files, serializes the collection to a string, creates the output
directory if it does not yet exist, and writes the file. Any file-write failure is reported via
`Context.WriteError` and results in exit code 1.

### Version

DemaConsulting.TestResults 1.7.0 is the required version.
