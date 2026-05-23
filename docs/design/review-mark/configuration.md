## Configuration Subsystem

### Overview

The Configuration subsystem is responsible for loading, validating, and processing the
ReviewMark YAML configuration file (`.reviewmark.yaml`). It also provides the
file-pattern-matching capability used to resolve glob patterns into concrete file lists.

### Responsibilities

- Deserialize `.reviewmark.yaml` into a strongly-typed configuration model
- Lint the loaded configuration and report any structural errors or warnings
- Resolve `needs-review` and per-review-set `paths` glob patterns into sorted file lists
- Compute SHA-256 fingerprints across resolved file sets
- Generate Review Plan and Review Report markdown documents
- Elaborate a review-set entry and produce a formatted Markdown description

### Units

| Unit | Source File | Purpose |
| --- | --- | --- |
| ReviewMarkConfiguration | `Configuration/ReviewMarkConfiguration.cs` | YAML parser and review-set processor |
| GlobMatcher | `Configuration/GlobMatcher.cs` | File pattern matching using glob syntax |

### Interfaces

`ReviewMarkConfiguration.Load(string path)` is the primary entry point. It reads and
deserializes the YAML file at `path`, lints the result, and returns a
`ReviewMarkLoadResult` with two members:

| Member | Type | Description |
| ------ | ---- | ----------- |
| `Configuration` | `ReviewMarkConfiguration?` | Parsed configuration, or `null` if loading failed |
| `Issues` | `IReadOnlyList<LintIssue>` | Lint errors and warnings found during loading |

When `Configuration` is non-null, the following properties are available on the `ReviewMarkConfiguration` object:

#### Properties

| Property | Type | Description |
| -------- | ---- | ----------- |
| `EvidenceSource` | `EvidenceSource` | Evidence-source configuration (type, location, optional credentials) |
| `Reviews` | `IReadOnlyList<ReviewSet>` | List of review-set definitions (Id, Title, Paths, fingerprinting methods) |

When `Configuration` is non-null, callers may invoke the following methods:

- **`GetNeedsReviewFiles(string dir)`** → `IReadOnlyList<string>` — Resolves `needs-review` glob patterns
- **`Reviews[i].GetFingerprint(string dir)`** → `string` — Computes a content-based
  SHA-256 fingerprint across the files resolved by the review-set's glob patterns.
  The fingerprint is rename-invariant (based on file content, not path). Called on
  individual `ReviewSet` instances from the `Reviews` collection.
- **`ElaborateReviewSet(string id, string dir, int markdownDepth = 1)`** → `ElaborateResult` —
  Builds an elaboration for one review-set
- **`PublishReviewPlan(string dir, int depth = 1)`** → `ReviewPlanResult` — Generates the Review Plan Markdown
- **`PublishReviewReport(ReviewIndex, string dir, int depth = 1)`** → `ReviewReportResult` —
  Produces Review Report

### Error Handling

- If the YAML file cannot be opened or is syntactically invalid, `Load()` returns a
  null `Configuration` with a descriptive entry in `Issues`.
- Structural lint errors (duplicate review IDs, unknown evidence-source type, missing
  required fields) are surfaced as `Issues` entries; `Configuration` may still be
  non-null for non-fatal errors.
- `ElaborateReviewSet` throws `ArgumentException` when the supplied `id` does not
  match any review-set in the configuration.
- File-system failures during glob pattern expansion (e.g., the working directory does
  not exist) propagate as `IOException` or `UnauthorizedAccessException` to the caller.

### Design

`ReviewMarkConfiguration` is the central orchestrator within the Configuration subsystem.
It holds the parsed configuration state and coordinates the two units:

- `GlobMatcher` is called by `ReviewMarkConfiguration` to expand the `needs-review` and
  per-review-set `paths` glob patterns into sorted file lists. `GlobMatcher` has no
  knowledge of `ReviewMarkConfiguration`; the dependency is one-directional.
- `ReviewMarkConfiguration` owns the SHA-256 fingerprinting algorithm, the Review Plan
  and Review Report generation, and the elaboration logic. None of these functions require
  knowledge of the Indexing subsystem; the `ReviewIndex` is passed in as a parameter to
  `PublishReviewReport()`.

The `Load()` method performs both parsing and linting in a single file read, keeping the
I/O surface minimal. Lint issues are accumulated into a list rather than thrown as
exceptions, allowing callers to distinguish parse failures from semantic warnings.

### Test Dependencies

Integration-level Configuration subsystem tests (in `ConfigurationTests.cs`) may use
`ReviewIndex` from the Indexing subsystem to construct realistic evidence fixtures when
exercising `PublishReviewReport()`. This is an intentional cross-subsystem dependency
limited to the test layer; the production `ReviewMarkConfiguration` code accepts
`ReviewIndex` only as a parameter and has no static import dependency on the Indexing
subsystem.
