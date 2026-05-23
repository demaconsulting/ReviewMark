## Program

### Purpose

`Program` is the entry point and execution dispatcher for the ReviewMark tool. It owns
the top-level `Main()` entry point, constructs the `Context` instance, dispatches to
the appropriate processing logic based on CLI flags, and returns a meaningful exit code.
There is no review-processing logic in `Program` itself; all domain work is delegated
to the Configuration, Indexing, and SelfTest subsystems through the `Context` carrier.

The `Program` software unit is the main entry point of the ReviewMark tool. It is
responsible for constructing the execution context, dispatching to the appropriate
processing logic based on parsed flags, and returning a meaningful exit code to the
calling process.

### Data Model

| Member | Type | Description |
| ------ | ---- | ----------- |
| `Version` | `string` (static property) | Tool version string embedded at build time; sourced first from `AssemblyInformationalVersionAttribute`, then from `AssemblyVersion`, and finally defaults to `"0.0.0"` when neither attribute is present |

`Program` holds no other state; `Main()`, `Run()`, and all helper methods are static.

### Key Methods

`Program.Main()` uses a three-tier exception handler to separate expected CLI errors
(`ArgumentException`, `InvalidOperationException`) from unexpected failures (rethrown).
`Program.Run()` implements a priority dispatch: flags are evaluated in a fixed order and
the first matching action is executed. This ensures that `--version`, `--help`,
`--validate`, and `--lint` are handled before definition-based logic, preventing
unintended side effects when diagnostic flags are combined with output flags.

All state is carried through the `Context` instance, which is created once in `Main()`
and disposed at the end of the `using` block. No global or static mutable state is used.

#### Version Property

`Program.Version` returns the tool version string. The version is embedded at build
time from the assembly metadata and follows semantic versioning conventions.

#### Main() Method

`Program.Main(string[] args)` is the process entry point. It:

1. Constructs a `Context` instance via `Context.Create(args)` inside a `using` block
2. Calls `Program.Run(Context)` to perform the requested operation
3. Returns `Context.ExitCode` as the process exit code

**Exception handling — three tiers:**

| Exception type | Action |
| -------------- | ------ |
| `ArgumentException` | Write `"Error: {message}"` to `Console.Error`; return exit code 1 |
| `InvalidOperationException` | Write `"Error: {message}"` to `Console.Error`; return exit code 1 |
| Any other exception | Write `"Unexpected error: {message}"` to `Console.Error`; rethrow |

`ArgumentException` is thrown by `Context.Create` when an unknown or malformed
argument is supplied. `InvalidOperationException` is thrown by `Context.Create`
when the log file cannot be opened, or by `RunDefinitionLogic` when a plan or
report file cannot be written. Other exceptions propagate as unhandled, which
terminates the process with a runtime-generated error exit code.

#### Run() Dispatch Logic

`Program.Run(Context)` evaluates the parsed flags in the following priority order,
executing the first matching action and returning:

1. If `--version` — print version and return
2. Print application banner (skipped for `--lint`)
3. If `--help` — print help and return
4. If `--validate` — run self-validation and return
5. If `--lint` — run configuration lint and return
6. Otherwise — run main tool logic (index scanning and/or Review Plan/Report/Elaborate)

The application banner (step 2) is always printed unless `--version` or `--lint` is
specified. Only one top-level action is performed per invocation. Actions later in the
priority order are not reached if an earlier flag is set.

#### PrintBanner()

`Program.PrintBanner(Context)` writes the application name, version, and copyright
notice to the console via `Context.WriteLine()`. The banner is printed for every
invocation except `--version` and `--lint`.

#### PrintHelp()

`Program.PrintHelp(Context)` writes usage information to the console via
`Context.WriteLine()`. The help text lists all supported flags and arguments with brief
descriptions.

#### RunLintLogic()

`Program.RunLintLogic(Context)` validates the definition file and reports issues:

1. Resolves the definition file path (from `--definition` or the default
   `.reviewmark.yaml` relative to the working directory).
2. Loads and lints the file via `ReviewMarkConfiguration.Load()`, collecting all
   detectable issues in one pass.
3. Writes each issue to the context via `ReportIssues()` — errors go to
   `Context.WriteError()`, warnings to `Context.WriteLine()`. The call to
   `Context.WriteError()` is also the mechanism by which the exit code is
   implicitly set to 1: `ReportIssues()` calls `Context.WriteError()` for each
   error-severity issue, and `Context.WriteError()` sets the internal error flag
   that drives `Context.ExitCode`.

No banner and no summary message are printed. Successful lint produces no output
(silence means the definition file is valid). This keeps the output clean for
integration with linting scripts and CI pipelines.

#### RunToolLogic()

`Program.RunToolLogic(Context)` is called when none of the early-exit flags
(`--version`, `--help`, `--validate`, `--lint`) are set. It:

1. Determines the working directory from `context.WorkingDirectory` or
   `Directory.GetCurrentDirectory()`.
2. If `context.IndexPaths` is non-empty, calls `RunIndexLogic()` to scan PDF
   evidence files and write an `index.json` file.
