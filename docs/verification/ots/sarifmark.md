## SarifMark

**Component**: DemaConsulting.SarifMark
**Role**: Generates markdown reports from SARIF static analysis output files.
**Acceptance approach**: Automated build pipeline verification.

SarifMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is verified through the GitHub Actions workflow (`build.yaml`),
where two steps run within the `build-docs` job. The "Run SarifMark self-validation"
step executes `dotnet sarifmark --validate` and writes results to
`artifacts/sarifmark-self-validation.trx`. The "Generate CodeQL Quality Report with
SarifMark" step reads the CodeQL SARIF output from `artifacts/csharp.sarif` and renders
it as a markdown quality report at `docs/code_quality/generated/codeql-quality.md`. A
non-zero exit from either step fails the CI build, providing evidence that SarifMark
read the SARIF file and generated the report correctly.

### Test scenario coverage

- **`SarifMark_SarifReading`** — SarifMark successfully reads a SARIF file from CodeQL
  code scanning and parses it without error. CI Evidence: "Run SarifMark self-validation"
  step in the `build-docs` job of `build.yaml`, writing results to
  `artifacts/sarifmark-self-validation.trx`.
- **`SarifMark_MarkdownReportGeneration`** — SarifMark generates a markdown quality
  report from a CodeQL SARIF input, producing
  `docs/code_quality/generated/codeql-quality.md`. CI Evidence: "Generate CodeQL Quality
  Report with SarifMark" step in the `build-docs` job of `build.yaml`, confirmed by the
  subsequent FileAssert validation.

Both scenarios together confirm `ReviewMark-OTS-SarifMark`: SarifMark correctly reads
SARIF input produced by CodeQL and renders it as a human-readable markdown report.

**Requirement coverage**: `ReviewMark-OTS-SarifMark`
