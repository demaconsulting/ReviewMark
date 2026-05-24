## DemaConsulting.TestResults

### Verification Approach

ReviewMark uses DemaConsulting.TestResults 1.7.0, referenced from
`DemaConsulting.ReviewMark.csproj`, to serialize self-validation output in the SelfTest subsystem.
The integration surface is `Validation.WriteResultsFile()`, which selects
`TrxSerializer.Serialize(testResults)` for `.trx` paths and `JUnitSerializer.Serialize(testResults)`
for `.xml` paths after `Validation.Run()` has accumulated results with `CreateTestResult()` and
`FinalizeTestResult()`. Fitness for intended use is verified by dedicated OTS tests in
`test/OtsSoftwareTests/DemaConsultingTestResultsTests.cs`, self-validation integration tests in
`test/DemaConsulting.ReviewMark.Tests/SelfTest/ValidationTests.cs`, and the `dotnet test` step in
the `build` matrix job of `build.yaml`, which publishes TRX evidence to `artifacts/`. No
project-specific issues have been observed in this validated serialization path.

### Test Scenarios

**TestResultsTrxSerialization**: A completed self-validation run can be serialized to MSTest TRX so
CI systems and downstream compliance tooling can consume the output without custom adapters. This
scenario is tested by `TrxSerializer_Serialize_CompletedTestRun_ContainsTestRunElement` and
`Validation_Run_WithTrxResultsFile_WritesFile`.

**TestResultsJUnitSerialization**: The same completed self-validation run can also be serialized to
JUnit XML when a `.xml` results path is requested, preserving portability across CI environments.
This scenario is tested by `JUnitSerializer_Serialize_CompletedTestRun_ContainsTestSuitesElement`
and `Validation_Run_WithXmlResultsFile_WritesFile`.

### Requirements Coverage

- **ReviewMark-OTS-TestResults-TrxSerialize**: DemaConsulting.TestResults shall serialize test run
  results to MSTest TRX format.
  - *TestResultsTrxSerialization*
    - `TrxSerializer_Serialize_CompletedTestRun_ContainsTestRunElement`
    - `Validation_Run_WithTrxResultsFile_WritesFile`
- **ReviewMark-OTS-TestResults-JUnitSerialize**: DemaConsulting.TestResults shall serialize test
  run results to JUnit XML format.
  - *TestResultsJUnitSerialization*
    - `JUnitSerializer_Serialize_CompletedTestRun_ContainsTestSuitesElement`
    - `Validation_Run_WithXmlResultsFile_WritesFile`
