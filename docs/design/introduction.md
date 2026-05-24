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

OTS Software Items (integration design in docs/design/ots/):
├── YamlDotNet (OTS) — YAML deserialization for the Configuration subsystem
├── PDFsharp (OTS) — PDF metadata reading for the Indexing subsystem
├── DemaConsulting.TestResults (OTS) — TRX/JUnit serialization for the SelfTest subsystem
└── Microsoft.Extensions.FileSystemGlobbing (OTS) — glob-pattern file matching for GlobMatcher
```

Each unit is described in detail in its own companion design document, linked from the folder layout below.

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

The design documentation follows the same hierarchy under `docs/design/review-mark/`:

```text
docs/design/
├── introduction.md                     — this document (software structure and folder layout)
├── review-mark.md                      — system-level design
├── review-mark/
│   ├── program.md                      — Program unit design
│   ├── cli.md                          — Cli subsystem overview
│   ├── cli/
│   │   └── context.md                  — Context unit design
│   ├── configuration.md                — Configuration subsystem overview
│   ├── configuration/
│   │   ├── review-mark-configuration.md — ReviewMarkConfiguration unit design
│   │   └── glob-matcher.md             — GlobMatcher unit design
│   ├── indexing.md                     — Indexing subsystem overview
│   ├── indexing/
│   │   ├── review-index.md             — ReviewIndex unit design
│   │   └── path-helpers.md             — PathHelpers unit design
│   ├── self-test.md                    — SelfTest subsystem overview
│   └── self-test/
│       └── validation.md               — Validation unit design
├── ots.md                              — OTS integration strategy overview
└── ots/
    ├── yamldotnet.md                   — YamlDotNet integration design
    ├── pdfsharp.md                     — PDFsharp integration design
    ├── dema-consulting-test-results.md — DemaConsulting.TestResults integration design
    └── microsoft-extensions-file-system-globbing.md — FileSystemGlobbing integration design
```

## Companion Artifact Structure

Design documents are companion artifacts to requirements, source code, and tests.
The list below shows how each artifact type maps to the same software structure:

- **System** — Req: `docs/reqstream/review-mark.yaml`,
  Design: `docs/design/review-mark.md`,
  Tests: `test/.../IntegrationTests.cs`
- **Program** — Req: `docs/reqstream/review-mark/program.yaml`,
  Design: `docs/design/review-mark/program.md`,
  Source: `src/.../Program.cs`, Tests: `test/.../ProgramTests.cs`
- **Cli subsystem** — Req: `docs/reqstream/review-mark/cli.yaml`,
  Design: `docs/design/review-mark/cli.md`,
  Source: `src/.../Cli/`
- **Context** — Req: `docs/reqstream/review-mark/cli/context.yaml`,
  Design: `docs/design/review-mark/cli/context.md`,
  Source: `src/.../Cli/Context.cs`, Tests: `test/.../ContextTests.cs`
- **Configuration subsystem** —
  Req: `docs/reqstream/review-mark/configuration.yaml`,
  Design: `docs/design/review-mark/configuration.md`,
  Source: `src/.../Configuration/`
- **ReviewMarkConfiguration** —
  Req: `docs/reqstream/review-mark/configuration/review-mark-configuration.yaml`,
  Design: `docs/design/review-mark/configuration/review-mark-configuration.md`,
  Source: `src/.../Configuration/ReviewMarkConfiguration.cs`,
  Tests: `test/.../ReviewMarkConfigurationTests.cs`
- **GlobMatcher** — Req: `docs/reqstream/review-mark/configuration/glob-matcher.yaml`,
  Design: `docs/design/review-mark/configuration/glob-matcher.md`,
  Source: `src/.../Configuration/GlobMatcher.cs`,
  Tests: `test/.../GlobMatcherTests.cs`
- **Indexing subsystem** — Req: `docs/reqstream/review-mark/indexing.yaml`,
  Design: `docs/design/review-mark/indexing.md`,
  Source: `src/.../Indexing/`
- **ReviewIndex** — Req: `docs/reqstream/review-mark/indexing/review-index.yaml`,
  Design: `docs/design/review-mark/indexing/review-index.md`,
  Source: `src/.../Indexing/ReviewIndex.cs`, Tests: `test/.../IndexingTests.cs`
- **PathHelpers** — Req: `docs/reqstream/review-mark/indexing/path-helpers.yaml`,
  Design: `docs/design/review-mark/indexing/path-helpers.md`,
  Source: `src/.../Indexing/PathHelpers.cs`, Tests: `test/.../IndexingTests.cs`
- **SelfTest subsystem** — Req: `docs/reqstream/review-mark/self-test.yaml`,
  Design: `docs/design/review-mark/self-test.md`,
  Source: `src/.../SelfTest/`
- **Validation** — Req: `docs/reqstream/review-mark/self-test/validation.yaml`,
  Design: `docs/design/review-mark/self-test/validation.md`,
  Source: `src/.../SelfTest/Validation.cs`, Tests: `test/.../ValidationTests.cs`
- **YamlDotNet (OTS)** — Design: `docs/design/ots/yamldotnet.md`
- **PDFsharp (OTS)** — Design: `docs/design/ots/pdfsharp.md`
- **DemaConsulting.TestResults (OTS)** — Design: `docs/design/ots/dema-consulting-test-results.md`
- **Microsoft.Extensions.FileSystemGlobbing (OTS)** —
  Design: `docs/design/ots/microsoft-extensions-file-system-globbing.md`

Requirement IDs referenced in the design chapters match identifiers in the ReqStream YAML files under `docs/reqstream/`.

## References

- [Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance) — methodology
  framework for automated compliance evidence generation
