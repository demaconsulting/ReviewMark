# Off-The-Shelf Components

This section documents the verification strategy applied to each off-the-shelf (OTS)
component used by ReviewMark. For each OTS component, acceptance is based on one of
the following approaches:

- **Automated test coverage** — unit or integration tests exercise the component's
  integration surface and confirm the expected behavior.
- **Established industry use** — the component is a widely adopted, actively maintained
  open-source project with its own test suite and release process.
- **Vendor assurance** — the component is supplied and maintained by the tool vendor
  with published quality practices.

The subsections below address each component individually. Component version constraints
are defined in the relevant project files and the requirements YAML in
`docs/reqstream/ots/`.

## Verification Strategy

Each OTS component is accepted using one of the following strategies, as documented in
its individual file under `docs/verification/ots/`:

- **Self-validation with CI evidence** — tools that support a `--validate` flag
  (BuildMark, FileAssert, ReqStream, ReviewMark, SarifMark, SonarMark, VersionMark) are
  invoked with `--validate` in the GitHub Actions CI pipeline, producing TRX result files
  in `artifacts/`. A non-zero exit fails the build.
- **FileAssert integration tests** — for external build tools without a self-test
  mechanism (Pandoc, WeasyPrint), dedicated FileAssert steps assert that each expected
  output file exists, contains valid metadata, and includes expected content strings.
- **Implicit framework self-validation** — for xUnit, successful execution of the full
  ReviewMark test suite constitutes evidence that xUnit discovers tests, runs them, and
  reports results in TRX format.

## Qualification Evidence

Evidence of OTS qualification is collected on every CI run of the GitHub Actions
`build.yaml` workflow:

- **TRX result files** in `artifacts/` from each `--validate` self-validation step, named
  `{tool}-self-validation.trx`.
- **FileAssert TRX files** (`artifacts/fileassert-*.trx`) asserting that Pandoc and
  WeasyPrint produced well-formed HTML and PDF outputs with correct metadata and content.
- **ReqStream enforcement output** — the `--enforce` run in `build-docs` consumes all TRX
  files and exits non-zero if any requirement is uncovered, providing a consolidated
  qualification gate across the full OTS set.

## Regression Approach

On any OTS component version upgrade:

1. Re-run the full CI pipeline and confirm all self-validation steps pass without error.
2. Review the vendor release notes for the upgraded component; identify any changes to
   features used by ReviewMark.
3. If the integration surface changes, update the corresponding OTS file, requirement, and
   test scenarios before merging the upgrade.
4. The passing ReqStream `--enforce` run provides final confirmation that all OTS
   requirements remain covered after the upgrade.
