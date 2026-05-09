## Indexing

### Verification Approach

The Indexing subsystem is verified through `IndexingTests.cs`, which exercises
`ReviewIndex` and `PathHelpers` working together with actual temporary directories.
Each test creates a fresh isolated directory with controlled index JSON or PDF files,
exercises the subsystem operations, and asserts on the resulting index state.

The constructor initializes the temporary directory; `Dispose` deletes it, ensuring
clean isolation between tests.

### Dependencies

| Mock / Stub              | Reason                                                         |
| ------------------------ | -------------------------------------------------------------- |
| Temporary directory      | Isolated filesystem prevents test interference                 |
| Fake JSON index file     | Provides controlled evidence index without real PDF evidence   |
| `FakeHttpMessageHandler` | Returns fixed JSON payload for URL-source tests                |

### Test Scenarios

#### Indexing_SafePathCombine_WithIndexPath_LoadsIndex

**Scenario**: A subdirectory index JSON is loaded using a path constructed with
`PathHelpers.SafePathCombine`.

**Expected**: The index contains the entries from the JSON file.

**Requirement coverage**: `ReviewMark-Indexing-LoadEvidence`, `ReviewMark-Indexing-SafePathCombine`

#### Indexing_ReviewIndex_SaveAndLoad_RoundTrip

**Scenario**: A populated index is loaded from JSON, saved to a new file, then reloaded.

**Expected**: All entries survive the round-trip.

**Requirement coverage**: `ReviewMark-Indexing-Save`, `ReviewMark-Indexing-LoadEvidence`

#### Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Load` is called with a `none`-type evidence source.

**Expected**: Returns an empty index immediately; no file system access occurs.

**Requirement coverage**: `ReviewMark-Indexing-LoadEvidence`

#### Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex

**Scenario**: `ReviewIndex.Load` is called with a `url`-type source and a fake HTTP client
returning a fixed JSON payload.

**Expected**: The index contains the entry from the JSON payload.

**Requirement coverage**: `ReviewMark-Indexing-LoadEvidence`

#### Indexing_SafePathCombine_WithTraversalInputs_Throws

**Scenario**: `PathHelpers.SafePathCombine` is called with path traversal inputs — first
with a `..`-based relative path (`../../etc/sensitive`) and then with an absolute path.

**Expected**: `ArgumentException` is thrown in both cases; directory traversal and
absolute-path injection are rejected.

**Boundary / error path**: Path traversal prevention.

**Requirement coverage**: `ReviewMark-Indexing-SafePathCombine`

#### Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Scan` is called against a directory that contains no PDF files
(only a plain text file).

**Expected**: Returns an empty index with no entries.

**Requirement coverage**: `ReviewMark-Indexing-ScanPdfEvidence`

#### Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex

**Scenario**: `ReviewIndex.Scan` is called against a directory containing a single PDF
with all required keyword metadata fields (`id`, `fingerprint`, `date`, `result`).

**Expected**: Returns an index populated with the evidence entry extracted from the PDF.

**Requirement coverage**: `ReviewMark-Indexing-ScanPdfEvidence`

### Requirements Coverage

- **ReviewMark-Indexing-LoadEvidence**: Indexing_SafePathCombine_WithIndexPath_LoadsIndex,
  Indexing_ReviewIndex_SaveAndLoad_RoundTrip, Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex,
  Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex
- **ReviewMark-Indexing-Save**: Indexing_ReviewIndex_SaveAndLoad_RoundTrip
- **ReviewMark-Indexing-SafePathCombine**: Indexing_SafePathCombine_WithIndexPath_LoadsIndex,
  Indexing_SafePathCombine_WithTraversalInputs_Throws
- **ReviewMark-Indexing-ScanPdfEvidence**: Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex,
  Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex
