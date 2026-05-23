### ReviewIndex

#### Verification Approach

ReviewIndex unit verification uses `IndexTests.cs` to exercise all evidence-source loaders, save and reload round-trips, PDF scan behavior, and query operations. The tests use temporary JSON and PDF fixtures, a fake `HttpMessageHandler` for URL scenarios, and real `PathHelpers` and `GlobMatcher` interactions where the production unit depends on them.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10, create temporary JSON and PDF fixtures in-process, and inject a fake HTTP client so no real network access is required.

#### Acceptance Criteria

- All ReviewIndex unit tests pass with zero failures.
- Each `ReviewMark-Index-*` requirement is traced to at least one scenario and test method.
- Loader selection, scan behavior, persistence, and evidence-query APIs all return the documented results for normal, boundary, and failure paths.

#### Test Scenarios

**ReviewIndex_Empty_ReturnsEmptyIndex**: `ReviewIndex.Empty` is called. Expected outcome: Returns an index with no entries; all query methods report empty/no results. Requirement coverage: `ReviewMark-Index-Empty`. This scenario is tested by `ReviewIndex_Empty_ReturnsEmptyIndex`.

**ReviewIndex_Load_EvidenceSource_NullSource_ThrowsArgumentNullException**: `ReviewIndex.Load` is called with a `null` evidence source. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-Index-EvidenceSource`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_NullSource_ThrowsArgumentNullException`.

**ReviewIndex_Load_EvidenceSource_UnknownType_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with an evidence source whose type is not recognized (e.g. `"unknown-type"`). Expected outcome: `InvalidOperationException` is thrown. Boundary or error path: Unsupported source type. Requirement coverage: `ReviewMark-Index-EvidenceSource`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_UnknownType_ThrowsInvalidOperationException`.

**ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex**: `ReviewIndex.Load` is called with `EvidenceSource` type `none`. Expected outcome: Returns an empty index with no entries. Requirement coverage: `ReviewMark-Index-EvidenceSource`, `ReviewMark-Index-EvidenceSource-None`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex`.

**ReviewIndex_Load_EvidenceSource_None_HttpClientOverload_ReturnsEmptyIndex**: `ReviewIndex.Load(EvidenceSource, HttpClient)` is called with a `none`-type source and a fake HTTP client that would fail if actually contacted. Expected outcome: Returns an empty index without making any HTTP request. Requirement coverage: `ReviewMark-Index-EvidenceSource`, `ReviewMark-Index-EvidenceSource-None`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_None_HttpClientOverload_ReturnsEmptyIndex`.

**ReviewIndex_Load_EvidenceSource_Fileshare_LoadsFromFile**: `ReviewIndex.Load` is called with a fileshare source pointing to a valid index JSON file written to a temporary path. Expected outcome: Returns an index containing the entry from the JSON file. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_LoadsFromFile`.

**ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex**: `ReviewIndex.Load` is called with a fileshare source pointing to a valid index JSON file containing two distinct review evidence entries. Expected outcome: Returns a populated index with both entries; all fields match the JSON. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex`.

**ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a fileshare path that does not exist. Expected outcome: `InvalidOperationException` is thrown. Boundary or error path: Missing file handling. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException`.

**ReviewIndex_Load_EvidenceSource_Fileshare_InvalidJson_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a fileshare source pointing to a file containing invalid JSON content. Expected outcome: `InvalidOperationException` is thrown. Boundary or error path: Malformed JSON content. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_InvalidJson_ThrowsInvalidOperationException`.

**ReviewIndex_Load_EvidenceSource_Fileshare_EmptyReviews_ReturnsEmptyIndex**: `ReviewIndex.Load` is called with a fileshare source pointing to a JSON file whose `reviews` array is empty. Expected outcome: Returns an empty index with no entries. Boundary or error path: Empty reviews array. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_EmptyReviews_ReturnsEmptyIndex`.

