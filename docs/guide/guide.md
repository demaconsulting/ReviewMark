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

# Configuration

ReviewMark is configured through a `.reviewmark.yaml` file, normally placed at the repository root.
The file has three top-level keys:

| Key               | Required | Description                                              |
| :---------------- | :------- | :------------------------------------------------------- |
| `needs-review`    | No       | Glob patterns identifying all files that require review  |
| `evidence-source` | Yes      | Location of the review evidence index                    |
| `reviews`         | Yes      | List of review sets, each grouping related files         |

A complete annotated example:

```yaml
# .reviewmark.yaml

# Patterns identifying every file in the repository that requires review.
# Processed in order; prefix a pattern with '!' to exclude.
needs-review:
  - "src/**/*.cs"
  - "src/**/*.yaml"
  - "docs/**/*.md"
  - "!**/obj/**"         # exclude build output
  - "!src/Generated/**"  # exclude auto-generated files

# Where to find the evidence index (index.json).
evidence-source:
  type: fileshare
  location: \\reviews.example.com\evidence\

# Review sets: groups of related files reviewed together.
reviews:
  - id: core-logic
    title: Review of core business logic
    paths:
      - "src/Core/**/*.cs"
      - "!src/Core/obj/**"   # exclude build output within the set

  - id: security-layer
    title: Review of authentication and authorization
    paths:
      - "src/Auth/**/*.cs"
      - "tests/Auth/**/*.cs"
```

## Review Sets

A **review set** is a named group of files that are reviewed together as a single unit. Each set
has three fields:

| Field   | Required | Description                                              |
| :------ | :------- | :------------------------------------------------------- |
| `id`    | Yes      | Stable identifier used in evidence PDFs                  |
| `title` | Yes      | Human-readable description shown in the plan and report  |
| `paths` | Yes      | Ordered list of glob include/exclude patterns            |

### Glob Patterns

Patterns are applied in the order they appear. A pattern prefixed with `!` is an **exclusion**:

```yaml
paths:
  - "src/Payments/**/*.cs"     # include all C# files under Payments
  - "tests/Payments/**/*.cs"   # include corresponding tests
  - "!src/Payments/obj/**"     # exclude build artifacts
```

The same `!`-prefix syntax applies to the top-level `needs-review` list.

### Fingerprinting

ReviewMark computes a cryptographic fingerprint for each review set from the hashes of all
matched files. The fingerprint changes whenever files are **added, removed, or modified**, but is
stable across renames or moves that keep the same set of file contents, so those do not
invalidate a review.

When a reviewer creates evidence, they record the current fingerprint in the PDF Keywords field.
ReviewMark matches that recorded fingerprint against the current fingerprint to determine whether
the review is still current.

### Design Guidance

Good review sets share these properties:

