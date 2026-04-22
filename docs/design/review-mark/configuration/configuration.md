# Configuration Subsystem

## Overview

The Configuration subsystem is responsible for loading, validating, and processing the
ReviewMark YAML configuration file (`.reviewmark.yaml`). It also provides the
file-pattern-matching capability used to resolve glob patterns into concrete file lists.

## Responsibilities

- Deserialize `.reviewmark.yaml` into a strongly-typed configuration model
- Lint the loaded configuration and report any structural errors or warnings
- Resolve `needs-review` and per-review-set `paths` glob patterns into sorted file lists
- Compute SHA-256 fingerprints across resolved file sets
- Generate Review Plan and Review Report markdown documents
- Elaborate a review-set entry and produce a formatted Markdown description

## Units

| Unit | Source File | Purpose |
| --- | --- | --- |
| ReviewMarkConfiguration | `Configuration/ReviewMarkConfiguration.cs` | YAML parser and review-set processor |
| GlobMatcher | `Configuration/GlobMatcher.cs` | File pattern matching using glob syntax |

## Interfaces / API

`ReviewMarkConfiguration.Load(string path)` is the primary entry point. It reads and
deserializes the YAML file at `path`, lints the result, and returns a
`ReviewMarkLoadResult` with two members:

| Member | Type | Description |
| ------ | ---- | ----------- |
| `Configuration` | `ReviewMarkConfiguration?` | Parsed configuration, or `null` if loading failed |
| `Issues` | `IReadOnlyList<LintIssue>` | Lint errors and warnings found during loading |

When `Configuration` is non-null, callers may invoke the following methods:

| Method | Signature | Returns | Description |
| ------ | --------- | ------- | ----------- |
| `GetNeedsReviewFiles` | `(string workingDirectory)` | `IReadOnlyList<string>` | Resolves `needs-review` glob patterns |
| `ElaborateReviewSet` | `(string id, string workingDirectory)` | `ElaborateResult` | Builds an elaboration for one review-set |
| `PublishReviewPlan` | `(string workingDirectory, int depth = 1)` | `ReviewPlanResult` | Generates the Review Plan Markdown |
| `PublishReviewReport` | `(ReviewIndex, string workingDirectory, int depth = 1)` | `ReviewReportResult` | Generates the Review Report Markdown |

## Error Handling

- If the YAML file cannot be opened or is syntactically invalid, `Load()` returns a
  null `Configuration` with a descriptive entry in `Issues`.
- Structural lint errors (duplicate review IDs, unknown evidence-source type, missing
  required fields) are surfaced as `Issues` entries; `Configuration` may still be
  non-null for non-fatal errors.
- `ElaborateReviewSet` throws `ArgumentException` when the supplied `id` does not
  match any review-set in the configuration.
- File-system failures during glob pattern expansion (e.g., the working directory does
  not exist) propagate as `IOException` or `UnauthorizedAccessException` to the caller.
