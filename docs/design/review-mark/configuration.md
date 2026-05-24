## Configuration

The Configuration subsystem is responsible for loading, validating, and processing the
ReviewMark YAML configuration file (`.reviewmark.yaml`). It provides file-pattern matching
and drives the generation of Review Plan and Review Report compliance documents.

### Overview

The Configuration subsystem solves the problem of interpreting the user-authored
`.reviewmark.yaml` file and turning it into actionable data for the rest of the tool. Its
boundaries are: it reads the definition YAML from disk and resolves glob patterns via
`GlobMatcher`; it does not load evidence indexes (that is the Indexing subsystem's
responsibility) but accepts a `ReviewIndex` as a parameter for report generation.

It contains two units:

| Unit | Source File | Purpose |
| ---- | ----------- | ------- |
| ReviewMarkConfiguration | `Configuration/ReviewMarkConfiguration.cs` | YAML parser and review-set processor |
| GlobMatcher | `Configuration/GlobMatcher.cs` | File pattern matching using glob syntax |

See the *ReviewMarkConfiguration Design* and *GlobMatcher Design* for full unit details.

### Interfaces

**`ReviewMarkConfiguration.Load(string filePath)`** → `ReviewMarkLoadResult`

- *Type*: In-process .NET static factory method
- *Role*: Provider — called by `Program.RunLintLogic()` and `Program.RunDefinitionLogic()`
- *Contract*: Reads and deserializes `.reviewmark.yaml`, lints the result, and returns a
  `ReviewMarkLoadResult` with `Configuration` (or `null` on error) and `Issues`
- *Constraints*: Returns a non-null `Configuration` only when no error-level issues are
  found. Lint errors and warnings are accumulated as `LintIssue` records.

When `Configuration` is non-null, the following instance properties and methods are available:

- **`EvidenceSource`** → `EvidenceSource` — parsed evidence-source block (`Type`, `Location`,
  optional credential env-var names)
- **`Reviews`** → `IReadOnlyList<ReviewSet>` — ordered list of review-set definitions
- **`GetNeedsReviewFiles(string dir)`** → `IReadOnlyList<string>`
- **`ElaborateReviewSet(string id, string dir, int markdownDepth = 1)`** → `ElaborateResult`
- **`PublishReviewPlan(string dir, int depth = 1)`** → `ReviewPlanResult`
- **`PublishReviewReport(ReviewIndex index, string dir, int depth = 1)`** → `ReviewReportResult`

**`GlobMatcher.GetMatchingFiles(string baseDirectory, IReadOnlyList<string> patterns)`**
→ `IReadOnlyList<string>`

- *Type*: In-process .NET static method
- *Role*: Provider — called by `ReviewMarkConfiguration` and `ReviewIndex.Scan()`
- *Contract*: Returns a sorted, deduplicated list of relative file paths matching the
  ordered include/exclude glob patterns; `!`-prefixed patterns are exclusions
- *Constraints*: Throws `ArgumentNullException` for null inputs; throws `ArgumentException`
  for empty base directory; file-system exceptions propagate to the caller

### Design

`ReviewMarkConfiguration` is the central unit. It coordinates `GlobMatcher` for file
enumeration and owns SHA-256 fingerprinting, document generation, and elaboration logic.

1. `Load()` reads the YAML file, deserializes it into internal model types via YamlDotNet,
   and delegates validation to `ValidateEvidenceSource()` and `ValidateReviews()`, which
   accumulate issues into a shared list. A `null` configuration is returned only when
   error-level issues are found.
2. For `PublishReviewPlan()` and `PublishReviewReport()`, `ReviewMarkConfiguration` calls
   `GlobMatcher.GetMatchingFiles()` to resolve each review-set's `paths` patterns and the
   top-level `needs-review` patterns into sorted file lists.
3. Fingerprinting is content-based and order-independent: per-file SHA-256 hashes are
   sorted lexicographically before concatenation and re-hashing, ensuring the fingerprint
   is insensitive to file enumeration order but sensitive to content changes.
4. `PublishReviewReport()` accepts `ReviewIndex` as a parameter and calls `GetEvidence()`
   for each review-set to determine Current, Stale, Missing, or Failed status.

`GlobMatcher` is stateless and has no knowledge of `ReviewMarkConfiguration`; the
dependency is strictly one-directional. Integration-level Configuration subsystem tests
may pass `ReviewIndex` objects from the Indexing subsystem to `PublishReviewReport()`;
this cross-subsystem dependency is limited to the test layer.
