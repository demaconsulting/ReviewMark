## xUnit

### Verification Approach

ReviewMark uses `xunit.v3` 3.2.2 and `xunit.runner.visualstudio` 3.1.5 in both test projects,
`test/DemaConsulting.ReviewMark.Tests` and `test/OtsSoftwareTests`, to discover, execute, and
report automated tests. Fitness for intended use is verified by the normal project test flow:
`build.ps1` runs `dotnet test`, and the `build` matrix job of `build.yaml` runs `dotnet test`
across Windows, Linux, and macOS with TRX logging enabled through
`--logger "trx;LogFilePrefix=${{ matrix.os }}" --results-directory artifacts`. Because xUnit is
the framework responsible for finding and running these tests, successful execution of the suite
constitutes direct self-validation of the integration, and the generated TRX files provide evidence
for downstream requirements tracing. No project-specific issues have been observed in this
validated execution and reporting path.

### Test Scenarios

**xUnitTestExecution**: xUnit discovers and executes representative ReviewMark tests that exercise
argument parsing, file output, and exception reporting, demonstrating that the framework runs the
project's normal unit and integration workload correctly. This scenario is tested by
`Context_Create_NoArguments_ReturnsDefaultContext`,
`Context_Create_VersionFlag_SetsVersionTrue`, `Context_Create_HelpFlag_SetsHelpTrue`,
`Context_Create_SilentFlag_SetsSilentTrue`, `Context_Create_ValidateFlag_SetsValidateTrue`,
`Context_Create_ResultsFlag_SetsResultsFile`, `Context_Create_LogFlag_OpensLogFile`,
`Context_Create_UnknownArgument_ThrowsArgumentException`, and
`Context_Create_ShortVersionFlag_SetsVersionTrue`.

**xUnitTrxReporting**: The same executed tests are emitted as TRX results during CI runs so the
build pipeline and ReqStream can consume consistent machine-readable evidence without extra
reporting glue code. This scenario is tested by `Context_Create_NoArguments_ReturnsDefaultContext`,
`Context_Create_VersionFlag_SetsVersionTrue`, `Context_Create_HelpFlag_SetsHelpTrue`,
`Context_Create_SilentFlag_SetsSilentTrue`, `Context_Create_ValidateFlag_SetsValidateTrue`,
`Context_Create_ResultsFlag_SetsResultsFile`, `Context_Create_LogFlag_OpensLogFile`,
`Context_Create_UnknownArgument_ThrowsArgumentException`, and
`Context_Create_ShortVersionFlag_SetsVersionTrue`.

### Requirements Coverage

- **ReviewMark-OTS-xUnit-Execute**: xUnit shall execute unit tests.
  - *xUnitTestExecution*
    - `Context_Create_NoArguments_ReturnsDefaultContext`
    - `Context_Create_VersionFlag_SetsVersionTrue`
    - `Context_Create_HelpFlag_SetsHelpTrue`
    - `Context_Create_SilentFlag_SetsSilentTrue`
    - `Context_Create_ValidateFlag_SetsValidateTrue`
    - `Context_Create_ResultsFlag_SetsResultsFile`
    - `Context_Create_LogFlag_OpensLogFile`
    - `Context_Create_UnknownArgument_ThrowsArgumentException`
    - `Context_Create_ShortVersionFlag_SetsVersionTrue`
- **ReviewMark-OTS-xUnit-Report**: xUnit shall report test results in TRX format.
  - *xUnitTrxReporting*
    - `Context_Create_NoArguments_ReturnsDefaultContext`
    - `Context_Create_VersionFlag_SetsVersionTrue`
    - `Context_Create_HelpFlag_SetsHelpTrue`
    - `Context_Create_SilentFlag_SetsSilentTrue`
    - `Context_Create_ValidateFlag_SetsValidateTrue`
    - `Context_Create_ResultsFlag_SetsResultsFile`
    - `Context_Create_LogFlag_OpensLogFile`
    - `Context_Create_UnknownArgument_ThrowsArgumentException`
    - `Context_Create_ShortVersionFlag_SetsVersionTrue`
