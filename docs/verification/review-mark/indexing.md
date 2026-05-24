## Indexing

### Verification Approach

Indexing subsystem verification uses `IndexingTests.cs` to exercise `ReviewIndex` and `PathHelpers` together with temporary directories, JSON fixtures, minimal PDF evidence files, and a fake in-process HTTP handler. This verifies fileshare, URL, and none-source loading, PDF scan behavior, save and reload round-trips, and safe path handling at the subsystem boundary without requiring real network access.

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. Each test creates an isolated temporary directory, URL-source scenarios inject a fake `HttpMessageHandler`, and PDF-scan scenarios use real minimal PDF fixtures.

### Acceptance Criteria

- All Indexing subsystem integration tests pass with zero failures.
- Each `ReviewMark-Indexing-*` requirement is traced to at least one scenario and test method.
- Evidence loading, index creation, persistence, and path-traversal rejection all behave as documented for both normal and error paths.

### Test Scenarios

**Indexing_SafePathCombine_WithIndexPath_LoadsIndex**: A subdirectory index JSON is loaded using a path constructed with `PathHelpers.SafePathCombine`. Expected outcome: The index contains the entries from the JSON file. Requirement coverage: `ReviewMark-Indexing-LoadEvidence`, `ReviewMark-Indexing-SafePathCombine`. This scenario is tested by `Indexing_SafePathCombine_WithIndexPath_LoadsIndex`.

**Indexing_ReviewIndex_SaveAndLoad_RoundTrip**: A populated index is loaded from JSON, saved to a new file, then reloaded. Expected outcome: All entries survive the round-trip. Requirement coverage: `ReviewMark-Indexing-Save`, `ReviewMark-Indexing-LoadEvidence`. This scenario is tested by `Indexing_ReviewIndex_SaveAndLoad_RoundTrip`.

**Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex**: `ReviewIndex.Load` is called with a `none`-type evidence source. Expected outcome: Returns an empty index immediately; no file system access occurs. Requirement coverage: `ReviewMark-Indexing-CreateEvidence`. This scenario is tested by `Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex`.

**Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex**: `ReviewIndex.Load` is called with a `url`-type source and a fake HTTP client returning a fixed JSON payload. Expected outcome: The index contains the entry from the JSON payload. Requirement coverage: `ReviewMark-Indexing-LoadEvidence`. This scenario is tested by `Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex`.

**Indexing_SafePathCombine_WithTraversalInputs_Throws**: `PathHelpers.SafePathCombine` is called with path traversal inputs — first with a `..`-based relative path (`../../etc/sensitive`) and then with an absolute path. Expected outcome: `ArgumentException` is thrown in both cases; directory traversal and absolute-path injection are rejected. Boundary or error path: Path traversal prevention. Requirement coverage: `ReviewMark-Indexing-SafePathCombine`. This scenario is tested by `Indexing_SafePathCombine_WithTraversalInputs_Throws`.

**Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex**: `ReviewIndex.Scan` is called against a directory that contains no PDF files (only a plain text file). Expected outcome: Returns an empty index with no entries. Requirement coverage: `ReviewMark-Indexing-ScanPdfEvidence`. This scenario is tested by `Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex`.

**Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex**: `ReviewIndex.Scan` is called against a directory containing a single PDF with all required keyword metadata fields (`id`, `fingerprint`, `date`, `result`). Expected outcome: Returns an index populated with the evidence entry extracted from the PDF. Requirement coverage: `ReviewMark-Indexing-ScanPdfEvidence`. This scenario is tested by `Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex`.

**Indexing_ReviewIndex_Load_MissingFile_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a `fileshare` source whose file path does not exist on disk. Expected outcome: `InvalidOperationException` is thrown with a message identifying the missing path. Boundary or error path: Missing evidence file. Requirement coverage: `ReviewMark-Indexing-LoadEvidence`. This scenario is tested by `Indexing_ReviewIndex_Load_MissingFile_ThrowsInvalidOperationException`.

**Indexing_ReviewIndex_Load_MalformedJson_ThrowsInvalidOperationException**: `ReviewIndex.Load` is called with a `fileshare` source pointing to a file that contains malformed (non-JSON) content. Expected outcome: `InvalidOperationException` is thrown describing the parse failure. Boundary or error path: Malformed JSON content. Requirement coverage: `ReviewMark-Indexing-LoadEvidence`. This scenario is tested by `Indexing_ReviewIndex_Load_MalformedJson_ThrowsInvalidOperationException`.

### Requirements Coverage

- **ReviewMark-Indexing-LoadEvidence**: Indexing_SafePathCombine_WithIndexPath_LoadsIndex,
  Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex,
  Indexing_ReviewIndex_Load_MissingFile_ThrowsInvalidOperationException,
  Indexing_ReviewIndex_Load_MalformedJson_ThrowsInvalidOperationException
- **ReviewMark-Indexing-CreateEvidence**: Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex
- **ReviewMark-Indexing-Save**: Indexing_ReviewIndex_SaveAndLoad_RoundTrip
- **ReviewMark-Indexing-SafePathCombine**: Indexing_SafePathCombine_WithIndexPath_LoadsIndex,
  Indexing_SafePathCombine_WithTraversalInputs_Throws
- **ReviewMark-Indexing-ScanPdfEvidence**: Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex,
  Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex
