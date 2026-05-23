## SonarMark

### Verification Approach

**Component**: DemaConsulting.SonarMark
**Role**: Generates markdown reports from SonarCloud analysis results.
**Acceptance approach**: Automated build pipeline verification.

SonarMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is verified through the GitHub Actions workflow (`build.yaml`),
where two steps run within the `build-docs` job. The "Run SonarMark self-validation"
step executes `dotnet sonarmark --validate` and writes results to
`artifacts/sonarmark-self-validation.trx`, confirming the tool is correctly installed
and its internal self-test scenarios pass. The "Generate SonarCloud Quality Report" step
calls `dotnet sonarmark --server https://sonarcloud.io … --report docs/code_quality/generated/sonar-quality.md`,
retrieving quality-gate, issues, and hotspots data from SonarCloud and rendering it as a
markdown document. A non-zero exit from either step fails the CI build.

The `--validate` self-validation step exercises four named test scenarios that cover the
full retrieval-and-reporting workflow: quality-gate status retrieval, issues retrieval,
hotspots retrieval, and markdown report generation.

### Test scenario coverage

- **`SonarMark_QualityGateRetrieval`** — SonarMark successfully retrieves the quality-gate
  status from a SonarCloud project. CI Evidence: "Run SonarMark self-validation" step in
  the `build-docs` job of `build.yaml`, writing results to
  `artifacts/sonarmark-self-validation.trx`.
- **`SonarMark_IssuesRetrieval`** — SonarMark successfully retrieves the list of open
  issues from a SonarCloud project. CI Evidence: Same "Run SonarMark self-validation"
  step, same TRX file.
- **`SonarMark_HotSpotsRetrieval`** — SonarMark successfully retrieves the list of
  security hotspots from a SonarCloud project. CI Evidence: Same "Run SonarMark
  self-validation" step, same TRX file.
- **`SonarMark_MarkdownReportGeneration`** — SonarMark generates a markdown quality report
  from retrieved SonarCloud data, producing the expected report document. CI Evidence:
  Same "Run SonarMark self-validation" step and the "Generate SonarCloud Quality Report"
  step in the `build-docs` job, confirmed by successful report generation.

### Requirements Coverage

- **ReviewMark-OTS-SonarMark**: SonarMark shall generate a SonarCloud quality report.
  - `SonarMark_QualityGateRetrieval`: verifies SonarMark retrieves quality-gate status from a
    SonarCloud project.
    - `SonarMark_QualityGateRetrieval`
  - `SonarMark_IssuesRetrieval`: verifies SonarMark retrieves the list of open issues from a
    SonarCloud project.
    - `SonarMark_IssuesRetrieval`
  - `SonarMark_HotSpotsRetrieval`: verifies SonarMark retrieves the list of security hotspots
    from a SonarCloud project.
    - `SonarMark_HotSpotsRetrieval`
  - `SonarMark_MarkdownReportGeneration`: verifies SonarMark generates a markdown quality
    report from retrieved SonarCloud data.
    - `SonarMark_MarkdownReportGeneration`
