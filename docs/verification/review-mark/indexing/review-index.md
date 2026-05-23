### ReviewIndex Verification

This document describes the unit-level verification design for the `ReviewIndex` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`Indexing/ReviewIndex.cs`.

#### Verification Strategy

`ReviewIndex` is verified with unit tests in `IndexTests.cs`. Tests exercise all source
types (none, fileshare, url), JSON round-trip serialization, PDF metadata extraction
(via the `Scan` method), and query operations (`GetEvidence`, `HasId`, `GetAllForId`).

#### Dependencies

| Mock / Stub             | Reason                                                     |
| ----------------------- | ---------------------------------------------------------- |
| Temporary JSON files    | Controlled fileshare evidence without real review PDFs     |
| `FakeHttpMessageHandler`| Returns fixed JSON for URL source tests                    |
| Temporary PDF files     | Real minimal PDF fixtures used for Scan metadata tests     |

#### Test Environment

N/A - standard test environment. Temporary JSON and minimal PDF fixture files are created
in-process for fileshare and scan tests. URL-source tests use an in-process
`FakeHttpMessageHandler`; no real network access is required. Temporary files are removed
after each test.

#### Acceptance Criteria

All ReviewIndex unit tests pass with zero failures. Every `ReviewMark-Index-*` and
`ReviewMark-EvidenceSource-*` requirement is covered by at least one passing test
scenario. Null inputs, missing files, malformed JSON, unsupported source types, and HTTP
error responses all produce the specified exception types.

#### Test Scenarios

##### ReviewIndex_Empty_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Empty` is called.

**Expected**: Returns an index with no entries; all query methods report empty/no results.

**Requirement coverage**: `ReviewMark-Index-Empty`

##### ReviewIndex_Load_EvidenceSource_NullSource_ThrowsArgumentNullException

**Scenario**: `ReviewIndex.Load` is called with a `null` evidence source.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Load_EvidenceSource_UnknownType_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with an evidence source whose type is not
recognized (e.g. `"unknown-type"`).

**Expected**: `InvalidOperationException` is thrown.

**Boundary / error path**: Unsupported source type.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Load` is called with `EvidenceSource` type `none`.

**Expected**: Returns an empty index with no entries.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`, `ReviewMark-Index-EvidenceSource-None`

##### ReviewIndex_Load_EvidenceSource_None_HttpClientOverload_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Load(EvidenceSource, HttpClient)` is called with a `none`-type
source and a fake HTTP client that would fail if actually contacted.

**Expected**: Returns an empty index without making any HTTP request.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`, `ReviewMark-Index-EvidenceSource-None`

##### ReviewIndex_Load_EvidenceSource_Fileshare_LoadsFromFile

**Scenario**: `ReviewIndex.Load` is called with a fileshare source pointing to a valid
index JSON file written to a temporary path.

**Expected**: Returns an index containing the entry from the JSON file.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex

**Scenario**: `ReviewIndex.Load` is called with a fileshare source pointing to a valid
index JSON file containing two distinct review evidence entries.

**Expected**: Returns a populated index with both entries; all fields match the JSON.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with a fileshare path that does not exist.

**Expected**: `InvalidOperationException` is thrown.

**Boundary / error path**: Missing file handling.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Fileshare_InvalidJson_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with a fileshare source pointing to a file
containing invalid JSON content.

**Expected**: `InvalidOperationException` is thrown.

**Boundary / error path**: Malformed JSON content.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Fileshare_EmptyReviews_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Load` is called with a fileshare source pointing to a JSON
file whose `reviews` array is empty.

**Expected**: Returns an empty index with no entries.

**Boundary / error path**: Empty reviews array.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Fileshare_MissingRequiredFields_SkipsInvalidEntries

**Scenario**: `ReviewIndex.Load` is called with a JSON file containing three entries:
one missing `id`, one missing `fingerprint`, and one fully valid.

**Expected**: Only the valid entry is present in the resulting index; the two
incomplete entries are silently skipped.

**Boundary / error path**: Partial / incomplete entry handling.

**Requirement coverage**: `ReviewMark-Index-LoadFromFile`

##### ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex

**Scenario**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns
a 200 OK with valid index JSON.

**Expected**: Returns a populated index.

**Requirement coverage**: `ReviewMark-Index-LoadFromStream`

##### ReviewIndex_Load_EvidenceSource_Url_NotFoundResponse_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns
HTTP 404.

