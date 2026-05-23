### Context

#### Purpose

The `Context` software unit is responsible for parsing command-line arguments and
providing a unified interface for output and logging throughout the tool. It acts as
the primary configuration carrier, passing parsed options from the CLI entry point
to all processing subsystems.

#### Data Model

Parsed command-line state is held as typed properties on the `Context` instance. See
the Properties section for the full property inventory and types. The only
post-construction mutable state is the internal error flag (set by `WriteError()`) and
the log file handle (released by `Dispose()`). No other mutable state exists after
`Context.Create()` returns.

#### Key Methods

##### Properties

The following properties are populated by `Context.Create()` from the command-line
arguments:

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Version` | bool | Requests version display |
| `Help` | bool | Requests help display |
| `Silent` | bool | Suppresses console output |
| `Validate` | bool | Requests self-validation run |
| `Lint` | bool | Requests configuration linting |
| `ResultsFile` | string? | Path for TRX/JUnit test results output |
| `DefinitionFile` | string? | Path to the `.reviewmark.yaml` configuration |
| `PlanFile` | string? | Output path for the Review Plan document |
| `Depth` | int | Default heading depth for all generated documents (default: 1; valid range: 1–5) |
| `PlanDepth` | int | Heading depth for the Review Plan (defaults to `Depth`) |
| `ReportFile` | string? | Output path for the Review Report document |
| `ReportDepth` | int | Heading depth for the Review Report (defaults to `Depth`) |
| `IndexPaths` | IReadOnlyList&lt;string&gt; | Scan paths for evidence index (empty when `--index` not specified) |
| `WorkingDirectory` | string? | Base directory for resolving relative paths |
| `Enforce` | bool | Fail if any review-set is not Current |
| `ElaborateId` | string? | Review-set ID to elaborate, or null if `--elaborate` was not specified |
| `ExitCode` | int | Computed output property; 0 = success, 1 = error. Set via `WriteError`. |

The `--log <file>` argument is consumed during `Context.Create()` to open the log file handle; the
path is not retained as a public property after initialization.

##### Argument Parsing

`Context.Create(string[] args)` is a factory method that processes the argument
array sequentially, recognizing both flag arguments (e.g., `--validate`) and
value arguments (e.g., `--plan <path>`). Internally, it delegates to the private
`ArgumentParser` inner class, which owns the actual parsing logic via its
`ParseArgument` method. Unrecognized or unsupported arguments cause
`ArgumentParser.ParseArgument` to throw an `ArgumentException`, which propagates
through `ArgumentParser.ParseArguments` to `Context.Create`. Callers of
`Context.Create` are expected to handle the exception and surface it as a CLI
error. The resulting `Context` instance holds the fully parsed state when
argument parsing succeeds.

The `--result` flag is accepted as an alias for `--results`; both set the
`ResultsFile` property.

The `--depth` flag sets the default heading depth for all generated documents.
`--plan-depth` and `--report-depth` override the default for their respective
documents when specified. The valid range for all three depth flags is 1–5
inclusive; values outside this range cause `ArgumentException` to be thrown.
When `--plan-depth` or `--report-depth` is omitted, the value from `--depth`
(or its default of 1) is used for that document.

##### Output Methods

- **`WriteLine(string)`** — Writes a line to the console (unless `Silent` is set) and to the log file
- **`WriteError(string)`** — Sets the internal error flag (causing `ExitCode` to return non-zero),
  writes error to console (unless `Silent`) and log file
- **`Dispose()`** — Closes the log file handle opened by `--log`, if any; called automatically at
  the end of the `using` block in `Program.Main()`

##### Exit Code

`Context.ExitCode` reflects the current error status of the tool run. It is set to
a non-zero value when an error is detected. The value of `ExitCode` is returned from
`Program.Main()` as the process exit code.

##### IDisposable Contract

`Context` implements `IDisposable`. Callers must dispose the instance (typically via a
`using` statement) to ensure the log file handle opened by `--log` is closed promptly.
`Program.Main()` wraps the `Context` in a `using` block so the log is always flushed and
closed before the process exits.

##### Logging

When a log file path is provided via the `--log` CLI argument, `Context` opens and
holds the log file handle for the duration of the tool run. All output written through
`WriteLine` and `WriteError` is duplicated to the log file. `Context.Create` delegates
log-file opening to the private `OpenLogFile` helper; any file-system exception raised
during opening is wrapped in an `InvalidOperationException` before propagating to the
caller. If the log file cannot be opened (for example, because the parent directory does
not exist or permissions deny access), `Context.Create` throws an `InvalidOperationException`
wrapping the underlying file-system exception.

#### Error Handling

| Exception | Condition | Handling |
| --------- | --------- | -------- |
| `ArgumentException` | Unrecognized or malformed argument during `Context.Create()` | Propagated to `Program.Main()`, which writes the message to `Console.Error` and returns exit code 1 |
| `InvalidOperationException` | Log file cannot be opened during `Context.Create()` | Propagated to `Program.Main()`, which writes the message to `Console.Error` and returns exit code 1 |

`WriteError()` does not throw; it sets the internal error flag and writes to the output
streams. Errors encountered during the tool run are communicated through `Context.ExitCode`
rather than exceptions.

#### Interactions

**Called by:**

- `Program.Main()` — creates the `Context` instance via `Context.Create(string[] args)` and
  disposes it at the end of the `using` block
- All processing subsystems (Configuration, Indexing, SelfTest) receive the `Context`
  instance as a parameter for output and configuration access

**Dependencies:**

- No dependencies on other ReviewMark units or subsystems; `Context.Create()` only accesses
  the file system to open the optional log file

#### Overview

`Context` is the command-line argument parser and output channel for ReviewMark. It
parses the raw `string[] args` array into a structured set of typed properties and
provides `WriteLine()` / `WriteError()` methods that optionally duplicate output to a
log file. It is created once by `Program.Main()` and passed to every processing
subsystem as both a configuration carrier and an output mechanism.

#### Interfaces

`Context` is created via the static factory method `Context.Create(string[] args)`. It
exposes read-only properties for all parsed flags and arguments (see the Properties
section), along with the following output and lifecycle methods:

- **`WriteLine(string)`** — writes to console (unless Silent) and to the optional log file
- **`WriteError(string)`** — writes to console (unless Silent), to the log file, and sets the internal error flag
- **`ExitCode`** (`int`) — returns 0 or 1 depending on whether `WriteError` has been called
- **`Dispose()`** — closes the log file handle

Throws `ArgumentException` on unrecognized or malformed arguments; throws
`InvalidOperationException` when the log file cannot be opened.

#### Design

Argument parsing is delegated to the private `ArgumentParser` inner class, which
processes arguments sequentially via `ParseArgument()`. This separation keeps `Context`
focused on property ownership and output, while `ArgumentParser` owns the parsing logic.

The immutability of parsed properties after construction prevents accidental mutation
during processing. The only post-construction state changes are the error flag (set by
`WriteError()`) and the log file handle (released by `Dispose()`). The `IDisposable`
contract ensures the log file is always closed before the process exits.
