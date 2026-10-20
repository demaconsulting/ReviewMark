# ReviewMark

## Verification Strategy

ReviewMark is verified primarily through system integration tests in
`test/DemaConsulting.ReviewMark.Tests/IntegrationTests.cs`. These tests launch the built
`DemaConsulting.ReviewMark.dll` through `dotnet`, exercise the public CLI end to end, and assert
on exit codes, generated files, and console output. Supporting unit tests are also used where a
system requirement depends on a specific internal contract, such as authenticated URL evidence
source configuration.

System-boundary test doubles are limited to temporary YAML definition files, temporary directories,
and transient output files. No external services or persistent evidence stores are required. The
same verification suite is also executed in the GitHub Actions build matrix across Windows, Linux,
and macOS and across .NET 8, .NET 9, and .NET 10.

## Test Environment

ReviewMark system verification runs under xUnit with Release builds of
`DemaConsulting.ReviewMark.dll`. Tests require the .NET SDK/runtime, local file-system access, and
write access to temporary directories for definitions, logs, reports, results files, and index
files. No network connectivity or external service dependencies are required for the mapped
scenarios.

Test parallelization is disabled in `test/DemaConsulting.ReviewMark.Tests/AssemblyInfo.cs`
because multiple scenarios launch `dotnet` subprocesses and manipulate transient files in the same
test run.

## Acceptance Criteria

- All mapped automated tests pass with zero failures.
- Every `ReviewMark-System-*` requirement is mapped to at least one named scenario and one passing
  test method.
- Valid CLI invocations return exit code 0, while invalid input and enforcement failures return a
  non-zero exit code.
- Generated plan, report, results, log, and index artifacts are created with the expected content
  when their corresponding options are used.
- Cross-platform verification completes successfully in CI across Windows, Linux, and macOS and
  across .NET 8, .NET 9, and .NET 10.

## System-Level Test Scenarios

**VersionDisplay**: ReviewMark writes a version string when invoked with `--version`, proving a
valid execution context can be created and the CLI can dispatch an immediate success path. This
scenario is tested by `ReviewMark_VersionFlag_Invoked_OutputsVersion`.

**HelpDisplay**: ReviewMark writes usage and option information when invoked with `--help`,
proving the CLI exposes operator guidance without requiring a definition file. This scenario is
tested by `ReviewMark_HelpFlag_Invoked_OutputsUsageInformation`.

**SelfValidationExecution**: ReviewMark runs its built-in self-test suite when invoked with
`--validate`, emits a validation summary, and exits successfully when all checks pass. This
scenario is tested by `ReviewMark_ValidateFlag_Invoked_RunsValidation`.

**ValidationResultsExport**: ReviewMark writes validation results in both TRX and JUnit XML when
`--results` is supplied, proving compatibility with downstream CI and traceability tooling. This
scenario is tested by `ReviewMark_ValidateFlag_WithTrxResultsPath_GeneratesTrxFile` and
`ReviewMark_ValidateFlag_WithXmlResultsPath_GeneratesJUnitFile`.

**SilentAndLoggedOutput**: ReviewMark can suppress console output with `--silent` and duplicate
normal output to a persistent log file with `--log`, preserving execution semantics while changing
output channels. This scenario is tested by `ReviewMark_SilentFlag_Invoked_SuppressesOutput` and
`ReviewMark_LogFlag_Invoked_WritesOutputToFile`.

**InvalidArgumentHandling**: ReviewMark rejects unknown command-line arguments with an error
message and a non-zero exit code, proving invalid invocations do not proceed with undefined
behavior. This scenario is tested by `ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError`.

**ReviewPlanGeneration**: ReviewMark loads a definition file, resolves review-sets, and writes a
review plan containing the expected review-set identifiers. This scenario is tested by
`ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`.

**ReviewReportGeneration**: ReviewMark loads a definition file, evaluates evidence status, and
writes a review report containing the expected review-set identifiers. This scenario is tested by
`ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`.

**EnforcementFailureOnNonCurrentReview**: ReviewMark exits non-zero when `--enforce` is used and
no current evidence exists, proving review compliance can gate automated workflows. This scenario
is tested by `ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`.

