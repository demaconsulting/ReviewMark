### ReviewMarkConfiguration

![Configuration Structure](ConfigurationView.svg)

#### Purpose

`ReviewMarkConfiguration` is responsible for parsing the `.reviewmark.yaml` configuration
file and performing all review-set processing. It coordinates file enumeration, fingerprint
computation, evidence lookup, and the generation of Review Plan and Review Report
compliance documents.

#### Data Model

**Top-level properties on a loaded `ReviewMarkConfiguration` instance:**

| Property | Type | Description |
| -------- | ---- | ----------- |
| `NeedsReviewPatterns` | `IReadOnlyList<string>` | Ordered list of glob patterns identifying files that need review (empty when omitted from YAML) |
| `EvidenceSource` | `EvidenceSource` | Parsed evidence-source block (`Type`, `Location`, optional credential env-var names) |
| `Reviews` | `IReadOnlyList<ReviewSet>` | Ordered list of review-set definitions |
| `GlobalContext` | `IReadOnlyList<string>` | Ordered list of glob patterns identifying global context files (empty when omitted from YAML) |

**YAML deserialization types (internal, not part of public API):**

| Class | Description |
| ----- | ----------- |
| `ReviewMarkYaml` | Root configuration object; contains `NeedsReview` patterns, `EvidenceSource`, `Reviews` list, and optional `Context` list |
| `EvidenceSourceYaml` | Describes how to locate the evidence index (`type`, `location`, optional `credentials`) |
| `ReviewSetYaml` | Describes a single review-set (`id`, `title`, `paths`, and optional `context` list) |
| `EvidenceCredentialsYaml` | Optional credentials block with `username-env` and `password-env` fields |

**Evidence source types:**

| Type | Description |
| ---- | ----------- |
| `none` | No evidence index; `location` is optional and ignored; all review-sets are Missing |
| `fileshare` | Evidence index read from the file path in `location` |
| `url` | Evidence index downloaded from the HTTP/HTTPS URL in `location` |

**Location environment-variable expansion:**

`ReviewMarkConfigurationHelpers.TryExpand` expands `${VAR}` references in `evidence-source.location`
only, using the regex `\$\$\{|\$\{([A-Za-z_][A-Za-z0-9_]*)\}` in a single left-to-right pass: `$${`
yields a literal `${`, and substituted values are never scanned again. An unset or empty variable, or any
remaining `${` that is not a valid reference, is an error naming the variable or quoting the malformed
reference text (never a substituted value). Sources of type `none` are skipped entirely, because their
`location` is ignored.
`BuildConfiguration` applies the expansion (throwing `ArgumentException` through `Parse`), and
`ValidateEvidenceSource` reports the same failure as an Error `LintIssue` so `Load` returns a null
configuration. Because expansion occurs in `BuildConfiguration`, it precedes the relative-path
resolution performed by `Load`. Credential fields (`username-env`, `password-env`) are env var names
and are not expanded.

**Result and internal API types:**

