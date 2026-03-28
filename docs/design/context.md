# Context

## Purpose

The `Context` software unit is responsible for parsing command-line arguments and
providing a unified interface for output and logging throughout the tool. It acts as
the primary configuration carrier, passing parsed options from the CLI entry point
to all processing subsystems.

## Properties

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
| `DefinitionFile` | string | Path to the `.reviewmark.yaml` configuration |
| `PlanFile` | string? | Output path for the Review Plan document |
| `PlanDepth` | int | Heading depth for the Review Plan |
| `ReportFile` | string? | Output path for the Review Report document |
| `ReportDepth` | int | Heading depth for the Review Report |
| `IndexPaths` | string[]? | Paths to scan when building an evidence index |
| `WorkingDirectory` | string | Base directory for resolving relative paths |
| `Enforce` | bool | Fail if any review-set is not Current |
| `Elaborate` | bool | Expand file lists in generated documents |

## Argument Parsing

`Context.Create(string[] args)` is a factory method that processes the argument
array sequentially, recognizing both flag arguments (e.g., `--validate`) and
value arguments (e.g., `--plan <path>`). Unrecognized arguments are silently
ignored. The resulting `Context` instance holds the fully parsed state.

## Output Methods

| Method | Description |
| ------ | ----------- |
| `WriteLine(string)` | Writes a line to the console (unless `Silent` is set) and to the log file |
| `WriteError(string)` | Writes an error line to the console and to the log file |

## Exit Code

`Context.ExitCode` reflects the current error status of the tool run. It is set to
a non-zero value when an error is detected. The value of `ExitCode` is returned from
`Program.Main()` as the process exit code.

## Logging

When a log file path is provided via the relevant CLI argument, `Context` opens and
holds the log file handle for the duration of the tool run. All output written through
`WriteLine` and `WriteError` is duplicated to the log file.

## Resource Cleanup

`Context` implements `IDisposable`. Disposal closes the log file handle if one is
open. `Program.Main()` constructs `Context` inside a `using` statement to guarantee
timely disposal regardless of how the tool exits.
