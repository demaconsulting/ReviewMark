# ReviewMark

[![GitHub forks][badge-forks]][link-forks]
[![GitHub stars][badge-stars]][link-stars]
[![GitHub contributors][badge-contributors]][link-contributors]
[![License][badge-license]][link-license]
[![Build][badge-build]][link-build]
[![Quality Gate][badge-quality]][link-quality]
[![Security][badge-security]][link-security]
[![NuGet][badge-nuget]][link-nuget]

DEMA Consulting tool for automated file-review evidence management in regulated environments.

## Features

- 🔐 **Cryptographic Fingerprinting** - SHA256 fingerprints detect content changes automatically
- 📂 **Evidence Querying** - Queries URL or file-share evidence stores via an `index.json` catalogue
- 📋 **Coverage Reporting** - Review plan shows which files are covered and flags uncovered files
- 📊 **Status Reporting** - Review report shows whether each review-set is Current, Stale, Missing, or Failed
- 🔍 **Review Elaboration** - `--elaborate` prints the ID, fingerprint, and file list for a review set
- 🚦 **Enforcement** - `--enforce` exits non-zero if any review-set is stale or missing, or any file is uncovered
- 🔄 **Re-indexing** - `--index` scans PDF evidence files and writes an up-to-date `index.json`
- ✅ **Self-Validation** - Built-in validation tests with TRX and JUnit output
- 🌐 **Multi-Platform** - Builds and runs on Windows, Linux, and macOS
- 🎯 **Multi-Runtime** - Targets .NET 8, 9, and 10
- 🚀 **CI/CD Integration** - Automate review evidence generation in your pipelines
- 📜 **Continuous Compliance** - Proves file coverage and currency ([Continuous Compliance][link-continuous-compliance])

## Role in Continuous Compliance

In the [Continuous Compliance][link-continuous-compliance] methodology, every compliance artifact
is generated automatically on each CI/CD run. ReviewMark fills the **file-review evidence** role:

| Artifact | Description |
| :------- | :---------- |
| Review Plan | Proves every file requiring review is covered by at least one named review-set |
| Review Report | Proves each review-set is current — the review evidence matches the current file-set fingerprint |

These Markdown documents are published as PDF/A-3u release artifacts alongside the requirements
trace matrix and code quality report, giving auditors a complete, automatically-maintained evidence
package on every release.

## Review Definition

Reviews are configured in a `.reviewmark.yaml` file at the repository root. This file defines
which files require review, where to find the evidence store, and how to group files into
named review-sets:

```yaml
# .reviewmark.yaml

# Patterns identifying all files in the repository that require review.
# Processed in order; prefix a pattern with '!' to exclude.
needs-review:
  - "**/*.cs"
  - "**/*.yaml"
  - "!**/obj/**"           # exclude build output
  - "!src/Generated/**"    # exclude auto-generated files

evidence-source:
  type: url                # 'url' or 'fileshare'
  location: https://reviews.example.com/evidence/

reviews:
  - id: Core-Logic
    title: Review of core business logic
    paths:
      - "src/Core/**/*.cs"
      - "src/Core/**/*.yaml"
      - "!src/Core/Generated/**"
  - id: Security-Layer
    title: Review of authentication and authorization
    paths:
      - "src/Auth/**/*.cs"
```

See [THEORY-OF-OPERATIONS.md][link-theory-of-operations] for the theory of operations including fingerprinting,
evidence indexing, and compliance report formats.

## Installation

Install the tool globally using the .NET CLI:

```bash
dotnet tool install -g DemaConsulting.ReviewMark
```

## Usage

```bash
# Display version
reviewmark --version

# Display help
reviewmark --help

# Run self-validation
reviewmark --validate

# Save validation results
reviewmark --validate --results results.trx

# Silent mode with logging
reviewmark --silent --log output.log
```

## Command-Line Options

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
| `--dir <directory>`       | Set the working directory for file operations                |
| `--enforce`               | Exit with non-zero code if there are review issues           |
| `--elaborate <id>`        | Print a Markdown elaboration of the specified review set     |

