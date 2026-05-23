## PDFsharp

PDFsharp is the .NET PDF creation and manipulation library used by the Indexing subsystem to read
metadata from review evidence PDF files.

### Purpose

PDFsharp was chosen for its ability to open existing PDF documents in a lightweight read-only
import mode without loading full content streams into memory. ReviewMark requires only the
Keywords metadata field from each evidence PDF; PDFsharp's `PdfDocumentOpenMode.Import` provides
exactly this access path with minimal overhead and no risk of accidental document modification.

### Features Used

- **`PdfReader.Open(string path, PdfDocumentOpenMode mode)`**: opens a PDF file at the given path
  using the specified open mode; ReviewMark always passes `PdfDocumentOpenMode.Import`
- **`PdfDocumentOpenMode.Import`**: open mode that loads only the cross-reference table and
  document information dictionary; content streams are not decoded and the document cannot be
  modified
- **`PdfDocument.Info.Keywords`**: the Keywords field from the PDF document information
  dictionary; ReviewMark encodes the review identifier and content fingerprint in this field using
  a structured key-value format

### Version Constraints

The project pins PDFsharp at version **6.2.4** in
`src/DemaConsulting.ReviewMark/DemaConsulting.ReviewMark.csproj`. Patch and minor upgrades are
managed automatically by Dependabot; major version upgrades require a review of vendor release
notes and, if the integration surface changes, an update to this document before merging. See
`docs/design/ots.md` for the project-wide OTS version management policy.

### Integration Pattern

`ReviewIndex.ScanPdfFile()` calls `PdfReader.Open(fullPath, PdfDocumentOpenMode.Import)` inside a
`using` statement to ensure the document handle is disposed immediately after the Keywords field is
read. The `doc.Info.Keywords` string is then parsed to extract the review identifier and
fingerprint. If the Keywords field is absent (`null`) it is treated as an empty string and the PDF
is skipped. If the field is present but does not contain the expected review metadata, the PDF is
also skipped, allowing the index scan to accumulate all valid evidence files even when the scanned
directory contains non-review PDFs.
