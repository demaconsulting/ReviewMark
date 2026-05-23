## SonarMark

### Verification Approach

This repository pins DemaConsulting.SonarMark version 1.5.0 in the local tool
manifest and uses that CLI in the `build-docs` job of `build.yaml` to generate
the SonarCloud portion of the code quality evidence.
Fitness for use is verified in two ways. First, the `Run SonarMark
self-validation` step executes `dotnet sonarmark --validate` and writes
`artifacts/sonarmark-self-validation.trx`. Second, the `Generate SonarCloud
Quality Report` step uses the same pinned tool version to query SonarCloud for
the ReviewMark SonarCloud project and render
`docs/code_quality/generated/sonar-quality.md`. Because either failure stops the
workflow, a passing CI run shows that the tool can retrieve the data needed by
this repository and generate the required markdown output. No project-specific
qualification issues are currently recorded for this pinned version.

### Test Scenarios

**SonarMarkQualityGateRetrieval**: SonarMark retrieves the current quality-gate
status from the configured SonarCloud project, proving the pinned tool version
can authenticate and read the primary pass/fail signal used in the quality
report. The expected outcome is a passing self-validation result recorded in
`artifacts/sonarmark-self-validation.trx`. This scenario is tested by
`SonarMark_QualityGateRetrieval`.

**SonarMarkIssuesRetrieval**: SonarMark retrieves the open-issues data set from
SonarCloud so that the repository can include actionable issue information in
its generated code quality evidence. The expected outcome is a passing
self-validation result for issue retrieval in
`artifacts/sonarmark-self-validation.trx`. This scenario is tested by
`SonarMark_IssuesRetrieval`.

**SonarMarkHotSpotsRetrieval**: SonarMark retrieves the security hot spot data
set from SonarCloud so that the quality report covers the security review data
used by this project. The expected outcome is a passing self-validation result
for security hot spot retrieval in `artifacts/sonarmark-self-validation.trx`.
This
scenario is tested by `SonarMark_HotSpotsRetrieval`.

**SonarMarkMarkdownReportGeneration**: SonarMark renders the retrieved
SonarCloud data into `docs/code_quality/generated/sonar-quality.md`, proving
that the tool is fit for the repository's intended use as a markdown evidence
producer. The expected outcome is a generated report with a zero-exit workflow
step in `build.yaml`. This scenario is tested by
`SonarMark_MarkdownReportGeneration`.

### Requirements Coverage

- **ReviewMark-OTS-SonarMark**: SonarMark shall generate a SonarCloud quality
  report.
  - *SonarMarkQualityGateRetrieval*: verifies SonarMark retrieves the current
    quality-gate status needed by the report.
    - `SonarMark_QualityGateRetrieval`
  - *SonarMarkIssuesRetrieval*: verifies SonarMark retrieves the open-issues
    data included in the report.
    - `SonarMark_IssuesRetrieval`
  - *SonarMarkHotSpotsRetrieval*: verifies SonarMark retrieves the
    security hot spot data included in the report.
    - `SonarMark_HotSpotsRetrieval`
  - *SonarMarkMarkdownReportGeneration*: verifies SonarMark renders the
    retrieved SonarCloud data into the markdown report consumed by this
    repository.
    - `SonarMark_MarkdownReportGeneration`
