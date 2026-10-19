## Program

![ReviewMark Structure](ReviewMarkView.svg)

### Purpose

`Program` is the process entry point and execution dispatcher for ReviewMark. It owns
`Main()`, constructs the `Context` instance, dispatches to the appropriate processing
logic based on parsed CLI flags, and returns a meaningful exit code. There is no
review-processing logic in `Program` itself; all domain work is delegated to the
Configuration, Indexing, and SelfTest subsystems through the `Context` carrier.

### Data Model

**`Version`**: `string` (static property) — Tool version string embedded at build time;
sourced first from `AssemblyInformationalVersionAttribute` (may include a git hash suffix),
then from `AssemblyVersionAttribute`, and finally defaults to `"0.0.0"` when neither
attribute is present. Stateless and thread-safe.

`Program` holds no other state; `Main()`, `Run()`, and all helper methods are static.

### Key Methods

**`Program.Main(string[] args)`**

- *Parameters*: `string[] args` — raw command-line arguments from the OS
- *Returns*: `int` — exit code; 0 for success, 1 for failure
- *Preconditions*: Called by the OS process launcher as the entry point
- *Postconditions*: All resources are released; exit code reflects success or failure

Creates a `Context` instance via `Context.Create(args)` inside a `using` block, calls
`Program.Run(context)`, and returns `context.ExitCode`. Uses a three-tier exception
handler:

| Exception | Action |
| --------- | ------ |
| `ArgumentException` | Write `"Error: {message}"` to `Console.Error`; return 1 |
| `InvalidOperationException` | Write `"Error: {message}"` to `Console.Error`; return 1 |
| Any other exception | Write `"Unexpected error: {message}"` to `Console.Error`; rethrow |

**`Program.Run(Context context)`**

- *Parameters*: `Context context` — fully initialized execution context
- *Returns*: void (outcome communicated through `context.ExitCode`)
- *Preconditions*: `context` is not null
- *Postconditions*: Exactly one top-level action has been performed

Evaluates parsed flags in a fixed priority order and executes the first matching action:

1. If `context.Version` — print version string via `context.WriteLine(Version)` and return
2. Print application banner (suppressed when `context.Lint` is true)
3. If `context.Help` — print help text and return
4. If `context.Validate` — call `Validation.Run(context)` and return
5. If `context.Lint` — call `RunLintLogic(context)` and return
6. Otherwise — call `RunToolLogic(context)`

**`Program.RunLintLogic(Context context)`**

Resolves the definition file path (`--definition` or default `.reviewmark.yaml` under the
working directory), calls `ReviewMarkConfiguration.Load()`, and reports all issues via
`loadResult.ReportIssues(context)`. No banner and no summary are printed; silence means
the definition file is valid.

**`Program.RunToolLogic(Context context)`**

Determines the working directory from `context.WorkingDirectory` or
`Directory.GetCurrentDirectory()`. If `context.IndexPaths` is non-empty, calls
`RunIndexLogic()`. If any definition-based action is requested (`--plan`, `--report`,
`--definition`, `--elaborate`, or `--affected`), calls `RunDefinitionLogic()`. If neither is requested,
prints a usage hint.

**`Program.RunIndexLogic(Context context, string directory)`**

Calls `ReviewIndex.Scan(directory, context.IndexPaths, onWarning: context.WriteLine)`,
then writes the index to `index.json` via `ReviewIndex.Save()`. Emits progress messages
before and after the scan. Per-file scan failures are forwarded as warnings via
`context.WriteLine()`. Write failures propagate as `InvalidOperationException`.

**`Program.RunDefinitionLogic(Context context, string directory, string definitionFile)`**

Loads the configuration via `ReviewMarkConfiguration.Load()`, reports lint issues via
`ReportIssues(context, includeWarnings: context.AffectedFile == null, alwaysWriteErrors: context.AffectedFile != null)`
(in `--affected` mode warnings are suppressed so stdout stays machine-readable, and errors are
always written to stderr, even with `--silent`), and stops if `Configuration` is null. Then:

- If `--plan`: generates the Review Plan via `PublishReviewPlan(directory, context.PlanDepth)`,
  writes it to `context.PlanFile`, and calls `HandleIssues()`.
- If `--report`: loads the evidence index via `ReviewIndex.Load(config.EvidenceSource)`,
  generates the Review Report via `PublishReviewReport(index, directory, context.ReportDepth)`,
  writes it to `context.ReportFile`, and calls `HandleIssues()`.
- If `--elaborate`: calls `config.ElaborateReviewSet(context.ElaborateId, directory)` and
  writes the result to the console; catches `ArgumentException` for unknown IDs and calls
  `context.WriteError($"Error: {ex.Message}")`.
