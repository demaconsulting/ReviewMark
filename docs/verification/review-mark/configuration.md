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

#### Configuration_NeedsReview_ValidConfig_ResolvesFiles

**Scenario**: A configuration with `needs-review: ["src/**/*.cs"]` is loaded; two `.cs`
files exist in `src/`.

**Expected**: `GetNeedsReviewFiles` returns exactly two files.

**Requirement coverage**: `ReviewMark-Configuration-NeedsReview`

#### Configuration_Fingerprinting_ContentModified_FingerprintDiffers

**Scenario**: A configuration is loaded before and after modifying a source file.

**Expected**: The fingerprints differ after the content change.

**Requirement coverage**: `ReviewMark-Configuration-Fingerprinting`

#### Configuration_PlanGeneration_ValidConfig_Succeeds

**Scenario**: A valid configuration is loaded and `PublishReviewPlan` is called.

**Expected**: The returned markdown contains the review set ID.

**Requirement coverage**: `ReviewMark-Configuration-PlanGeneration`

#### Configuration_ReportGeneration_ValidConfig_Succeeds

**Scenario**: A valid configuration is loaded and `PublishReviewReport` is called.

**Expected**: The returned markdown contains the review set ID.

**Requirement coverage**: `ReviewMark-Configuration-ReportGeneration`

#### Configuration_Elaboration_ValidId_Succeeds

**Scenario**: A valid configuration file is loaded and `ElaborateReviewSet` is called with a known review-set ID.

**Expected**: The returned elaboration markdown contains the review-set ID, fingerprint, and file paths.

**Requirement coverage**: `ReviewMark-Configuration-Elaboration`

#### Configuration_LoadConfig_ElaborateUnknownId_ThrowsArgumentException

**Scenario**: A valid configuration file is loaded and `ElaborateReviewSet` is called with an ID that does not exist.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Unknown review-set ID validation at subsystem level.

**Requirement coverage**: `ReviewMark-Configuration-ElaborateUnknownId`

#### Configuration_LoadConfig_MalformedYaml_ReturnsIssues

**Scenario**: A configuration file with invalid/malformed YAML is loaded.

**Expected**: The configuration is null and the issues list is non-empty.

**Boundary / error path**: Malformed YAML input.

**Requirement coverage**: `ReviewMark-Configuration-MalformedYaml`

#### Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged

**Scenario**: A review-set fingerprint is computed before and after renaming one of its source files.

**Expected**: The fingerprint is identical before and after renaming (content-based, not path-based).

**Requirement coverage**: `ReviewMark-Configuration-Fingerprinting`

### Requirements Coverage

- **ReviewMark-Configuration-NeedsReview**: Configuration_NeedsReview_ValidConfig_ResolvesFiles
- **ReviewMark-Configuration-Fingerprinting**: Configuration_Fingerprinting_ContentModified_FingerprintDiffers, Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged
- **ReviewMark-Configuration-PlanGeneration**: Configuration_PlanGeneration_ValidConfig_Succeeds
- **ReviewMark-Configuration-ReportGeneration**: Configuration_ReportGeneration_ValidConfig_Succeeds
- **ReviewMark-Configuration-Elaboration**: Configuration_Elaboration_ValidId_Succeeds
- **ReviewMark-Configuration-ElaborateUnknownId**: Configuration_LoadConfig_ElaborateUnknownId_ThrowsArgumentException
- **ReviewMark-Configuration-MalformedYaml**: Configuration_LoadConfig_MalformedYaml_ReturnsIssues
