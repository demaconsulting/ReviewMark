## Indexing

The Indexing subsystem is responsible for loading review evidence from an external index
and for safe file-path manipulation. It provides the lookup engine that determines whether
each review-set is Current, Stale, Missing, or Failed.

### Overview

The Indexing subsystem solves the problem of abstracting the evidence store so that callers
do not need to know whether evidence was loaded from a file share, downloaded over HTTP,
built from PDF scans, or is simply absent. Its boundary is: it reads evidence from external
sources (files, URLs, PDFs) and exposes a uniform query interface; it does not interpret
review-set definitions but accepts glob patterns from `GlobMatcher` for PDF scanning.

It contains two units:

| Unit | Source File | Purpose |
| ---- | ----------- | ------- |
| ReviewIndex | `Indexing/ReviewIndex.cs` | Review evidence loader and query engine |
| PathHelpers | `Indexing/PathHelpers.cs` | Safe path-combination utility |

See the *ReviewIndex Design* and *PathHelpers Design* for full unit details.

### Interfaces

**`ReviewIndex` static factory methods:**

- **`ReviewIndex.Empty()`** → `ReviewIndex` — empty index with no entries
- **`ReviewIndex.Load(EvidenceSource)`** → `ReviewIndex` — loads from configured source
- **`ReviewIndex.Load(EvidenceSource, HttpClient)`** → `ReviewIndex` — testable overload
- **`ReviewIndex.Scan(string dir, IReadOnlyList<string> paths, Action<string>? onWarning)`**
  → `ReviewIndex` — builds an index by scanning PDF files

**`ReviewIndex` instance methods:**

- **`Save(string filePath)`** — writes the index to a JSON file
- **`Save(Stream stream)`** — writes the index to a stream (testable overload)
- **`HasId(string id)`** → `bool` — true if any evidence exists for the given ID
- **`GetEvidence(string id, string fingerprint)`** → `ReviewEvidence?` — matching record or null
- **`GetAllForId(string id)`** → `IReadOnlyList<ReviewEvidence>` — all records for an ID

**`PathHelpers`:**

- **`SafePathCombine(string basePath, string relativePath)`** → `string` — combines paths,
  rejecting traversal sequences

The subsystem exposes no public types beyond `ReviewIndex`, `ReviewEvidence`, and
`PathHelpers`; all members are `internal` to the assembly.

### Design

`ReviewIndex` is the primary unit. It owns all evidence-store interaction and exposes a
uniform interface regardless of source type. `PathHelpers` is a pure stateless utility
called by `ReviewIndex.Scan()`.

**Load path** (evidence already indexed):

1. `Program` calls `ReviewIndex.Load(EvidenceSource)`.
2. `ReviewIndex` dispatches to the appropriate loader: empty index for `none`, local file
   read for `fileshare`, or HTTP download for `url` (with optional Basic-auth credentials
   from environment variables named by `UsernameEnv` and `PasswordEnv`).
3. The loaded JSON is deserialized into `ReviewEvidence` records and stored in a two-level
   `Dictionary<string, Dictionary<string, ReviewEvidence>>` keyed by `(id, fingerprint)`.
4. The populated `ReviewIndex` is returned for use in report generation.

**Scan path** (building the index from PDF evidence files):

1. `Program` calls `ReviewIndex.Scan(directory, paths, onWarning)`.
2. `GlobMatcher.GetMatchingFiles()` resolves the glob patterns into sorted PDF file paths.
3. For each matched file, `ReviewIndex` opens the PDF with PDFsharp and reads the
   `Keywords` document property; the keywords string is parsed into key-value pairs.
4. Entries with all required fields (`id`, `fingerprint`, `date`, `result`) are added to
   the index; PDFs that cannot be opened or are missing required metadata trigger the
   `onWarning` callback.
5. The completed `ReviewIndex` is returned, and `Program` calls `Save()` to persist
   `index.json`.

The two-level dictionary storage enables O(1) evidence lookup. The testable
`Load(EvidenceSource, HttpClient)` overload and the `Save(Stream)` overload allow unit
tests to exercise loading and saving without real network or file-system access.
