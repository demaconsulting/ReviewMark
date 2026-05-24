## SelfTest

The SelfTest subsystem provides a built-in self-validation framework that allows ReviewMark
to qualify itself as a tool for use in regulated environments.

### Overview

The SelfTest subsystem solves the problem of providing repeatable, structured evidence that
the ReviewMark tool operates correctly in a target deployment environment. Its boundary is
narrow: it executes a fixed suite of integration tests against a temporary working directory
and produces a structured results file. It consumes the Configuration and Indexing
subsystems internally to construct valid runtime environments for each test case.

The subsystem contains a single unit: **Validation** (`SelfTest/Validation.cs`) — the
self-validation test runner. See the *Validation Design* for full unit details.

### Interfaces

**`Validation.Run(Context context)`**

- *Type*: In-process .NET static method
- *Role*: Provider — called by `Program.Run()` when `--validate` is set
- *Contract*: Executes the full self-validation suite; writes a pass/fail summary to the
  console via `context.WriteLine()`; writes a TRX or JUnit XML results file when
  `context.ResultsFile` is set; calls `context.WriteError()` if any test fails, which sets
  `context.ExitCode` to 1
- *Constraints*: Throws for infrastructure failures (e.g., temporary directory creation);
  result-file write failures are caught and logged via `WriteError()` without stopping the
  remaining tests

### Design

`Validation` is the sole unit. It runs each built-in test case sequentially against an
isolated temporary working directory, ensuring test cases do not interfere with each other
or with the caller's environment.

1. `Validation.Run()` validates that `context` is not null, then executes each test case
   in sequence, writing per-test results inline.
2. Each test is timed; `DemaConsulting.TestResults` accumulates `TestResult` records with
   `TestName`, `Outcome`, `StartTime`, `EndTime`, and `Duration`.
3. After all tests complete, a human-readable summary table (pass count, fail count, total)
   is written to the console and, if `context.ResultsFile` is set, results are serialized
   to TRX (`.trx`) or JUnit XML (`.xml`) format.
4. If any test fails, `context.WriteError()` is called, setting `context.ExitCode` to 1.

The self-validation suite covers: version display, help display, plan generation, report
generation, index scanning, enforce mode, working directory override, elaborate mode, lint
mode, and the `--depth` flag.

Infrastructure failures propagate as unhandled exceptions to `Program.Main()`, where the
third-tier handler catches and rethrows them.