**Expected**: `InvalidOperationException` is thrown identifying the failed URL.

**Boundary / error path**: HTTP error response.

**Requirement coverage**: `ReviewMark-Index-LoadFromStream`

##### ReviewIndex_Load_EvidenceSource_Url_InvalidJson_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns
200 OK with malformed JSON.

**Expected**: `InvalidOperationException` is thrown describing the parse failure.

**Boundary / error path**: Malformed HTTP response body.

**Requirement coverage**: `ReviewMark-Index-LoadFromStream`

##### ReviewIndex_Load_EvidenceSource_NullHttpClient_ThrowsArgumentNullException

**Scenario**: `ReviewIndex.Load(EvidenceSource, HttpClient)` is called with a null
`HttpClient`.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null HTTP client rejection.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Save_Stream_NullStream_ThrowsArgumentNullException

**Scenario**: `ReviewIndex.Save(Stream)` is called with a null stream.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null stream rejection.

**Requirement coverage**: `ReviewMark-Index-Save`

##### ReviewIndex_Save_File_EmptyPath_ThrowsArgumentException

**Scenario**: `ReviewIndex.Save(string)` is called with an empty string path.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Empty path rejection.

**Requirement coverage**: `ReviewMark-Index-Save`

##### ReviewIndex_Save_File_NullPath_ThrowsArgumentException

**Scenario**: `ReviewIndex.Save(string)` is called with a `null` path.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Null path rejection.

**Requirement coverage**: `ReviewMark-Index-Save`

##### ReviewIndex_Save_RoundTrip_PreservesAllEntries

**Scenario**: A populated index is saved to a stream and reloaded.

**Expected**: All entries are preserved after the round-trip.

**Requirement coverage**: `ReviewMark-Index-Save`

##### ReviewIndex_Scan_NoMatchingFiles_LeavesIndexEmpty

**Scenario**: `ReviewIndex.Scan` is called on an empty directory; no PDFs are present.

**Expected**: Returns an empty index with no entries.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex

**Scenario**: `ReviewIndex.Scan` is called on a directory containing a PDF with all four
required keyword metadata fields (`id`, `fingerprint`, `date`, `result`).

**Expected**: Returns an index with one entry whose fields match the PDF keywords.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning

**Scenario**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `id` entry.

**Expected**: The PDF is skipped; the warning callback is invoked; the index remains empty.

**Boundary / error path**: Missing required `id` field.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning

**Scenario**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `fingerprint` entry.

**Expected**: The PDF is skipped; the warning callback is invoked; the index remains empty.

**Boundary / error path**: Missing required `fingerprint` field.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithMissingDate_SkipsWithWarning

**Scenario**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `date` entry.

**Expected**: The PDF is skipped; the warning callback is invoked; the index remains empty.

**Boundary / error path**: Missing required `date` field.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithMissingResult_SkipsWithWarning

**Scenario**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `result` entry.

**Expected**: The PDF is skipped; the warning callback is invoked; the index remains empty.

**Boundary / error path**: Missing required `result` field.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning

**Scenario**: `ReviewIndex.Scan` processes a PDF with an empty Keywords field.

**Expected**: The PDF is skipped; the warning callback is invoked; the index remains empty.

**Boundary / error path**: Empty Keywords field.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries

**Scenario**: `ReviewIndex.Scan` is called on a directory containing two PDFs, each with
distinct IDs and fingerprints.

**Expected**: Returns an index with both entries; each entry's fields match its PDF's keywords.

**Requirement coverage**: `ReviewMark-Index-PdfParsing`

##### ReviewIndex_Scan_ClearsExistingEntries

**Scenario**: An existing loaded index contains an entry; `ReviewIndex.Scan` is then called
on an empty directory.

**Expected**: The scan returns a fresh index that does not contain any entries from the
separately loaded index.

**Boundary / error path**: Freshness — scan creates a new independent index.

**Requirement coverage**: `ReviewMark-Index-Freshness`

##### ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence

**Scenario**: `GetEvidence` is called with an ID and fingerprint that exist in the index.

**Expected**: Returns the matching evidence record.

**Requirement coverage**: `ReviewMark-Index-GetEvidence`

##### ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull

**Scenario**: `GetEvidence` is called with a known ID but wrong fingerprint.

**Expected**: Returns null.

**Boundary / error path**: Fingerprint mismatch.

**Requirement coverage**: `ReviewMark-Index-GetEvidence`

