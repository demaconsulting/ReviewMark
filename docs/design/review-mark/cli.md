## Cli

The Cli subsystem is responsible for parsing and owning the command-line interface of
ReviewMark. It exposes a single unit — `Context` — that processes the raw `string[] args`
array into a structured set of typed properties consumed by the rest of the tool.

### Overview

The Cli subsystem solves the problem of translating raw process arguments into a
well-typed configuration carrier available to all processing subsystems. Its boundary is
tightly scoped: it performs no file I/O beyond optionally opening a log file, and it calls
no other ReviewMark subsystem.

The subsystem contains a single unit: **Context** (`Cli/Context.cs`) — the command-line
argument parser and I/O owner. See the *Context Design* for full details.

### Interfaces

**`Context.Create(string[] args)`** → `Context`

- *Type*: In-process .NET static factory method
- *Role*: Provider — other subsystems receive the `Context` instance created here
- *Contract*: Parses all supported command-line flags into a fully initialized `Context`
  instance with typed properties for every recognized flag. All flags are listed in the
  *ReviewMark Design* External Interfaces section. Also exposes `WriteLine(string)`,
  `WriteError(string)`, the computed `ExitCode` property, and `IDisposable.Dispose()`
- *Constraints*: Throws `ArgumentException` for unrecognized or malformed arguments; throws
  `InvalidOperationException` when the log file specified by `--log` cannot be opened

### Design

The `Context` unit owns all argument-parsing logic through its private `ArgumentParser`
inner class. `ArgumentParser.ParseArguments()` processes the argument array sequentially,
delegating individual tokens to `ParseArgument()`, which dispatches known flags via a
`switch` statement and accumulates their values.

Once `Context.Create()` returns, all parsed properties are immutable. The only mutable
state after construction is the internal error flag (set by `WriteError()`) and the log
file handle (released by `Dispose()`). The `IDisposable` contract ensures the log file is
always closed before the process exits.

`PlanDepth` defaults to `Depth` when `--plan-depth` is not specified; `ReportDepth`
defaults to `Depth` when `--report-depth` is not specified. Multiple `--index` arguments
are accumulated into a `List<string>` and exposed as `IReadOnlyList<string>`.
