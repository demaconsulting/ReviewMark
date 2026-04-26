# Program

## Purpose

The `Program` software unit is the main entry point of the ReviewMark tool. It is
responsible for constructing the execution context, dispatching to the appropriate
processing logic based on parsed flags, and returning a meaningful exit code to the
calling process.

## Version Property

`Program.Version` returns the tool version string. The version is embedded at build
time from the assembly metadata and follows semantic versioning conventions.

## Main() Method

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

## Run() Dispatch Logic

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

## PrintBanner()

`Program.PrintBanner(Context)` writes the application name, version, and copyright
notice to the console via `Context.WriteLine()`. The banner is printed for every
invocation except `--version` and `--lint`.

## PrintHelp()

`Program.PrintHelp(Context)` writes usage information to the console via
`Context.WriteLine()`. The help text lists all supported flags and arguments with brief
descriptions.

## RunLintLogic()

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

## RunToolLogic()

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

## RunIndexLogic()

`Program.RunIndexLogic(Context, string directory)` scans PDF files using
`ReviewIndex.Scan(directory, context.IndexPaths)` and writes the resulting
index to `index.json` in the working directory via `ReviewIndex.Save()`.
Warnings from the scan (e.g., PDFs missing required metadata) are forwarded
to `context.WriteLine()`. Progress messages `"Scanning PDF evidence files..."`
and `"Index written to {indexFile}"` are emitted via `context.WriteLine()`
before and after the scan respectively.

## RunDefinitionLogic()

`Program.RunDefinitionLogic(Context, string directory, string definitionFile)`
handles the definition-based workflow:

1. Loads the configuration file via `ReviewMarkConfiguration.Load()`.
2. Reports all lint issues via `loadResult.ReportIssues(context)`.
3. If `Configuration` is null after loading, returns immediately.
4. If `--plan` is set, generates the Review Plan Markdown and writes it to
   the specified file; wraps I/O failures as `InvalidOperationException`.
5. If `--report` is set, loads the evidence index via `ReviewIndex.Load()`,
   generates the Review Report Markdown, and writes it to the specified file.
6. If `--elaborate` is set, calls `config.ElaborateReviewSet()` and writes the
   result to the console via `context.WriteLine()`; catches `ArgumentException`
   for unknown IDs and calls `context.WriteError()` with the exception message,
   which sets the exit code to 1.

## HandleIssues()

`Program.HandleIssues(Context, bool hasIssues, string message)` translates a
boolean issue flag into a context message:

- If `hasIssues` is false, it does nothing.
- If `context.Enforce` is true, calls `context.WriteError(message)` (sets
  exit code to 1).
- Otherwise, calls `context.WriteLine($"Warning: {message}")` (non-fatal).
