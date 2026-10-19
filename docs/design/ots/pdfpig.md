## PdfPig

PdfPig is the .NET PDF reading library used by the Indexing subsystem to read metadata from review
evidence PDF files.

### Purpose

PdfPig was chosen for its ability to open existing PDF documents read-only and expose the document
information dictionary without any further processing of page content. ReviewMark requires only the
Keywords metadata field from each evidence PDF; PdfPig's `PdfDocument.Open` provides exactly this
access path with minimal overhead and no risk of accidental document modification.

### Features Used

- **`PdfDocument.Open(string path)`**: opens a PDF file at the given path; PdfPig documents are
  always read-only, so there is no open mode to select
- **`PdfDocument.Information.Keywords`**: the Keywords field from the PDF document information
  dictionary; ReviewMark encodes four required fields in this field using a structured
  space-separated `name=value` format: `id`, `fingerprint`, `date`, and `result`

### Integration Pattern

`ReviewIndex.ProcessPdf()` calls `PdfDocument.Open(fullPath)` inside a `using` statement to ensure
the document handle is disposed immediately after the Keywords field is read (`PdfDocument` is
`IDisposable`). The `doc.Information.Keywords` string is then parsed to extract the `id`,
`fingerprint`, `date`, and `result` fields. If the Keywords field is absent (`null`) it is treated
as an empty string and the PDF is skipped with a warning. If the field is present but any of the
four required fields are absent or empty, the PDF is also skipped with a warning identifying the
missing field, allowing the index scan to accumulate all valid evidence files even when the scanned
directory contains non-review PDFs or partially-tagged PDFs.

### Behavioral Notes

- PdfPig opens documents read-only; there is no import mode concept and no way to modify the
  document.
- Corrupt or non-PDF files cause PdfPig to throw `PdfDocumentFormatException` (and occasionally
  other exception types). `ReviewIndex.Scan()` catches `Exception` around PDF processing, so the
  file is skipped with a warning that includes the library-provided message. The exception types
  and message text are library-defined and are not asserted by ReviewMark tests.
