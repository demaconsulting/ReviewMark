# Introduction

This document defines how the ReviewMark verification collection is organized and how the
repository records verification coverage for the ReviewMark system, its local software items,
and its off-the-shelf software items.

## Purpose

This document provides the verification design context for ReviewMark, a .NET command-line tool
for automated file-review evidence management in regulated environments. It tells reviewers which
software items are covered by this collection, how verification artifacts align with requirements,
design, source, and tests, and where OTS qualification evidence is recorded so coverage can be
confirmed without reverse-engineering the test code.

## Scope

This collection covers verification documentation for the following software items.

Local items:

- **ReviewMark** - system-level verification
- **Program** - unit verification
- **Cli** subsystem and **Context** unit
- **Configuration** subsystem with **ReviewMarkConfiguration** and **GlobMatcher** units
- **Indexing** subsystem with **ReviewIndex** and **PathHelpers** units
- **SelfTest** subsystem with **Validation** unit

OTS items:

- **BuildMark**
- **DemaConsulting.TestResults**
- **FileAssert**
- **Microsoft.Extensions.FileSystemGlobbing**
- **Pandoc**
- **PDFsharp**
- **ReqStream**
- **ReviewMark**
- **SarifMark**
- **SonarMark**
- **VersionMark**
- **WeasyPrint**
- **xUnit**
- **YamlDotNet**

Shared packages:

- N/A - ReviewMark does not consume shared packages from other repositories.

Out of scope:

- Test projects as standalone software items
- Build pipeline implementation details except where CI outputs are cited as verification evidence
- Deployment and packaging details
- Internals of third-party components beyond the integration surfaces used by ReviewMark

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

OTS Software Items (integration and qualification evidence in docs/design/ots/ and
docs/verification/ots/):
├── BuildMark
├── DemaConsulting.TestResults
├── FileAssert
├── Microsoft.Extensions.FileSystemGlobbing
├── Pandoc
├── PDFsharp
├── ReqStream
├── ReviewMark
├── SarifMark
├── SonarMark
├── VersionMark
├── WeasyPrint
├── xUnit
└── YamlDotNet
```

## Companion Artifact Structure

Local items have parallel artifacts in the following repository locations:

- **System** - Req: `docs/reqstream/review-mark.yaml`, Design: `docs/design/review-mark.md`,
  Verification: `docs/verification/review-mark.md`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/IntegrationTests.cs`
- **Program** - Req: `docs/reqstream/review-mark/program.yaml`,
  Design: `docs/design/review-mark/program.md`,
  Verification: `docs/verification/review-mark/program.md`, Source:
  `src/DemaConsulting.ReviewMark/Program.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/ProgramTests.cs`
- **Cli subsystem** - Req: `docs/reqstream/review-mark/cli.yaml`,
  Design: `docs/design/review-mark/cli.md`,
  Verification: `docs/verification/review-mark/cli.md`, Source:
  `src/DemaConsulting.ReviewMark/Cli/`
- **Context** - Req: `docs/reqstream/review-mark/cli/context.yaml`,
  Design: `docs/design/review-mark/cli/context.md`,
  Verification: `docs/verification/review-mark/cli/context.md`, Source:
  `src/DemaConsulting.ReviewMark/Cli/Context.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/ContextTests.cs`
- **Configuration subsystem** - Req: `docs/reqstream/review-mark/configuration.yaml`,
  Design: `docs/design/review-mark/configuration.md`,
  Verification: `docs/verification/review-mark/configuration.md`, Source:
  `src/DemaConsulting.ReviewMark/Configuration/`
- **ReviewMarkConfiguration** -
  Req: `docs/reqstream/review-mark/configuration/review-mark-configuration.yaml`,
  Design: `docs/design/review-mark/configuration/review-mark-configuration.md`,
  Verification: `docs/verification/review-mark/configuration/review-mark-configuration.md`,
  Source: `src/DemaConsulting.ReviewMark/Configuration/ReviewMarkConfiguration.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/Configuration/ReviewMarkConfigurationTests.cs`
- **GlobMatcher** - Req: `docs/reqstream/review-mark/configuration/glob-matcher.yaml`,
  Design: `docs/design/review-mark/configuration/glob-matcher.md`,
  Verification: `docs/verification/review-mark/configuration/glob-matcher.md`, Source:
  `src/DemaConsulting.ReviewMark/Configuration/GlobMatcher.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/Configuration/GlobMatcherTests.cs`
- **Indexing subsystem** - Req: `docs/reqstream/review-mark/indexing.yaml`,
  Design: `docs/design/review-mark/indexing.md`,
  Verification: `docs/verification/review-mark/indexing.md`, Source:
  `src/DemaConsulting.ReviewMark/Indexing/`
- **ReviewIndex** - Req: `docs/reqstream/review-mark/indexing/review-index.yaml`,
  Design: `docs/design/review-mark/indexing/review-index.md`,
  Verification: `docs/verification/review-mark/indexing/review-index.md`, Source:
  `src/DemaConsulting.ReviewMark/Indexing/ReviewIndex.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/IndexingTests.cs`
- **PathHelpers** - Req: `docs/reqstream/review-mark/indexing/path-helpers.yaml`,
  Design: `docs/design/review-mark/indexing/path-helpers.md`,
  Verification: `docs/verification/review-mark/indexing/path-helpers.md`, Source:
  `src/DemaConsulting.ReviewMark/Indexing/PathHelpers.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/IndexingTests.cs`
- **SelfTest subsystem** - Req: `docs/reqstream/review-mark/self-test.yaml`,
  Design: `docs/design/review-mark/self-test.md`,
  Verification: `docs/verification/review-mark/self-test.md`, Source:
  `src/DemaConsulting.ReviewMark/SelfTest/`
- **Validation** - Req: `docs/reqstream/review-mark/self-test/validation.yaml`,
  Design: `docs/design/review-mark/self-test/validation.md`,
  Verification: `docs/verification/review-mark/self-test/validation.md`, Source:
  `src/DemaConsulting.ReviewMark/SelfTest/Validation.cs`, Tests:
  `test/DemaConsulting.ReviewMark.Tests/ValidationTests.cs`

OTS items use repository-level integration and qualification artifacts parallel to the system
folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Shared packages:

- N/A - no shared package verification artifacts are required for this repository.

Review-sets are defined in `.reviewmark.yaml`.

## References

- [Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance)
- [ReviewMark releases](https://github.com/demaconsulting/ReviewMark/releases)
