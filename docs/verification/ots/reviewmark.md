## ReviewMark

**Component**: DemaConsulting.ReviewMark (this tool)
**Role**: Generates review plans, review reports, and validates review completeness.
**Acceptance approach**: Self-test and automated unit/integration test coverage.

ReviewMark verifies itself through the `--validate` command (self-test). This executes
the tool's own built-in self-test suite against its own definition and confirms the
tool is correctly installed and operating. The self-test is run as part of `build.ps1`.

Unit and integration tests in `test/` provide additional coverage of the individual
subsystems (Cli, Configuration, Indexing, SelfTest, Program).

**Requirement coverage**: `ReviewMark-OTS-ReviewMark`
