## Configuration

### Verification Approach

The Configuration subsystem is verified through `ConfigurationTests.cs`, which exercises
`ReviewMarkConfiguration` and `GlobMatcher` working together with actual temporary file
systems. Each test creates a fresh temporary directory with controlled definition files
and source files, loads the configuration, and asserts on the resulting state.

The constructor initializes the temporary directory; `Dispose` deletes it, ensuring each
test operates in a clean environment.

### Dependencies

| Mock / Stub         | Reason                                                        |
| ------------------- | ------------------------------------------------------------- |
| Temporary directory | Isolated filesystem prevents test interference                |
| Temporary YAML file | Controlled definition file with known configuration content   |

### Test Scenarios

#### Configuration_LoadConfig_ResolvesNeedsReviewFiles

**Scenario**: A configuration with `needs-review: ["src/**/*.cs"]` is loaded; two `.cs`
files exist in `src/`.

**Expected**: `GetNeedsReviewFiles` returns exactly two files.

**Requirement coverage**: `ReviewMark-Configuration-NeedsReview`

#### Configuration_LoadConfig_FingerprintReflectsFileContent

**Scenario**: A configuration is loaded before and after modifying a source file.

**Expected**: The fingerprints differ after the content change.

**Requirement coverage**: `ReviewMark-Configuration-Fingerprinting`

#### Configuration_LoadConfig_PlanGenerationSucceeds

**Scenario**: A valid configuration is loaded and `PublishReviewPlan` is called.

**Expected**: The returned markdown contains the review set ID.

**Requirement coverage**: `ReviewMark-Configuration-PlanGeneration`

#### Configuration_LoadConfig_ReportGenerationSucceeds

**Scenario**: A valid configuration is loaded and `PublishReviewReport` is called.

**Expected**: The returned markdown contains the review set ID.

**Requirement coverage**: `ReviewMark-Configuration-ReportGeneration`

### Requirements Coverage

- **ReviewMark-Configuration-NeedsReview**: Configuration_LoadConfig_ResolvesNeedsReviewFiles
- **ReviewMark-Configuration-Fingerprinting**: Configuration_LoadConfig_FingerprintReflectsFileContent
- **ReviewMark-Configuration-PlanGeneration**: Configuration_LoadConfig_PlanGenerationSucceeds
- **ReviewMark-Configuration-ReportGeneration**: Configuration_LoadConfig_ReportGenerationSucceeds
