### Validation

#### Purpose

`Validation` implements the self-validation framework for ReviewMark. It executes a
built-in suite of integration tests against a temporary working directory and writes
structured results to a TRX or JUnit XML file. Self-validation allows the tool to
verify its own correct operation in a target environment, qualifying it for use in
regulated deployment contexts where the tool is part of a qualified software chain.

#### Data Model

N/A — static utility class with no instance state.

#### Key Methods

**`Validation.Run(Context context)`**

Orchestrates all self-validation tests. No return value is produced; the outcome is
communicated through `Context.ExitCode`.

Steps:

1. Validates that `context` is not null.
2. Prints a validation header to the console via `Context.WriteLine()`.
3. Executes each test case in sequence, writing per-test results inline.
4. Writes a summary table to the console.
5. Writes structured results to the configured output file (TRX or JUnit format) if
   `Context.ResultsFile` is set; the format is inferred from the file extension.
6. Calls `Context.WriteError()` for any test failure, causing `Context.ExitCode` to
   return a non-zero value.

The test suite creates a `TestResults` object named `"ReviewMark Self-Validation"` and
covers the following 10 scenarios:

- **Version display** — tool correctly reports its version
- **Help display** — tool correctly displays help text
- **Plan generation** — Review Plan is generated correctly for a known configuration
- **Report generation** — Review Report is generated correctly for a known configuration
- **Index scanning** — evidence index is created correctly by scanning a directory
- **Enforce mode** — tool returns non-zero exit code when enforce mode detects uncovered review sets
- **Working directory override** — relative paths are resolved correctly when the working directory is overridden
- **Elaborate mode** — file lists are expanded in generated documents when elaborate mode is active
- **Lint mode** — configuration errors are detected correctly
- **Depth flag** — tool respects the `--depth` flag, adjusting heading depth in generated documents

#### Error Handling

- Unsupported `ResultsFile` extension: `WriteError` is called and no results file is
  written; the run continues but exits with a non-zero code.
- I/O exceptions when writing the results file are caught, logged via `WriteError`, and
  the run continues, but the process exits with a non-zero code.

#### Dependencies

- **`Context`** (Cli subsystem) — used for all output and to communicate failure via
  `WriteError()`, which sets `Context.ExitCode` to a non-zero value
- **`ReviewMarkConfiguration`** (Configuration subsystem) — used internally to construct
  valid runtime environments for individual test cases
- **`ReviewIndex`** (Indexing subsystem) — used internally to construct valid runtime
  environments for test cases that exercise evidence loading and report generation
- **`DemaConsulting.TestResults`** (OTS) — used for TRX and JUnit XML serialization of
  test results when `Context.ResultsFile` is set

#### Callers

- **`Program.Run()`** — calls `Validation.Run(Context)` when the `--validate` flag is set
