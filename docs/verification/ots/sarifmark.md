## SarifMark

### Verification Approach

This repository pins DemaConsulting.SarifMark version 1.3.2 in the local tool
manifest and uses that CLI in the `build-docs` job of `build.yaml` to transform
CodeQL SARIF output into markdown evidence. Fitness
for use is verified by both self-validation and live pipeline execution. The
`Run SarifMark self-validation` step executes `dotnet sarifmark --validate` and
writes `artifacts/sarifmark-self-validation.trx`. The `Generate CodeQL Quality
Report with SarifMark` step then processes `artifacts/csharp.sarif` and writes
`docs/code_quality/generated/codeql-quality.md`, which is later checked by
FileAssert. Because either failure stops the workflow, a passing CI run shows
that the pinned tool version can read the repository's SARIF input and render
its required markdown report. No project-specific qualification issues are
currently recorded for this pinned version.

### Test Scenarios

**SarifMarkSarifReading**: SarifMark reads the CodeQL SARIF file produced in the
same workflow, proving the tool can parse the machine-readable analysis format
used by this repository. The expected outcome is a passing self-validation
result in `artifacts/sarifmark-self-validation.trx`. This scenario is tested by
`SarifMark_SarifReading`.

**SarifMarkMarkdownReportGeneration**: SarifMark renders the parsed SARIF data
into `docs/code_quality/generated/codeql-quality.md`, proving the tool is fit
for its intended use as the CodeQL markdown report generator in this build. The
expected outcome is a generated report followed by a successful FileAssert
validation step. This scenario is tested by
`SarifMark_MarkdownReportGeneration`.

### Requirements Coverage

- **ReviewMark-OTS-SarifMark**: SarifMark shall convert CodeQL SARIF results
  into a markdown report.
  - *SarifMarkSarifReading*: verifies SarifMark reads and parses the CodeQL
    SARIF input used by the build.
    - `SarifMark_SarifReading`
  - *SarifMarkMarkdownReportGeneration*: verifies SarifMark renders the parsed
    SARIF input into the markdown report consumed by this repository.
    - `SarifMark_MarkdownReportGeneration`
