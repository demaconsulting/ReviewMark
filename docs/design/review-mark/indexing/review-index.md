### ReviewIndex

#### Overview

`ReviewIndex` manages the loading, querying, and building of the review evidence index.
It abstracts the evidence store behind a uniform in-memory interface so that callers
do not need to know whether evidence was loaded from a file share, downloaded over HTTP,
built from PDF scans, or is simply absent.

#### Interfaces

Static factory methods:

- **`ReviewIndex.Empty()`** → `ReviewIndex`
- **`ReviewIndex.Load(EvidenceSource)`** → `ReviewIndex`
- **`ReviewIndex.Load(EvidenceSource, HttpClient)`** → `ReviewIndex` (testable overload)
- **`ReviewIndex.Scan(string dir, IReadOnlyList<string> paths, Action<string>? onWarning)`** → `ReviewIndex`

Instance methods:

- **`Save(string filePath)`**, **`Save(Stream stream)`**
- **`HasId(string id)`** → `bool`
- **`GetEvidence(string id, string fingerprint)`** → `ReviewEvidence?`
- **`GetAllForId(string id)`** → `IReadOnlyList<ReviewEvidence>`

#### Purpose

The `ReviewIndex` software unit manages the loading, querying, and creation of the review
evidence index. It abstracts the evidence store behind a uniform interface so that
the rest of the tool does not need to know whether evidence is stored on a fileshare,
served over HTTP, or absent entirely.

#### Data Model

`ReviewIndex` maintains a two-level dictionary as its sole instance state, keyed first
by review-set `Id` and then by `Fingerprint`:

| Field | Type | Description |
| ----- | ---- | ----------- |
| Internal store | `Dictionary<string, Dictionary<string, ReviewEvidence>>` | Evidence records indexed by `(Id, Fingerprint)` for O(1) lookup |

The dictionary is populated during construction (via `Load()` or `Scan()`) and is
read-only thereafter. See the ReviewEvidence Record and Evidence Index Format sections
for the record-level data model.

#### ReviewEvidence Record

