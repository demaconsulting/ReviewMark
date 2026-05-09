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

Verification design documents are companion artifacts to requirements, design, source
code, and tests. The parallel tree below shows how each artifact type maps to the same
software structure:

```text
docs/requirements_doc/   - compiled requirements document (generated)
docs/reqstream/          - requirements source YAML files
docs/design/             - software design document source
docs/verification/       - this document (verification design source)
src/DemaConsulting.ReviewMark/   - implementation source
test/DemaConsulting.ReviewMark.Tests/   - test source
```

Each chapter in this verification document corresponds to a unit or subsystem chapter
in the design document. Requirement IDs referenced in the Requirements Coverage sections
match identifiers defined in the ReqStream YAML files under `docs/reqstream/`.

## References

- See the *ReviewMark Software Design* document for implementation details of each unit.
- See the *ReviewMark Requirements* document for the full requirements specification.
- See the ReviewMark repository at <https://github.com/demaconsulting/ReviewMark>.
