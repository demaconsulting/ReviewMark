## PdfPig

### Verification Approach

ReviewMark uses PdfPig 0.1.16, referenced from `DemaConsulting.ReviewMark.csproj`, to read the
Keywords field of evidence PDFs in `ReviewIndex.ProcessPdf()`. No dedicated OTS test is needed:
fitness for ReviewMark's use is verified by the `ReviewIndex_Scan_*` tests in
`test/DemaConsulting.ReviewMark.Tests/Indexing/IndexTests.cs`, which build real PDFs and scan them,
and by the `dotnet test` step in the `build` matrix job of `build.yaml`, which publishes TRX
evidence to `artifacts/`. PdfPig's behavior beyond this use is not characterized.

### Test Scenarios

**PdfPigReadMetadata**: Valid review metadata in a PDF's Keywords field is read and carried into
the in-memory review index. Tested by `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex` and
`ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`.

**PdfPigMissingMetadata**: PDFs with missing or incomplete Keywords metadata are skipped with a
warning so other evidence files are still indexed. Tested by
`ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`,
`ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`, and
`ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`.

### Requirements Coverage

- **ReviewMark-OTS-PdfPig-ReadMetadata**: ReviewMark shall read the review metadata from the
  Keywords field of evidence PDF files using PdfPig.
  - *PdfPigReadMetadata*
    - `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`
    - `ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`
  - *PdfPigMissingMetadata*
    - `ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`
