# Cli Subsystem

## Overview

The Cli subsystem is responsible for parsing and owning the command-line interface of
ReviewMark. It exposes a single software unit — Context — that processes the raw
`string[] args` array into a structured set of properties consumed by the rest of the
tool.

## Responsibilities

- Parse all supported command-line flags and arguments into a typed `Context` object
- Validate that no unrecognized arguments are supplied
- Own the output channels (stdout and optional log file) and the process exit code
- Propagate the `--silent` flag to suppress non-error output

## Units

| Unit    | Source File              | Purpose                                      |
|---------|--------------------------|----------------------------------------------|
| Context | `Cli/Context.cs`         | Command-line argument parser and I/O owner   |

## Supported Flags

All flags are parsed by `Context.Create(string[] args)`. The following table lists every
supported flag, its type, aliases, and constraints:

| Flag | Alias(es) | Type | Constraint | Description |
|------|-----------|------|------------|-------------|
| `--version` | `-v` | bool | — | Display version string only |
| `--help` | `-?`, `-h` | bool | — | Display usage information |
| `--silent` | — | bool | — | Suppress all console output |
| `--validate` | — | bool | — | Run self-validation tests |
| `--lint` | — | bool | — | Validate the definition file and report issues |
| `--log <file>` | — | string | Valid file path | Write all output to a log file |
| `--results <file>` | `--result` | string | Valid file path | Write validation results (TRX or JUnit) |
| `--definition <file>` | — | string | Valid file path | Override default `.reviewmark.yaml` path |
| `--plan <file>` | — | string | Valid file path | Output path for the Review Plan Markdown document |
| `--depth <#>` | — | int | 1–5 | Default heading depth for all generated documents (default: 1) |
| `--plan-depth <#>` | — | int | 1–5 | Heading depth for the Review Plan (overrides `--depth`) |
| `--report <file>` | — | string | Valid file path | Output path for the Review Report Markdown document |
| `--report-depth <#>` | — | int | 1–5 | Heading depth for the Review Report (overrides `--depth`) |
| `--index <glob-path>` | — | string (repeatable) | Glob expression | Scan PDF evidence files matching the glob path |
| `--dir <directory>` | — | string | Valid directory path | Set the working directory for file operations |
| `--enforce` | — | bool | — | Exit with non-zero code if any review-set is not Current |
| `--elaborate <id>` | — | string | Non-empty review-set ID | Print a Markdown elaboration of the specified review set |

**Depth defaulting**: `PlanDepth` defaults to `Depth` when `--plan-depth` is not
specified; `ReportDepth` defaults to `Depth` when `--report-depth` is not specified.

**`--index` is repeatable**: Multiple `--index <glob-path>` arguments may be provided;
all matching PDF files are combined into a single index scan.

## Error Handling

Unrecognized or malformed arguments cause `Context.Create` to throw an `ArgumentException`.
`Program.Main` catches this exception, writes the error message to `Console.Error`, and
returns exit code 1. The process never exits silently on an argument error.

Value arguments (`--log`, `--plan`, `--results`, etc.) require a non-empty following
token. If the token is missing, an `ArgumentException` is thrown with a message that
names the flag and describes what is expected.

Integer arguments (`--depth`, `--plan-depth`, `--report-depth`) require a positive
integer value in the range 1–5. Values outside this range cause an `ArgumentException`.
