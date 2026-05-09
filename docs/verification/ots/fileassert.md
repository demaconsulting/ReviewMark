## FileAssert

**Component**: DemaConsulting.FileAssert
**Role**: Validates that required files are present and well-formed as part of the CI build.
**Acceptance approach**: Automated build pipeline verification.

FileAssert is invoked in the GitHub Actions CI workflow (`build.yaml`) within the
`build-docs` job. After Pandoc and WeasyPrint generate each document group, a dedicated
"Assert ... Documents with FileAssert" step validates the outputs. The "Run FileAssert
self-validation" step runs `dotnet fileassert --validate` after all document groups are
generated, producing `artifacts/fileassert-self-validation.trx`. A non-zero exit from
any FileAssert step fails the build, providing evidence that FileAssert is operating
correctly.

### Test scenario coverage

- **`FileAssert_VersionDisplay`** — FileAssert's self-validation confirms the tool can
  display its version, proving it is correctly installed and operationally available.
  CI Evidence: "Run FileAssert self-validation" step in the `build-docs` job of
  `build.yaml`, writing results to `artifacts/fileassert-self-validation.trx`.
- **`FileAssert_HelpDisplay`** — FileAssert's self-validation confirms the tool can
  display its help text, proving the CLI interface is correctly wired.
  CI Evidence: Same "Run FileAssert self-validation" step, same TRX file.

Both scenarios together confirm `ReviewMark-OTS-FileAssert`: FileAssert is present,
operational, and able to perform its assertion role in the pipeline.

**Requirement coverage**: `ReviewMark-OTS-FileAssert`
