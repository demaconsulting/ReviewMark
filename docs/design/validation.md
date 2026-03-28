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

## Test Coverage

The self-validation suite covers the following scenarios:

- **Version display**: Tool correctly reports its version
- **Help display**: Tool correctly displays help text
- **Plan generation**: Review Plan is generated correctly for a known configuration
- **Report generation**: Review Report is generated correctly for a known configuration
- **Index scanning**: Evidence index is created correctly by scanning a directory
- **Enforce mode**: Tool returns non-zero exit code when enforce mode detects uncovered review sets
- **Working directory override**: Relative paths are resolved correctly when the working directory is overridden
- **Elaborate mode**: File lists are expanded in generated documents when elaborate mode is active
- **Lint mode**: Configuration errors are detected correctly

## Console Output

In addition to the structured results file, `Validation.Run()` writes a human-readable
summary to the console. The summary includes a table of all tests with their pass/fail
status, followed by detailed output for any failing tests to aid diagnosis.
