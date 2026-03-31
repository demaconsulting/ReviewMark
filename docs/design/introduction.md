# Introduction

This document provides the detailed design for the ReviewMark tool, a .NET command-line
application for automated file-review evidence management in regulated environments.

## Purpose

The purpose of this document is to describe the internal design of each software unit that
comprises ReviewMark. It captures data models, algorithms, key methods, and inter-unit
interactions at a level of detail sufficient for formal code review, compliance verification,
and future maintenance. The document does not restate requirements; it explains how they are
realized.

## Scope

This document covers the detailed design of the following software units:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Context** — command-line argument parser and I/O owner (`Cli/Context.cs`)
- **ReviewMarkConfiguration** — YAML configuration parser and review-set processor (`Configuration/ReviewMarkConfiguration.cs`)
- **GlobMatcher** — file pattern matching using glob syntax (`Configuration/GlobMatcher.cs`)
- **ReviewIndex** — review evidence loader and query engine (`Indexing/ReviewIndex.cs`)
- **PathHelpers** — file path utilities (`Indexing/PathHelpers.cs`)
- **Validation** — self-validation test runner (`SelfTest/Validation.cs`)

The following topics are out of scope:

- External library internals (YamlDotNet, PDFsharp, DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging

## Software Structure

The following tree shows how the ReviewMark software items are organized across the system,
subsystem, and unit levels:

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

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.ReviewMark/
├── Program.cs                          — entry point and execution orchestrator
├── Cli/
│   └── Context.cs                      — command-line argument parser and I/O owner
├── Configuration/
│   ├── ReviewMarkConfiguration.cs      — YAML configuration parser and review-set processor
│   └── GlobMatcher.cs                  — file pattern matching using glob syntax
├── Indexing/
│   ├── ReviewIndex.cs                  — review evidence loader and query engine
│   └── PathHelpers.cs                  — file path utilities
└── SelfTest/
    └── Validation.cs                   — self-validation test runner
```

The test project mirrors the same layout under `test/DemaConsulting.ReviewMark.Tests/`.

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## References

- [ReviewMark Architecture][arch]
- [ReviewMark User Guide][guide]
- [ReviewMark Repository][repo]

[arch]: ../../THEORY-OF-OPERATIONS.md
[guide]: ../../README.md
[repo]: https://github.com/demaconsulting/ReviewMark
