## FileAssert

### Verification Approach

This repository pins DemaConsulting.FileAssert version 0.3.0 in the local tool
manifest and uses that CLI as the document-output assertion gate in the
`build-docs` job of `build.yaml`. Fitness for use is verified in two
complementary ways. First, FileAssert is exercised repeatedly in the live
pipeline through the `Assert ... Documents with FileAssert` steps, which execute
the checked-in assertions from `.fileassert.yaml` against generated HTML and PDF
artifacts for the build notes, code quality, code review, design, verification,
user guide, and requirements document sets. Second, the `Run FileAssert
self-validation` step executes `dotnet fileassert --validate` and writes
`artifacts/fileassert-self-validation.trx`. Because any assertion failure or
self-validation failure stops the workflow, a passing CI run shows that the
pinned tool version is fit for the repository's intended use as an automated
artifact validator. No project-specific qualification issues are currently
recorded for this pinned version.

### Test Scenarios

**FileAssertVersionDisplay**: FileAssert reports its version during
self-validation, proving the pinned tool is installed correctly and can be
invoked by the build pipeline before it is used to gate generated documents. The
expected outcome is a passing self-validation result in
`artifacts/fileassert-self-validation.trx`. This scenario is tested by
`FileAssert_VersionDisplay`.

**FileAssertHelpDisplay**: FileAssert reports its help text during
self-validation, proving the CLI surface expected by the workflow is available
and operational. The expected outcome is a passing self-validation result in
`artifacts/fileassert-self-validation.trx`. This scenario is tested by
`FileAssert_HelpDisplay`.

### Requirements Coverage

- **ReviewMark-OTS-FileAssert**: FileAssert shall confirm operational
  availability by successfully completing self-validation.
  - *FileAssertVersionDisplay*: verifies FileAssert is installed correctly and
    can execute its version-display self-check.
    - `FileAssert_VersionDisplay`
  - *FileAssertHelpDisplay*: verifies FileAssert exposes the expected CLI help
    surface during self-validation.
    - `FileAssert_HelpDisplay`