##### ReviewIndex_GetEvidence_UnknownId_ReturnsNull

**Scenario**: `GetEvidence` is called with an ID that does not exist in the index.

**Expected**: Returns null.

**Boundary / error path**: Unknown ID lookup.

**Requirement coverage**: `ReviewMark-Index-GetEvidence`

##### ReviewIndex_HasId_ExistingId_ReturnsTrue

**Scenario**: `HasId` is called with an ID that exists in the index.

**Expected**: Returns true.

**Requirement coverage**: `ReviewMark-Index-HasId`

##### ReviewIndex_HasId_UnknownId_ReturnsFalse

**Scenario**: `HasId` is called with an ID that does not exist.

**Expected**: Returns false.

**Boundary / error path**: Unknown ID lookup.

**Requirement coverage**: `ReviewMark-Index-HasId`

##### ReviewIndex_GetAllForId_ExistingId_ReturnsAllEntries

**Scenario**: `GetAllForId` is called with an ID that has two entries (different fingerprints).

**Expected**: Returns a collection containing both entries.

**Requirement coverage**: `ReviewMark-Index-GetAllForId`

##### ReviewIndex_GetAllForId_UnknownId_ReturnsEmptyList

**Scenario**: `GetAllForId` is called with an ID that does not exist in the index.

**Expected**: Returns an empty collection (not null).

**Boundary / error path**: Unknown ID — empty collection returned.

**Requirement coverage**: `ReviewMark-Index-GetAllForId`

#### Requirements Coverage

- **ReviewMark-Index-EvidenceSource**: ReviewIndex_Load_EvidenceSource_NullSource_ThrowsArgumentNullException,
  ReviewIndex_Load_EvidenceSource_UnknownType_ThrowsInvalidOperationException,
  ReviewIndex_Load_EvidenceSource_NullHttpClient_ThrowsArgumentNullException
- **ReviewMark-Index-LoadFromFile**: ReviewIndex_Load_EvidenceSource_Fileshare_LoadsFromFile,
  ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex,
  ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException,
  ReviewIndex_Load_EvidenceSource_Fileshare_InvalidJson_ThrowsInvalidOperationException,
  ReviewIndex_Load_EvidenceSource_Fileshare_EmptyReviews_ReturnsEmptyIndex,
  ReviewIndex_Load_EvidenceSource_Fileshare_MissingRequiredFields_SkipsInvalidEntries
- **ReviewMark-Index-LoadFromStream**: ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex,
  ReviewIndex_Load_EvidenceSource_Url_NotFoundResponse_ThrowsInvalidOperationException,
  ReviewIndex_Load_EvidenceSource_Url_InvalidJson_ThrowsInvalidOperationException
- **ReviewMark-Index-EvidenceSource-None**: ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex,
  ReviewIndex_Load_EvidenceSource_None_HttpClientOverload_ReturnsEmptyIndex
- **ReviewMark-Index-Empty**: ReviewIndex_Empty_ReturnsEmptyIndex
- **ReviewMark-Index-PdfParsing**: ReviewIndex_Scan_NoMatchingFiles_LeavesIndexEmpty,
  ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex,
  ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning,
  ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning,
  ReviewIndex_Scan_PdfWithMissingDate_SkipsWithWarning,
  ReviewIndex_Scan_PdfWithMissingResult_SkipsWithWarning,
  ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning,
  ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries
- **ReviewMark-Index-Freshness**: ReviewIndex_Scan_ClearsExistingEntries
- **ReviewMark-Index-Save**: ReviewIndex_Save_Stream_NullStream_ThrowsArgumentNullException,
  ReviewIndex_Save_File_EmptyPath_ThrowsArgumentException,
  ReviewIndex_Save_File_NullPath_ThrowsArgumentException,
  ReviewIndex_Save_RoundTrip_PreservesAllEntries
- **ReviewMark-Index-GetEvidence**: ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence,
  ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull,
  ReviewIndex_GetEvidence_UnknownId_ReturnsNull
- **ReviewMark-Index-HasId**: ReviewIndex_HasId_ExistingId_ReturnsTrue,
  ReviewIndex_HasId_UnknownId_ReturnsFalse
- **ReviewMark-Index-GetAllForId**: ReviewIndex_GetAllForId_ExistingId_ReturnsAllEntries,
  ReviewIndex_GetAllForId_UnknownId_ReturnsEmptyList
