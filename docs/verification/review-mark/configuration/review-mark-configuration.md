### ReviewMarkConfiguration

#### Verification Approach

ReviewMarkConfiguration unit verification uses `ReviewMarkConfigurationTests.cs` with inline YAML strings, temporary configuration files, real `GlobMatcher` file resolution, and `ReviewIndex` fixtures where report generation requires evidence. The tests validate parsing, integrated linting, fingerprinting, Markdown generation, elaboration, and issue reporting through the unit's public surface rather than through mocks.

#### Test Environment

N/A - standard test environment. Tests run under xUnit on .NET 8, 9, and 10, create temporary YAML and source files in-process, and require no external services or network access.

#### Acceptance Criteria

- All ReviewMarkConfiguration unit tests pass with zero failures.
- Each `ReviewMark-Config-*` requirement is traced to at least one scenario and test method.
- Configuration loading returns deterministic diagnostics, fingerprints remain content based, and generated Markdown honors the requested heading depth constraints.

#### Test Scenarios

**ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException**: `ReviewMarkConfiguration.Parse` is called with null. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null input rejection. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException`.

**ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration**: `ReviewMarkConfiguration.Parse` is called with valid YAML. Expected outcome: Returns a non-null configuration object. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration`.

**ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues**: `ReviewMarkConfiguration.Load` is called with a valid definition file. Expected outcome: Returns a result with non-null configuration and no issues. Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues`.

**ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue**: `ReviewMarkConfiguration.Load` is called with a path that does not exist. Expected outcome: Result has null configuration and at least one error-level issue. Boundary or error path: Missing file handling. Requirement coverage: `ReviewMark-Config-LoadingNullOnError`. This scenario is tested by `ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue`.

**ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue**: `ReviewMarkConfiguration.Load` is called with a YAML missing `evidence-source`. Expected outcome: Result has null configuration and at least one error-level issue. Requirement coverage: `ReviewMark-Config-Loading`, `ReviewMark-Config-LoadingNullOnError`. This scenario is tested by `ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue`.

**ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues**: `PublishReviewPlan` is called when all needs-review files are covered. Expected outcome: Returned markdown contains plan content; no issues. Requirement coverage: `ReviewMark-Config-PlanGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues`.

**ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues**: `PublishReviewReport` is called with a current review in the index. Expected outcome: Report markdown shows "Current" status; no issues. Requirement coverage: `ReviewMark-Config-ReportGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues`.

**ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration**: `ElaborateReviewSet` is called with a valid review set ID. Expected outcome: Returns markdown containing the ID, fingerprint, and file list. Requirement coverage: `ReviewMark-Config-Elaboration`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration`.

**ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException**: `ElaborateReviewSet` is called with an ID not in the configuration. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Unknown review set ID. Requirement coverage: `ReviewMark-Config-ElaborationUnknownIdRejection`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException`.

**ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly**: `ReviewMarkConfiguration.Parse` is called with YAML containing three `needs-review` patterns. Expected outcome: `NeedsReviewPatterns` contains all three patterns in order. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly**: `ReviewMarkConfiguration.Parse` is called with YAML containing a `url` evidence source. Expected outcome: `EvidenceSource.Type` is `"url"`, `Location` is set, credentials are null. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly**: `ReviewMarkConfiguration.Parse` is called with YAML containing one review. Expected outcome: `Reviews` has one entry with expected `Id`, `Title`, and `Paths`. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly**: `ReviewMarkConfiguration.Parse` is called with YAML containing credential environment variable names. Expected outcome: `EvidenceSource.UsernameEnv` and `PasswordEnv` are set correctly. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly`.

**ReviewMarkConfiguration_GetNeedsReviewFiles_ReturnsMatchingFiles**: `GetNeedsReviewFiles` is called on a configuration with a `.cs` pattern; one `.cs` and one `.txt` file exist. Expected outcome: Only the `.cs` file is returned. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_GetNeedsReviewFiles_ReturnsMatchingFiles`.

**ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint**: Two directories with identical file content; `GetFingerprint` called on each. Expected outcome: Both fingerprints are equal. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint`.

**ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint**: Two directories with different file content; `GetFingerprint` called on each. Expected outcome: The fingerprints differ. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint`.

**ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint**: Two directories where one file differs only in name but has identical content; `GetFingerprint` called on each. Expected outcome: Both fingerprints are equal (content-based, not path-based). Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint`.

**ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue**: `ReviewMarkConfiguration.Load` is called with a file containing invalid YAML syntax. Expected outcome: Result has null configuration and one error issue naming the file and line. Boundary or error path: Invalid YAML syntax. Requirement coverage: `ReviewMark-Config-Loading`, `ReviewMark-Config-LoadingNullOnError`. This scenario is tested by `ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue`.

**ReviewMarkConfiguration_Load_MultipleErrors_ReturnsAllIssues**: `ReviewMarkConfiguration.Load` is called with a file missing `evidence-source` AND containing duplicate review IDs. Expected outcome: Result has null configuration and both errors are reported (does not stop at first). Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkConfiguration_Load_MultipleErrors_ReturnsAllIssues`.

**ReviewMarkConfiguration_Load_FileshareRelativeLocation_ResolvesToAbsolutePath**: `ReviewMarkConfiguration.Load` is called with a config having a relative `fileshare` location. Expected outcome: The `EvidenceSource.Location` is resolved to an absolute path under the config file's directory. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Load_FileshareRelativeLocation_ResolvesToAbsolutePath`.

**ReviewMarkConfiguration_Load_NoneEvidenceSource_NoIssues**: `ReviewMarkConfiguration.Load` is called with a config having `evidence-source: type: none`. Expected outcome: No issues; configuration is non-null. Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkConfiguration_Load_NoneEvidenceSource_NoIssues`.

**ReviewMarkLoadResult_ReportIssues_RoutesIssuesToContext**: A `ReviewMarkLoadResult` with one warning and one error calls `ReportIssues` on a context. Expected outcome: Exit code is 1; both messages appear in the log. Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkLoadResult_ReportIssues_RoutesIssuesToContext`.

**ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError**: `ReviewMarkConfiguration.Load` is called with a config whose review set paths list contains only whitespace. Expected outcome: Null configuration with a lint error referencing `"paths"`. Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError`.

**ReviewMarkConfiguration_Load_WhitespaceOnlyContextEntries_ReturnsLintWarning**: `ReviewMarkConfiguration.Load` is called with a config whose review set context list contains a whitespace-only entry. Expected outcome: Non-null configuration with a single lint warning referencing `"context"`. Boundary or error path: Whitespace-only context entry. Requirement coverage: `ReviewMark-Config-Loading`. This scenario is tested by `ReviewMarkConfiguration_Load_WhitespaceOnlyContextEntries_ReturnsLintWarning`.

**ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly**: `ReviewMarkConfiguration.Parse` is called with YAML containing `evidence-source: type: none`. Expected outcome: `EvidenceSource.Type` is `"none"` and `Location` is empty. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_NoneEvidenceSource_NoLocationRequired**: `ReviewMarkConfiguration.Parse` is called with YAML containing a `none` source and no `location` field. Expected outcome: Parsing succeeds without throwing; `EvidenceSource.Type` is `"none"`. Requirement coverage: `ReviewMark-Config-Reading`. This scenario is tested by `ReviewMarkConfiguration_Parse_NoneEvidenceSource_NoLocationRequired`.

**ReviewMarkConfiguration_PublishReviewPlan_UncoveredFiles_HasIssues**: `PublishReviewPlan` is called when at least one needs-review file is not covered by any review set. Expected outcome: `HasIssues` is true; the uncovered file appears in the Markdown. Requirement coverage: `ReviewMark-Config-PlanGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewPlan_UncoveredFiles_HasIssues`.

**ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepth_UsedForHeadings**: `PublishReviewPlan` is called with `markdownDepth: 2`. Expected outcome: Main heading is at level 2; subheading at level 3. Requirement coverage: `ReviewMark-Config-PlanMarkdownDepth`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepth_UsedForHeadings`.

**ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepthAbove5_Throws**: `PublishReviewPlan` is called with `markdownDepth: 6`. Expected outcome: `ArgumentOutOfRangeException` is thrown. Boundary or error path: Depth exceeds maximum. Requirement coverage: `ReviewMark-Config-PlanMarkdownDepthValidation`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepthAbove5_Throws`.

**ReviewMarkConfiguration_PublishReviewReport_StaleReview_HasIssues**: `PublishReviewReport` is called with an index having an outdated fingerprint. Expected outcome: `HasIssues` is true; Markdown shows "Stale". Requirement coverage: `ReviewMark-Config-ReportGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_StaleReview_HasIssues`.

**ReviewMarkConfiguration_PublishReviewReport_FailedReview_HasIssues**: `PublishReviewReport` is called with an index having a matching fingerprint but a failing result. Expected outcome: `HasIssues` is true; Markdown shows "Failed". Requirement coverage: `ReviewMark-Config-ReportGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_FailedReview_HasIssues`.

**ReviewMarkConfiguration_PublishReviewReport_MissingReview_HasIssues**: `PublishReviewReport` is called with an empty index. Expected outcome: `HasIssues` is true; Markdown shows "Missing". Requirement coverage: `ReviewMark-Config-ReportGeneration`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_MissingReview_HasIssues`.