3. If any definition-based action is requested (`--plan`, `--report`,
   `--definition`, or `--elaborate`), calls `RunDefinitionLogic()`.
4. If neither index nor definition actions are requested, prints a usage hint
   via `context.WriteLine()`.

#### RunIndexLogic()

`Program.RunIndexLogic(Context, string directory)` scans PDF files using
`ReviewIndex.Scan(directory, context.IndexPaths)` and writes the resulting
index to `index.json` in the working directory via `ReviewIndex.Save()`.
Warnings from the scan (e.g., PDFs missing required metadata) are forwarded
to `context.WriteLine()`. Progress messages `"Scanning PDF evidence files..."`
and `"Index written to {indexFile}"` are emitted via `context.WriteLine()`
before and after the scan respectively.

If `ReviewIndex.Scan()` throws an unexpected exception, it propagates unhandled to
`Main()`, which writes `"Unexpected error: {message}"` to `Console.Error` and rethrows.

#### RunDefinitionLogic()

`Program.RunDefinitionLogic(Context, string directory, string definitionFile)`
handles the definition-based workflow:

1. Loads the configuration file via `ReviewMarkConfiguration.Load()`.
2. Reports all lint issues via `loadResult.ReportIssues(context)`.
3. If `Configuration` is null after loading, returns immediately.
4. If `--plan` is set, generates the Review Plan Markdown via
   `ReviewMarkConfiguration.PublishReviewPlan()`, passing `context.PlanDepth` as
   the heading depth, and writes the result to the specified file; wraps I/O
   failures as `InvalidOperationException`.
5. If `--report` is set, loads the evidence index via `ReviewIndex.Load()`,
   generates the Review Report Markdown via
   `ReviewMarkConfiguration.PublishReviewReport()`, passing `context.ReportDepth`
   as the heading depth, and writes the result to the specified file.
6. If `--elaborate` is set, calls `config.ElaborateReviewSet()` and writes the
   result to the console via `context.WriteLine()`; catches `ArgumentException`
   for unknown IDs and calls `context.WriteError($"Error: {ex.Message}")` with the formatted message,
   which sets the exit code to 1.

#### HandleIssues()

`Program.HandleIssues(Context, bool hasIssues, string message)` translates a
boolean issue flag into a context message:

- If `hasIssues` is false, it does nothing.
- If `context.Enforce` is true, calls `context.WriteError(message)` (sets
  exit code to 1).
- Otherwise, calls `context.WriteLine($"Warning: {message}")` (non-fatal).

### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| `ArgumentException` | `Context.Create()` — unrecognized or malformed argument | Caught in `Main()`; message written to `Console.Error`; exit code 1 returned |
| `InvalidOperationException` | `Context.Create()` (log file open failure) or `RunDefinitionLogic()` (plan/report I/O failure) | Caught in `Main()`; message written to `Console.Error`; exit code 1 returned |
| Any other exception | Unexpected failure in any subsystem | Message written to `Console.Error` as `"Unexpected error: {message}"`; exception rethrown |

`ArgumentException` thrown by `ElaborateReviewSet` for an unknown review-set ID is caught
inside `RunDefinitionLogic()` and routed through `Context.WriteError()`, setting the exit
code to 1 without propagating the exception further.

### Interactions

`Program` exposes the following public surface:

- **`Program.Version`** (`string`) — the tool version string, embedded at build time from assembly metadata
- **`Program.Main(string[] args)`** (`int`) — the process entry point; returns 0 on success or 1 on failure

`Program` consumes:

- **`Context.Create(string[] args)`** — factory method in the Cli subsystem
- **`Validation.Run(Context)`** — entry point of the SelfTest subsystem
- **`ReviewMarkConfiguration.Load(string)`** — entry point of the Configuration subsystem
- **`ReviewIndex.Scan(...)`** and **`ReviewIndex.Load(...)`** — loading methods of the Indexing subsystem

**Calls:**

- `Context.Create(string[] args)` — Cli subsystem; constructs the execution context from
  command-line arguments
- `Validation.Run(Context)` — SelfTest subsystem; invoked when `--validate` is set
- `ReviewMarkConfiguration.Load(string)` — Configuration subsystem; invoked for lint and
  definition-based workflows
- `ReviewMarkConfiguration.PublishReviewPlan(string, int)` — Configuration subsystem;
  invoked when `--plan` is set, passing `context.PlanDepth` as the heading depth
- `ReviewMarkConfiguration.PublishReviewReport(ReviewIndex, string, int)` — Configuration
  subsystem; invoked when `--report` is set, passing `context.ReportDepth` as the heading depth
- `ReviewMarkConfiguration.ElaborateReviewSet(string, string, int)` — Configuration
  subsystem; invoked when `--elaborate` is set
- `ReviewIndex.Scan(...)` — Indexing subsystem; invoked when `--index` paths are specified
- `ReviewIndex.Load(EvidenceSource)` — Indexing subsystem; invoked when generating a report
- `ReviewIndex.Save(string)` — Indexing subsystem; writes `index.json` after scanning

**Called by:**

- The operating system / process launcher; `Program.Main()` is the process entry point and
  has no callers within the assembly.