- If `--affected`: calls `RunAffectedLogic()`.

**`Program.RunAffectedLogic(Context context, ReviewMarkConfiguration config, string directory)`**

Reads the changed-file list from standard input (`--affected -`) or from the named file.
Read failures (`IOException`, `UnauthorizedAccessException`, `NotSupportedException`,
`ObjectDisposedException`, `ArgumentException`, `SecurityException`) for either source are wrapped in `InvalidOperationException`
("Failed to read changed-files list '<source>': ..."; the source text for stdin is
`standard input`). The text is parsed with `PathHelpers.ParseChangedPaths(text, directory)` and
passed to `config.GetAffectedReviewSets()`. The list is read once and no git command or
file-system lookup of the changed paths is performed. Output is written via `context.WriteLine()`
(so `--silent` and `--log` apply), and the application banner is suppressed for
`--affected` so the output can be piped or parsed. Output lines are tab-delimited because
review-set IDs may contain spaces and paths may too; the id and path fields are escaped with
`PathHelpers.EscapeRecordField()` (`\` → `\\`, TAB → `\t`, LF → `\n`, CR → `\r`):

| Line | Meaning |
| ---- | ------- |
| `No affected review-sets` | Sentinel (not a record) written first when no review set is affected (including empty input) |
| `affected<TAB><id><TAB><path>` | One line per affected review set and matching changed file, in definition order then path order |
| `unreviewed<TAB><path>` | A changed file that matches `needs-review` (or any file when it is empty) but matches no review set's `paths`; context patterns do not count |

The exit code is 0 even when unreviewed files are reported. Lint warnings are not written in this
mode (use `--lint`). The failure modes are:

| Failure | Result |
| ------- | ------ |
| Unreadable list file or stdin | `InvalidOperationException`; `Main()` writes `Error: ...` to stderr, exit 1 |
| Missing or invalid definition | Lint errors via `WriteError` to stderr, even with `--silent`; exit 1 |
| `--affected` with `--plan`, `--report`, `--elaborate`, `--index` or `--lint` | `ArgumentException` from `Context.Create()`; `Main()` writes `Error: ...` to stderr, exit 1 |

Undecodable input bytes are replacement-decoded and matched as text.

**`Program.HandleIssues(Context context, bool hasIssues, string message)`**

If `hasIssues` is false, does nothing. If `context.Enforce` is true, calls
`context.WriteError(message)` (sets exit code to 1). Otherwise calls
`context.WriteLine($"Warning: {message}")` (non-fatal).

### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| `ArgumentException` | `Context.Create()` — unrecognized or malformed argument | Caught in `Main()`; message to `Console.Error`; return 1 |
| `InvalidOperationException` | `Context.Create()` (log file) or `RunDefinitionLogic()` (file I/O, unreadable `--affected` list or stdin) | Caught in `Main()`; message to `Console.Error`; return 1 |
| `ArgumentException` | `ElaborateReviewSet()` — unknown review-set ID | Caught in `RunDefinitionLogic()`; routed through `context.WriteError()` |
| Any other exception | Unexpected failure | Message to `Console.Error` as `"Unexpected error: {message}"`; rethrown. Unhandled exceptions are outside the 0/1 exit-code contract |

### Dependencies

- **`Context`** (Cli subsystem) — `Context.Create(string[] args)` constructs the execution
  context; `context.WriteLine()`, `context.WriteError()`, and `context.ExitCode` are used
  throughout
- **`Validation`** (SelfTest subsystem) — `Validation.Run(context)` invoked for `--validate`
- **`ReviewMarkConfiguration`** (Configuration subsystem) — `Load()`, `PublishReviewPlan()`,
  `PublishReviewReport()`, `ElaborateReviewSet()`, `GetAffectedReviewSets()` and
  `ReviewMarkLoadResult.ReportIssues()` invoked for lint and definition-based workflows
- **`PathHelpers`** (Indexing subsystem) — `ParseChangedPaths()` and `EscapeRecordField()` invoked
  for `--affected`
- **`ReviewIndex`** (Indexing subsystem) — `Scan()`, `Load()`, `Save()` invoked for index
  scanning and report generation
- **`PathHelpers`** (Indexing subsystem) — `SafePathCombine()` used to resolve default
  definition file and `index.json` paths under the working directory

### Callers

N/A — entry point, called by the host environment. `Program.Main()` is the process entry
point and has no callers within the assembly. `Program.Run()` is called only by `Main()`
and by the self-validation test suite in `Validation.cs`.
