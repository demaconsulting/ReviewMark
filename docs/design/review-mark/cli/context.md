### Context

#### Purpose

`Context` is the command-line argument parser and output channel for ReviewMark. It
parses the raw `string[] args` array into a structured set of typed properties and
provides `WriteLine()` and `WriteError()` methods that optionally duplicate output to a
log file. It is created once by `Program.Main()` and passed to every processing subsystem
as both a configuration carrier and an output mechanism.

#### Data Model

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Version` | `bool` | Requests version display |
| `Help` | `bool` | Requests help display |
| `Silent` | `bool` | Suppresses console output |
| `Validate` | `bool` | Requests self-validation run |
| `Lint` | `bool` | Requests configuration linting |
| `ResultsFile` | `string?` | Path for TRX/JUnit test results output |
| `DefinitionFile` | `string?` | Path to the `.reviewmark.yaml` configuration |
| `PlanFile` | `string?` | Output path for the Review Plan document |
| `Depth` | `int` | Default heading depth for all generated documents (default: 1; valid range: 1–5) |
| `PlanDepth` | `int` | Heading depth for the Review Plan (defaults to `Depth`) |
| `ReportFile` | `string?` | Output path for the Review Report document |
| `ReportDepth` | `int` | Heading depth for the Review Report (defaults to `Depth`) |
| `IndexPaths` | `IReadOnlyList<string>` | Scan paths for evidence index (empty when `--index` not specified) |
| `WorkingDirectory` | `string?` | Base directory for resolving relative paths |
| `Enforce` | `bool` | Fail if any review-set is not Current |
| `ElaborateId` | `string?` | Review-set ID to elaborate, or null if `--elaborate` was not specified |
| `ExitCode` | `int` | Computed; 0 = success, 1 = one or more calls to `WriteError()` occurred |

The `--log <file>` argument is consumed during `Context.Create()` to open the log file
handle; the path is not retained as a public property.

The only post-construction mutable state is the internal error flag (set by `WriteError()`)
and the log file handle (released by `Dispose()`).

#### Key Methods

**`Context.Create(string[] args)`**

- *Parameters*: `string[] args` — raw command-line arguments
- *Returns*: `Context` — fully initialized instance with all properties set
- *Preconditions*: `args` is not null
- *Postconditions*: All parsed properties are immutable; log file opened if `--log` was given

Delegates to the private `ArgumentParser` inner class, which processes the argument array
sequentially via `ParseArguments()` → `ParseArgument()`. The `--result` alias is accepted
for `--results`. `PlanDepth` defaults to `Depth`; `ReportDepth` defaults to `Depth`.
Integer depth flags require positive integers in the range 1–5.

**`Context.WriteLine(string message)`**

Writes a line to stdout (unless `Silent` is set) and to the log file if one is open.

**`Context.WriteError(string message)`**

Sets the internal error flag (causing `ExitCode` to return 1), writes the message to
stderr in red (unless `Silent` is set), and writes to the log file if one is open. Once
set, the error flag is never cleared; `ExitCode` returns 1 for the remainder of the
process lifetime.

**`Context.Dispose()`**

Closes the log file handle opened by `--log`, if any. Called automatically at the end of
the `using` block in `Program.Main()`.

#### Error Handling

| Exception | Condition | Handling |
| --------- | --------- | -------- |
| `ArgumentException` | Unrecognized or malformed argument during `Context.Create()` | Propagated to `Program.Main()`, which writes the message to `Console.Error` and returns 1 |
| `InvalidOperationException` | Log file cannot be opened during `Context.Create()` | Propagated to `Program.Main()`, which writes the message to `Console.Error` and returns 1 |

`WriteError()` does not throw; it sets the internal error flag and writes to output
streams. Errors during the tool run are communicated through `ExitCode`, not exceptions.

Value arguments require a non-empty following token; missing tokens cause
`ArgumentException`. Integer arguments require a positive integer in the range 1–5;
values outside this range cause `ArgumentException`.

#### Dependencies

- No dependencies on other ReviewMark units or subsystems. `Context.Create()` only
  accesses the file system to open the optional log file.

#### Callers

- **`Program.Main()`** — creates the `Context` instance via `Context.Create(string[] args)`
  and disposes it at the end of the `using` block
- **All processing subsystems** (Configuration, Indexing, SelfTest) — receive the `Context`
  instance as a parameter for output and configuration access
