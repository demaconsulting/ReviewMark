# OTS Dependencies

This section describes the overall integration strategy for the off-the-shelf (OTS) runtime
libraries used by ReviewMark. Each OTS item's specific integration design is documented in
`docs/design/ots/{name}.md`.

## Selection Criteria

OTS items are selected based on the following criteria:

- **License compatibility**: only MIT-licensed packages are acceptable, consistent with
  ReviewMark's own MIT license
- **Maturity and maintenance**: packages must be actively maintained with a track record of
  stable releases and published changelogs
- **NuGet availability**: packages must be published on NuGet.org with reproducible builds
- **API appropriateness**: the package must provide a focused, well-documented API for the
  required functionality without introducing unnecessary transitive dependencies

## Version Management Policy

- Version constraints are declared in the project file (`DemaConsulting.ReviewMark.csproj`)
  and managed using Dependabot for patch and minor upgrades
- Major version upgrades require a review of vendor release notes and, if the integration
  surface changes, an update to the relevant design and verification documents before merging
- No lock files are used; the project file constraints are the authoritative version policy

## General Integration Approach

- OTS items are used directly through their public APIs; no adapter or wrapper classes are
  introduced because the integration surface is isolated to a single unit in each case
- Each OTS item is consumed by exactly one subsystem, keeping the integration surface narrow
  and auditable
- Error handling at OTS boundaries follows standard .NET patterns: library exceptions are
  caught at the subsystem boundary, reported via `Context.WriteError`, and converted to a
  non-zero application exit code

## Qualification Strategy

- Integration tests in `test/DemaConsulting.ReviewMark.Tests/` exercise each OTS integration
  surface and confirm the expected behavior
- The full test suite is executed on every CI run (`dotnet test` in `build.yaml`)
- Vendor release notes are reviewed before any OTS version upgrade; if a consumed feature
  changes, the affected design and verification documents are updated before merging the upgrade
