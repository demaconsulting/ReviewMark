## Cli

### Verification Approach

The Cli subsystem is verified through `CliTests.cs`, which exercises the `Context`
class and `Program.Run` together with controlled argument arrays and output capture.
Each test targets a specific flag or argument combination and validates the correct
end-to-end behavior including parsing, dispatching, output, and exit code.

### Dependencies

| Mock / Stub     | Reason                                                              |
| --------------- | ------------------------------------------------------------------- |
| `StringWriter`  | Captures context output for assertion without console side effects  |
| Temporary files | Provide controlled configuration inputs for plan/report operations  |

### Test Scenarios

#### Cli_VersionFlag_OutputsVersionOnly

**Scenario**: CLI is invoked via `Context.Create(["--version"])` and `Program.Run`.

**Expected**: Output equals the version string only; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Version`

#### Cli_HelpFlag_OutputsUsageInformation

**Scenario**: CLI is invoked with `--help`.

**Expected**: Output contains "Usage:", "Options:", "--version"; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Help`

#### Cli_SilentFlag_SuppressesOutput

**Scenario**: CLI is invoked with `--silent`.

**Expected**: Console output is empty; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Silent`

#### Cli_ValidateFlag_RunsValidation

**Scenario**: CLI is invoked with `--validate`.

**Expected**: Output contains validation summary; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Validate`

#### Cli_ResultsFlag_GeneratesTrxFile

**Scenario**: CLI is invoked with `--validate --results <file>.trx`.

**Expected**: TRX results file is created; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Results`

#### Cli_LogFlag_WritesOutputToFile

**Scenario**: CLI is invoked with `--log <file>`.

**Expected**: Log file is created; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Log`

#### Cli_DepthFlag_SetsDefaultHeadingDepth

**Scenario**: CLI is invoked with `--depth 2 --plan <file>`.

**Expected**: Generated plan uses level-2 headings; exit code is 0.

**Requirement coverage**: `ReviewMark-Cmd-Depth`

#### Cli_ErrorOutput_UnknownArg_WritesToStderr

**Scenario**: CLI is invoked with `--unknown-arg-xyz`.

**Expected**: Error message appears on stderr; exit code is non-zero.

**Requirement coverage**: `ReviewMark-Cmd-ErrorOutput`

#### Cli_InvalidArgs_ReturnsNonZeroExitCode

**Scenario**: CLI is invoked with an unknown argument.

**Expected**: Exit code is non-zero.

**Requirement coverage**: `ReviewMark-Cmd-InvalidArgs`

#### Cli_ExitCode_ReturnsNonZeroOnError

**Scenario**: CLI is invoked with an invalid argument.

**Expected**: Exit code is 1.

**Requirement coverage**: `ReviewMark-Cmd-ExitCode`

### Requirements Coverage

- **ReviewMark-Cmd-Version**: Cli_VersionFlag_OutputsVersionOnly
- **ReviewMark-Cmd-Help**: Cli_HelpFlag_OutputsUsageInformation
- **ReviewMark-Cmd-Silent**: Cli_SilentFlag_SuppressesOutput
- **ReviewMark-Cmd-Validate**: Cli_ValidateFlag_RunsValidation
- **ReviewMark-Cmd-Results**: Cli_ResultsFlag_GeneratesTrxFile
- **ReviewMark-Cmd-Log**: Cli_LogFlag_WritesOutputToFile
- **ReviewMark-Cmd-Depth**: Cli_DepthFlag_SetsDefaultHeadingDepth
- **ReviewMark-Cmd-ErrorOutput**: Cli_ErrorOutput_UnknownArg_WritesToStderr
- **ReviewMark-Cmd-InvalidArgs**: Cli_InvalidArgs_ReturnsNonZeroExitCode
- **ReviewMark-Cmd-ExitCode**: Cli_ExitCode_ReturnsNonZeroOnError
