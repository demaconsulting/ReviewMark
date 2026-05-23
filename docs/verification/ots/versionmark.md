## VersionMark

### Verification Approach

ReviewMark uses the `DemaConsulting.VersionMark` local tool at version 1.4.3, declared in
`.config/dotnet-tools.json`, to capture tool-version metadata and publish a consolidated versions
report for the build notes. The integration surface is the `dotnet versionmark --capture`,
`--validate`, and `--publish` commands used throughout `build.yaml`: the `quality-checks` job,
the cross-platform `build` matrix job, the `integration-test` matrix job, and the `build-docs`
job all capture JSON version records, while the `build-docs` job also runs self-validation and
publishes `docs/build_notes/generated/versions.md` from `artifacts/**/versionmark-*.json`. A
non-zero exit from any self-validation or publish step fails the pipeline, so successful CI runs
provide direct evidence that VersionMark is fit for this project's traceability workflow. No
project-specific issues have been observed in this validated capture-and-publish path.

### Test Scenarios

**VersionMarkCapture**: VersionMark captures version metadata for the tools used in each pipeline
stage and writes structured JSON records without interrupting the build. This scenario is tested by
`VersionMark_CapturesVersions`.

**VersionMarkReportGeneration**: VersionMark aggregates the captured JSON records and publishes a
human-readable markdown report for the build notes document. This scenario is tested by
`VersionMark_GeneratesMarkdownReport`.

### Requirements Coverage

- **ReviewMark-OTS-VersionMark-Capture**: VersionMark shall capture tool version metadata.
  - *VersionMarkCapture*
    - `VersionMark_CapturesVersions`
- **ReviewMark-OTS-VersionMark-Report**: VersionMark shall generate a markdown versions report.
  - *VersionMarkReportGeneration*
    - `VersionMark_GeneratesMarkdownReport`
