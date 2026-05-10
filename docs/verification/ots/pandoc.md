## Pandoc

**Component**: Pandoc (<https://pandoc.org/>)
**Role**: Converts Markdown source documents into valid HTML as part of the documentation
build pipeline. WeasyPrint subsequently renders the HTML to PDF.
**Acceptance approach**: Automated test coverage.

Pandoc is a widely adopted open-source universal document converter with over a decade
of active development, extensive automated testing, and broad production usage.

ReviewMark does not embed Pandoc; it is an external build dependency. Correct Pandoc
behaviour is confirmed by FileAssert integration tests in the GitHub Actions CI workflow
(`build.yaml`), which run within the `build-docs` job. Each document group has a
dedicated Pandoc HTML generation step followed by a FileAssert validation step that
asserts the HTML file exists, contains a valid `<title>` element, and includes expected
content strings.

### Test scenario coverage

- **`Pandoc_BuildNotesHtml`** — Pandoc generated
  `docs/build_notes/generated/build_notes.html` with a valid title element and
  "Build Notes" content. CI Evidence: "Assert Build Notes Documents with FileAssert"
  step → `artifacts/fileassert-build-notes.trx`.
- **`Pandoc_CodeQualityHtml`** — Pandoc generated
  `docs/code_quality/generated/quality.html` with a valid title element and
  "CodeQL" content. CI Evidence: "Assert Code Quality Documents with FileAssert"
  step → `artifacts/fileassert-code-quality.trx`.
- **`Pandoc_ReviewPlanHtml`** — Pandoc generated
  `docs/code_review_plan/generated/plan.html` with a valid title element and
  "Review Plan" content. CI Evidence: "Assert Code Review Documents with FileAssert"
  step → `artifacts/fileassert-code-review.trx`.
- **`Pandoc_ReviewReportHtml`** — Pandoc generated
  `docs/code_review_report/generated/report.html` with a valid title element and
  "Review Report" content. CI Evidence: "Assert Code Review Documents with FileAssert"
  step → `artifacts/fileassert-code-review.trx`.
- **`Pandoc_DesignHtml`** — Pandoc generated
  `docs/design/generated/design.html` with a valid title element and "Design"
  content. CI Evidence: "Assert Design Documents with FileAssert"
  step → `artifacts/fileassert-design.trx`.
- **`Pandoc_VerificationHtml`** — Pandoc generated
  `docs/verification/generated/verification.html` with a valid title element and
  "Verification" content. CI Evidence: "Assert Verification Documents with FileAssert"
  step → `artifacts/fileassert-verification.trx`.
- **`Pandoc_UserGuideHtml`** — Pandoc generated
  `docs/user_guide/generated/user_guide.html` with a valid title element and
  "User Guide" content. CI Evidence: "Assert User Guide Documents with FileAssert"
  step → `artifacts/fileassert-user-guide.trx`.

Each scenario directly satisfies `ReviewMark-OTS-Pandoc` by providing FileAssert-verified
evidence that Pandoc converted Markdown to well-formed HTML containing expected content.

**Requirement coverage**: `ReviewMark-OTS-Pandoc`