`ReviewEvidence` is an immutable record that holds the in-memory representation of a
single review record once the index has been loaded or scanned.

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Id` | string | The review-set identifier |
| `Fingerprint` | string | The SHA-256 fingerprint of the reviewed files |
| `Date` | string | The date of the review (e.g. `2026-02-14`) |
| `Result` | string | The review outcome (`pass` or `fail`) |
| `File` | string | The relative path to the review evidence PDF |

The `ReviewIndex` holds these records in a two-level
`Dictionary<string, Dictionary<string, ReviewEvidence>>` keyed first by `Id` and
then by `Fingerprint`, which enables O(1) lookup by both fields simultaneously.

#### Evidence Index Format

The evidence index is a JSON file (`index.json`) containing an array of review records.
Each record has the following fields:

| Field | Type | Description |
| ----- | ---- | ----------- |
| `id` | string | Unique identifier for the review record (matches the review-set `id` in `.reviewmark.yaml`) |
| `fingerprint` | string | SHA-256 fingerprint of the file-set at time of review |
| `date` | string | Date the review was conducted |
| `result` | string | Review outcome (`pass` or `fail`) |
| `file` | string | Relative path to the PDF evidence file |

#### Key Methods

##### ReviewIndex.Load(EvidenceSource)

`ReviewIndex.Load(EvidenceSource)` selects a loading strategy based on the evidence
source type (see below). For `url` sources, the tool constructs an `HttpClient`
internally and applies a pre-emptive `Authorization: Basic <base64>` header when both
credential environment-variable names (`UsernameEnv` and `PasswordEnv` from the
`EvidenceSource`) are set and the corresponding environment variables are non-empty.
The encoded credential is `Base64(UTF-8("<username>:<password>"))`.  
This overload is **not** exposed for test injection; see
`Load(EvidenceSource, HttpClient)` for the testable overload.

- **`none`** — Returns an empty index (equivalent to `ReviewIndex.Empty()`)
- **`fileshare`** — Reads `index.json` from the specified file path
- **`url`** — Downloads `index.json` from the specified HTTP or HTTPS URL, with optional
  Basic-auth credentials read from environment variables

##### Error Behavior

- **`fileshare` — file missing or unreadable**: If the file at the specified path does not
  exist or cannot be read, an `InvalidOperationException` is thrown with a message
  identifying the path and the underlying I/O failure.
- **`fileshare` — malformed JSON**: If the file exists but cannot be deserialized as a
  valid evidence index, an `InvalidOperationException` is thrown with a message describing
  the parse failure.
- **`url` — HTTP request fails**: If the HTTP or HTTPS request fails (e.g., network
  error, non-success status code), an `InvalidOperationException` is thrown with a message
  identifying the URL and the HTTP status or network error.
- **`url` — malformed response**: If the response body is not valid evidence-index JSON,
  an `InvalidOperationException` is thrown with a message describing the parse failure.

##### ReviewIndex.Load(EvidenceSource, HttpClient)

`ReviewIndex.Load(EvidenceSource, HttpClient)` is an internally-visible overload that
accepts a caller-supplied `HttpClient`. It is exposed to allow unit tests to inject a
fake `HttpMessageHandler` when testing `url`-type evidence sources, avoiding real
network calls. The behavior is identical to the single-argument overload except that
the caller provides the `HttpClient` instead of having one created internally.

##### ReviewIndex.Scan()

`ReviewIndex.Scan(directory, paths, onWarning)` scans a directory for PDF files matching
the given glob patterns. For each PDF file found, it reads embedded metadata to
extract the review record fields and returns a populated in-memory `ReviewIndex`.
The `onWarning` parameter is an optional `Action<string>?` callback invoked with a
warning message when a PDF is skipped due to missing or incomplete metadata fields.
When a PDF file cannot be opened or read (e.g., the file is corrupt or access is
denied), `onWarning` is invoked with a descriptive message and scanning continues
with the next file; no exception is propagated to the caller.
The caller (e.g., `Program`) is responsible for choosing an output path and calling
`Save(...)` on the returned index to produce `index.json` as part of the `--index`
workflow.

##### ReviewIndex.Empty()

`ReviewIndex.Empty()` returns an index with no records. It is used when the evidence
source type is `none`, resulting in all review-sets being reported as Missing.

##### ReviewIndex.Save()

`ReviewIndex` provides two overloads for persisting the index to `index.json` format:

- `Save(string filePath)` — writes the serialized index to the specified file path
- `Save(Stream stream)` — writes the serialized index to the provided stream

Both overloads serialize all `ReviewEvidence` records in the index to JSON format.
The `Save(string filePath)` overload is used by the `--index` workflow in `Program`
to write the output file after scanning.

##### ReviewIndex.GetEvidence()

`ReviewIndex.GetEvidence(string id, string fingerprint)` returns the `ReviewEvidence`
record whose `Id` matches `id` and whose `Fingerprint` matches `fingerprint`, or `null`
if no such record exists.

##### ReviewIndex.HasId()

`ReviewIndex.HasId(string id)` returns `true` if the index contains at least one record
with the given `id`, regardless of fingerprint. Returns `false` if no record exists for
the id.

##### ReviewIndex.GetAllForId()

`ReviewIndex.GetAllForId(string id)` returns all `ReviewEvidence` records that have the
given `id`, as a read-only indexed collection (`IReadOnlyList<ReviewEvidence>`). Returns an
empty collection if no records exist for the id.

#### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| `InvalidOperationException` | `Load()` — unrecognized source type, file-read failure, HTTP error, or malformed JSON response | Propagated to the caller (`Program.RunDefinitionLogic()` or `Program.RunIndexLogic()`) |
| `ArgumentException` | `Save(string filePath)` — null or empty `filePath` | Propagated to the caller |
| `InvalidOperationException` | `Save(string filePath)` — file write failure (I/O error, permissions, path not found) | Propagated to the caller |

During `Scan()`, PDFs that cannot be opened or are missing required metadata fields trigger
the `onWarning` callback with a descriptive message; no exception is propagated and scanning
continues with remaining files.

#### Interactions

**Called by:**

- `Program.RunDefinitionLogic()` — calls `ReviewIndex.Load(EvidenceSource)` to load the
  evidence index for report generation
- `Program.RunIndexLogic()` — calls `ReviewIndex.Scan()` to build the index from PDF files
  and `Save(string)` to persist `index.json`
- `ReviewMarkConfiguration.PublishReviewReport()` — calls `GetEvidence()` and `HasId()` for
  each review-set to determine review status

**Dependencies:**

- `GlobMatcher` (Configuration subsystem) — called by `Scan()` to resolve PDF evidence glob
  patterns into file paths
- `PathHelpers` (Indexing subsystem) — called by `Scan()` for safe path combination when
  constructing the absolute path of each matched PDF file
- `PDFsharp` (OTS) — used by `Scan()` to open PDF files and read the `Keywords` metadata
  field containing the review record data

#### Design

The two-level `Dictionary<string, Dictionary<string, ReviewEvidence>>` storage keyed by
`(id, fingerprint)` enables O(1) evidence lookup, matching the primary query pattern in
`PublishReviewReport()`. Source-type dispatch is encapsulated in `Load()`: the `none`,
`fileshare`, and `url` cases are handled internally, and all three paths produce an
identical `ReviewIndex` interface for callers.

The testable `Load(EvidenceSource, HttpClient)` overload accepts a caller-supplied
`HttpClient`, allowing unit tests to inject a fake `HttpMessageHandler` without real
network calls. The `Save(Stream)` overload similarly enables unit tests to capture
output without writing to disk.
