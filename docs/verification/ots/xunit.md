## xUnit

**Component**: xunit.v3 (<https://xunit.net/>)
**Role**: Test framework for all ReviewMark unit and integration tests.
**Acceptance approach**: Established industry use and automated test coverage.

xUnit.net v3 is a widely adopted open-source .NET testing framework with a large
active community, extensive documentation, and its own comprehensive test suite. It
is used by the .NET team and many major open-source projects.

All ReviewMark unit and integration tests are written using xUnit.net v3. The test
suite is run as part of `build.ps1`. A successful test run confirms that xUnit is
correctly executing and reporting results.

**Requirement coverage**: `ReviewMark-OTS-xUnit`
