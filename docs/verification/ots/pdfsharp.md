## PDFsharp

### Verification Approach

ReviewMark uses PDFsharp 6.2.4, referenced from `DemaConsulting.ReviewMark.csproj`, to read PDF
document metadata in the Indexing subsystem. The integration surface is `PdfReader.Open` with
`PdfDocumentOpenMode.Import` and `doc.Info.Keywords` inside `ReviewIndex.ProcessPdf()`, where
ReviewMark extracts `id`, `fingerprint`, `date`, and `result` values from the Keywords field and
skips incomplete evidence files with warnings. Fitness for intended use is verified by dedicated
OTS tests in `test/OtsSoftwareTests/PDFsharpTests.cs`, index integration tests in
`test/DemaConsulting.ReviewMark.Tests/Indexing/IndexTests.cs`, and the `dotnet test` step in the
`build` matrix job of `build.yaml`, which publishes TRX evidence to `artifacts/`. No
project-specific issues have been observed in this validated import-only usage.

### Test Scenarios

**PDFsharpReadMetadata**: A PDF opened in import mode exposes the Keywords metadata that ReviewMark
uses as its evidence payload, and valid review metadata is carried through into the in-memory
review index. This scenario is tested by `PdfReader_Open_ImportMode_ExposesKeywordsField`,
`ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`, and
`ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`.

**PDFsharpGracefulMetadataAbsenceHandling**: Missing or incomplete Keywords metadata does not cause
a crash; instead, ReviewMark skips the PDF and reports warnings so unrelated evidence files can
still be indexed. This scenario is tested by `PdfReader_Open_ImportMode_NoKeywords_ReturnsNullOrEmpty`,
`ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`,
`ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`, and
`ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`.

### Requirements Coverage

- **ReviewMark-OTS-PDFsharp-ReadMetadata**: PDFsharp shall provide access to the Keywords metadata
  field of a PDF document opened in import mode.
  - *PDFsharpReadMetadata*
    - `PdfReader_Open_ImportMode_ExposesKeywordsField`
    - `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`
    - `ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`
  - *PDFsharpGracefulMetadataAbsenceHandling*
    - `PdfReader_Open_ImportMode_NoKeywords_ReturnsNullOrEmpty`
    - `ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`
