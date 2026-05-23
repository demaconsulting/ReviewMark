### ReviewMarkConfiguration Verification

This document describes the unit-level verification design for the `ReviewMarkConfiguration`
unit. It defines the test scenarios, dependency usage, and requirement coverage for
`Configuration/ReviewMarkConfiguration.cs`.

#### Verification Approach

`ReviewMarkConfiguration` is verified with unit tests in `ReviewMarkConfigurationTests.cs`.
Tests parse inline YAML strings or load from temporary files, then assert on the resulting
configuration model properties and generated Markdown output.

#### Dependencies

`ReviewMarkConfiguration` depends on `GlobMatcher` for file resolution, but these
unit tests exercise the full stack with real temporary files rather than mocks, because
the integration is simple and deterministic.

#### Test Environment

N/A - standard test environment. Tests parse inline YAML strings or write temporary YAML
definition files in-process; no external services or network access are required.
Temporary files and directories are created and deleted within each test.

#### Acceptance Criteria

All ReviewMarkConfiguration unit tests pass with zero failures. Every `ReviewMark-Config-*`
requirement is covered by at least one passing test scenario. Null inputs, non-existent
files, invalid YAML, missing evidence-source blocks, and duplicate review-set IDs all
produce the specified exception or error-level load result.

#### Test Scenarios

##### ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException

**Scenario**: `ReviewMarkConfiguration.Parse` is called with null.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null input rejection.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration

**Scenario**: `ReviewMarkConfiguration.Parse` is called with valid YAML.

**Expected**: Returns a non-null configuration object.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues

**Scenario**: `ReviewMarkConfiguration.Load` is called with a valid definition file.

**Expected**: Returns a result with non-null configuration and no issues.

**Requirement coverage**: `ReviewMark-Config-Loading`

##### ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue

**Scenario**: `ReviewMarkConfiguration.Load` is called with a path that does not exist.

**Expected**: Result has null configuration and at least one error-level issue.

**Boundary / error path**: Missing file handling.

**Requirement coverage**: `ReviewMark-Config-LoadingNullOnError`

##### ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue

**Scenario**: `ReviewMarkConfiguration.Load` is called with a YAML missing
`evidence-source`.

**Expected**: Result has null configuration and at least one error-level issue.

**Requirement coverage**: `ReviewMark-Config-Loading`, `ReviewMark-Config-LoadingNullOnError`

##### ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues

**Scenario**: `PublishReviewPlan` is called when all needs-review files are covered.

**Expected**: Returned markdown contains plan content; no issues.

**Requirement coverage**: `ReviewMark-Config-PlanGeneration`

##### ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues

**Scenario**: `PublishReviewReport` is called with a current review in the index.

**Expected**: Report markdown shows "Current" status; no issues.

**Requirement coverage**: `ReviewMark-Config-ReportGeneration`

##### ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration

**Scenario**: `ElaborateReviewSet` is called with a valid review set ID.

**Expected**: Returns markdown containing the ID, fingerprint, and file list.

**Requirement coverage**: `ReviewMark-Config-Elaboration`

##### ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException

**Scenario**: `ElaborateReviewSet` is called with an ID not in the configuration.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Unknown review set ID.

**Requirement coverage**: `ReviewMark-Config-ElaborationUnknownIdRejection`

##### ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing three `needs-review` patterns.

**Expected**: `NeedsReviewPatterns` contains all three patterns in order.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing a `url` evidence source.

**Expected**: `EvidenceSource.Type` is `"url"`, `Location` is set, credentials are null.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing one review.

**Expected**: `Reviews` has one entry with expected `Id`, `Title`, and `Paths`.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing credential environment variable names.

**Expected**: `EvidenceSource.UsernameEnv` and `PasswordEnv` are set correctly.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_GetNeedsReviewFiles_ReturnsMatchingFiles

**Scenario**: `GetNeedsReviewFiles` is called on a configuration with a `.cs` pattern; one `.cs` and one `.txt` file exist.

**Expected**: Only the `.cs` file is returned.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint

**Scenario**: Two directories with identical file content; `GetFingerprint` called on each.

**Expected**: Both fingerprints are equal.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint

**Scenario**: Two directories with different file content; `GetFingerprint` called on each.

**Expected**: The fingerprints differ.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint

**Scenario**: Two directories where one file differs only in name but has identical
content; `GetFingerprint` called on each.

**Expected**: Both fingerprints are equal (content-based, not path-based).

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue

**Scenario**: `ReviewMarkConfiguration.Load` is called with a file containing invalid YAML syntax.

**Expected**: Result has null configuration and one error issue naming the file and line.