**ReviewMarkConfiguration_PublishReviewReport_MarkdownDepth_UsedForHeadings**: `PublishReviewReport` is called with `markdownDepth: 2`. Expected outcome: Main heading starts with `" ## Review Status"`. Requirement coverage: `ReviewMark-Config-ReportMarkdownDepth`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_MarkdownDepth_UsedForHeadings`.

**ReviewMarkConfiguration_PublishReviewReport_MarkdownDepthAbove5_Throws**: `PublishReviewReport` is called with `markdownDepth: 6`. Expected outcome: `ArgumentOutOfRangeException` is thrown. Boundary or error path: Depth exceeds maximum. Requirement coverage: `ReviewMark-Config-ReportMarkdownDepthValidation`. This scenario is tested by `ReviewMarkConfiguration_PublishReviewReport_MarkdownDepthAbove5_Throws`.

**ReviewMarkConfiguration_ElaborateReviewSet_NullId_ThrowsArgumentNullException**: `ElaborateReviewSet` is called with null as the ID. Expected outcome: `ArgumentNullException` is thrown. Boundary or error path: Null ID rejection. Requirement coverage: `ReviewMark-Config-ElaborationNullRejection`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_NullId_ThrowsArgumentNullException`.

**ReviewMarkConfiguration_ElaborateReviewSet_WhitespaceId_ThrowsArgumentException**: `ElaborateReviewSet` is called with a whitespace-only string as the ID. Expected outcome: `ArgumentException` is thrown. Boundary or error path: Whitespace/empty ID rejection. Requirement coverage: `ReviewMark-Config-ElaborationNullRejection`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_WhitespaceId_ThrowsArgumentException`.

**ReviewMarkConfiguration_ElaborateReviewSet_ContainsFullFingerprint**: `ElaborateReviewSet` is called with a valid ID and a source file present. Expected outcome: The full 64-character hex fingerprint appears in the Markdown. Requirement coverage: `ReviewMark-Config-Elaboration`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_ContainsFullFingerprint`.

**ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepth_UsedForHeadings**: `ElaborateReviewSet` is called with `markdownDepth: 2`. Expected outcome: Main heading starts with `" ## Core-Logic"`; Files subheading at level 3. Requirement coverage: `ReviewMark-Config-ElaborationMarkdownDepth`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepth_UsedForHeadings`.

**ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepthAbove5_Throws**: `ElaborateReviewSet` is called with `markdownDepth: 6`. Expected outcome: `ArgumentOutOfRangeException` is thrown. Boundary or error path: Depth exceeds maximum. Requirement coverage: `ReviewMark-Config-ElaborationMarkdownDepthValidation`. This scenario is tested by `ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepthAbove5_Throws`.

**ReviewMarkConfiguration_Parse_GlobalContext_ParsedCorrectly**: `ReviewMarkConfiguration.Parse`
is called with YAML containing a top-level `context:` list. Expected outcome: `GlobalContext`
contains all listed entries in order. Requirement coverage: `ReviewMark-Config-ContextParsing`.
This scenario is tested by `ReviewMarkConfiguration_Parse_GlobalContext_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_ReviewSetContext_ParsedCorrectly**: `ReviewMarkConfiguration.Parse`
is called with YAML containing a per-review-set `context:` list. Expected outcome: the review
set's `Context` contains all listed entries in order. Requirement coverage:
`ReviewMark-Config-ContextParsing`. This scenario is tested by
`ReviewMarkConfiguration_Parse_ReviewSetContext_ParsedCorrectly`.

**ReviewMarkConfiguration_Parse_NoContext_DefaultsToEmpty**: `ReviewMarkConfiguration.Parse` is
called with YAML that omits both the top-level `context:` and the per-review-set `context:`.
Expected outcome: `GlobalContext` is empty and `ReviewSet.Context` is empty. Requirement
coverage: `ReviewMark-Config-ContextParsing`. This scenario is tested by
`ReviewMarkConfiguration_Parse_NoContext_DefaultsToEmpty`.

