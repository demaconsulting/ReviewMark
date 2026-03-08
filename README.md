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

- **Cryptographic Fingerprinting**: Computes SHA256 fingerprints of defined file-sets so that
  content changes are detected and reviews go stale automatically
- **Evidence Querying**: Queries a review evidence store (URL or file-share) for corresponding
  code-review PDFs via an `index.json` catalogue
- **Coverage Reporting**: Generates a review plan showing which files are covered and flagging
  any files that are not included in any review-set
- **Status Reporting**: Generates a review report showing whether each review-set is Current,
  Stale, or Missing
- **Enforcement**: `--enforce` flag causes a non-zero exit code if any review-set is stale or
  missing, or if any file is not covered by a review-set
- **Re-indexing**: `--reindex` command scans a folder of review PDFs and regenerates `index.json`
  from PDF Keywords metadata
- **Self-Validation**: Built-in validation tests with TRX/JUnit output
- **Multi-Platform Support**: Builds and runs on Windows, Linux, and macOS
- **Multi-Runtime Support**: Targets .NET 8, 9, and 10
- **Comprehensive CI/CD**: GitHub Actions workflows with quality checks, builds, and
  integration tests
- **Continuous Compliance**: Compliance evidence generated automatically on every CI run, following
  the [Continuous Compliance][link-continuous-compliance] methodology

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

See [ARCHITECTURE.md][link-architecture] for the full design including fingerprinting, evidence
indexing, and compliance report formats.

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

| Option               | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| `-v`, `--version`    | Display version information                                  |
| `-?`, `-h`, `--help` | Display help message                                         |
| `--silent`           | Suppress console output                                      |
| `--validate`         | Run self-validation                                          |
| `--results <file>`   | Write validation results to file (TRX or JUnit format)       |
| `--log <file>`       | Write output to log file                                     |

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

Total Tests: 2
Passed: 2
Failed: 0
```

Each test in the report proves:

- **`ReviewMark_VersionDisplay`** - `--version` outputs a valid version string.
- **`ReviewMark_HelpDisplay`** - `--help` outputs usage and options information.

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
[link-architecture]: https://github.com/demaconsulting/ReviewMark/blob/main/ARCHITECTURE.md
[link-continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