**Boundary / error path**: Invalid YAML syntax.

**Requirement coverage**: `ReviewMark-Config-Loading`, `ReviewMark-Config-LoadingNullOnError`

##### ReviewMarkConfiguration_Load_MultipleErrors_ReturnsAllIssues

**Scenario**: `ReviewMarkConfiguration.Load` is called with a file missing
`evidence-source` AND containing duplicate review IDs.

**Expected**: Result has null configuration and both errors are reported (does not stop at first).

**Requirement coverage**: `ReviewMark-Config-Loading`

##### ReviewMarkConfiguration_Load_FileshareRelativeLocation_ResolvesToAbsolutePath

**Scenario**: `ReviewMarkConfiguration.Load` is called with a config having a relative `fileshare` location.

**Expected**: The `EvidenceSource.Location` is resolved to an absolute path under the config file's directory.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Load_NoneEvidenceSource_NoIssues

**Scenario**: `ReviewMarkConfiguration.Load` is called with a config having `evidence-source: type: none`.

**Expected**: No issues; configuration is non-null.

**Requirement coverage**: `ReviewMark-Config-Loading`

##### ReviewMarkLoadResult_ReportIssues_RoutesIssuesToContext

**Scenario**: A `ReviewMarkLoadResult` with one warning and one error calls `ReportIssues` on a context.

**Expected**: Exit code is 1; both messages appear in the log.

**Requirement coverage**: `ReviewMark-Config-Loading`

##### ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError

**Scenario**: `ReviewMarkConfiguration.Load` is called with a config whose review set paths list contains only whitespace.

**Expected**: Null configuration with a lint error referencing `"paths"`.

**Requirement coverage**: `ReviewMark-Config-Loading`

##### ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing `evidence-source: type: none`.

**Expected**: `EvidenceSource.Type` is `"none"` and `Location` is empty.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_Parse_NoneEvidenceSource_NoLocationRequired

**Scenario**: `ReviewMarkConfiguration.Parse` is called with YAML containing a `none` source and no `location` field.

**Expected**: Parsing succeeds without throwing; `EvidenceSource.Type` is `"none"`.

**Requirement coverage**: `ReviewMark-Config-Reading`

##### ReviewMarkConfiguration_PublishReviewPlan_UncoveredFiles_HasIssues

**Scenario**: `PublishReviewPlan` is called when at least one needs-review file is not covered by any review set.

**Expected**: `HasIssues` is true; the uncovered file appears in the Markdown.

**Requirement coverage**: `ReviewMark-Config-PlanGeneration`

##### ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepth_UsedForHeadings

**Scenario**: `PublishReviewPlan` is called with `markdownDepth: 2`.

**Expected**: Main heading is at level 2; subheading at level 3.

**Requirement coverage**: `ReviewMark-Config-PlanMarkdownDepth`

##### ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepthAbove5_Throws

**Scenario**: `PublishReviewPlan` is called with `markdownDepth: 6`.

**Expected**: `ArgumentOutOfRangeException` is thrown.

**Boundary / error path**: Depth exceeds maximum.

**Requirement coverage**: `ReviewMark-Config-PlanMarkdownDepthValidation`

##### ReviewMarkConfiguration_PublishReviewReport_StaleReview_HasIssues

**Scenario**: `PublishReviewReport` is called with an index having an outdated fingerprint.

**Expected**: `HasIssues` is true; Markdown shows "Stale".

**Requirement coverage**: `ReviewMark-Config-ReportGeneration`

##### ReviewMarkConfiguration_PublishReviewReport_FailedReview_HasIssues

**Scenario**: `PublishReviewReport` is called with an index having a matching fingerprint but a failing result.

**Expected**: `HasIssues` is true; Markdown shows "Failed".

**Requirement coverage**: `ReviewMark-Config-ReportGeneration`

##### ReviewMarkConfiguration_PublishReviewReport_MissingReview_HasIssues

**Scenario**: `PublishReviewReport` is called with an empty index.

**Expected**: `HasIssues` is true; Markdown shows "Missing".

**Requirement coverage**: `ReviewMark-Config-ReportGeneration`

##### ReviewMarkConfiguration_PublishReviewReport_MarkdownDepth_UsedForHeadings

**Scenario**: `PublishReviewReport` is called with `markdownDepth: 2`.

**Expected**: Main heading starts with `"## Review Status"`.

**Requirement coverage**: `ReviewMark-Config-ReportMarkdownDepth`

##### ReviewMarkConfiguration_PublishReviewReport_MarkdownDepthAbove5_Throws

**Scenario**: `PublishReviewReport` is called with `markdownDepth: 6`.

