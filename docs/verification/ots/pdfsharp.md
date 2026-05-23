## PDFsharp

### Verification Approach

**Component**: PDFsharp (<https://docs.pdfsharp.net/>)
**Role**: PDF import library used by the Indexing subsystem to read the Keywords metadata field
from review evidence PDF files.
**Acceptance approach**: Automated test coverage.

PDFsharp's integration surface in ReviewMark is limited to `PdfReader.Open` and
`PdfDocument.Info.Keywords`, used by `ReviewIndex.ScanPdfFile()` to extract the review identifier
and content fingerprint embedded in each evidence PDF. The OTS integration is exercised directly by
`DemaConsulting.ReviewMark.OtsSoftwareTests`, and through ReviewMark's higher-level behavior by
`IndexTests.cs`.

### Test Scenarios

#### PDFsharpReadMetadata

Evidence that PDFsharp correctly opens a PDF file in import mode and exposes the Keywords field
from the document information dictionary.

- **`PdfReader_Open_ImportMode_ExposesKeywordsField`** — a PDF containing a Keywords value is
  opened with `PdfDocumentOpenMode.Import` and `doc.Info.Keywords` returns the expected value,
  confirming direct OTS API access.
- **`PdfReader_Open_ImportMode_NoKeywords_ReturnsNullOrEmpty`** — a PDF created without a
  Keywords value is opened in import mode and `doc.Info.Keywords` is null or empty, confirming
  graceful handling of absent metadata.
- **`ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`** — a PDF containing correctly
  formatted review metadata in its Keywords field is opened and the review identifier and
  fingerprint are extracted into the index.
- **`ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`** — a PDF with no Keywords field is
  opened without error and skipped with a warning, confirming graceful handling of absent metadata.
- **`ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`** — a PDF whose Keywords field lacks a
  review identifier is skipped with a warning.
- **`ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`** — a PDF whose Keywords field
  lacks a fingerprint is skipped with a warning.
- **`ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`** — multiple PDFs in the same directory
  are each opened and their metadata extracted, confirming per-file handling.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

### Requirements Coverage

- **ReviewMark-OTS-PDFsharp-ReadMetadata**: PDFsharp shall provide access to the Keywords metadata
  field of a PDF document opened in import mode.
  - *PDFsharpReadMetadata*: verifies PDFsharp opens PDFs, reads Keywords metadata, and handles
    absent or incomplete metadata gracefully.
    - `PdfReader_Open_ImportMode_ExposesKeywordsField`
    - `PdfReader_Open_ImportMode_NoKeywords_ReturnsNullOrEmpty`
    - `ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex`
    - `ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning`
    - `ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning`
    - `ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries`
