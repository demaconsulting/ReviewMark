## SelfTest Subsystem

### Overview

The SelfTest subsystem provides a self-validation framework that allows ReviewMark to
qualify itself as a tool for use in regulated environments. It executes a built-in suite
of integration tests against a temporary working directory and reports the results.

### Interfaces

The SelfTest subsystem exposes a single public entry point:

- **`Validation.Run(Context context)`** — executes the full self-validation suite;
  writes results to the console and, if `Context.ResultsFile` is set, to a TRX or
  JUnit XML file; calls `Context.WriteError()` if any test fails

`Validation.Run()` consumes the Configuration and Indexing subsystems internally to
construct valid runtime environments for each test case, and uses `Context` for output.
It does not expose any other types or methods to callers.

### Design

The `Validation` unit is the sole component of the SelfTest subsystem. It executes each
test case sequentially against a temporary working directory, isolating test cases from
one another and from the caller's environment.

Result output uses the `DemaConsulting.TestResults` library, which abstracts TRX and
JUnit XML serialization; `Validation` selects the format by inspecting the file extension
of `Context.ResultsFile`. Failures in test infrastructure propagate as unhandled
exceptions to `Program.Main()`, where the third-tier exception handler catches and
rethrows them. Result-file I/O failures are caught and logged via `WriteError()`, setting
a non-zero exit code without stopping the remaining tests.

### Responsibilities

- Orchestrate the execution of the built-in validation test suite
- Write test results to a TRX or JUnit XML file for ingestion by CI pipelines
- Output a human-readable summary table to the console
- Set the process exit code to reflect overall pass/fail status

### Units

| Unit       | Source File               | Purpose                                          |
|------------|---------------------------|--------------------------------------------------|
| Validation | `SelfTest/Validation.cs`  | Self-validation test runner                      |

### Entry Point

`Validation.Run(Context context)` is the single public entry point for this
subsystem. It is called by `Program.Run()` when the `--validate` flag is set.
`Validation.Run` depends on the `Configuration` and `Indexing` subsystems
(to construct a valid runtime environment for each test case) and on the `Cli`
subsystem (to report results through the context).

The method:

1. Runs each built-in test case against a temporary working directory.
2. Writes a TRX or JUnit XML results file if `--results` was specified.
3. Writes a human-readable summary table (pass count, fail count, total) to
   the console via `context.WriteLine()`.
4. Sets the context exit code to 1 if any test case fails.

### Error Handling

If test infrastructure setup fails (for example, the temporary directory cannot
be created, or a required file cannot be written), the exception propagates
out of `Validation.Run()` to `Program.Main()`, where it is caught by the
third-tier handler, written to `Console.Error`, and rethrown as an unhandled
exception.
