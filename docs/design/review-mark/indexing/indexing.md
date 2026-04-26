# Indexing Subsystem

## Overview

The Indexing subsystem is responsible for loading review evidence from an external index
and for safe file-path manipulation. It provides the lookup engine that determines whether
each review-set is Current, Stale, Missing, or Failed.

## Responsibilities

- Load the evidence index from a `none`, `fileshare`, or `url` source
- Scan a set of PDF files, extract structured metadata from the Keywords field, and
  produce an `index.json` evidence index
- Save the evidence index to a JSON file for later loading
- Provide safe path-combination utilities that prevent directory-traversal attacks

## Units

| Unit          | Source File                    | Purpose                                              |
|---------------|--------------------------------|------------------------------------------------------|
| ReviewIndex   | `Indexing/ReviewIndex.cs`      | Review evidence loader and query engine              |
| PathHelpers   | `Indexing/PathHelpers.cs`      | File path utilities (safe path combination)          |

## Cross-Unit Interaction and Data Flow

`ReviewIndex` is the primary unit of the subsystem. It depends on `GlobMatcher`
(from the Configuration subsystem) to resolve glob patterns into sorted file lists
during PDF scanning, and on `PathHelpers` (in this subsystem) for safe path
combination when constructing output file paths.

The data flow through the subsystem follows two distinct paths:

**Load path** (evidence already indexed):

1. `Program` calls `ReviewIndex.Load(EvidenceSource)` with the configured source.
2. `ReviewIndex` dispatches to the appropriate loader: empty index for `none`,
   local file read for `fileshare`, or HTTP download for `url`.
3. The loaded JSON is deserialized into internal `ReviewEvidence` records and
   stored in a two-level dictionary keyed by `(id, fingerprint)`.
4. The populated `ReviewIndex` is returned to `Program` for use in report
   generation.

**Scan path** (building the index from PDF evidence files):

1. `Program` calls `ReviewIndex.Scan(directory, paths, onWarning)`.
2. `GlobMatcher.GetMatchingFiles` resolves the glob patterns into a sorted list
   of PDF file paths.
3. For each matched file, `ReviewIndex` opens the PDF with PDFsharp and reads
   the `Keywords` document property.
4. The keywords string is parsed into key-value pairs; entries with all required
   fields (`id`, `fingerprint`, `date`, `result`) are added to the index.
5. PDFs that cannot be opened or are missing required metadata trigger the
   `onWarning` callback with a descriptive message.
6. The completed `ReviewIndex` is returned, and `Program` calls `Save()` to
   persist it as `index.json`.

## API

`ReviewIndex` exposes the following public API (all members are `internal` to the
assembly):

### Static Factory Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `Empty` | `() → ReviewIndex` | Returns a new empty index with no entries |
| `Load` | `(EvidenceSource) → ReviewIndex` | Loads the index from the configured source |
| `Load` | `(EvidenceSource, HttpClient) → ReviewIndex` | Testable overload with injected HttpClient |
| `Scan` | `(string dir, IReadOnlyList<string> paths, Action<string>? onWarning) → ReviewIndex` | Builds an index by scanning PDF files |

### Instance Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `Save` | `(string filePath)` | Saves the index to a JSON file |
| `Save` | `(Stream stream)` | Saves the index to a stream (testable overload) |
| `HasId` | `(string id) → bool` | Returns true if any evidence exists for the given ID |
| `GetEvidence` | `(string id, string fingerprint) → ReviewEvidence?` | Returns matching evidence or null |
| `GetAllForId` | `(string id) → IReadOnlyList<ReviewEvidence>` | Returns all evidence entries for an ID |

`PathHelpers` exposes:

| Method | Signature | Description |
|--------|-----------|-------------|
| `SafePathCombine` | `(string base, string relative) → string` | Combines paths, rejecting traversal sequences |

## Normal Operation

During a typical review plan or report generation run:

1. `ReviewIndex.Load` is called with the `EvidenceSource` from the configuration.
   - For `none` sources, an empty `ReviewIndex` is returned immediately with no
     file system or network access.
   - For `fileshare` sources, the JSON file at `EvidenceSource.Location` is read
     and deserialized.
   - For `url` sources, an HTTP GET request is issued to `EvidenceSource.Location`
     and the response body is deserialized as JSON.
2. The loaded index is passed to `ReviewMarkConfiguration.PublishReviewReport()`,
   which calls `GetEvidence` for each review-set to determine its status.
3. When the `--index` flag is used, `ReviewIndex.Scan` is called first to rebuild
   the index from PDF files, and `Save` is called to write `index.json`.

## Error Handling

- If the evidence source type is unrecognized, `Load` throws
  `InvalidOperationException` with a descriptive message.
- If the `fileshare` JSON file cannot be read or contains invalid JSON, `Load`
  throws `InvalidOperationException` wrapping the underlying exception.
- If the `url` HTTP request returns a non-success status code or the response
  body is not valid JSON, `Load` throws `InvalidOperationException`.
- If `filePath` is null or empty in `Save(string)`, `ArgumentException` is thrown.
- PDFs that cannot be opened during `Scan` produce a warning via `onWarning`
  and are skipped; the scan continues with remaining files.
- `SafePathCombine` throws `ArgumentException` for any path segment containing
  traversal sequences (`..`) or absolute paths.
