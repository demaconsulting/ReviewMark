## xUnit

### Verification Approach

**Component**: xunit.v3 + xunit.runner.visualstudio (<https://xunit.net/>)
**Role**: Test framework for all ReviewMark unit and integration tests.
**Acceptance approach**: Established industry use and automated test coverage.

xUnit.net v3 is a widely adopted open-source .NET testing framework with a large
active community, extensive documentation, and its own comprehensive test suite. It
is used by the .NET team and many major open-source projects.

All ReviewMark unit and integration tests are written using xUnit.net v3. The test
suite is run as part of `build.ps1` and in the `build` matrix job of `build.yaml`
(`dotnet test … --logger "trx;LogFilePrefix={os}" --results-directory artifacts`).
A successful test run confirms that xUnit discovered, executed, and reported results
for all test methods.

Because xUnit discovers and runs these tests, and produces TRX output consumed by the
requirements trace matrix, their successful completion constitutes self-validation of
the framework.

### Test scenario coverage

#### xUnitTestExecution

Evidence that xUnit discovers and executes all test methods in the ReviewMark test suite.
Any passing test confirms the framework performs discovery and execution correctly.

- **`Context_Create_NoArguments_ReturnsDefaultContext`** — Parsing an empty argument list
  returns a default-initialized context.
- **`Context_Create_VersionFlag_SetsVersionTrue`** — Parsing `--version` sets the version
  flag to true in the context.
- **`Context_Create_HelpFlag_SetsHelpTrue`** — Parsing `--help` sets the help flag to
  true in the context.
- **`Context_Create_SilentFlag_SetsSilentTrue`** — Parsing `--silent` sets the silent
  flag to true in the context.
- **`Context_Create_ValidateFlag_SetsValidateTrue`** — Parsing `--validate` sets the
  validate flag to true in the context.
- **`Context_Create_ResultsFlag_SetsResultsFile`** — Parsing `--results <file>` captures
  the results file path in the context.
- **`Context_Create_LogFlag_OpensLogFile`** — Parsing `--log <file>` opens the specified
  log file in the context.
- **`Context_Create_UnknownArgument_ThrowsArgumentException`** — Parsing an unrecognized
  argument raises an `ArgumentException`.
- **`Context_Create_ShortVersionFlag_SetsVersionTrue`** — Parsing `-v` (short form) sets
  the version flag to true in the context.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing
TRX result files to `artifacts/`.

#### xUnitTrxReporting

Evidence that xUnit produces well-formed TRX output consumed by ReqStream for requirements
traceability. The same test methods listed under `xUnitTestExecution` provide this evidence:
each `dotnet test` run writes TRX files to `artifacts/` that ReqStream subsequently
processes during a successful `--enforce` run.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing
TRX result files to `artifacts/`.

### Requirements Coverage

- **ReviewMark-OTS-xUnit-Execute**: xUnit shall execute unit tests.
  - *xUnitTestExecution*: verifies xUnit discovers and executes all test methods in the
    ReviewMark test suite, with any passing test constituting evidence of correct execution.
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
  - *xUnitTrxReporting*: verifies xUnit produces well-formed TRX output consumed by
    ReqStream for requirements traceability, confirmed by a successful `--enforce` run.
    - `Context_Create_NoArguments_ReturnsDefaultContext`
    - `Context_Create_VersionFlag_SetsVersionTrue`
    - `Context_Create_HelpFlag_SetsHelpTrue`
    - `Context_Create_SilentFlag_SetsSilentTrue`
    - `Context_Create_ValidateFlag_SetsValidateTrue`
    - `Context_Create_ResultsFlag_SetsResultsFile`
    - `Context_Create_LogFlag_OpensLogFile`
    - `Context_Create_UnknownArgument_ThrowsArgumentException`
    - `Context_Create_ShortVersionFlag_SetsVersionTrue`
