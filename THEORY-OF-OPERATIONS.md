# File Review

In regulated environments, software artifacts — source files, configuration, requirements, test code —
must be formally reviewed before release. Tracking which files have been reviewed, whether those reviews
are current, and whether every file in the repository is covered by a review is a significant manual
burden. Without automation, reviews go stale silently and coverage gaps are discovered only at audit time.

**ReviewMark** automates file-review evidence management. It computes cryptographic fingerprints of
defined file-sets, queries a review evidence store (URL or file-share) for corresponding code-review
PDFs, and produces two compliance documents with every CI/CD run.

## How It Works

### Review Definition File

Reviews are configured in a `.reviewmark.yaml` definition file at the repository root:

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
      - "!src/Core/Generated/**"   # exclude auto-generated files within the set
  - id: Security-Layer
    title: Review of authentication and authorization
    paths:
      - "src/Auth/**/*.cs"
```

The `needs-review` section defines the full set of files in the repository that are subject to
review. ReviewMark uses this to detect any file that is not covered by any review-set — a coverage
gap that would otherwise go unnoticed until an audit.

Each review-set is a named group of ordered glob patterns. Patterns prefixed with `!` are exclusions
and are applied in the order they appear, allowing fine-grained control over which files are included.

### Fingerprinting

The fingerprint for a review-set is computed as follows:

1. Resolve all files matched by the ordered include/exclude glob patterns.
2. Compute a SHA256 hash of the content of each matched file.
3. Sort the resulting SHA256 hashes lexicographically.
4. Concatenate the sorted hashes into a single string.
5. Compute a final SHA256 hash of that concatenated string.

Sorting by content hash rather than file path means that **renaming a file does not invalidate the
review** — only actual content changes cause the fingerprint to change. This avoids spurious review
expiry due to refactoring or directory restructuring.

### Evidence Source

ReviewMark queries the configured evidence source for review PDFs. Two source types are supported:

| Type | Description |
| :--- | :---------- |
| `url` | HTTP/HTTPS endpoint; credentials supplied via environment variables |
| `fileshare` | UNC or local file-system path; credentials supplied via environment variables |

#### Evidence Index

Rather than imposing a file-naming convention on the evidence store, ReviewMark uses an
**index file** — `index.json` — located at the root of the evidence source. The index is a
machine-maintained catalogue of all available review PDFs and their metadata. ReviewMark fetches
this index at the start of each run and looks up each review-set by ID and fingerprint.

The index has the following structure:

```json
{
  "reviews": [
    {
      "id": "Core-Logic",
      "fingerprint": "a3f9c2d1e4b5...",
      "date": "2026-02-14",
      "result": "pass",
      "file": "CR-2026-014 Core Logic Review.pdf"
    },
    {
      "id": "Security-Layer",
      "fingerprint": "c72b8a3f91e0...",
      "date": "2025-11-03",
      "result": "pass",
      "file": "CR-2025-089 Security Layer Review.pdf"
    }
  ]
}
```

| Field | Description |
| :---- | :---------- |
| `id` | Matches a review ID in the definition file |
| `fingerprint` | The file-set fingerprint at the time of review |
| `date` | Date the review was completed |
| `result` | Outcome of the review: `pass` or `fail` |
| `file` | File name of the evidence PDF in the evidence store |

ReviewMark determines review status by looking up the current review ID and fingerprint in the
index:

| Status | Condition |
| :----- | :-------- |
| **Current** | An entry exists matching both `id` and `fingerprint`, with `result` of `pass` |
| **Stale** | One or more entries exist matching `id`, but none match the current `fingerprint` |
| **Missing** | No entries exist for the `id` at all |

A **stale** review means the files have changed since the last review was performed — the review
evidence exists but no longer corresponds to the current file-set. A **missing** review means no
review has ever been recorded for this review-set.

#### Re-indexing

The index is not maintained by hand. Instead, ReviewMark provides a `--index` command that scans
PDF evidence files matching a glob path, reads the embedded metadata from each PDF using
[PdfSharp](https://github.com/empira/PDFsharp), and writes an up-to-date `index.json` to the
working directory.

Use `--dir` to set the working directory without changing the process directory:

```bash
dotnet reviewmark --dir \\reviews.example.com\evidence\ --index "**/*.pdf"
```

Alternatively, change to the directory first:

```bash
cd \\reviews.example.com\evidence\
dotnet reviewmark --index "**/*.pdf"
```

Review teams deposit completed review PDFs into the evidence store folder with whatever file name
their QMS document-numbering standard requires. Running `--index` regenerates the index from the
PDF metadata — the tool never dictates file names.

#### PDF Metadata Format

ReviewMark reads review metadata from the standard PDF **Keywords** field, using a simple
`name=value` space-separated format:

```text
id=Core-Logic fingerprint=a3f9c2d1e4b5... date=2026-03-08 result=pass
```

| Key | Description |
| :-- | :---------- |
| `id` | The review ID matching the definition file |
| `fingerprint` | The file-set fingerprint at the time of review |
| `date` | Date the review was completed (ISO 8601: `YYYY-MM-DD`) |
| `result` | Outcome of the review: `pass` or `fail` |

Using the standard Keywords field means the metadata is readable by any PDF viewer or document
management system without requiring custom property support. PDFs that do not carry the expected
keys in their Keywords field are skipped with a warning during indexing.

#### Credentials

Credentials for protected evidence sources are supplied as environment variables, keeping secrets out
of the definition file and out of source control. The expected variable names are configured per
source:

```yaml
evidence-source:
  type: url
  location: https://reviews.example.com/evidence/
  credentials:
    username-env: REVIEWMARK_USER
    password-env: REVIEWMARK_TOKEN
