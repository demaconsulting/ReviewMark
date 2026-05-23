## WeasyPrint

### Verification Approach

**Component**: WeasyPrint (<https://weasyprint.org/>)
**Role**: Converts HTML documents to PDF as part of the documentation build pipeline.
**Acceptance approach**: FileAssert integration tests validating each PDF output.

WeasyPrint is a widely adopted open-source HTML/CSS-to-PDF converter used in the build
pipeline. ReviewMark does not embed WeasyPrint; it is an external build dependency.
Correct WeasyPrint behavior is confirmed by FileAssert integration tests in the GitHub
Actions CI workflow (`build.yaml`), which run within the `build-docs` job on the
`windows-latest` runner. Each document group has a dedicated WeasyPrint PDF generation
step followed by a FileAssert validation step that asserts the PDF file exists, contains
correct PDF metadata (Title, Author, Subject), has at least the minimum expected page
count, and contains expected document text content.

FileAssert integration tests validate that each WeasyPrint invocation produced a
well-formed PDF with correct metadata, at least one page, and expected document content.

### Test scenario coverage

- **`WeasyPrint_BuildNotesPdf`** (Build Notes) — WeasyPrint generated
  `"docs/generated/ReviewMark Build Notes.pdf"` with Title containing "ReviewMark",
  Author "DEMA Consulting", Subject "Build notes", at least 1 page, and text containing
  "Build Notes". CI Evidence: "Assert Build Notes Documents with FileAssert" step →
  `artifacts/fileassert-build-notes.trx`.
- **`WeasyPrint_CodeQualityPdf`** (Code Quality) — WeasyPrint generated
  `"docs/generated/ReviewMark Code Quality.pdf"` with Title containing "Code Quality",
  Author "DEMA Consulting", Subject "Code Quality", at least 1 page, and text containing
  "CodeQL". CI Evidence: "Assert Code Quality Documents with FileAssert" step →
  `artifacts/fileassert-code-quality.trx`.
- **`WeasyPrint_ReviewPlanPdf`** (Review Plan) — WeasyPrint generated
  `"docs/generated/ReviewMark Review Plan.pdf"` with Title containing "Review Plan",
  Author "DEMA Consulting", Subject "Review Plan", at least 1 page, and text containing
  "Review Plan". CI Evidence: "Assert Code Review Documents with FileAssert" step →
  `artifacts/fileassert-code-review.trx`.
- **`WeasyPrint_ReviewReportPdf`** (Review Report) — WeasyPrint generated
  `"docs/generated/ReviewMark Review Report.pdf"` with Title containing "Review Report",
  Author "DEMA Consulting", Subject "Review Report", at least 1 page, and text containing
  "Review Report". CI Evidence: "Assert Code Review Documents with FileAssert" step →
  `artifacts/fileassert-code-review.trx`.
- **`WeasyPrint_DesignPdf`** (Design) — WeasyPrint generated
  `"docs/generated/ReviewMark Software Design.pdf"` with Title containing "Design",
  Author "DEMA Consulting", Subject "Design Document", at least 3 pages, and text
  containing "Design". CI Evidence: "Assert Design Documents with FileAssert" step →
  `artifacts/fileassert-design.trx`.
- **`WeasyPrint_VerificationPdf`** (Verification) — WeasyPrint generated
  `"docs/generated/ReviewMark Software Verification Design.pdf"` with Title containing
  "Verification", Author "DEMA Consulting", Subject "Verification design document",
  at least 3 pages, and text containing "Verification". CI Evidence: "Assert
  Verification Documents with FileAssert" step → `artifacts/fileassert-verification.trx`.
- **`WeasyPrint_UserGuidePdf`** (User Guide) — WeasyPrint generated
  `"docs/generated/ReviewMark User Guide.pdf"` with Title containing "User Guide",
  Author "DEMA Consulting", Subject "File-Review Evidence Management", at least 3 pages,
  and text containing "User Guide". CI Evidence: "Assert User Guide Documents with
  FileAssert" step → `artifacts/fileassert-user-guide.trx`.

### Requirements Coverage

- **ReviewMark-OTS-WeasyPrint**: WeasyPrint shall convert HTML documents to valid PDF.
  - `WeasyPrint_BuildNotesPdf`: verifies WeasyPrint generates `ReviewMark Build Notes.pdf`
    with correct metadata (Title, Author, Subject) and expected document content.
    - `WeasyPrint_BuildNotesPdf`
  - `WeasyPrint_CodeQualityPdf`: verifies WeasyPrint generates `ReviewMark Code Quality.pdf`
    with correct metadata and expected document content.
    - `WeasyPrint_CodeQualityPdf`
  - `WeasyPrint_ReviewPlanPdf`: verifies WeasyPrint generates `ReviewMark Review Plan.pdf`
    with correct metadata and expected document content.
    - `WeasyPrint_ReviewPlanPdf`
  - `WeasyPrint_ReviewReportPdf`: verifies WeasyPrint generates `ReviewMark Review Report.pdf`
    with correct metadata and expected document content.
    - `WeasyPrint_ReviewReportPdf`
  - `WeasyPrint_DesignPdf`: verifies WeasyPrint generates `ReviewMark Software Design.pdf`
    with correct metadata and at least the expected page count and document content.
    - `WeasyPrint_DesignPdf`
  - `WeasyPrint_VerificationPdf`: verifies WeasyPrint generates `ReviewMark Software
    Verification Design.pdf` with correct metadata and expected document content.
    - `WeasyPrint_VerificationPdf`
  - `WeasyPrint_UserGuidePdf`: verifies WeasyPrint generates `ReviewMark User Guide.pdf`
    with correct metadata and expected document content.
    - `WeasyPrint_UserGuidePdf`
