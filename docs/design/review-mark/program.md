## Program

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
`--definition`, or `--elaborate`), calls `RunDefinitionLogic()`. If neither is requested,
prints a usage hint.

**`Program.RunIndexLogic(Context context, string directory)`**

Calls `ReviewIndex.Scan(directory, context.IndexPaths, onWarning: context.WriteLine)`,
then writes the index to `index.json` via `ReviewIndex.Save()`. Emits progress messages
before and after the scan. Per-file scan failures are forwarded as warnings via
`context.WriteLine()`. Write failures propagate as `InvalidOperationException`.

**`Program.RunDefinitionLogic(Context context, string directory, string definitionFile)`**

Loads the configuration via `ReviewMarkConfiguration.Load()`, reports lint issues, and
stops if `Configuration` is null. Then:

- If `--plan`: generates the Review Plan via `PublishReviewPlan(directory, context.PlanDepth)`,
  writes it to `context.PlanFile`, and calls `HandleIssues()`.
- If `--report`: loads the evidence index via `ReviewIndex.Load(config.EvidenceSource)`,
  generates the Review Report via `PublishReviewReport(index, directory, context.ReportDepth)`,
  writes it to `context.ReportFile`, and calls `HandleIssues()`.
- If `--elaborate`: calls `config.ElaborateReviewSet(context.ElaborateId, directory)` and
  writes the result to the console; catches `ArgumentException` for unknown IDs and calls
  `context.WriteError($"Error: {ex.Message}")`.

**`Program.HandleIssues(Context context, bool hasIssues, string message)`**

If `hasIssues` is false, does nothing. If `context.Enforce` is true, calls
`context.WriteError(message)` (sets exit code to 1). Otherwise calls
`context.WriteLine($"Warning: {message}")` (non-fatal).

### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| `ArgumentException` | `Context.Create()` — unrecognized or malformed argument | Caught in `Main()`; message to `Console.Error`; return 1 |
| `InvalidOperationException` | `Context.Create()` (log file) or `RunDefinitionLogic()` (file I/O) | Caught in `Main()`; message to `Console.Error`; return 1 |
| `ArgumentException` | `ElaborateReviewSet()` — unknown review-set ID | Caught in `RunDefinitionLogic()`; routed through `context.WriteError()` |
| Any other exception | Unexpected failure | Message to `Console.Error` as `"Unexpected error: {message}"`; rethrown |

### Dependencies

- **`Context`** (Cli subsystem) — `Context.Create(string[] args)` constructs the execution
  context; `context.WriteLine()`, `context.WriteError()`, and `context.ExitCode` are used
  throughout
- **`Validation`** (SelfTest subsystem) — `Validation.Run(context)` invoked for `--validate`
- **`ReviewMarkConfiguration`** (Configuration subsystem) — `Load()`, `PublishReviewPlan()`,
  `PublishReviewReport()`, `ElaborateReviewSet()` invoked for lint and definition-based workflows
- **`ReviewIndex`** (Indexing subsystem) — `Scan()`, `Load()`, `Save()` invoked for index
  scanning and report generation
- **`PathHelpers`** (Indexing subsystem) — `SafePathCombine()` used to resolve default
  definition file and `index.json` paths under the working directory

### Callers

N/A — entry point, called by the host environment. `Program.Main()` is the process entry
point and has no callers within the assembly. `Program.Run()` is called only by `Main()`
and by the self-validation test suite in `Validation.cs`.