**ReviewSet_GetFingerprint_ContextNotIncluded**: Two `ReviewSet` instances are created with the
same `paths:` patterns but different `context:` patterns; both are resolved against a directory
containing a single source file. Expected outcome: both fingerprints are equal, confirming that
context patterns do not affect the fingerprint. Boundary or error path: context isolation.
Requirement coverage: `ReviewMark-Config-ContextExcludedFromFingerprint`. This scenario is tested
by `ReviewSet_GetFingerprint_ContextNotIncluded`.

**ReviewMarkConfiguration_ElaborateReviewSet_GlobalContext_AppearsInOutput**:
`ElaborateReviewSet` is called on a configuration with a global `context:` list; the context
files exist on disk. Expected outcome: the output Markdown contains a `Context` subsection and
each context file is listed with the `[global]` label. Requirement coverage:
`ReviewMark-Config-ContextInElaboration`. This scenario is tested by
`ReviewMarkConfiguration_ElaborateReviewSet_GlobalContext_AppearsInOutput`.

**ReviewMarkConfiguration_ElaborateReviewSet_LocalContext_AppearsInOutput**:
`ElaborateReviewSet` is called on a configuration with a per-review-set `context:` list; the
context files exist on disk. Expected outcome: the output Markdown contains a `Context`
subsection and each context file is listed with the `[local]` label. Requirement coverage:
`ReviewMark-Config-ContextInElaboration`. This scenario is tested by
`ReviewMarkConfiguration_ElaborateReviewSet_LocalContext_AppearsInOutput`.

**ReviewMarkConfiguration_ElaborateReviewSet_NoContext_ContextSectionOmitted**:
`ElaborateReviewSet` is called on a configuration with no context lists and no context files on
disk. Expected outcome: the output Markdown does not contain a `Context` subsection heading.
Requirement coverage: `ReviewMark-Config-ContextInElaboration`. This scenario is tested by
`ReviewMarkConfiguration_ElaborateReviewSet_NoContext_ContextSectionOmitted`.

**ReviewMarkConfiguration_ElaborateReviewSet_ContextNotUnderReview**: `ElaborateReviewSet` is
called on a configuration with context files that are not in the `paths:` list. Expected outcome:
the context files do not appear in the `Files` subsection. Requirement coverage:
`ReviewMark-Config-ContextExcludedFromCoverage`. This scenario is tested by
`ReviewMarkConfiguration_ElaborateReviewSet_ContextNotUnderReview`.

**ReviewMarkConfiguration_PublishReviewPlan_ContextOnlyFile_StillReportedAsUncovered**:
`PublishReviewPlan` is called when a file matching `needs-review` appears only in a review set's
`context:` list and is not matched by any `paths:` entry. Expected outcome: `HasIssues` is true
and the context-only file appears in the Markdown as uncovered. Boundary or error path: context
isolation from coverage. Requirement coverage: `ReviewMark-Config-ContextExcludedFromCoverage`.
This scenario is tested by
`ReviewMarkConfiguration_PublishReviewPlan_ContextOnlyFile_StillReportedAsUncovered`.

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
  ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError,
  ReviewMarkConfiguration_Load_WhitespaceOnlyContextEntries_ReturnsLintWarning
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
- **ReviewMark-Config-ContextParsing**:
  ReviewMarkConfiguration_Parse_GlobalContext_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_ReviewSetContext_ParsedCorrectly,
  ReviewMarkConfiguration_Parse_NoContext_DefaultsToEmpty
- **ReviewMark-Config-ContextExcludedFromFingerprint**: ReviewSet_GetFingerprint_ContextNotIncluded
- **ReviewMark-Config-ContextExcludedFromCoverage**: ReviewMarkConfiguration_ElaborateReviewSet_ContextNotUnderReview, ReviewMarkConfiguration_PublishReviewPlan_ContextOnlyFile_StillReportedAsUncovered
- **ReviewMark-Config-ContextInElaboration**:
  ReviewMarkConfiguration_ElaborateReviewSet_GlobalContext_AppearsInOutput,
  ReviewMarkConfiguration_ElaborateReviewSet_LocalContext_AppearsInOutput,
  ReviewMarkConfiguration_ElaborateReviewSet_NoContext_ContextSectionOmitted
