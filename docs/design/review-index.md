# ReviewIndex

## Purpose

The `ReviewIndex` software unit manages the loading, querying, and creation of the review
evidence index. It abstracts the evidence store behind a uniform interface so that
the rest of the tool does not need to know whether evidence is stored on a fileshare,
served over HTTP, or absent entirely.

## ReviewEvidence Record

`ReviewEvidence` is an immutable record that holds the in-memory representation of a
single review record once the index has been loaded or scanned.

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Id` | string | The review-set identifier |
| `Fingerprint` | string | The SHA-256 fingerprint of the reviewed files |
| `Date` | string | The date of the review (e.g. `2026-02-14`) |
| `Result` | string | The review outcome (`pass` or `fail`) |
| `File` | string | The file name of the review evidence PDF |

The `ReviewIndex` holds these records in a two-level
`Dictionary<string, Dictionary<string, ReviewEvidence>>` keyed first by `Id` and
then by `Fingerprint`, which enables O(1) lookup by both fields simultaneously.

## Evidence Index Format

The evidence index is a JSON file (`index.json`) containing an array of review records.
Each record has the following fields:

| Field | Type | Description |
| ----- | ---- | ----------- |
| `id` | string | Unique identifier for the review record (matches the review-set `id` in `.reviewmark.yaml`) |
| `fingerprint` | string | SHA-256 fingerprint of the file-set at time of review |
| `date` | string | Date the review was conducted |
| `result` | string | Review outcome (`pass` or `fail`) |
| `file` | string | Relative path to the PDF evidence file |

## ReviewIndex.Load()

`ReviewIndex.Load(EvidenceSource)` selects a loading strategy based on the evidence
source type:

| Source Type | Behavior |
| ----------- | -------- |
| `none` | Returns an empty index (equivalent to `ReviewIndex.Empty()`) |
| `fileshare` | Reads `index.json` from the specified file path |
| `url` | Downloads `index.json` from the specified HTTP or HTTPS URL |

## ReviewIndex.Scan()

`ReviewIndex.Scan(directory, patterns)` scans a directory for PDF files matching
the given glob patterns. For each PDF file found, it reads embedded metadata to
extract the review record fields and returns a populated in-memory `ReviewIndex`.
The caller (e.g., `Program`) is responsible for choosing an output path and calling
`Save(...)` on the returned index to produce `index.json` as part of the `--index`
workflow.

## ReviewIndex.Empty()

`ReviewIndex.Empty()` returns an index with no records. It is used when the evidence
source type is `none`, resulting in all review-sets being reported as Missing.

## ReviewIndex.GetStatus()

`ReviewIndex.GetStatus(id, fingerprint)` determines the review status of a
review-set by looking up the `id` in the loaded index:

1. Look up `id` in the index
   - If not found — return `Missing`
2. Check if there is a record whose `Fingerprint` matches the supplied `fingerprint`
   - If no matching fingerprint exists — return `Stale`
   - If a matching fingerprint exists:
     - If the `Result` is `pass` — return `Current`
     - If the `Result` is not `pass` — return `Failed`

| Status | Meaning |
| ------ | ------- |
| `Current` | The review record matches the current fingerprint and has a passing result |
| `Failed` | The review record matches the current fingerprint but the result is not passing |
| `Stale` | A record exists for the id but the fingerprint does not match the current one |
| `Missing` | No review record exists for the id |
