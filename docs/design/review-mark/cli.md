## Cli

![Cli Structure](CliView.svg)

The Cli subsystem is responsible for parsing and owning the command-line interface of
ReviewMark. It exposes two units: `Context`, which processes the raw `string[] args` array into a
structured set of typed properties consumed by the rest of the tool, and `HelpCommand`, which
answers the built-in `help` command from embedded help cards.

### Overview

The Cli subsystem solves the problem of translating raw process arguments into a
well-typed configuration carrier available to all processing subsystems. Its boundary is
tightly scoped: `Context` only parses arguments and owns console and log output (the only file
I/O is optionally opening the `--log` file), and it calls no other ReviewMark subsystem. The
`--affected` workflow itself is executed by `Program`, which uses the `AffectedFile` value parsed
by `Context`.

The subsystem contains two units: **Context** (`Cli/Context.cs`) — the command-line
argument parser and I/O owner — and **HelpCommand** (`Cli/HelpCommand.cs`) — the built-in
`help` command. See the *Context Design* and the *HelpCommand Design* for full details.

#### Affected workflow

`Context` parses `--affected <file|->` into `AffectedFile`; `Program` then runs the workflow:

- **Input sources**: a named file, or `-` for standard input. Entries may be separated by LF,
  CRLF or NUL.
- **Value validation**: a missing, empty, whitespace-only or option-like value is rejected with an
  `ArgumentException` whose message starts with `--affected requires`. A single-dash value other
  than `-` is accepted.
- **Exclusivity**: `--affected` cannot be combined with `--plan`, `--report`, `--elaborate`,
  `--index` or `--lint`.
- **Modifiers**: `--definition`, `--dir`, `--silent` and `--log` are allowed.
- **Output**: stdout carries escaped tab-delimited `affected<TAB>id<TAB>path` and `unreviewed<TAB>path` records, or the
  sentinel `No affected review-sets`; no banner (when affected mode runs; `--help` and `--validate` still print it) and
  no lint warnings are printed.
- **Errors**: errors are written to stderr (even with `--silent`) and the exit code is 1.

### Interfaces

**`Context.Create(string[] args)`** → `Context`

- *Type*: In-process .NET static factory method
- *Role*: Provider — other subsystems receive the `Context` instance created here
- *Contract*: Parses all supported command-line flags into a fully initialized `Context`
  instance with typed properties for every recognized flag, including `AffectedFile` (the
  `--affected` value: a file path or `-`, or `null` when not supplied). All flags are listed in the
  *ReviewMark Design* External Interfaces section. Also exposes `WriteLine(string)`,
  `WriteError(string)`, the computed `ExitCode` property, and `IDisposable.Dispose()`
- *Constraints*: Throws `ArgumentException` for unrecognized or malformed arguments (including an
  invalid `--affected` value or `--affected` combined with an exclusive operation); throws
  `InvalidOperationException` when the log file specified by `--log` cannot be opened

**`HelpCommand.TryRun(string[] args, out int exitCode)`** → `bool`

- *Type*: In-process .NET instance method (on the shared `HelpCommand.Default` instance)
- *Role*: Provider — `Program.Main()` offers the arguments to it before `Context.Create()`
- *Contract*: Returns `false`, writing nothing, when the first argument is not `help`. Otherwise
  writes the answer to the console, returns `true`, and sets the exit code (see *Help Command* in
  the *ReviewMark Design*)
- *Constraints*: Does not throw for user input; does not use `Context`

**`HelpCommand.Footer`** → `string`

- *Type*: In-process .NET instance property
- *Role*: Provider — `Program.PrintHelp()` ends the usage text with it
- *Contract*: Returns the one-line pointer to the help command that HelpCards supplies
- *Constraints*: Reading it never builds the help catalog

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

`Context` and `HelpCommand` do not call each other: when the help command answers, the invocation
ends with its exit code and no `Context` is created. `HelpCommand` writes its answers to the
console itself; it is the one exception to `Context` owning all output, because the `help`
command is answered before a `Context` exists.