- **Cohesive** — group implementation files with their corresponding tests and any documentation
  they are paired with (e.g. a module's `.md` file alongside its `.cs` files).
- **Stable `id`** — choose an identifier that will not need to change as the code evolves, such
  as `authentication-module` or `payment-api`. The `id` is embedded in every evidence PDF, so
  renaming it breaks the evidence chain.
- **Right-sized** — a set that is too large is difficult to review in a single sitting; a set that
  is too small creates an unmanageable number of review artifacts.
- **Exclude generated files** — use `!` patterns to omit `obj/`, `bin/`, and other build outputs
  that change frequently without meaningful content changes.

Example of a well-structured set that groups a feature module with its tests and documentation:

```yaml
reviews:
  - id: payment-processor
    title: Payment processing module
    paths:
      - "src/Payments/**/*.cs"
      - "tests/Payments/**/*.cs"
      - "docs/payments.md"
      - "!src/Payments/obj/**"
      - "!tests/Payments/obj/**"
```

## Evidence Source

The `evidence-source` block tells ReviewMark where to find `index.json` — the catalogue of
completed review PDFs.

### Source Types

| Type         | Description                                     |
| :----------- | :---------------------------------------------- |
| `fileshare`  | UNC path or local directory                     |
| `url`        | HTTP or HTTPS endpoint                          |

#### File Share

```yaml
evidence-source:
  type: fileshare
  location: \\reviews.example.com\evidence\
```

#### URL

```yaml
evidence-source:
  type: url
  location: https://reviews.example.com/evidence/
```

### Credentials

For authenticated URL sources, supply credentials through environment variables so that secrets
are never stored in the definition file or source control:

```yaml
evidence-source:
  type: url
  location: https://reviews.example.com/evidence/
  credentials:
    username-env: REVIEWMARK_USER   # name of the environment variable holding the username
    password-env: REVIEWMARK_TOKEN  # name of the environment variable holding the password
```

In a CI/CD pipeline, map repository secrets to those environment variables:

```yaml
- name: Run ReviewMark
  env:
    REVIEWMARK_USER: ${{ secrets.REVIEW_USER }}
    REVIEWMARK_TOKEN: ${{ secrets.REVIEW_TOKEN }}
  run: reviewmark --plan plan.md --report report.md --enforce
```

# Typical Workflow

The following steps describe the end-to-end workflow for a repository that uses ReviewMark.

## Step 1 — Create the Definition File

Add a `.reviewmark.yaml` at the repository root. Define `needs-review` patterns to identify every
file that must be reviewed, an `evidence-source` pointing to your review evidence store, and at
least one review set grouping related files.

```yaml
needs-review:
  - "src/**/*.cs"
  - "docs/**/*.md"
  - "!**/obj/**"

evidence-source:
  type: fileshare
  location: \\reviews.example.com\evidence\

reviews:
  - id: core-module
    title: Core module implementation
    paths:
      - "src/Core/**/*.cs"
      - "tests/Core/**/*.cs"
      - "docs/core.md"
      - "!**/obj/**"
```

## Step 2 — Generate the Review Plan

Run ReviewMark with `--plan` to produce the Review Plan document. The plan lists every review set,
how many files it covers, its current fingerprint, and reports any files that are not covered by
any review set.

```bash
reviewmark --plan docs/review/review-plan.md
```

The plan is checked into the repository alongside the source code so that reviewers have a structured
starting point.

## Step 3 — Perform and Record the Review

A reviewer works through the review plan, examining each file in the listed review sets. When the
review is complete, the reviewer creates a PDF (following your organization's QMS numbering standard)
and embeds the review metadata in the PDF Keywords field:

```text
id=core-module fingerprint=a3f9c2d1... date=2026-03-15 result=pass
```

The PDF is deposited in the evidence store folder. ReviewMark never dictates file names — the
reviewer uses whatever name the QMS requires.

## Step 4 — Update the Evidence Index

Scan the evidence store to regenerate `index.json`. This step is typically run on the evidence
server after each new PDF is deposited:

```bash
reviewmark --dir \\reviews.example.com\evidence\ --index "**/*.pdf"
```

The `--index` flag may be repeated to cover evidence organized across multiple sub-directories:

```bash
reviewmark --dir \\reviews.example.com\evidence\ --index "2025/**/*.pdf" --index "2026/**/*.pdf"
```

## Step 5 — Generate the Review Report

Run ReviewMark with both `--plan` and `--report` to produce the Review Report alongside the plan.
The report shows the status of each review set — Current, Stale, Failed, or Missing — and lists
the referenced evidence documents.

```bash
reviewmark --plan docs/review/review-plan.md --report docs/review/review-report.md
```

## Step 6 — Enforce Compliance in CI

Add `--enforce` to fail the CI pipeline when any review set is missing, stale, or failed, or when
any file matching `needs-review` is not covered by a review set:

```bash
reviewmark \
  --plan    docs/review/review-plan.md \
  --report  docs/review/review-report.md \
  --enforce
```

With `--enforce`, the pipeline blocks until all outstanding reviews are completed and the evidence
index is updated.

# Examples

## Example 1: Complete CI/CD Run

Generate the review plan and report, and enforce compliance in a single command. Credentials are
supplied as environment variables mapped from CI secrets.

```bash
REVIEWMARK_USER=${{ secrets.REVIEW_USER }} \
REVIEWMARK_TOKEN=${{ secrets.REVIEW_TOKEN }} \
reviewmark \
  --definition .reviewmark.yaml \
  --plan    docs/review/review-plan.md \
  --report  docs/review/review-report.md \
  --enforce
```

With a corresponding definition file:

```yaml
# .reviewmark.yaml
needs-review:
  - "src/**/*.cs"
  - "docs/**/*.md"
  - "!**/obj/**"

evidence-source:
  type: url
  location: https://reviews.example.com/evidence/
  credentials:
    username-env: REVIEWMARK_USER
    password-env: REVIEWMARK_TOKEN

reviews:
  - id: core-module
    title: Core module implementation
    paths:
      - "src/Core/**/*.cs"
      - "tests/Core/**/*.cs"
      - "docs/core.md"
      - "!**/obj/**"

  - id: api-layer
    title: Public API layer
    paths:
      - "src/Api/**/*.cs"
      - "tests/Api/**/*.cs"
      - "docs/api.md"
      - "!**/obj/**"
```

## Example 2: Indexing Evidence Across Year Directories

When evidence PDFs are organized by year, supply multiple `--index` patterns to scan all of them
in a single run:

```bash
reviewmark \
  --dir \\reviews.example.com\evidence\ \
  --index "2024/**/*.pdf" \
  --index "2025/**/*.pdf" \
  --index "2026/**/*.pdf"
```

## Example 3: Excluding Generated Files in a Review Set

Use `!`-prefixed patterns to exclude build outputs and auto-generated files from a review set,
keeping the fingerprint stable across builds:

```yaml
reviews:
  - id: data-layer
    title: Data access layer
    paths:
      - "src/Data/**/*.cs"      # include all C# source files
      - "tests/Data/**/*.cs"    # include all corresponding tests
      - "!src/Data/obj/**"      # exclude build output
      - "!src/Data/bin/**"      # exclude compiled binaries
      - "!src/Data/Generated/**" # exclude auto-generated entity classes
```

## Example 4: Self-Validation with Results

```bash
reviewmark --validate --results validation-results.trx
```

## Example 5: Silent Mode with Logging

```bash
reviewmark --silent --log tool-output.log
```

<!-- Link References -->
[continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
