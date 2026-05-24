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
  dictionary; ReviewMark encodes four required fields in this field using a structured
  space-separated `name=value` format: `id`, `fingerprint`, `date`, and `result`

### Integration Pattern

`ReviewIndex.ProcessPdf()` calls `PdfReader.Open(fullPath, PdfDocumentOpenMode.Import)` inside a
`using` statement to ensure the document handle is disposed immediately after the Keywords field is
read. The `doc.Info.Keywords` string is then parsed to extract the `id`, `fingerprint`, `date`, and
`result` fields. If the Keywords field is absent (`null`) it is treated as an empty string and the
PDF is skipped with a warning. If the field is present but any of the four required fields are
absent or empty, the PDF is also skipped with a warning identifying the missing field, allowing the
index scan to accumulate all valid evidence files even when the scanned directory contains
non-review PDFs or partially-tagged PDFs.
