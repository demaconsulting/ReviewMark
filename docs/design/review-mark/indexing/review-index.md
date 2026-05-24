### ReviewIndex

#### Purpose

`ReviewIndex` manages the loading, querying, and building of the review evidence index.
It abstracts the evidence store behind a uniform in-memory interface so that callers do
not need to know whether evidence was loaded from a file share, downloaded over HTTP,
built from PDF scans, or is simply absent.

#### Data Model

**`ReviewIndex` instance state:**

| Field | Type | Description |
| ----- | ---- | ----------- |
| Internal store | `Dictionary<string, Dictionary<string, ReviewEvidence>>` | Evidence records indexed by `(Id, Fingerprint)` for O(1) lookup |

The dictionary is populated during construction (via `Load()` or `Scan()`) and is
read-only thereafter.

**`ReviewEvidence` record:**

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Id` | `string` | The review-set identifier |
| `Fingerprint` | `string` | SHA-256 fingerprint of the reviewed files |
| `Date` | `string` | Date of the review (e.g. `2026-02-14`) |
| `Result` | `string` | Review outcome (`pass` or `fail`) |
| `File` | `string` | Relative path to the review evidence PDF |

**Evidence index JSON format** (`index.json`): top-level object with a `reviews` array;
each entry has fields `id`, `fingerprint`, `date`, `result`, and `file`.

#### Key Methods

**`ReviewIndex.Load(EvidenceSource)`** → `ReviewIndex`

- *Parameters*: `EvidenceSource` — configured evidence source (type, location, credentials)
- *Returns*: `ReviewIndex` populated from the specified source
- *Preconditions*: Evidence source type is one of `none`, `fileshare`, or `url`
- *Postconditions*: All records from the source are in the internal dictionary

Dispatches by source type: `none` → `Empty()`; `fileshare` → reads and deserializes the
JSON file at `EvidenceSource.Location`; `url` → issues an HTTP GET to
`EvidenceSource.Location`. For `url` sources, if both `UsernameEnv` and `PasswordEnv`
environment variables are set and non-empty, a pre-emptive `Authorization: Basic
<base64>` header is added, encoding `Base64(UTF-8("<username>:<password>"))`.

**`ReviewIndex.Load(EvidenceSource, HttpClient)`** → `ReviewIndex` (testable overload)

Accepts a caller-supplied `HttpClient` to allow unit tests to inject a fake
`HttpMessageHandler` without real network calls. Behavior is identical to the
single-argument overload.

**`ReviewIndex.Scan(string dir, IReadOnlyList<string> paths, Action<string>? onWarning)`**
→ `ReviewIndex`

- *Parameters*: `dir` — scan root; `paths` — glob patterns for PDF files; `onWarning` —
  optional callback invoked with a descriptive message when a PDF is skipped
- *Returns*: `ReviewIndex` built from scanned PDF metadata
- *Preconditions*: `dir` is a valid directory path; `paths` is not null
- *Postconditions*: All PDFs with valid metadata are in the returned index

Calls `GlobMatcher.GetMatchingFiles(dir, paths)` to enumerate PDFs. For each matched
file, calls `PdfReader.Open(fullPath, PdfDocumentOpenMode.Import)` via PDFsharp and reads
`doc.Info.Keywords`. The keywords string is parsed into key-value pairs; entries with all
required fields are added to the index. PDFs that cannot be opened or are missing required
fields trigger `onWarning` and are skipped; the scan continues.

**`ReviewIndex.Empty()`** → `ReviewIndex`

Returns an index with no records. Used when evidence source type is `none`.

**`ReviewIndex.Save(string filePath)`** / **`ReviewIndex.Save(Stream stream)`**

Serializes all `ReviewEvidence` records to JSON format and writes to the specified file
or stream. Used by `Program.RunIndexLogic()` to write `index.json` after scanning.

**`ReviewIndex.GetEvidence(string id, string fingerprint)`** → `ReviewEvidence?`

Returns the `ReviewEvidence` record matching both `id` and `fingerprint`, or null if none
exists. O(1) lookup via the two-level dictionary.

**`ReviewIndex.HasId(string id)`** → `bool`

Returns true if the index contains at least one record with the given `id`, regardless of
fingerprint.

**`ReviewIndex.GetAllForId(string id)`** → `IReadOnlyList<ReviewEvidence>`

Returns all `ReviewEvidence` records with the given `id`. Returns an empty list if none exist.

#### Error Handling

| Exception | Source | Handling |
| --------- | ------ | -------- |
| `InvalidOperationException` | `Load()` — unrecognized source type, file-read failure, HTTP error, or malformed JSON | Propagated to `Program.RunDefinitionLogic()` or `Program.RunIndexLogic()` |
| `ArgumentException` | `Save(string)` — null or empty `filePath` | Propagated to the caller |
| `InvalidOperationException` | `Save(string)` — file write failure | Propagated to the caller |

During `Scan()`, PDFs that cannot be opened or are missing required metadata trigger
`onWarning` with a descriptive message; no exception is propagated.

#### Dependencies

- **`GlobMatcher`** (Configuration subsystem) — called by `Scan()` to resolve PDF
  evidence glob patterns into file paths
- **`PathHelpers`** (Indexing subsystem) — called by `Scan()` for safe path combination
  when constructing the absolute path of each matched PDF
- **`PDFsharp`** (OTS) — used by `Scan()` to open PDF files and read the `Keywords`
  metadata field

#### Callers

- **`Program.RunDefinitionLogic()`** — calls `Load(EvidenceSource)` to load the evidence
  index for report generation
- **`Program.RunIndexLogic()`** — calls `Scan()` to build the index from PDF files and
  `Save(string)` to persist `index.json`
- **`ReviewMarkConfiguration.PublishReviewReport()`** — calls `GetEvidence()` and `HasId()`
  for each review-set to determine review status