## Self Validation

Running self-validation produces a report containing the following information:

```text
# DEMA Consulting ReviewMark

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| Tool Version        | <version>                                          |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp> UTC                                    |

✓ ReviewMark_VersionDisplay - Passed
✓ ReviewMark_HelpDisplay - Passed
✓ ReviewMark_ReviewPlanGeneration - Passed
✓ ReviewMark_ReviewReportGeneration - Passed
✓ ReviewMark_IndexScan - Passed
✓ ReviewMark_WorkingDirectoryOverride - Passed
✓ ReviewMark_Enforce - Passed
✓ ReviewMark_Elaborate - Passed

Total Tests: 8
Passed: 8
Failed: 0
```

Each test in the report proves:

- **`ReviewMark_VersionDisplay`** - `--version` outputs a valid version string.
- **`ReviewMark_HelpDisplay`** - `--help` outputs usage and options information.
- **`ReviewMark_ReviewPlanGeneration`** - `--definition` + `--plan` generates a review plan.
- **`ReviewMark_ReviewReportGeneration`** - `--definition` + `--report` generates a review report.
- **`ReviewMark_IndexScan`** - `--index` scans PDF evidence files and writes `index.json`.
- **`ReviewMark_WorkingDirectoryOverride`** - `--dir` overrides the working directory for file operations.
- **`ReviewMark_Enforce`** - `--enforce` exits with non-zero code when reviews have issues.
- **`ReviewMark_Elaborate`** - `--elaborate` prints a Markdown elaboration of a review set.

See the [User Guide][link-guide] for more details on the self-validation tests.

On validation failure the tool will exit with a non-zero exit code.

## Documentation

Generated documentation includes:

- **Build Notes**: Release information and changes
- **User Guide**: Comprehensive usage documentation
- **Code Quality Report**: CodeQL and SonarCloud analysis results
- **Requirements**: Functional and non-functional requirements
- **Requirements Justifications**: Detailed requirement rationale
- **Trace Matrix**: Requirements to test traceability

## License

Copyright (c) DEMA Consulting. Licensed under the MIT License. See [LICENSE][link-license] for details.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

<!-- Badge References -->
[badge-forks]: https://img.shields.io/github/forks/demaconsulting/ReviewMark?style=plastic
[badge-stars]: https://img.shields.io/github/stars/demaconsulting/ReviewMark?style=plastic
[badge-contributors]: https://img.shields.io/github/contributors/demaconsulting/ReviewMark?style=plastic
[badge-license]: https://img.shields.io/github/license/demaconsulting/ReviewMark?style=plastic
[badge-build]: https://img.shields.io/github/actions/workflow/status/demaconsulting/ReviewMark/build_on_push.yaml?style=plastic
[badge-quality]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_ReviewMark&metric=alert_status
[badge-security]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_ReviewMark&metric=security_rating
[badge-nuget]: https://img.shields.io/nuget/v/DemaConsulting.ReviewMark?style=plastic

<!-- Link References -->
[link-forks]: https://github.com/demaconsulting/ReviewMark/network/members
[link-stars]: https://github.com/demaconsulting/ReviewMark/stargazers
[link-contributors]: https://github.com/demaconsulting/ReviewMark/graphs/contributors
[link-license]: https://github.com/demaconsulting/ReviewMark/blob/main/LICENSE
[link-build]: https://github.com/demaconsulting/ReviewMark/actions/workflows/build_on_push.yaml
[link-quality]: https://sonarcloud.io/dashboard?id=demaconsulting_ReviewMark
[link-security]: https://sonarcloud.io/dashboard?id=demaconsulting_ReviewMark
[link-nuget]: https://www.nuget.org/packages/DemaConsulting.ReviewMark
[link-guide]: https://github.com/demaconsulting/ReviewMark/blob/main/docs/guide/guide.md
[link-theory-of-operations]: https://github.com/demaconsulting/ReviewMark/blob/main/THEORY-OF-OPERATIONS.md
[link-continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
