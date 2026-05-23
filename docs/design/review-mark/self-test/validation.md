### Validation

#### Purpose

The `Validation` software unit implements the self-validation framework for
ReviewMark. Self-validation allows the tool to verify its own correct operation
in a target environment, which is a requirement for regulated deployment contexts
where the tool itself is part of a qualified software chain.

#### Data Model

N/A — static utility class with no instance state.

#### Key Methods

##### Validation.Run()

`Validation.Run(Context)` orchestrates all self-validation tests. It:

1. Validates that `context` is not null
2. Prints a validation header to the console via `Context.WriteLine()`
3. Executes each test case in sequence, writing per-test results inline
4. Writes a summary table to the console
5. Writes results to the configured output file (TRX or JUnit format) if `ResultsFile` is set
6. Calls `Context.WriteError()` when any test fails, which causes `Context.ExitCode` to return a non-zero value

##### Test Output Format

Results are written using the `DemaConsulting.TestResults` library, which supports
both TRX (Visual Studio Test Results) and JUnit XML output formats. The output format
is inferred from the file extension of `ResultsFile`.

##### Test Coverage

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
- **Depth flag**: Tool respects the `--depth` flag, adjusting heading depth in generated documents

##### Console Output

In addition to the structured results file, `Validation.Run()` writes a human-readable
summary to the console. The summary includes a table of all tests with their pass/fail
status, followed by detailed output for any failing tests to aid diagnosis.

#### Error Handling

- If `ResultsFile` has an unsupported file extension, `WriteError` is called and no results
  file is written; the validation run continues, but the process is still considered failed
  because the logged error causes a non-zero exit code.
- I/O exceptions when writing the results file are caught, logged via `WriteError`, and the
  run continues, but the process is still considered failed because the logged error causes
  a non-zero exit code.

#### Interactions

**Called by:**

- `Program.Run()` — calls `Validation.Run(Context)` when the `--validate` flag is set

**Dependencies:**

- `Context` (Cli subsystem) — used for all output and to communicate failure via
  `WriteError()`, which sets `Context.ExitCode` to a non-zero value
- `ReviewMarkConfiguration` (Configuration subsystem) — used internally to construct
  valid runtime environments for individual test cases
- `ReviewIndex` (Indexing subsystem) — used internally to construct valid runtime
  environments for test cases that exercise evidence loading and report generation
- `DemaConsulting.TestResults` (OTS) — used by `Validation.Run()` for TRX and JUnit XML
  serialization of test results when `Context.ResultsFile` is set

#### Overview

`Validation` implements the self-validation framework for ReviewMark. It executes a
built-in suite of integration tests against a temporary working directory and writes
structured results to a TRX or JUnit XML file. Self-validation allows the tool to
qualify itself for use in regulated environments where the tool is part of a qualified
software chain.

#### Interfaces

`Validation` exposes a single static method:

- **`Validation.Run(Context context)`** — executes the full self-validation suite

No return value is produced; the outcome is communicated through `Context.ExitCode`.
