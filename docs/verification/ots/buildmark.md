## BuildMark

**Component**: DemaConsulting.BuildMark
**Role**: Provides the `buildmark` CLI tool used in the build pipeline.
**Acceptance approach**: Established industry use and automated build pipeline verification.

BuildMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is verified through the GitHub Actions workflow (`build.yaml`),
where the "Run BuildMark self-validation" step and the "Generate Build Notes with BuildMark"
step run as part of the `build-docs` job. A successful CI pipeline run provides evidence
that BuildMark executed without error and produced its expected markdown output.

### Test scenario coverage

- **`BuildMark_MarkdownReportGeneration`** — BuildMark successfully queries the GitHub
  Actions API and generates a markdown build-notes document from workflow run metadata.
  CI Evidence: "Run BuildMark self-validation" step in the `build-docs` job of
  `build.yaml`, writing results to `artifacts/buildmark-self-validation.trx`.

**Requirement coverage**: `ReviewMark-OTS-BuildMark`
