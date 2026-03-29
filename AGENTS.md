# Agent Quick Reference

Comprehensive guidance for AI agents working on ReviewMark - a tool for automated
file-review evidence management in regulated environments.

## Standards Application (ALL Agents Must Follow)

Before performing any work, agents must read and apply the relevant standards from `.github/standards/`:

- **`csharp-language.md`** - For C# code development (literate programming, XML docs, dependency injection)
- **`csharp-testing.md`** - For C# test development (AAA pattern, naming, MSTest anti-patterns)
- **`reqstream-usage.md`** - For requirements management (traceability, semantic IDs, source filters)
- **`reviewmark-usage.md`** - For file review management (review-sets, file patterns, enforcement)
- **`software-items.md`** - For software categorization (system/subsystem/unit/OTS classification)
- **`technical-documentation.md`** - For documentation creation and maintenance (structure, Pandoc, README best practices)

Load only the standards relevant to your specific task scope and apply their
quality checks and guidelines throughout your work.

## Agent Delegation Guidelines

The default agent should handle simple, straightforward tasks directly.
Delegate to specialized agents only for specific scenarios:

- **Light development work** (small fixes, simple features) → Call @developer agent
- **Light quality checking** (linting, basic validation) → Call @quality agent
- **Formal feature implementation** (complex, multi-step) → Call the `@implementation` agent
- **Formal bug resolution** (complex debugging, systematic fixes) → Call the `@implementation` agent
- **Formal reviews** (compliance verification, detailed analysis) → Call @code-review agent
- **Template consistency** (downstream repository alignment) → Call @repo-consistency agent

## Available Specialized Agents

- **code-review** - Agent for performing formal reviews using standardized
  review processes
- **developer** - General-purpose software development agent that applies
  appropriate standards based on the work being performed
- **implementation** - Orchestrator agent that manages quality implementations
  through a formal state machine workflow
- **quality** - Quality assurance agent that grades developer work against DEMA
  Consulting standards and Continuous Compliance practices
- **repo-consistency** - Ensures downstream repositories remain consistent with
  the TemplateDotNetTool template patterns and best practices

## Quality Gate Enforcement (ALL Agents Must Verify)

Configuration files and scripts are self-documenting with their design intent and
modification policies in header comments.

1. **Linting Standards**: `./lint.sh` (Unix) or `lint.bat` (Windows) - comprehensive linting suite
2. **Build Quality**: Zero warnings (`TreatWarningsAsErrors=true`)
3. **Static Analysis**: SonarQube/CodeQL passing with no blockers
4. **Requirements Traceability**: `dotnet reqstream --enforce` passing
5. **Test Coverage**: All requirements linked to passing tests
6. **Documentation Currency**: All docs current and generated
7. **File Review Status**: All reviewable files have current reviews

## Tech Stack

- C# (latest), .NET 8.0/9.0/10.0, dotnet CLI, NuGet

## Key Files

- **`requirements.yaml`** - All requirements with test linkage (enforced via `dotnet reqstream --enforce`)
- **`.editorconfig`** - Code style (file-scoped namespaces, 4-space indent, UTF-8, LF endings)
- **`.cspell.yaml`, `.markdownlint-cli2.yaml`, `.yamllint.yaml`** - Linting configs

### Spell check word list policy

**Never** add a word to the `.cspell.yaml` word list in order to silence a spell-checking failure.
Doing so defeats the purpose of spell-checking and reduces the quality of the repository.

- If cspell flags a word that is **misspelled**, fix the spelling in the source file.
- If cspell flags a word that is a **genuine technical term** (tool name, project identifier, etc.) and is
  spelled correctly, raise a **proposal** (e.g. comment in a pull request) explaining why the word
  should be added. The proposal must be reviewed and approved before the word is added to the list.

## Requirements

- All requirements MUST be linked to tests (prefer `ReviewMark_*` self-validation tests)
- Not all tests need to be linked to requirements (tests may exist for corner cases, design testing, failure-testing, etc.)
- Enforced in CI: `dotnet reqstream --requirements requirements.yaml --tests "test-results/**/*.trx" --enforce`
- When adding features: add requirement + link to test