**IndexGeneration**: ReviewMark scans configured PDF glob paths and writes `index.json`, even when
no evidence PDFs are found, proving the index-generation workflow completes deterministically. This
scenario is tested by `ReviewMark_IndexFlag_OnEmptyDirectory_CreatesIndexJson` and
`ReviewMark_IndexFlag_WithRepeat_ScansAllPaths`.

**WorkingDirectoryOverride**: ReviewMark resolves its default definition file relative to the
`--dir` argument, allowing scripted execution against repositories outside the current process
working directory. This scenario is tested by `ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory`.

**ReviewSetElaboration**: ReviewMark prints a Markdown elaboration for a named review-set,
including the selected review-set identifier. This scenario is tested by
`ReviewMark_ElaborateFlag_WithValidId_OutputsElaboration`.

**ChangedFileAffectedReviewSets**: ReviewMark reads a changed-file list from a file or standard
input (LF, CRLF or NUL separated), matches it against the review-set globs without file system
access (so deleted files count), and prints escaped tab-delimited `affected` and `unreviewed`
records while exiting 0. When nothing is affected it prints `No affected review-sets`. No banner
is printed and standard error stays empty. Unreadable input, a missing or invalid definition,
conflicting options or an option-like value write an error to standard error and exit 1; the
error is still written to standard error when `--silent` is specified. This scenario is tested by
`ReviewMark_AffectedFlag_Stdin_ListsAffectedReviewSets`,
`ReviewMark_AffectedFlag_NamedFileCrlf_ListsAffectedReviewSets`,
`ReviewMark_AffectedFlag_NulSeparatedInput_ListsAffectedReviewSets`,
`ReviewMark_AffectedFlag_DeletedPath_ListsAffectedReviewSets`,
`ReviewMark_AffectedFlag_WithChangedFile_ListsAffectedReviewSets`,
`ReviewMark_AffectedFlag_UnreviewedPath_ReportsUnreviewedExitZero`,
`ReviewMark_AffectedFlag_ExactOutputFormat_NoBannerNoStderr`,
`ReviewMark_AffectedFlag_NoMatch_ReportsNoAffected`,
`ReviewMark_AffectedFlag_EmptyInput_ReportsNoAffected`,
`ReviewMark_AffectedFlag_EscapedNames_AreEscaped`,
`ReviewMark_AffectedFlag_MissingInputFile_ExitsOne`,
`ReviewMark_AffectedFlag_ProhibitedCombination_ExitsOne`,
`ReviewMark_AffectedFlag_OptionLikeValue_ExitsOne`,
`ReviewMark_AffectedFlag_EmptyOrWhitespaceValue_ExitsOne` (empty and whitespace-only values exit 1
with a `--affected requires` message and empty standard output),
`ReviewMark_AffectedFlag_Silent_InvalidDefinition_ReportsStderr`,
`ReviewMark_AffectedFlag_InvalidDefinition_ReportsStderr` (without `--silent`, standard error contains
a `<file>: error:` lint issue line and standard output is empty), and
`ReviewMark_AffectedFlag_MissingDefinition_ExitsOne`.

**HeadingDepthControl**: ReviewMark applies `--depth`, `--plan-depth`, and `--report-depth` to
Markdown output so plan, report, and validation output can be embedded at the required outline
level. This scenario is tested by `ReviewMark_DepthFlag_Invoked_SetsDefaultHeadingDepth`,
`ReviewMark_PlanDepthFlag_Invoked_OverridesPlanHeadingOnly`,
`ReviewMark_ReportDepthFlag_Invoked_OverridesReportHeadingOnly`, and
`ReviewMark_DepthFlag_WithValidate_SetsValidationHeadingDepth`.

**LintValidation**: ReviewMark validates the definition file in lint mode, stays silent on
success, reports only issue messages on failure, and suppresses the normal version banner in both
cases. This scenario is tested by `ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput` and
`ReviewMark_LintFlag_WithInvalidConfig_ReportsOnlyIssueMessages`.

**AuthenticatedUrlEvidenceSourceConfiguration**: ReviewMark accepts `username-env` and
`password-env` entries in the `evidence-source.credentials` block so authenticated URL evidence
sources can be configured without storing secrets in YAML. This scenario is tested by
`ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly`.

