### Context

#### Purpose

The `Context` software unit is responsible for parsing command-line arguments and
providing a unified interface for output and logging throughout the tool. It acts as
the primary configuration carrier, passing parsed options from the CLI entry point
to all processing subsystems.

#### Properties

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

#### Argument Parsing

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

#### Output Methods

- **`WriteLine(string)`** — Writes a line to the console (unless `Silent` is set) and to the log file
- **`WriteError(string)`** — Sets the internal error flag (causing `ExitCode` to return non-zero),
  writes error to console (unless `Silent`) and log file
- **`Dispose()`** — Closes the log file handle opened by `--log`, if any; called automatically at
  the end of the `using` block in `Program.Main()`

#### Exit Code

`Context.ExitCode` reflects the current error status of the tool run. It is set to
a non-zero value when an error is detected. The value of `ExitCode` is returned from
`Program.Main()` as the process exit code.

#### IDisposable Contract

`Context` implements `IDisposable`. Callers must dispose the instance (typically via a
`using` statement) to ensure the log file handle opened by `--log` is closed promptly.
`Program.Main()` wraps the `Context` in a `using` block so the log is always flushed and
closed before the process exits.

#### Logging

When a log file path is provided via the `--log` CLI argument, `Context` opens and
holds the log file handle for the duration of the tool run. All output written through
`WriteLine` and `WriteError` is duplicated to the log file. If the log file cannot be
opened (for example, because the parent directory does not exist or permissions deny
access), `Context.Create` throws an `InvalidOperationException` wrapping the underlying
file-system exception.
