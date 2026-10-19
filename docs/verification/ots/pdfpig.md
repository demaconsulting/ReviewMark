## PdfPig

### Verification Approach

ReviewMark uses PdfPig 0.1.16, referenced from `DemaConsulting.ReviewMark.csproj`, to read PDF
document metadata in the Indexing subsystem. The integration surface is `PdfDocument.Open` and
`doc.Information.Keywords` inside `ReviewIndex.ProcessPdf()`, where ReviewMark extracts `id`,
`fingerprint`, `date`, and `result` values from the Keywords field and skips incomplete evidence
files with warnings. Fitness for intended use is verified by dedicated OTS tests in
`test/OtsSoftwareTests/PdfPigTests.cs`, index integration tests in
`test/DemaConsulting.ReviewMark.Tests/Indexing/IndexTests.cs`, and the `dotnet test` step in the
`build` matrix job of `build.yaml`, which publishes TRX evidence to `artifacts/`. No
project-specific issues have been observed in this validated read-only usage.

PdfPig differs from the previously used library in that documents are always opened read-only (no
open mode), and malformed files raise `PdfDocumentFormatException` (or other library-defined
exceptions) with library-defined messages. `ReviewIndex.Scan()` catches all exceptions per file, so
tests do not assert on exception types or library message text.

### Test Scenarios

**PdfPigReadMetadata**: A PDF opened with PdfPig exposes the Keywords metadata that ReviewMark
uses as its evidence payload, and valid review metadata is carried through into the in-memory
review index. This scenario is tested by `PdfDocument_Open_ExposesKeywordsField`,
`ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`, and
`ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`.

**PdfPigGracefulMetadataAbsenceHandling**: Missing or incomplete Keywords metadata does not cause
a crash; instead, ReviewMark skips the PDF and reports warnings so unrelated evidence files can
still be indexed. This scenario is tested by `PdfDocument_Open_NoKeywords_ReturnsNullOrEmpty`,
`ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`,
`ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`, and
`ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`.

### Requirements Coverage

- **ReviewMark-OTS-PdfPig-ReadMetadata**: PdfPig shall provide access to the Keywords metadata
  field of a PDF document opened read-only.
  - *PdfPigReadMetadata*
    - `PdfDocument_Open_ExposesKeywordsField`
    - `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`
    - `ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`
  - *PdfPigGracefulMetadataAbsenceHandling*
    - `PdfDocument_Open_NoKeywords_ReturnsNullOrEmpty`
    - `ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`
