# Agent Quick Reference

Comprehensive guidance for AI agents working on ReviewMark - a tool for automated
file-review evidence management in regulated environments.

## Available Specialized Agents

- **requirements** - Develops requirements and ensures test coverage linkage
- **technical-writer** - Creates accurate documentation following regulatory best practices
- **software-developer** - Writes production code and self-validation tests in literate style
- **test-developer** - Creates unit and integration tests following AAA pattern
- **code-quality** - Enforces linting, static analysis, and security standards
- **code-review** - Assists in performing formal file reviews
- **repo-consistency** - Ensures downstream repositories remain consistent with template patterns

## Agent Selection

- To fix a bug, call the @software-developer agent with the **context** of the bug details and **goal** of resolving
  the issue while maintaining code quality.
- To add a new feature, call the @requirements agent with the **request** to define feature requirements and **context**
  of business needs and **goal** of comprehensive requirement specification.
- To write or fix tests, call the @test-developer agent with the **context** of the functionality to be tested and
  **goal** of achieving comprehensive test coverage.
- To update documentation, call the @technical-writer agent with the **context** of changes requiring documentation and
  **goal** of maintaining current and accurate documentation.
- To manage requirements and traceability, call the @requirements agent with the **context** of requirement changes and
  **goal** of maintaining compliance traceability.
- To resolve quality or linting issues, call the @code-quality agent with the **context** of quality gate failures and
  **goal** of achieving compliance standards.
- To update linting tools or scripts, call the @code-quality agent with the **context** of tool requirements and
  **goal** of maintaining quality infrastructure.
- To address security alerts or scanning issues, call the @code-quality agent with the **context** of security findings
  and **goal** of resolving vulnerabilities.
- To perform file reviews, call the @code-review agent with the **context** of files requiring review and **goal** of
  compliance verification.
- To ensure template consistency, call the @repo-consistency agent with the **context** of downstream repository
  and **goal** of maintaining template alignment.

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

When agents need to write report files to communicate with each other or the user, follow these guidelines:

- **Naming Convention**: Use the pattern `AGENT_REPORT_xxxx.md` (e.g., `AGENT_REPORT_analysis.md`, `AGENT_REPORT_results.md`)
- **Purpose**: These files are for temporary inter-agent communication and should not be committed
- **Exclusions**: Files matching `AGENT_REPORT_*.md` are automatically:
  - Excluded from git (via .gitignore)
  - Excluded from markdown linting
  - Excluded from spell checking
