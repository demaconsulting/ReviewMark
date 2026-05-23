## VersionMark

### Verification Approach

**Component**: DemaConsulting.VersionMark
**Role**: Captures tool version metadata and publishes a versions markdown document.
**Acceptance approach**: Automated build pipeline verification.

VersionMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is verified through the GitHub Actions workflow (`build.yaml`),
where "Run VersionMark self-validation" steps execute `dotnet versionmark --validate`
in three separate jobs:

- The `quality-checks` job runs VersionMark self-validation, writing results to
  `artifacts/versionmark-self-validation-quality.trx`.
- The `build` matrix job runs VersionMark self-validation on each operating system
  (windows-latest, ubuntu-latest, macos-latest), writing results to
  `artifacts/versionmark-self-validation-{os}.trx`.
- The `build-docs` job runs VersionMark self-validation, writing results to
  `artifacts/versionmark-self-validation.trx`, and subsequently runs
  `dotnet versionmark --publish` to generate the `docs/build_notes/generated/versions.md`
  report from all collected `artifacts/**/versionmark-*.json` capture files.

A non-zero exit from any self-validation step fails the CI build, providing
cross-platform evidence that VersionMark captured tool version information and generated
the versions markdown report correctly.

### Test scenario coverage

- **`VersionMark_CapturesVersions`** — VersionMark successfully captures version metadata
  for each tool in the pipeline and writes a JSON capture file without error.
  CI Evidence: "Run VersionMark self-validation" steps in the `quality-checks` job
  (`artifacts/versionmark-self-validation-quality.trx`), the `build` matrix job
  (`artifacts/versionmark-self-validation-{os}.trx`), and the `build-docs` job
  (`artifacts/versionmark-self-validation.trx`) of `build.yaml`.
- **`VersionMark_GeneratesMarkdownReport`** — VersionMark aggregates captured version JSON
  files and generates a markdown versions report from the pipeline metadata.
  CI Evidence: "Run VersionMark self-validation" step in the `build-docs` job of
  `build.yaml`, confirmed by the "Publish Tool Versions" step that generates
  `docs/build_notes/generated/versions.md`.

### Requirements Coverage

- **ReviewMark-OTS-VersionMark-Capture**: VersionMark shall capture tool version metadata.
  - `VersionMark_CapturesVersions`: verifies VersionMark captures version metadata for each
    pipeline tool and writes a JSON capture file without error.
    - `VersionMark_CapturesVersions`
- **ReviewMark-OTS-VersionMark-Report**: VersionMark shall generate a markdown versions
  report.
  - `VersionMark_GeneratesMarkdownReport`: verifies VersionMark aggregates captured version
    JSON files and generates a markdown versions report.
    - `VersionMark_GeneratesMarkdownReport`