```

In CI/CD, these are mapped from repository or organization secrets:

```yaml
- name: Run ReviewMark
  env:
    REVIEWMARK_USER: ${{ secrets.REVIEW_USER }}
    REVIEWMARK_TOKEN: ${{ secrets.REVIEW_TOKEN }}
  run: >
    dotnet reviewmark
    --definition file-review.yaml
    --plan    docs/review/review-plan.md
    --report  docs/review/review-report.md
    --enforce
```

## Outputs

### Review Plan

The review plan is a Markdown document proving that all files subject to review are covered by at
least one review-set. It lists each review-set, the files it covers, and reports coverage status:

```markdown
## Review Coverage

| Review ID      | Title                          | Files | Fingerprint  |
| :------------- | :----------------------------- | ----: | :----------- |
| Core-Logic     | Review of core business logic  | 14    | `a3f9…`      |
| Security-Layer | Review of auth/authorization   | 6     | `c72b…`      |

### Coverage

⚠ 2 file(s) require review but are not covered by any review-set:
- `src/Utilities/StringHelper.cs`
- `src/Utilities/DateHelper.cs`
```

When all files are covered the `Coverage` subsection reads:

```markdown
### Coverage

All files requiring review are covered by a review-set.
```

### Review Report

The review report shows the status of each review against the current file-set fingerprint:

```markdown
## Review Status

| Review ID      | Status       | Date       | Result |
| :------------- | :----------- | :--------- | :----- |
| Core-Logic     | ✅ Current   | 2026-02-14 | Pass   |
| Security-Layer | ⚠ Stale     | 2025-11-03 | Pass   |
| Auth-Module    | ❌ Failed    | 2026-03-01 | Fail   |
| Persistence    | ❌ Missing   |            |        |

### Referenced Documents

- Core-Logic: CR-2026-014 Core Logic Review.pdf
- Security-Layer: CR-2025-089 Security Layer Review.pdf
- Auth-Module: CR-2026-021 Auth Module Review.pdf
```

- **Current** — the index contains a matching entry for the current ID and fingerprint with a `pass` result.
- **Failed** — the index contains a matching entry for the current ID and fingerprint but the result is not `pass`.
- **Stale** — the index contains entries for the ID, but none match the current fingerprint;
  the most recent entry's date is shown in the table and the referenced document is listed below.
- **Missing** — the index contains no entries for the ID at all.

## Enforcement

The `--enforce` flag causes ReviewMark to exit with a non-zero code if any review-set is failed,
stale, or missing, or if any file matching `needs-review` is not covered by a review-set. This blocks
downstream pipeline stages until the issues are resolved:

```bash
dotnet reviewmark \
  --definition file-review.yaml \
  --plan    docs/review/review-plan.md \
  --report  docs/review/review-report.md \
  --enforce
```

## CI/CD Integration

ReviewMark runs in the document generation stage, after all build and test jobs are complete:

```yaml
- name: Run ReviewMark
  env:
    REVIEWMARK_USER: ${{ secrets.REVIEW_USER }}
    REVIEWMARK_TOKEN: ${{ secrets.REVIEW_TOKEN }}
  run: >
    dotnet reviewmark
    --definition file-review.yaml
    --plan    docs/review/review-plan.md
    --report  docs/review/review-report.md
    --enforce
```

The generated Markdown documents feed into the standard Pandoc → Weasyprint pipeline and are published
as PDF/A-3u release artifacts alongside the requirements trace matrix and code quality report.

## Indexing the Evidence Store

When new review PDFs are deposited into the evidence store, the index must be regenerated. This is
typically performed by the review team after completing a review, or as a scheduled job.

Use `--dir` to target the evidence store directly:

```bash
dotnet reviewmark --dir \\reviews.example.com\evidence\ --index "**/*.pdf"
```

Or change to the evidence store directory first:

```bash
cd \\reviews.example.com\evidence\
dotnet reviewmark --index "**/*.pdf"
```

ReviewMark scans all PDF files matching the glob path, reads the Keywords field from each using
PdfSharp, parses the `name=value` pairs, and writes a fresh `index.json` to the working directory.
PDFs whose Keywords field does not contain the required keys are skipped with a warning.

## Self-Validation

ReviewMark includes a built-in `--validate` command that verifies fingerprinting, index parsing,
evidence matching, and report generation using mock data — no live evidence store required:

```bash
dotnet reviewmark --validate --results artifacts/reviewmark-self-validation.trx
```

The resulting TRX file is consumed by [ReqStream](requirements.md) as test coverage evidence for
ReviewMark's own requirements.

## Standards Alignment

The review plan and review report together provide the artifact-review evidence required by:

- **IEC 62443** — design review and verification records
- **ISO 26262** — software unit and integration review evidence
- **DO-178C** — peer review records for software life-cycle data
