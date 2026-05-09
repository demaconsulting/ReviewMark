## xUnit

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

The following test methods, linked in `ReviewMark-OTS-xUnit-Execute` and
`ReviewMark-OTS-xUnit-Report`, provide evidence that xUnit discovers tests, runs them,
and reports results in TRX format. Any test passing through xUnit proves the framework
performs all three behaviours correctly.

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
- **`Context_Create_UnknownArgument_ThrowsArgumentException`** — Parsing an unrecognised
  argument raises an `ArgumentException`.
- **`Context_Create_ShortVersionFlag_SetsVersionTrue`** — Parsing `-v` (short form) sets
  the version flag to true in the context.

CI evidence source for all scenarios: `dotnet test` step in the `build` matrix job of
`build.yaml`, writing TRX result files to `artifacts/`.

**Requirement coverage**: `ReviewMark-OTS-xUnit-Execute`, `ReviewMark-OTS-xUnit-Report`
