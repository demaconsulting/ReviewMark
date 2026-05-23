## Pandoc

### Verification Approach

This repository pins DemaConsulting.PandocTool version 3.9.0.2 in the local
tool manifest, which exposes the `pandoc` command used throughout `build.yaml`
to convert repository markdown collections into HTML before
WeasyPrint renders the final PDFs. Pandoc is not embedded into the ReviewMark
codebase, so fitness for use is verified through repeated pipeline execution of
its real integration path rather than through a local wrapper test. The
`build-docs` job runs `dotnet pandoc` with each document collection's
`definition.yaml` file and then immediately runs FileAssert checks from
`.fileassert.yaml` against the generated HTML outputs. Because the build fails if
any expected HTML file is missing, lacks a title element, or omits required
content, a passing CI run shows that the pinned Pandoc tool version can convert
this repository's markdown collections into usable HTML evidence. No
project-specific qualification issues are currently recorded for this pinned
version.

### Test Scenarios

**PandocBuildNotesHtmlGeneration**: Pandoc converts the build notes collection
into `docs/build_notes/generated/build_notes.html`, proving it can render the
repository's release-note inputs into HTML with a valid title and expected
content. The expected outcome is a passing FileAssert check for the generated
file. This scenario is tested by `Pandoc_BuildNotesHtml`.

**PandocCodeQualityHtmlGeneration**: Pandoc converts the code quality collection
into `docs/code_quality/generated/quality.html`, proving it can render the
CodeQL and SonarCloud markdown inputs used by this repository. The expected
outcome is a passing FileAssert check for the generated file. This scenario is
tested by `Pandoc_CodeQualityHtml`.

**PandocReviewPlanHtmlGeneration**: Pandoc converts the code review plan
collection into `docs/code_review_plan/generated/plan.html`, proving it can
render ReviewMark's generated planning markdown into the published HTML form.
The expected outcome is a passing FileAssert check for the generated file. This
scenario is tested by `Pandoc_ReviewPlanHtml`.

**PandocReviewReportHtmlGeneration**: Pandoc converts the code review report
collection into `docs/code_review_report/generated/report.html`, proving it can
render ReviewMark's generated reporting markdown into the published HTML form.
The expected outcome is a passing FileAssert check for the generated file. This
scenario is tested by `Pandoc_ReviewReportHtml`.

**PandocDesignHtmlGeneration**: Pandoc converts the software design collection
into `docs/design/generated/design.html`, proving it can render the checked-in
technical design documentation used by this repository. The expected outcome is
a passing FileAssert check for the generated file. This scenario is tested by
`Pandoc_DesignHtml`.

**PandocVerificationHtmlGeneration**: Pandoc converts the verification
collection into `docs/verification/generated/verification.html`, proving it can
render the verification design and OTS verification content used for compliance
evidence. The expected outcome is a passing FileAssert check for the generated
file. This scenario is tested by `Pandoc_VerificationHtml`.

**PandocUserGuideHtmlGeneration**: Pandoc converts the user guide collection
into `docs/user_guide/generated/user_guide.html`, proving it can render the
end-user documentation published by the repository. The expected outcome is a
passing FileAssert check for the generated file. This scenario is tested by
`Pandoc_UserGuideHtml`.

### Requirements Coverage

- **ReviewMark-Pandoc-ConvertMarkdown**: Pandoc shall convert Markdown documents
  to HTML containing a valid title element and expected document content.
  - *PandocBuildNotesHtmlGeneration*: verifies Pandoc generates the build notes
    HTML output with a title element and expected content.
    - `Pandoc_BuildNotesHtml`
  - *PandocCodeQualityHtmlGeneration*: verifies Pandoc generates the code
    quality HTML output with a title element and expected content.
    - `Pandoc_CodeQualityHtml`
  - *PandocReviewPlanHtmlGeneration*: verifies Pandoc generates the review plan
    HTML output with a title element and expected content.
    - `Pandoc_ReviewPlanHtml`
  - *PandocReviewReportHtmlGeneration*: verifies Pandoc generates the review
    report HTML output with a title element and expected content.
    - `Pandoc_ReviewReportHtml`
  - *PandocDesignHtmlGeneration*: verifies Pandoc generates the design HTML
    output with a title element and expected content.
    - `Pandoc_DesignHtml`
  - *PandocVerificationHtmlGeneration*: verifies Pandoc generates the
    verification HTML output with a title element and expected content.
    - `Pandoc_VerificationHtml`
  - *PandocUserGuideHtmlGeneration*: verifies Pandoc generates the user guide
    HTML output with a title element and expected content.
    - `Pandoc_UserGuideHtml`