**EvidenceLocationExpansion**: ReviewMark expands `${VAR}` environment variable references in the
`evidence-source.location` of the definition file, so lint succeeds silently when the variable is set
and fails with an error naming the variable when it is unset. This scenario is tested by
`ReviewMark_LintFlag_EvidenceLocationEnvironmentVariable_ExpandsOrReportsError`.

**ExecutionContextManagement**: ReviewMark parses arguments into a stable execution context and
maintains consistent exit-code and output behavior from startup through error handling. This
scenario is tested by `ReviewMark_VersionFlag_Invoked_OutputsVersion` and
`ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError`.

**CrossPlatformRuntimeMatrix**: ReviewMark verification is repeated in the GitHub Actions
`integration-test` job across Windows, Linux, and macOS and across .NET 8, .NET 9, and .NET 10,
proving the packaged tool remains operational across supported platforms and runtimes. This
scenario is tested by `ReviewMark_VersionFlag_Invoked_OutputsVersion` and
`ReviewMark_ValidateFlag_Invoked_RunsValidation` in the CI matrix defined in
`.github/workflows/build.yaml`.

**InvalidLogPathHandling**: ReviewMark returns a non-zero exit code and surfaces an error when
`--log` points to a path that cannot be written, proving output-channel setup failures are handled
deterministically. This scenario is tested by `ReviewMark_LogFlag_WithInvalidPath_ReturnsNonZero`.

## Requirements Coverage

- **ReviewMark-System-ReviewPlan**: *ReviewPlanGeneration* - verified by
  `ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`; *WorkingDirectoryOverride* -
  verified by `ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory`.
- **ReviewMark-System-ReviewReport**: *ReviewReportGeneration* - verified by
  `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`.
- **ReviewMark-System-Enforce**: *EnforcementFailureOnNonCurrentReview* - verified by
  `ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`.
- **ReviewMark-System-Credentials**: *AuthenticatedUrlEvidenceSourceConfiguration* - verified by
  `ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly`.
- **ReviewMark-System-EvidenceLocationExpansion**: *EvidenceLocationExpansion* - verified by
  `ReviewMark_LintFlag_EvidenceLocationEnvironmentVariable_ExpandsOrReportsError`.
- **ReviewMark-System-IndexScan**: *IndexGeneration* - verified by
  `ReviewMark_IndexFlag_OnEmptyDirectory_CreatesIndexJson`.
- **ReviewMark-System-IndexScan-Repeat**: *IndexGeneration* - verified by
  `ReviewMark_IndexFlag_WithRepeat_ScansAllPaths`.
- **ReviewMark-System-Validate**: *SelfValidationExecution* - verified by
  `ReviewMark_ValidateFlag_Invoked_RunsValidation`.
- **ReviewMark-System-Version**: *VersionDisplay* - verified by
  `ReviewMark_VersionFlag_Invoked_OutputsVersion`.
- **ReviewMark-System-Help**: *HelpDisplay* - verified by
  `ReviewMark_HelpFlag_Invoked_OutputsUsageInformation`.
- **ReviewMark-System-WorkingDirectory**: *WorkingDirectoryOverride* - verified by
  `ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory`.
- **ReviewMark-System-Elaborate**: *ReviewSetElaboration* - verified by
  `ReviewMark_ElaborateFlag_WithValidId_OutputsElaboration`.
- **ReviewMark-System-Affected-Input-Source**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_Stdin_ListsAffectedReviewSets` and
  `ReviewMark_AffectedFlag_WithChangedFile_ListsAffectedReviewSets`.
- **ReviewMark-System-Affected-Input-Separators**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_Stdin_ListsAffectedReviewSets`,
  `ReviewMark_AffectedFlag_NamedFileCrlf_ListsAffectedReviewSets`, and
  `ReviewMark_AffectedFlag_NulSeparatedInput_ListsAffectedReviewSets`.
