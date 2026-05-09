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

**Requirement coverage**: `ReviewMark-Config-Elaboration`

#### Requirements Coverage

- **ReviewMark-Config-Reading**: ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException,
  ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration
- **ReviewMark-Config-Loading**: ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues,
  ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue
- **ReviewMark-Config-LoadingNullOnError**: ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue,
  ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue
- **ReviewMark-Config-PlanGeneration**: ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues
- **ReviewMark-Config-ReportGeneration**: ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues
- **ReviewMark-Config-Elaboration**: ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration,
  ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException
