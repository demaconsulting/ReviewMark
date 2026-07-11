# Introduction

This document provides the detailed design for ReviewMark — a .NET command-line application for
automated file-review evidence management in regulated environments. It covers local software
items (systems, subsystems, and units) and the OTS software items they consume.

## Purpose

The purpose of this document is to define the design for each software item in ReviewMark — full
architectural and detailed design for local items (systems, subsystems, and units), and
integration and usage design for OTS software items. A reviewer should be able to understand how
each item satisfies its requirements without reading source code. The document does not restate
requirements; it explains how they are realized.

## Scope

This document covers the following software items:

Local items:

- **ReviewMark**: system, subsystem, and unit design for all local components.

OTS items:

- **BuildMark**: integration and usage design.
- **DemaConsulting.TestResults**: integration and usage design.
- **FileAssert**: integration and usage design.
- **Microsoft.Extensions.FileSystemGlobbing**: integration and usage design.
- **Pandoc**: integration and usage design.
- **PDFsharp**: integration and usage design.
- **ReqStream**: integration and usage design.
- **ReviewMark**: integration and usage design.
- **SarifMark**: integration and usage design.
- **SonarMark**: integration and usage design.
- **SysML2Tools**: integration and usage design.
- **VersionMark**: integration and usage design.
- **WeasyPrint**: integration and usage design.
- **xUnit**: integration and usage design.
- **YamlDotNet**: integration and usage design.

The following topics are out of scope:

- External library internals
- Build pipeline configuration
- Deployment and packaging
- Test projects

## Software Structure

The software structure is modeled in SysML2 under `docs/sysml2/` and rendered to the
diagram below by SysML2Tools as part of the build pipeline. AI agents should query the
SysML2 model directly (see the `sysml2tools-query` skill) rather than parsing this
diagram or the prose below.

![Software Structure](SoftwareStructureView.svg)

## Folder Layout

- **src/** - source files and projects
  - **DemaConsulting.ReviewMark/** - ReviewMark system source
    - **Cli/** - Cli subsystem
    - **Configuration/** - Configuration subsystem
    - **Indexing/** - Indexing subsystem
    - **SelfTest/** - SelfTest subsystem

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Rendered SysML2 diagrams (SVG) illustrate software structure; text tables are used in preference
  to hand-maintained diagrams elsewhere, since hand-maintained diagrams may not render in all PDF
  viewers and drift out of sync with the model.

## Companion Artifact Structure

Each in-house software item has corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/review-mark.yaml`, `docs/reqstream/review-mark/.../{item}.yaml`
- Design docs: `docs/design/review-mark.md`, `docs/design/review-mark/.../{item}.md`
- Verification: `docs/verification/review-mark.md`, `docs/verification/review-mark/.../{item}.md`
- Source code: `src/DemaConsulting.ReviewMark/.../{Item}.cs`
- Tests: `test/DemaConsulting.ReviewMark.Tests/.../{Item}Tests.cs`

OTS items have integration/usage design docs at `docs/design/ots/{ots-name}.md` describing how
ReviewMark integrates the third-party library or tool; their artifacts sit parallel to system
folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md` _(present for runtime library and tooling integrations
  with local integration surface to describe; some pipeline-only tools have no design page since
  there is no local integration code)_
- Verification: `docs/verification/ots/{ots-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- ReviewMark User Guide — the `README.md` document at the root of the ReviewMark repository.
- ReviewMark Repository — the `demaconsulting/ReviewMark` source repository hosted on GitHub.
- [Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance) — methodology
  framework for automated compliance evidence generation
