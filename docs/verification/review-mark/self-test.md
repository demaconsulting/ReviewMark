## SelfTest

### Verification Approach

The SelfTest subsystem is verified through `SelfTestTests.cs`, which exercises the
`SelfTestRunner` by running it against the built assembly's own definition and
checking that it returns a passing result.

All SelfTest tests are run sequentially (parallelisation is disabled at assembly
level) because they exercise real file system and process state.

### Dependencies

| Dependency            | Reason                                                     |
| --------------------- | ---------------------------------------------------------- |
| Built assembly output | Self-test is integration-level; requires a real build      |

### Test Scenarios

#### SelfTest_Run_BuiltAssembly_PassesAllTests

**Scenario**: `SelfTestRunner.Run` is called against the tool's own built assembly.

**Expected**: All tests pass; exit code is 0.

**Requirement coverage**: `ReviewMark-SelfTest-Run`

#### SelfTest_Run_BuiltAssembly_ReturnsNonEmptyResults

**Scenario**: `SelfTestRunner.Run` is called and completes successfully.

**Expected**: Result contains at least one test result entry.

**Requirement coverage**: `ReviewMark-SelfTest-Run`

### Requirements Coverage

- **ReviewMark-SelfTest-Run**: SelfTest_Run_BuiltAssembly_PassesAllTests,
  SelfTest_Run_BuiltAssembly_ReturnsNonEmptyResults
