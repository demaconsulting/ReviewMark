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

## ReviewIndex.Save()

`ReviewIndex` provides two overloads for persisting the index to `index.json` format:

- `Save(string filePath)` — writes the serialized index to the specified file path
- `Save(Stream stream)` — writes the serialized index to the provided stream

Both overloads serialize all `ReviewEvidence` records in the index to JSON format.
The `Save(string filePath)` overload is used by the `--index` workflow in `Program`
to write the output file after scanning.

## ReviewIndex.GetEvidence()

`ReviewIndex.GetEvidence(string id, string fingerprint)` returns the `ReviewEvidence`
record whose `Id` matches `id` and whose `Fingerprint` matches `fingerprint`, or `null`
if no such record exists.

## ReviewIndex.HasId()

`ReviewIndex.HasId(string id)` returns `true` if the index contains at least one record
with the given `id`, regardless of fingerprint. Returns `false` if no record exists for
the id.

## ReviewIndex.GetAllForId()

`ReviewIndex.GetAllForId(string id)` returns all `ReviewEvidence` records that have the
given `id`, as an enumerable collection. Returns an empty collection if no records exist
for the id.
