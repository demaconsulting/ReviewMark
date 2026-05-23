## DemaConsulting.TestResults

### Verification Approach

**Component**: DemaConsulting.TestResults
(<https://github.com/demaconsulting/TestResults>)
**Role**: Test results object model and serialization library used by the SelfTest subsystem to
produce TRX and JUnit XML output from self-validation runs.
**Acceptance approach**: Automated test coverage.

The integration surface is `TrxSerializer.Serialize` and `JUnitSerializer.Serialize`, called by
`Validation.WriteResultsFile()` based on the results file extension. Both serialization paths are
exercised directly by `DemaConsulting.ReviewMark.OtsSoftwareTests`, and through ReviewMark's
higher-level behavior by `ValidationTests.cs`.

### Test Scenarios

#### TestResultsTrxSerialization

Evidence that DemaConsulting.TestResults correctly serializes a completed test run to MSTest TRX
format.

- **`TrxSerializer_Serialize_CompletedTestRun_ContainsTestRunElement`** — a `TestResults` instance
  with one passed result is serialized directly via `TrxSerializer.Serialize` and the output
  contains `<TestRun`, confirming correct OTS API behavior.
- **`Validation_Run_WithTrxResultsFile_WritesFile`** — running self-validation with a `.trx`
  results path creates a non-empty TRX file containing a `TestRun` element, confirming
  `TrxSerializer` produced valid output.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

#### TestResultsJUnitSerialization

Evidence that DemaConsulting.TestResults correctly serializes a completed test run to JUnit XML
format.

- **`JUnitSerializer_Serialize_CompletedTestRun_ContainsTestSuitesElement`** — a `TestResults`
  instance with one passed result is serialized directly via `JUnitSerializer.Serialize` and the
  output contains `testsuites`, confirming correct OTS API behavior.
- **`Validation_Run_WithXmlResultsFile_WritesFile`** — running self-validation with a `.xml`
  results path creates a non-empty JUnit XML file containing a `testsuites` element, confirming
  `JUnitSerializer` produced valid output.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

### Requirements Coverage

- **ReviewMark-OTS-TestResults-TrxSerialize**: DemaConsulting.TestResults shall serialize test run
  results to MSTest TRX format.
  - *TestResultsTrxSerialization*: verifies `TrxSerializer` produces a well-formed TRX file from a
    completed test run.
    - `TrxSerializer_Serialize_CompletedTestRun_ContainsTestRunElement`
    - `Validation_Run_WithTrxResultsFile_WritesFile`
- **ReviewMark-OTS-TestResults-JUnitSerialize**: DemaConsulting.TestResults shall serialize test
  run results to JUnit XML format.
  - *TestResultsJUnitSerialization*: verifies `JUnitSerializer` produces a well-formed JUnit XML
    file from a completed test run.
    - `JUnitSerializer_Serialize_CompletedTestRun_ContainsTestSuitesElement`
    - `Validation_Run_WithXmlResultsFile_WritesFile`
