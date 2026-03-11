# Introduction

## Purpose

ReviewMark is a tool for automated file-review evidence management in regulated environments.
It computes cryptographic fingerprints of defined file-sets, queries a review evidence store
for corresponding code-review PDFs, and produces compliance documents with every CI/CD run.

## Scope

This user guide covers:

- Installation instructions
- Usage examples for common tasks
- Command-line options reference
- Practical examples for various scenarios

# Continuous Compliance

This tool follows the [Continuous Compliance][continuous-compliance] methodology, which ensures
compliance evidence is generated automatically on every CI run.

## Key Practices

- **Requirements Traceability**: Every requirement is linked to passing tests, and a trace matrix is
  auto-generated on each release
- **Linting Enforcement**: markdownlint, cspell, and yamllint are enforced before any build proceeds
- **Automated Audit Documentation**: Each release ships with generated requirements, justifications,
  trace matrix, and quality reports
- **CodeQL and SonarCloud**: Security and quality analysis runs on every build

# Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install -g DemaConsulting.ReviewMark
```

# Usage

## Display Version

Display the tool version:

```bash
reviewmark --version
```

## Display Help

Display usage information:

```bash
reviewmark --help
```

## Self-Validation

Self-validation produces a report demonstrating that ReviewMark is functioning
correctly. This is useful in regulated industries where tool validation evidence is required.

### Running Validation

To perform self-validation:

```bash
reviewmark --validate
```

To save validation results to a file:

```bash
reviewmark --validate --results results.trx
```

The results file format is determined by the file extension: `.trx` for TRX (MSTest) format,
or `.xml` for JUnit format.

### Validation Report

The validation report contains the tool version, machine name, operating system version,
.NET runtime version, timestamp, and test results.

Example validation report:

```text
# DEMA Consulting ReviewMark

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| Tool Version        | 1.0.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Ubuntu 22.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.0                                        |
| Time Stamp          | 2024-01-15 10:30:00 UTC                            |

✓ ReviewMark_VersionDisplay - Passed
✓ ReviewMark_HelpDisplay - Passed
✓ ReviewMark_DefinitionPlan - Passed
✓ ReviewMark_DefinitionReport - Passed
✓ ReviewMark_IndexScan - Passed
✓ ReviewMark_Dir - Passed
✓ ReviewMark_Enforce - Passed

Total Tests: 7
Passed: 7
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`ReviewMark_VersionDisplay`** - `--version` outputs a valid version string.
- **`ReviewMark_HelpDisplay`** - `--help` outputs usage and options information.
- **`ReviewMark_DefinitionPlan`** - `--definition` + `--plan` generates a review plan.
- **`ReviewMark_DefinitionReport`** - `--definition` + `--report` generates a review report.
- **`ReviewMark_IndexScan`** - `--index` scans PDF evidence files and writes `index.json`.
- **`ReviewMark_Dir`** - `--dir` overrides the working directory for file operations.
- **`ReviewMark_Enforce`** - `--enforce` exits with non-zero code when reviews have issues.

## Silent Mode

Suppress console output:

```bash
reviewmark --silent
```

## Logging

Write output to a log file:

```bash
reviewmark --log output.log
```

# Command-Line Options

The following command-line options are supported:

| Option                    | Description                                                  |
| ------------------------- | ------------------------------------------------------------ |
| `-v`, `--version`         | Display version information                                  |
| `-?`, `-h`, `--help`      | Display help message                                         |
| `--silent`                | Suppress console output                                      |
| `--validate`              | Run self-validation                                          |
| `--results <file>`        | Write validation results to file (TRX or JUnit format)       |
| `--log <file>`            | Write output to log file                                     |
| `--definition <file>`     | Specify the definition YAML file (default: .reviewmark.yaml) |
| `--plan <file>`           | Write review plan to the specified Markdown file             |
| `--plan-depth <#>`        | Set the heading depth for the review plan (default: 1)       |
| `--report <file>`         | Write review report to the specified Markdown file           |
| `--report-depth <#>`      | Set the heading depth for the review report (default: 1)     |
| `--index <glob-path>`     | Index PDF evidence files matching the glob path              |
| `--dir <directory>`       | Set the working directory for default paths and glob paths   |
| `--enforce`               | Exit with non-zero code if there are review issues           |

## Working Directory (`--dir`)

`--dir` sets the root directory used for operations that do not have an explicit path
provided by another argument. Specifically it affects:

- **Default definition file** — when `--definition` is omitted, `.reviewmark.yaml` is
  resolved relative to `--dir` (or the current directory if `--dir` is also omitted).
- **Glob scanning** — `--index` glob patterns are rooted at `--dir`, and `index.json`
  is written there.
- **File scanning** — when generating a plan or report, source files are enumerated
  relative to `--dir`.

Paths that the caller explicitly supplies via `--definition`, `--plan`, or `--report`
are used exactly as provided and are **not** re-rooted under `--dir`. This keeps each
argument independent: specifying one argument's path cannot silently change another
argument's path.

# Examples

## Example 1: Basic Usage

```bash
reviewmark
```

## Example 2: Self-Validation with Results

```bash
reviewmark --validate --results validation-results.trx
```

## Example 3: Silent Mode with Logging

```bash
reviewmark --silent --log tool-output.log
```

<!-- Link References -->
[continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