## Test Source Filters

Test links in `requirements.yaml` can include a source filter prefix to restrict which test results count as
evidence. This is critical for platform and framework requirements - **do not remove these filters**.

- `windows@TestName` - proves the test passed on a Windows platform
- `ubuntu@TestName` - proves the test passed on a Linux (Ubuntu) platform
- `macos@TestName` - proves the test passed on a macOS platform
- `net8.0@TestName` - proves the test passed under the .NET 8 target framework
- `net9.0@TestName` - proves the test passed under the .NET 9 target framework
- `net10.0@TestName` - proves the test passed under the .NET 10 target framework
- `dotnet8.x@TestName` - proves the self-validation test ran on a machine with .NET 8.x runtime
- `dotnet9.x@TestName` - proves the self-validation test ran on a machine with .NET 9.x runtime
- `dotnet10.x@TestName` - proves the self-validation test ran on a machine with .NET 10.x runtime

Without the source filter, a test result from any platform/framework satisfies the requirement. Adding the filter
ensures the CI evidence comes specifically from the required environment.

## Testing

- **Test Naming**: `ReviewMark_MethodUnderTest_Scenario` for self-validation tests
- **Self-Validation**: All tests run via `--validate` flag and can output TRX/JUnit format
- **Test Framework**: Uses DemaConsulting.TestResults library for test result generation

## Code Style

- **XML Docs**: On ALL members (public/internal/private) with spaces after `///` in summaries
- **Errors**: `ArgumentException` for parsing, `InvalidOperationException` for runtime issues
- **Namespace**: File-scoped namespaces only
- **Using Statements**: Top of file only (no nested using declarations except for IDisposable)
- **String Formatting**: Use interpolated strings ($"") for clarity

## Project Structure

- **Context.cs**: Handles command-line argument parsing, logging, and output
- **Program.cs**: Main entry point with version/help/validation routing
- **Validation.cs**: Self-validation tests with TRX/JUnit output support

## Build and Test

```bash
# Build the project
dotnet build --configuration Release

# Run unit tests
dotnet test --configuration Release

# Run self-validation
dotnet run --project src/DemaConsulting.ReviewMark \
  --configuration Release --framework net10.0 --no-build -- --validate

# Use convenience scripts
./build.sh    # Linux/macOS
build.bat     # Windows
```

## Documentation

- **User Guide**: `docs/guide/guide.md`
- **Requirements**: `requirements.yaml` -> auto-generated docs
- **Build Notes**: Auto-generated via BuildMark
- **Code Quality**: Auto-generated via CodeQL and SonarMark
- **Trace Matrix**: Auto-generated via ReqStream
- **CHANGELOG.md**: Not present - changes are captured in the auto-generated build notes

## Markdown Link Style

- **AI agent markdown files** (`.github/agents/*.md`): Use inline links `[text](url)` so URLs are visible in agent context
- **README.md**: Use absolute URLs (shipped in NuGet package)
- **All other markdown files**: Use reference-style links `[text][ref]` with `[ref]: url` at document end

## CI/CD

- **Quality Checks**: Markdown lint, spell check, YAML lint
- **Build**: Multi-platform (Windows/Linux/macOS)
- **CodeQL**: Security scanning
- **Integration Tests**: .NET 8/9/10 on Windows/Linux/macOS
- **Documentation**: Auto-generated via Pandoc + Weasyprint

## Common Tasks

```bash
# Format code
dotnet format

# Run all linters
./lint.sh     # Linux/macOS
lint.bat      # Windows

# Pack as NuGet tool
dotnet pack --configuration Release
```

## Agent Report Files

Upon completion, create a report file at `.agent-logs/[agent-name]-[subject]-[unique-id].md` that includes:

- A concise summary of the work performed
- Any important decisions made and their rationale
- Follow-up items, open questions, or TODOs

Store agent logs in the `.agent-logs/` folder so they are ignored via `.gitignore` and excluded from linting and commits.
