## BuildMark

### Verification Approach

This repository pins DemaConsulting.BuildMark version 1.2.2 in the local tool
manifest and uses that CLI in the `build-docs` job of `build.yaml` to generate
the build-notes evidence package. Fitness for use is
verified by both self-validation and live report generation. The `Run BuildMark
self-validation` step executes `dotnet buildmark --validate` and writes
`artifacts/buildmark-self-validation.trx`. The `Generate Build Notes with
BuildMark` step then runs the same pinned tool version against the current
GitHub Actions context and writes `docs/build_notes/generated/build_notes.md`.
Because either failure stops the workflow, a passing CI run shows that BuildMark
can query the workflow metadata needed by this repository and render the
required markdown evidence. No project-specific qualification issues are
currently recorded for this pinned version.

### Test Scenarios

**BuildMarkMarkdownReportGeneration**: BuildMark queries the active GitHub
Actions run and renders the resulting metadata into
`docs/build_notes/generated/build_notes.md`, proving the tool is fit for the
repository's intended use as the build-notes generator. The expected outcome is
a generated markdown report together with a passing self-validation result in
`artifacts/buildmark-self-validation.trx`. This scenario is tested by
`BuildMark_MarkdownReportGeneration`.

### Requirements Coverage

- **BuildMark-Core-GenerateBuildNotes**: BuildMark shall generate build-notes
  documentation from GitHub Actions metadata.
  - *BuildMarkMarkdownReportGeneration*: verifies BuildMark retrieves GitHub
    Actions metadata and renders the build-notes markdown report used by this
    repository.
    - `BuildMark_MarkdownReportGeneration`
