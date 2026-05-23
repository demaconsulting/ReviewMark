# OTS Verification

This section describes the overall qualification strategy for off-the-shelf software used by
ReviewMark. The per-item files under `docs/verification/ots/` provide the detailed verification
approach and requirements coverage for each individual OTS software item.

## Verification Strategy

ReviewMark uses three complementary approaches to verify OTS software items.

- Runtime libraries used directly by the product code, including YamlDotNet, PDFsharp,
  DemaConsulting.TestResults, and Microsoft.Extensions.FileSystemGlobbing, are verified through
  repository unit and integration tests that exercise the exact parsing, metadata,
  serialization, and globbing behaviors ReviewMark depends on.
- xUnit is qualified through successful execution of the ReviewMark test suites and their TRX
  outputs because the repository depends on xUnit for test discovery, execution, and reporting.
- Tooling OTS items such as BuildMark, ReqStream, ReviewMark, SarifMark, SonarMark,
  VersionMark, Pandoc, WeasyPrint, and FileAssert are verified through a combination of
  self-validation (`--validate`) and output assertions in the GitHub Actions workflow defined in
  `.github/workflows/build.yaml`.

The detailed evidence source and requirement mapping for each component are recorded in the
companion files under `docs/verification/ots/`.

## Qualification Evidence

Qualification evidence is collected automatically by `.github/workflows/build.yaml` and published
as CI artifacts. Primary evidence includes:

- Self-validation TRX files such as `artifacts/buildmark-self-validation.trx`,
  `artifacts/versionmark-self-validation.trx`, `artifacts/reviewmark-self-validation.trx`,
  `artifacts/sarifmark-self-validation.trx`, `artifacts/sonarmark-self-validation.trx`, and
  `artifacts/reqstream-self-validation.trx`
- `dotnet test` TRX output from the repository test projects, which provides evidence for xUnit
  and for runtime libraries exercised by ReviewMark unit and integration tests
- FileAssert TRX outputs such as `artifacts/fileassert-build-notes.trx`,
  `artifacts/fileassert-code-quality.trx`, `artifacts/fileassert-code-review.trx`,
  `artifacts/fileassert-design.trx`, `artifacts/fileassert-verification.trx`,
  `artifacts/fileassert-user-guide.trx`, `artifacts/fileassert-self-validation.trx`, and
  `artifacts/fileassert-requirements.trx`, which confirm Pandoc and WeasyPrint produced the
  expected documents and metadata
- VersionMark capture artifacts that record the exact OTS tool versions used during the build and
  support upgrade impact assessment

## Regression Approach

When an OTS version changes, ReviewMark re-qualifies the affected component by re-running the full
repository build and CI matrix, reviewing the vendor release notes for the changed package or
tool, and updating the affected requirements, design, and verification artifacts if the consumed
behavior changes. An upgrade is not accepted until the refreshed automated evidence remains green,
including `dotnet test`, the ReviewMark integration-test matrix, all applicable self-validation
steps, and the FileAssert checks that cover generated document outputs.
