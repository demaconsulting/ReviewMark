# Validation

## Purpose

The `Validation` software unit implements the self-validation framework for
ReviewMark. Self-validation allows the tool to verify its own correct operation
in a target environment, which is a requirement for regulated deployment contexts
where the tool itself is part of a qualified software chain.

## Validation.Run()

`Validation.Run(Context)` orchestrates all self-validation tests. It:

1. Creates a test suite using the `DemaConsulting.TestResults` library
2. Executes each test case in sequence
3. Writes results to the configured output file (TRX or JUnit format) if `ResultsFile` is set
4. Writes a summary table and per-test results to the console via `Context.WriteLine()`
5. Sets `Context.ExitCode` to a non-zero value if any test fails

## Test Output Format

Results are written using the `DemaConsulting.TestResults` library, which supports
both TRX (Visual Studio Test Results) and JUnit XML output formats. The output format
is inferred from the file extension of `ResultsFile`.

## Test Naming Convention

All test names follow the pattern `ReviewMark_MethodOrScenario`, where
`MethodOrScenario` identifies the behavior under test. This convention ensures
test results are identifiable in test dashboards and traceability matrices.

## Test Coverage

The self-validation suite covers the following scenarios:

| Test Name | Scenario |
| --------- | -------- |
| `ReviewMark_Version` | Tool correctly reports its version |
| `ReviewMark_Help` | Tool correctly displays help text |
| `ReviewMark_Plan` | Review Plan is generated correctly for a known configuration |
| `ReviewMark_Report` | Review Report is generated correctly for a known configuration |
| `ReviewMark_IndexScan` | Evidence index is created correctly by scanning a directory |
| `ReviewMark_Enforce` | Tool returns non-zero exit code when enforce mode detects uncovered sets |
| `ReviewMark_WorkingDirectory` | Working directory override resolves paths correctly |
| `ReviewMark_Elaborate` | Elaborate mode expands file lists in generated documents |
| `ReviewMark_Lint` | Lint mode detects configuration errors correctly |

## Console Output

In addition to the structured results file, `Validation.Run()` writes a human-readable
summary to the console. The summary includes a table of all tests with their pass/fail
status, followed by detailed output for any failing tests to aid diagnosis.