**ReviewIndex_Load_EvidenceSource_Fileshare_MissingRequiredFields_SkipsInvalidEntries**: `ReviewIndex.Load` is called with a JSON file containing three entries: one missing `id`, one missing `fingerprint`, and one fully valid. Expected outcome: Only the valid entry is present in the resulting index; the two incomplete entries are silently skipped. Boundary or error path: Partial / incomplete entry handling. Requirement coverage: `ReviewMark-Index-LoadFromFile`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Fileshare_MissingRequiredFields_SkipsInvalidEntries`.

**ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns a 200 OK with valid index JSON. Expected outcome: Returns a populated index. Requirement coverage: `ReviewMark-Index-LoadFromStream`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex`.

**ReviewIndex_Load_EvidenceSource_Url_NotFoundResponse_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns HTTP 404. Expected outcome: `InvalidOperationException` is thrown identifying the failed URL. Boundary or error path: HTTP error response. Requirement coverage: `ReviewMark-Index-LoadFromStream`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Url_NotFoundResponse_ThrowsInvalidOperationException`.

**ReviewIndex_Load_EvidenceSource_Url_InvalidJson_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns 200 OK with malformed JSON. Expected outcome: `InvalidOperationException` is thrown describing the parse failure. Boundary or error path: Malformed HTTP response body. Requirement coverage: `ReviewMark-Index-LoadFromStream`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_Url_InvalidJson_ThrowsInvalidOperationException`.

**ReviewIndex_Load_EvidenceSource_NullHttpClient_ThrowsArgumentNullException**: `ReviewIndex.Load(EvidenceSource, HttpClient)` is called with a null `HttpClient`. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null HTTP client rejection. Requirement coverage: `ReviewMark-Index-EvidenceSource`. This scenario is tested by `ReviewIndex_Load_EvidenceSource_NullHttpClient_ThrowsArgumentNullException`.

**ReviewIndex_Save_Stream_NullStream_ThrowsArgumentNullException**: `ReviewIndex.Save(Stream)` is called with a null stream. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null stream rejection. Requirement coverage: `ReviewMark-Index-Save`. This scenario is tested by `ReviewIndex_Save_Stream_NullStream_ThrowsArgumentNullException`.

**ReviewIndex_Save_File_EmptyPath_ThrowsArgumentException**: `ReviewIndex.Save(string)` is called with an empty string path. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Empty path rejection. Requirement coverage: `ReviewMark-Index-Save`. This scenario is tested by `ReviewIndex_Save_File_EmptyPath_ThrowsArgumentException`.

**ReviewIndex_Save_File_NullPath_ThrowsArgumentException**: `ReviewIndex.Save(string)` is called with a `null` path. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Null path rejection. Requirement coverage: `ReviewMark-Index-Save`. This scenario is tested by `ReviewIndex_Save_File_NullPath_ThrowsArgumentException`.

**ReviewIndex_Save_RoundTrip_PreservesAllEntries**: A populated index is saved to a stream and reloaded. Expected outcome: All entries are preserved after the round-trip. Requirement coverage: `ReviewMark-Index-Save`. This scenario is tested by `ReviewIndex_Save_RoundTrip_PreservesAllEntries`.

**ReviewIndex_Scan_NoMatchingFiles_LeavesIndexEmpty**: `ReviewIndex.Scan` is called on an empty directory; no PDFs are present. Expected outcome: Returns an empty index with no entries. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_NoMatchingFiles_LeavesIndexEmpty`.

**ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex**: `ReviewIndex.Scan` is called on a directory containing a PDF with all four required keyword metadata fields (`id`, `fingerprint`, `date`, `result`). Expected outcome: Returns an index with one entry whose fields match the PDF keywords. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`.

**ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `id` entry. Expected outcome: The PDF is skipped; the warning callback is invoked; the index remains empty. Boundary or error path: Missing required `id` field. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`.

**ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `fingerprint` entry. Expected outcome: The PDF is skipped; the warning callback is invoked; the index remains empty. Boundary or error path: Missing required `fingerprint` field. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`.