| Type | Description |
| ---- | ----------- |
| `EvidenceSource` | Immutable record: `Type`, `Location`, `UsernameEnv`, `PasswordEnv` |
| `ReviewSet` | Class with `Id`, `Title`, `Paths`, `Context`, `GetFingerprint(dir, constraint?)`, `GetFiles(dir, constraint?)` |
| `LintSeverity` | Enum: `Warning`, `Error` |
| `LintIssue` | Record: `Location`, `Severity`, `Description`; `ToString()` formats as `{location}: {severity}: {description}` |
| `ReviewMarkLoadResult` | Record: `Configuration` (null if error-level issues found), `Issues` |
| `ReviewPlanResult` | Record: `Markdown`, `HasIssues` |
| `ReviewReportResult` | Record: `Markdown`, `HasIssues` |
| `ElaborateResult` | Record: `Markdown` |
| `AffectedReviewSet` | Record: `Id`, `Files` (sorted changed files matching the review set's paths) |
| `AffectedResult` | Record: `ReviewSets` (definition order), `Unreviewed` (sorted) |

#### Key Methods

**`ReviewMarkConfiguration.GetNeedsReviewFiles(string directory)`** → `IReadOnlyList<string>` (sorted, relative paths)

Resolves `NeedsReviewPatterns` against `directory` using `GlobMatcher` and returns a sorted list
of relative file paths that require review. This method is used by `PublishReviewPlan`,
`PublishReviewReport`, and `ElaborateReviewSet` to compute the constraint passed to
`ReviewSet.GetFiles(dir, constraint)` and `ReviewSet.GetFingerprint(dir, constraint)`. When
`NeedsReviewPatterns` is empty the callers treat the result as an absent constraint (null) rather
than an empty set, preserving backward compatibility.

**`ReviewMarkConfiguration.Load(string filePath)`** → `ReviewMarkLoadResult`

- *Parameters*: `string filePath` — path to the `.reviewmark.yaml` file
- *Returns*: `ReviewMarkLoadResult` — contains `Configuration` (null on error) and `Issues`
- *Preconditions*: `filePath` is a valid file path
- *Postconditions*: All detectable lint issues are in `Issues`; `Configuration` is non-null
  only when no error-level issues were found

Reads the YAML file, then calls `ReviewMarkConfigurationHelpers.DeserializeRaw()` which
deserializes via YamlDotNet with `NullNamingConvention` and `IgnoreUnmatchedProperties`,
relying on `[YamlMember(Alias = "...")]` attributes for all key mappings. Validation is
delegated to `ValidateEvidenceSource()` and `ValidateReviews()`, which accumulate issues
into a shared list. Both parsing and linting are performed in a single file read.

**`ReviewMarkConfiguration.Parse(string yaml)`** → `ReviewMarkConfiguration`

- *Parameters*: `string yaml` — YAML content to parse
- *Returns*: A populated `ReviewMarkConfiguration` instance
- *Preconditions*: `yaml` is non-null
- *Postconditions*: Returns a fully populated configuration; throws on any parse or validation
  error rather than accumulating issues

Delegates to `ReviewMarkConfigurationHelpers.DeserializeRaw(yaml, filePath: null)` followed
by `BuildConfiguration()`. Because no file path is provided, YAML errors surface as
`ArgumentException` (not `InvalidOperationException`), preserving the unit-test contract.
Missing required fields (no evidence-source block) also result in
`ArgumentException`. Used by unit tests to construct a `ReviewMarkConfiguration` directly
from YAML strings without touching the file system.

**`ReviewMarkConfiguration.PublishReviewPlan(string dir, int depth = 1)`** → `ReviewPlanResult`

Computes the needs-review file set once via `GlobMatcher.GetMatchingFiles()` and converts it to a
`HashSet<string>` constraint when `NeedsReviewPatterns` is non-empty (null constraint when empty, for
backward compatibility). For each review set, resolves its file list by calling `ReviewSet.GetFiles(dir,
constraint)` and its fingerprint by calling `ReviewSet.GetFingerprint(dir, constraint)` with the same
constraint. The constraint limits each review set to files in the needs-review set, so build artifacts
excluded from needs-review are also excluded from coverage and fingerprints. Returns the Markdown document
and a boolean indicating whether any files lack coverage. The `depth` parameter controls heading level.

**`ReviewMarkConfiguration.PublishReviewReport(ReviewIndex index, string dir, int depth = 1)`**
→ `ReviewReportResult`

Computes the needs-review constraint (same logic as `PublishReviewPlan`) once and reuses it for all
review sets. For each review-set, calls `ReviewSet.GetFingerprint(dir, constraint)` so that build
artifacts excluded from needs-review do not affect the hash. Calls `index.GetEvidence(id, fingerprint)`
to determine status (Current, Stale, Missing, or Failed), and generates the report table. Returns the
Markdown document and a boolean indicating whether any review-set is non-current.

**`ReviewMarkConfiguration.ElaborateReviewSet(string id, string dir, int markdownDepth = 1)`**
→ `ElaborateResult`

Looks up the review-set with the given `id`. Computes the needs-review constraint (same logic as
`PublishReviewPlan`). Calls `ReviewSet.GetFingerprint(dir, constraint)` and `ReviewSet.GetFiles(dir,
constraint)` with the constraint so that build artifacts excluded from needs-review are also excluded
from the elaborated file list and fingerprint. Returns a Markdown document with a heading at
`markdownDepth`, a metadata table (ID, Title, Fingerprint), an optional Context subsection, and a
Files subheading. The Context subsection lists all resolved context files as plain paths, and is
omitted when no context files resolve.

**`ReviewMarkConfiguration.GetAffectedReviewSets(IEnumerable<string> changedPaths)`**
→ `AffectedResult`

Maps normalized changed paths onto review sets using path-string matching only
(`GlobMatcher.IsMatch()`); no file-system access occurs, so deleted files are handled. A changed
file is considered only if it matches `NeedsReviewPatterns` (or `NeedsReviewPatterns` is empty);
other files are ignored. For each review set in definition order, the files matching its `Paths`
make it affected. Context patterns are not consulted because context files are not fingerprinted.
Considered files that match no review set's `Paths` (after ordered include/exclude evaluation)
are returned in `Unreviewed`: "unreviewed" therefore means the path matches `needs-review` (or
`needs-review` is empty) but no review set's `paths`; `context` patterns do not count.
Files are de-duplicated and sorted with `StringComparer.Ordinal`.

**`ReviewMarkLoadResult.ReportIssues(Context context, bool includeWarnings = true, bool alwaysWriteErrors = false)`**

Routes errors to `Context.WriteError(message, alwaysWriteErrors)` and, when `includeWarnings` is
true, warnings to `Context.WriteLine()`. `--affected` mode passes `false` for `includeWarnings` so
warnings do not pollute stdout, and `true` for `alwaysWriteErrors` so errors reach stderr even
with `--silent`.

**Needs-review constraint:** `PublishReviewPlan`, `PublishReviewReport`, and `ElaborateReviewSet`
each compute a needs-review constraint before iterating review sets. When `NeedsReviewPatterns`
is non-empty, the constraint is a `HashSet<string>` of all files matched by the top-level
`needs-review` patterns. When `NeedsReviewPatterns` is empty (the key is absent or the list
contains only whitespace entries), the constraint is `null` and no filtering is applied — this
preserves backward compatibility for configurations written before `needs-review` was introduced.

**`ReviewSet.GetFiles(string dir, IReadOnlySet<string>? constraint = null)`**
→ `IReadOnlyList<string>` (sorted, relative paths)

Calls `GlobMatcher.GetMatchingFiles()` with the review set's ordered `Paths` patterns. When
`constraint` is non-null, filters the result to include only files whose relative paths appear
in the constraint set. When `constraint` is null, returns all glob-matched files without filtering.
Context patterns are never passed here; context is not under review.

**`ReviewSet.GetFingerprint(string dir, IReadOnlySet<string>? constraint = null)`** → `string`

Delegates to `GetFiles(dir, constraint)` to obtain the constrained file list, then applies the
fingerprinting algorithm below.

**Combined ordered context resolution:** Global and per-review-set context patterns are
concatenated into a single ordered pattern list (`GlobalContext` first, then `review.Context`)
and passed as a single call to `GlobMatcher.GetMatchingFiles()`. This leverages `GlobMatcher`'s
existing ordered include/exclude semantics so that a per-review-set exclusion pattern (prefixed
with `!`) can suppress a file that was added by a global context pattern, without requiring any
change to the global configuration. Because `GlobMatcher` deduplicates internally via its
`HashSet` accumulator, no explicit `Distinct()` call is needed.

**The files-under-review set is resolved before context** so that any context file that also
appears in the review set's `paths:` list is suppressed from the Context subsection; a file
cannot serve both purposes in the same elaboration output.**
Throws `ArgumentException`
for unknown IDs; throws `ArgumentOutOfRangeException` when `markdownDepth > 5`.

**Coverage note:** Context entries are reference material only. A file listed in `context:` is
not considered covered for needs-review purposes. Only `paths:` entries provide review coverage.
A file that matches `needs-review` but appears only in `context:` will still be reported as
uncovered by `PublishReviewPlan()`.

**Fingerprinting algorithm:**

1. For each file in the review-set, read its contents and compute a SHA-256 hash.
2. Convert each hash to a lowercase hex string; collect all per-file hashes and sort them
   lexicographically.
3. Concatenate the sorted hashes and compute a SHA-256 hash of the result.
4. Return the final hash as a hex string.

Sorting per-file hashes before combining ensures the fingerprint is insensitive to
file enumeration order but sensitive to content changes.

**`ValidateEvidenceSource()`** (internal): validates the `evidence-source` block; adds
errors for null block, missing or unknown `type`, and missing `location` for non-`none`
types.

**`ValidateReviews()`** (internal): iterates reviews by index; adds errors for missing
`id`, duplicate `id`, missing `title`, and missing or empty `paths`.

#### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| None | File open failure or unexpected I/O in `Load()` | Caught inside `Load()` and returned as an error-level `LintIssue` with a null `Configuration`; nothing is propagated. Reported by `ReportIssues()` |
| `ArgumentException` | Unknown review-set ID in `ElaborateReviewSet()` | Propagated to `Program.RunDefinitionLogic()`, which catches it and calls `context.WriteError()` |
| `ArgumentOutOfRangeException` | `markdownDepth > 5` in `ElaborateReviewSet()` | Propagated to the caller |
| `IOException` / `UnauthorizedAccessException` | File-system failure during glob pattern expansion | Propagated to the caller |

Lint errors (duplicate IDs, missing fields, invalid evidence-source type) are surfaced as
`LintIssue` entries in `ReviewMarkLoadResult.Issues` and do not cause exceptions.

#### Dependencies

- **`GlobMatcher`** (Configuration subsystem) — called to resolve `needs-review` and
  review-set glob patterns into sorted file lists
- **`ReviewIndex`** (Indexing subsystem) — accepted as a parameter in
  `PublishReviewReport()`; no static import dependency (passed in by the caller)
- **`YamlDotNet`** (OTS) — used by `Load()` to deserialize the `.reviewmark.yaml` file
  into the internal model types

#### Callers

- **`Program.RunLintLogic()`** — calls `Load()` then reports issues via `ReportIssues()`
- **`Program.RunDefinitionLogic()`** — calls `Load()`, `PublishReviewPlan()`,
  `PublishReviewReport()`, and `ElaborateReviewSet()`