- **ReviewMark-System-Affected-Matching**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_DeletedPath_ListsAffectedReviewSets` and
  `ReviewMark_AffectedFlag_WithChangedFile_ListsAffectedReviewSets`.
- **ReviewMark-System-Affected-Unreviewed**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_UnreviewedPath_ReportsUnreviewedExitZero`.
- **ReviewMark-System-Affected-Unreviewed-ExitZero**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_UnreviewedPath_ReportsUnreviewedExitZero`.
- **ReviewMark-System-Affected-Output-Records**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_ExactOutputFormat_NoBannerNoStderr` and
  `ReviewMark_AffectedFlag_EscapedNames_AreEscaped`.
- **ReviewMark-System-Affected-Output-Sentinel**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_NoMatch_ReportsNoAffected` and
  `ReviewMark_AffectedFlag_EmptyInput_ReportsNoAffected`.
- **ReviewMark-System-Affected-Output-NoBanner**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_ExactOutputFormat_NoBannerNoStderr`.
- **ReviewMark-System-Affected-Errors-Input**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_MissingInputFile_ExitsOne`.
- **ReviewMark-System-Affected-Errors-Definition**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_MissingDefinition_ExitsOne` and
  `ReviewMark_AffectedFlag_InvalidDefinition_ReportsStderr`.
- **ReviewMark-System-Affected-Errors-Conflict**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_ProhibitedCombination_ExitsOne`.
- **ReviewMark-System-Affected-Errors-Value**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_OptionLikeValue_ExitsOne` and
  `ReviewMark_AffectedFlag_EmptyOrWhitespaceValue_ExitsOne`.
- **ReviewMark-System-Affected-Errors-Silent**: *ChangedFileAffectedReviewSets* - verified by
  `ReviewMark_AffectedFlag_Silent_InvalidDefinition_ReportsStderr`.
- **ReviewMark-System-LintValidation**: *LintValidation* - verified by
  `ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput` and
  `ReviewMark_LintFlag_WithInvalidConfig_ReportsOnlyIssueMessages`.
- **ReviewMark-System-LintSilenceOnSuccess**: *LintValidation* - verified by
  `ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput`.
- **ReviewMark-System-Silent**: *SilentAndLoggedOutput* - verified by
  `ReviewMark_SilentFlag_Invoked_SuppressesOutput`.
- **ReviewMark-System-Log**: *SilentAndLoggedOutput* - verified by
  `ReviewMark_LogFlag_Invoked_WritesOutputToFile`; *InvalidLogPathHandling* - verified by
  `ReviewMark_LogFlag_WithInvalidPath_ReturnsNonZero`.
- **ReviewMark-System-Depth**: *HeadingDepthControl* - verified by
  `ReviewMark_DepthFlag_Invoked_SetsDefaultHeadingDepth` and
  `ReviewMark_DepthFlag_WithValidate_SetsValidationHeadingDepth`.
- **ReviewMark-System-PlanDepth**: *HeadingDepthControl* - verified by
  `ReviewMark_PlanDepthFlag_Invoked_OverridesPlanHeadingOnly`.
- **ReviewMark-System-ReportDepth**: *HeadingDepthControl* - verified by
  `ReviewMark_ReportDepthFlag_Invoked_OverridesReportHeadingOnly`.
- **ReviewMark-System-InvalidArgs**: *InvalidArgumentHandling* - verified by
  `ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError`.
- **ReviewMark-System-Results**: *ValidationResultsExport* - verified by
  `ReviewMark_ValidateFlag_WithTrxResultsPath_GeneratesTrxFile` and
  `ReviewMark_ValidateFlag_WithXmlResultsPath_GeneratesJUnitFile`.
- **ReviewMark-System-Definition**: *ReviewPlanGeneration* - verified by
  `ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`; *ReviewReportGeneration* -
  verified by `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`.
- **ReviewMark-System-CrossPlatform**: *CrossPlatformRuntimeMatrix* - verified by
  `ReviewMark_VersionFlag_Invoked_OutputsVersion` and
  `ReviewMark_ValidateFlag_Invoked_RunsValidation` in the CI integration-test matrix.
- **ReviewMark-System-ExecutionContext**: *ExecutionContextManagement* - verified by
  `ReviewMark_VersionFlag_Invoked_OutputsVersion` and
  `ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError`.
