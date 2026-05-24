## WeasyPrint

### Verification Approach

ReviewMark uses the `DemaConsulting.WeasyPrintTool` local tool at version 68.1.0, declared in
`.config/dotnet-tools.json`, to render generated HTML documents to PDF/A-3u in the `build-docs`
job of `build.yaml`. The integration surface is the `dotnet weasyprint` command used for the Build
Notes, Code Quality, Review Plan, Review Report, Design, Verification, and User Guide document
collections. Fitness for intended use is verified by paired FileAssert checks defined in
`.fileassert.yaml` and executed immediately after each PDF generation step, confirming that each PDF
exists, contains the expected metadata, has at least the minimum page count, and includes expected
text content. The resulting TRX evidence is written to `artifacts/fileassert-*.trx`. No
project-specific issues have been observed in this validated document-rendering path.

### Test Scenarios

**WeasyPrintBuildNotesPdf**: The Build Notes HTML output is rendered to a valid PDF with the
expected title, author, subject, minimum page count, and visible "Build Notes" content. This
scenario is tested by `WeasyPrint_BuildNotesPdf`.

**WeasyPrintCodeQualityPdf**: The Code Quality HTML output is rendered to a valid PDF with the
expected metadata and visible "CodeQL" content. This scenario is tested by
`WeasyPrint_CodeQualityPdf`.

**WeasyPrintReviewPlanPdf**: The Review Plan HTML output is rendered to a valid PDF with the
expected metadata and visible "Review Plan" content. This scenario is tested by
`WeasyPrint_ReviewPlanPdf`.

**WeasyPrintReviewReportPdf**: The Review Report HTML output is rendered to a valid PDF with the
expected metadata and visible "Review Report" content. This scenario is tested by
`WeasyPrint_ReviewReportPdf`.

**WeasyPrintDesignPdf**: The Design HTML output is rendered to a valid PDF with the expected
metadata, at least three pages, and visible design content. This scenario is tested by
`WeasyPrint_DesignPdf`.

**WeasyPrintVerificationPdf**: The Verification HTML output is rendered to a valid PDF with the
expected metadata, at least three pages, and visible verification content. This scenario is tested
by `WeasyPrint_VerificationPdf`.

**WeasyPrintUserGuidePdf**: The User Guide HTML output is rendered to a valid PDF with the expected
metadata, at least three pages, and visible user guide content. This scenario is tested by
`WeasyPrint_UserGuidePdf`.

### Requirements Coverage

- **ReviewMark-OTS-WeasyPrint**: WeasyPrint shall convert HTML documents to valid PDF.
  - *WeasyPrintBuildNotesPdf*
    - `WeasyPrint_BuildNotesPdf`
  - *WeasyPrintCodeQualityPdf*
    - `WeasyPrint_CodeQualityPdf`
  - *WeasyPrintReviewPlanPdf*
    - `WeasyPrint_ReviewPlanPdf`
  - *WeasyPrintReviewReportPdf*
    - `WeasyPrint_ReviewReportPdf`
  - *WeasyPrintDesignPdf*
    - `WeasyPrint_DesignPdf`
  - *WeasyPrintVerificationPdf*
    - `WeasyPrint_VerificationPdf`
  - *WeasyPrintUserGuidePdf*
    - `WeasyPrint_UserGuidePdf`
