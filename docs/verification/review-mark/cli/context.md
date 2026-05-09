### Context Verification

This document describes the unit-level verification design for the `Context` unit. It
defines the test scenarios, dependency usage, and requirement coverage for `Cli/Context.cs`.

#### Verification Approach

`Context` is verified with unit tests in `ContextTests.cs`. Because `Context` depends
only on .NET base class library types (`Console`, `StreamWriter`, `Path`), no mocking or
test doubles are required. Tests call `Context.Create` with controlled argument arrays,
inspect the resulting properties and exit codes, and verify output written to captured streams.

#### Dependencies

`Context` has no dependencies on other tool units. All dependencies are real .NET BCL
types; no mocking is needed at this level.

#### Test Scenarios

##### Context_Create_NoArguments_ReturnsDefaultContext

**Scenario**: `Context.Create` is called with an empty argument array.

**Expected**: All boolean flags are false; exit code is 0.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_VersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["--version"]`.

**Expected**: `Version` property is true; `Help` is false; exit code is 0.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ShortVersionFlag_SetsVersionTrue

**Scenario**: `Context.Create` is called with `["-v"]`.

**Expected**: `Version` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_HelpFlag_SetsHelpTrue

**Scenario**: `Context.Create` is called with `["--help"]`.

**Expected**: `Help` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_SilentFlag_SetsSilentTrue

**Scenario**: `Context.Create` is called with `["--silent"]`.

**Expected**: `Silent` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_ValidateFlag_SetsValidateTrue

**Scenario**: `Context.Create` is called with `["--validate"]`.

**Expected**: `Validate` property is true.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_Create_UnknownArgument_ThrowsArgumentException

**Scenario**: `Context.Create` is called with `["--unknown"]`.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Unknown argument rejection.

**Requirement coverage**: `ReviewMark-Context-Parsing`

##### Context_WriteLine_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteLine`.

**Expected**: The message appears on standard output.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteLine_Silent_DoesNotWriteToConsole

**Scenario**: A silent `Context` calls `WriteLine`.

**Expected**: Standard output receives nothing.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteError_NotSilent_WritesToConsole

**Scenario**: A non-silent `Context` calls `WriteError`.

**Expected**: The message appears on standard error.

**Requirement coverage**: `ReviewMark-Context-Output`

##### Context_WriteError_SetsErrorExitCode

**Scenario**: A `Context` calls `WriteError`.

**Expected**: `ExitCode` is 1 after the call.

**Requirement coverage**: `ReviewMark-Context-Output`

#### Requirements Coverage

- **`ReviewMark-Context-Parsing`**: Context_Create_NoArguments_ReturnsDefaultContext,
  Context_Create_VersionFlag_SetsVersionTrue, Context_Create_ShortVersionFlag_SetsVersionTrue,
  Context_Create_HelpFlag_SetsHelpTrue, Context_Create_SilentFlag_SetsSilentTrue,
  Context_Create_ValidateFlag_SetsValidateTrue, Context_Create_UnknownArgument_ThrowsArgumentException
- **`ReviewMark-Context-Output`**: Context_WriteLine_NotSilent_WritesToConsole,
  Context_WriteLine_Silent_DoesNotWriteToConsole, Context_WriteError_NotSilent_WritesToConsole,
  Context_WriteError_SetsErrorExitCode
