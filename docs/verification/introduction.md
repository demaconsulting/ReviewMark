# Introduction

This document provides the verification design for ReviewMark, a .NET command-line tool
for automated file-review evidence management in regulated environments.

## Purpose

The purpose of this document is to describe how each software requirement for ReviewMark
is verified. For each unit, subsystem, and OTS component it identifies the test class,
test methods, mock or stub dependencies, and the requirement identifiers that each test
satisfies. The document provides a traceable record of verification coverage that
supports formal code review, compliance audit, and ongoing maintenance.

## Scope

This document covers the verification design for the complete ReviewMark system,
including all in-house subsystems and units and all Off-The-Shelf (OTS) components.

In-house software items verified in this document:

- **Program** - entry point and execution orchestrator
- **Cli** subsystem - `Context` unit (command-line argument parser and I/O owner)
- **Configuration** subsystem - `ReviewMarkConfiguration` and `GlobMatcher` units
- **Indexing** subsystem - `ReviewIndex` and `PathHelpers` units
- **SelfTest** subsystem - `Validation` unit

OTS components verified in this document:

- **BuildMark** - build notes generation tool
- **FileAssert** - file content assertion tool
- **Pandoc** - document conversion tool
- **ReqStream** - requirements traceability tool
- **ReviewMark** - code review enforcement tool (self-referential)
- **SarifMark** - SARIF report generation tool
- **SonarMark** - SonarCloud report generation tool
- **VersionMark** - tool version capture tool
- **WeasyPrint** - HTML-to-PDF renderer
- **xUnit** - unit testing framework

The following topics are out of scope:

- External library internals not listed above
- Build pipeline configuration beyond the steps referenced as evidence
- Deployment and packaging

## Software Structure

The following tree shows how the ReviewMark software items are organized across the
system, subsystem, and unit levels:

```text
ReviewMark (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── Configuration (Subsystem)
│   ├── ReviewMarkConfiguration (Unit)
│   └── GlobMatcher (Unit)
├── Indexing (Subsystem)
│   ├── ReviewIndex (Unit)
│   └── PathHelpers (Unit)
└── SelfTest (Subsystem)
    └── Validation (Unit)
```

## Companion Artifact Structure

The list below shows how each artifact type maps to the same software structure,
using per-item path patterns:

- **System** — Req: `docs/reqstream/review-mark.yaml`,
  Design: `docs/design/review-mark.md`,
  Verification: `docs/verification/review-mark.md`,
  Tests: `test/.../IntegrationTests.cs`
- **Program** — Req: `docs/reqstream/review-mark/program.yaml`,
  Design: `docs/design/review-mark/program.md`,
  Verification: `docs/verification/review-mark/program.md`,
  Source: `src/.../Program.cs`, Tests: `test/.../ProgramTests.cs`
- **Cli subsystem** — Req: `docs/reqstream/review-mark/cli/cli.yaml`,
  Design: `docs/design/review-mark/cli.md`,
  Verification: `docs/verification/review-mark/cli.md`,
  Source: `src/.../Cli/`
- **Context** — Req: `docs/reqstream/review-mark/cli/context.yaml`,
  Design: `docs/design/review-mark/cli/context.md`,
  Verification: `docs/verification/review-mark/cli/context.md`,
  Source: `src/.../Cli/Context.cs`, Tests: `test/.../ContextTests.cs`
- **Configuration subsystem** —
  Req: `docs/reqstream/review-mark/configuration/configuration.yaml`,
  Design: `docs/design/review-mark/configuration.md`,
  Verification: `docs/verification/review-mark/configuration.md`,
  Source: `src/.../Configuration/`
- **ReviewMarkConfiguration** —
  Req: `docs/reqstream/review-mark/configuration/review-mark-configuration.yaml`,
  Design: `docs/design/review-mark/configuration/review-mark-configuration.md`,
  Verification: `docs/verification/review-mark/configuration/review-mark-configuration.md`,
  Source: `src/.../Configuration/ReviewMarkConfiguration.cs`,
  Tests: `test/.../ReviewMarkConfigurationTests.cs`
- **GlobMatcher** — Req: `docs/reqstream/review-mark/configuration/glob-matcher.yaml`,
  Design: `docs/design/review-mark/configuration/glob-matcher.md`,
  Verification: `docs/verification/review-mark/configuration/glob-matcher.md`,
  Source: `src/.../Configuration/GlobMatcher.cs`,
  Tests: `test/.../GlobMatcherTests.cs`
- **Indexing subsystem** — Req: `docs/reqstream/review-mark/indexing/indexing.yaml`,
  Design: `docs/design/review-mark/indexing.md`,
  Verification: `docs/verification/review-mark/indexing.md`,
  Source: `src/.../Indexing/`
- **ReviewIndex** — Req: `docs/reqstream/review-mark/indexing/review-index.yaml`,
  Design: `docs/design/review-mark/indexing/review-index.md`,
  Verification: `docs/verification/review-mark/indexing/review-index.md`,
  Source: `src/.../Indexing/ReviewIndex.cs`, Tests: `test/.../IndexingTests.cs`
- **PathHelpers** — Req: `docs/reqstream/review-mark/indexing/path-helpers.yaml`,
  Design: `docs/design/review-mark/indexing/path-helpers.md`,
  Verification: `docs/verification/review-mark/indexing/path-helpers.md`,
  Source: `src/.../Indexing/PathHelpers.cs`, Tests: `test/.../IndexingTests.cs`
- **SelfTest subsystem** — Req: `docs/reqstream/review-mark/self-test/self-test.yaml`,
  Design: `docs/design/review-mark/self-test.md`,
  Verification: `docs/verification/review-mark/self-test.md`,
  Source: `src/.../SelfTest/`
- **Validation** — Req: `docs/reqstream/review-mark/self-test/validation.yaml`,
  Design: `docs/design/review-mark/self-test/validation.md`,
  Verification: `docs/verification/review-mark/self-test/validation.md`,
  Source: `src/.../SelfTest/Validation.cs`, Tests: `test/.../ValidationTests.cs`

OTS components verified in this document have their requirements at:

| OTS Component | Requirements |
| ------------- | ------------ |
| ReviewMark (self-referential) | `docs/reqstream/ots/reviewmark.yaml` |
| BuildMark | `docs/reqstream/ots/buildmark.yaml` |
| FileAssert | `docs/reqstream/ots/fileassert.yaml` |
| Pandoc | `docs/reqstream/ots/pandoc.yaml` |
| ReqStream | `docs/reqstream/ots/reqstream.yaml` |
| SarifMark | `docs/reqstream/ots/sarifmark.yaml` |
| SonarMark | `docs/reqstream/ots/sonarmark.yaml` |
| VersionMark | `docs/reqstream/ots/versionmark.yaml` |
| WeasyPrint | `docs/reqstream/ots/weasyprint.yaml` |
| xUnit | `docs/reqstream/ots/xunit.yaml` |

Each chapter in this verification document corresponds to a unit or subsystem chapter
in the design document. Requirement IDs referenced in the Requirements Coverage sections
match identifiers defined in the ReqStream YAML files under `docs/reqstream/`.

## References

- See the *ReviewMark Software Design* document for implementation details of each unit.
- See the *ReviewMark Requirements* document for the full requirements specification.
- See the ReviewMark repository at <https://github.com/demaconsulting/ReviewMark>.