**ReviewIndex_Scan_PdfWithMissingDate_SkipsWithWarning**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `date` entry. Expected outcome: The PDF is skipped; the warning callback is invoked; the index remains empty. Boundary or error path: Missing required `date` field. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithMissingDate_SkipsWithWarning`.

**ReviewIndex_Scan_PdfWithMissingResult_SkipsWithWarning**: `ReviewIndex.Scan` processes a PDF whose Keywords field has no `result` entry. Expected outcome: The PDF is skipped; the warning callback is invoked; the index remains empty. Boundary or error path: Missing required `result` field. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithMissingResult_SkipsWithWarning`.

**ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning**: `ReviewIndex.Scan` processes a PDF with an empty Keywords field. Expected outcome: The PDF is skipped; the warning callback is invoked; the index remains empty. Boundary or error path: Empty Keywords field. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`.

**ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries**: `ReviewIndex.Scan` is called on a directory containing two PDFs, each with distinct IDs and fingerprints. Expected outcome: Returns an index with both entries; each entry's fields match its PDF's keywords. Requirement coverage: `ReviewMark-Index-PdfParsing`. This scenario is tested by `ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`.

**ReviewIndex_Scan_ClearsExistingEntries**: An existing loaded index contains an entry; `ReviewIndex.Scan` is then called on an empty directory. Expected outcome: The scan returns a fresh index that does not contain any entries from the separately loaded index. Boundary or error path: Freshness — scan creates a new independent index. Requirement coverage: `ReviewMark-Index-Freshness`. This scenario is tested by `ReviewIndex_Scan_ClearsExistingEntries`.

**ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence**: `GetEvidence` is called with an ID and fingerprint that exist in the index. Expected outcome: Returns the matching evidence record. Requirement coverage: `ReviewMark-Index-GetEvidence`. This scenario is tested by `ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence`.

**ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull**: `GetEvidence` is called with a known ID but wrong fingerprint. Expected outcome: Returns null. Boundary or error path: Fingerprint mismatch. Requirement coverage: `ReviewMark-Index-GetEvidence`. This scenario is tested by `ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull`.

**ReviewIndex_GetEvidence_UnknownId_ReturnsNull**: `GetEvidence` is called with an ID that does not exist in the index. Expected outcome: Returns null. Boundary or error path: Unknown ID lookup. Requirement coverage: `ReviewMark-Index-GetEvidence`. This scenario is tested by `ReviewIndex_GetEvidence_UnknownId_ReturnsNull`.

**ReviewIndex_HasId_ExistingId_ReturnsTrue**: `HasId` is called with an ID that exists in the index. Expected outcome: Returns true. Requirement coverage: `ReviewMark-Index-HasId`. This scenario is tested by `ReviewIndex_HasId_ExistingId_ReturnsTrue`.

**ReviewIndex_HasId_UnknownId_ReturnsFalse**: `HasId` is called with an ID that does not exist. Expected outcome: Returns false. Boundary or error path: Unknown ID lookup. Requirement coverage: `ReviewMark-Index-HasId`. This scenario is tested by `ReviewIndex_HasId_UnknownId_ReturnsFalse`.

**ReviewIndex_GetAllForId_ExistingId_ReturnsAllEntries**: `GetAllForId` is called with an ID that has two entries (different fingerprints). Expected outcome: Returns a collection containing both entries. Requirement coverage: `ReviewMark-Index-GetAllForId`. This scenario is tested by `ReviewIndex_GetAllForId_ExistingId_ReturnsAllEntries`.

**ReviewIndex_GetAllForId_UnknownId_ReturnsEmptyList**: `GetAllForId` is called with an ID that does not exist in the index. Expected outcome: Returns an empty collection (not null). Boundary or error path: Unknown ID — empty collection returned. Requirement coverage: `ReviewMark-Index-GetAllForId`. This scenario is tested by `ReviewIndex_GetAllForId_UnknownId_ReturnsEmptyList`.

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