**Expected**: `ArgumentOutOfRangeException` is thrown.

**Boundary / error path**: Depth exceeds maximum.

**Requirement coverage**: `ReviewMark-Config-ReportMarkdownDepthValidation`

##### ReviewMarkConfiguration_ElaborateReviewSet_NullId_ThrowsArgumentNullException

**Scenario**: `ElaborateReviewSet` is called with null as the ID.

**Expected**: `ArgumentNullException` is thrown.

**Boundary / error path**: Null ID rejection.

**Requirement coverage**: `ReviewMark-Config-ElaborationNullRejection`

##### ReviewMarkConfiguration_ElaborateReviewSet_WhitespaceId_ThrowsArgumentException

**Scenario**: `ElaborateReviewSet` is called with a whitespace-only string as the ID.

**Expected**: `ArgumentException` is thrown.

**Boundary / error path**: Whitespace/empty ID rejection.

**Requirement coverage**: `ReviewMark-Config-ElaborationNullRejection`

##### ReviewMarkConfiguration_ElaborateReviewSet_ContainsFullFingerprint

**Scenario**: `ElaborateReviewSet` is called with a valid ID and a source file present.

**Expected**: The full 64-character hex fingerprint appears in the Markdown.

**Requirement coverage**: `ReviewMark-Config-Elaboration`

##### ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepth_UsedForHeadings

**Scenario**: `ElaborateReviewSet` is called with `markdownDepth: 2`.

**Expected**: Main heading starts with `"## Core-Logic"`; Files subheading at level 3.

**Requirement coverage**: `ReviewMark-Config-ElaborationMarkdownDepth`

##### ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepthAbove5_Throws

**Scenario**: `ElaborateReviewSet` is called with `markdownDepth: 6`.

**Expected**: `ArgumentOutOfRangeException` is thrown.

**Boundary / error path**: Depth exceeds maximum.

**Requirement coverage**: `ReviewMark-Config-ElaborationMarkdownDepthValidation`

#### Requirements Coverage

- **ReviewMark-Config-Reading**:
  ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException,
  ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration,
  ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly,
  ReviewMarkConfiguration_GetNeedsReviewFiles_ReturnsMatchingFiles,
  ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint,
  ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint,
  ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint,
  ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_NoneEvidenceSource_NoLocationRequired,
  ReviewMarkConfiguration_Load_FileshareRelativeLocation_ResolvesToAbsolutePath
- **ReviewMark-Config-Loading**:
  ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues,
  ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue,
  ReviewMarkConfiguration_Load_MultipleErrors_ReturnsAllIssues,
  ReviewMarkConfiguration_Load_NoneEvidenceSource_NoIssues,
  ReviewMarkLoadResult_ReportIssues_RoutesIssuesToContext,
  ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError
- **ReviewMark-Config-LoadingNullOnError**:
  ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue,
  ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue,
  ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue
- **ReviewMark-Config-PlanGeneration**: ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues, ReviewMarkConfiguration_PublishReviewPlan_UncoveredFiles_HasIssues
- **ReviewMark-Config-PlanMarkdownDepth**: ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepth_UsedForHeadings
- **ReviewMark-Config-PlanMarkdownDepthValidation**: ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepthAbove5_Throws
- **ReviewMark-Config-ReportGeneration**:
  ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues,
  ReviewMarkConfiguration_PublishReviewReport_StaleReview_HasIssues,
  ReviewMarkConfiguration_PublishReviewReport_FailedReview_HasIssues,
  ReviewMarkConfiguration_PublishReviewReport_MissingReview_HasIssues
- **ReviewMark-Config-ReportMarkdownDepth**: ReviewMarkConfiguration_PublishReviewReport_MarkdownDepth_UsedForHeadings
- **ReviewMark-Config-ReportMarkdownDepthValidation**: ReviewMarkConfiguration_PublishReviewReport_MarkdownDepthAbove5_Throws
- **ReviewMark-Config-Elaboration**: ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration, ReviewMarkConfiguration_ElaborateReviewSet_ContainsFullFingerprint
- **ReviewMark-Config-ElaborationNullRejection**:
  ReviewMarkConfiguration_ElaborateReviewSet_NullId_ThrowsArgumentNullException,
  ReviewMarkConfiguration_ElaborateReviewSet_WhitespaceId_ThrowsArgumentException
- **ReviewMark-Config-ElaborationUnknownIdRejection**: ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException
- **ReviewMark-Config-ElaborationMarkdownDepth**: ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepth_UsedForHeadings
- **ReviewMark-Config-ElaborationMarkdownDepthValidation**: ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepthAbove5_Throws
