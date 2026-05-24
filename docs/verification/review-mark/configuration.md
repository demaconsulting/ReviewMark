## Configuration

### Verification Approach

Configuration subsystem verification uses `ConfigurationTests.cs` to exercise `ReviewMarkConfiguration` and `GlobMatcher` together against temporary directories, real YAML definition files, and real file sets. These tests verify file discovery, fingerprinting, plan and report generation, elaboration, and malformed-YAML handling across the subsystem boundary; `ReviewIndex` is used only where report generation needs realistic evidence input.

### Test Environment

Tests run under xUnit on .NET 8, 9, and 10 across Windows, Linux, and macOS. Each test creates a fresh temporary directory, writes controlled `.reviewmark.yaml` and source files, and removes the directory during cleanup. No external services or network access are required.

### Acceptance Criteria

- All Configuration subsystem integration tests pass with zero failures.
- Each `ReviewMark-Configuration-*` requirement is traced to at least one scenario and test method.
- Valid configurations generate deterministic fingerprints and Markdown outputs, while malformed inputs produce the documented diagnostics.

### Test Scenarios

**Configuration_NeedsReview_ValidConfig_ResolvesFiles**: A configuration with `needs-review: ["src/**/*.cs"]` is loaded; two `.cs` files exist in `src/`. Expected outcome: `GetNeedsReviewFiles` returns exactly two files. Requirement coverage: `ReviewMark-Configuration-NeedsReview`. This scenario is tested by `Configuration_NeedsReview_ValidConfig_ResolvesFiles`.

**Configuration_Fingerprinting_ContentModified_FingerprintDiffers**: A configuration is loaded before and after modifying a source file. Expected outcome: The fingerprints differ after the content change. Requirement coverage: `ReviewMark-Configuration-Fingerprinting`. This scenario is tested by `Configuration_Fingerprinting_ContentModified_FingerprintDiffers`.

**Configuration_PlanGeneration_ValidConfig_Succeeds**: A valid configuration is loaded and `PublishReviewPlan` is called. Expected outcome: The returned markdown contains the review set ID. Requirement coverage: `ReviewMark-Configuration-PlanGeneration`. This scenario is tested by `Configuration_PlanGeneration_ValidConfig_Succeeds`.

**Configuration_ReportGeneration_ValidConfig_Succeeds**: A valid configuration is loaded and `PublishReviewReport` is called. Expected outcome: The returned markdown contains the review set ID. Requirement coverage: `ReviewMark-Configuration-ReportGeneration`. This scenario is tested by `Configuration_ReportGeneration_ValidConfig_Succeeds`.

**Configuration_Elaboration_ValidId_Succeeds**: A valid configuration file is loaded and `ElaborateReviewSet` is called with a known review-set ID. Expected outcome: The returned elaboration markdown contains the review-set ID, fingerprint, and file paths. Requirement coverage: `ReviewMark-Configuration-Elaboration`. This scenario is tested by `Configuration_Elaboration_ValidId_Succeeds`.

**Configuration_ElaborateReviewSet_UnknownId_ThrowsArgumentException**: A valid configuration file is loaded and `ElaborateReviewSet` is called with an ID that does not exist. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Unknown review-set ID validation at subsystem level. Requirement coverage: `ReviewMark-Configuration-ElaborateUnknownId`. This scenario is tested by `Configuration_ElaborateReviewSet_UnknownId_ThrowsArgumentException`.

**Configuration_LoadConfig_MalformedYaml_ReturnsIssues**: A configuration file with invalid/malformed YAML is loaded. Expected outcome: The configuration is null and the issues list is non-empty. Boundary or error path: Malformed YAML input. Requirement coverage: `ReviewMark-Configuration-MalformedYaml`. This scenario is tested by `Configuration_LoadConfig_MalformedYaml_ReturnsIssues`.

**Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged**: A review-set fingerprint is computed before and after renaming one of its source files. Expected outcome: The fingerprint is identical before and after renaming (content-based, not path-based). Requirement coverage: `ReviewMark-Configuration-Fingerprinting`. This scenario is tested by `Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged`.

### Requirements Coverage

- **ReviewMark-Configuration-NeedsReview**: Configuration_NeedsReview_ValidConfig_ResolvesFiles
- **ReviewMark-Configuration-Fingerprinting**: Configuration_Fingerprinting_ContentModified_FingerprintDiffers, Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged
- **ReviewMark-Configuration-PlanGeneration**: Configuration_PlanGeneration_ValidConfig_Succeeds
- **ReviewMark-Configuration-ReportGeneration**: Configuration_ReportGeneration_ValidConfig_Succeeds
- **ReviewMark-Configuration-Elaboration**: Configuration_Elaboration_ValidId_Succeeds
- **ReviewMark-Configuration-ElaborateUnknownId**: Configuration_ElaborateReviewSet_UnknownId_ThrowsArgumentException
- **ReviewMark-Configuration-MalformedYaml**: Configuration_LoadConfig_MalformedYaml_ReturnsIssues
