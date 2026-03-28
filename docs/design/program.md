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

Any unexpected exception that escapes `Run()` is logged to the standard error stream
via `Console.Error` and then rethrown. As a result, the process terminates due to the
unhandled exception and the final exit code is determined by the .NET runtime rather
than by `Program.Main` explicitly returning a non-zero value.

## Run() Dispatch Logic

`Program.Run(Context)` evaluates the parsed flags in the following priority order,
executing the first matching action and returning:

1. If `--version` — print version and return
2. If `--help` — print banner and return
3. If `--validate` — run self-validation and return
4. If `--lint` — run configuration lint and return
5. If `--index` paths provided — scan and write evidence index, then return
6. Otherwise — generate Review Plan and/or Review Report and return

Only one top-level action is performed per invocation. Actions later in the priority
order are not reached if an earlier flag is set.

## PrintBanner()

`Program.PrintBanner(Context)` writes the help text to the console via
`Context.WriteLine()`. The banner lists all supported flags and arguments with brief
descriptions.
