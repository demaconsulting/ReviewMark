## ReviewMark

### Verification Approach

**Component**: DemaConsulting.ReviewMark (this tool)
**Role**: Scans file evidence stores, generates review plan and review report documents,
and enforces that all governed files have current review records.
**Acceptance approach**: Self-test and automated unit/integration test coverage.

ReviewMark verifies itself through the `--validate` command (self-test). This executes
the tool's own built-in self-test suite and confirms the tool is correctly installed and
operating. The self-test is run via the "Run ReviewMark self-validation" step in the
`build-docs` job of the GitHub Actions workflow (`build.yaml`), producing
`artifacts/reviewmark-self-validation.trx`.

Unit and integration tests in `test/` provide additional coverage of the individual
subsystems (Cli, Configuration, Indexing, SelfTest, Program) and the four OTS-level
capabilities described below.

### Test scenario coverage

- **`ReviewMark_ValidateFlag_Invoked_RunsValidation`** — ReviewMark runs its built-in
  self-test suite via `--validate`, exits successfully, and outputs a validation summary
  — confirming the tool is correctly installed and operational.
- **`ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`** — ReviewMark exits with a
  non-zero code when `--enforce` is supplied and the evidence source contains no matching
  review records, proving enforcement behavior is operative.
  Requirement: `ReviewMark-OTS-ReviewMark-Enforce`
- **`ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`** — ReviewMark generates
  a markdown review plan file from a definition file, and the plan contains the configured
  review-set identifier. Requirement: `ReviewMark-OTS-ReviewMark-Plan`
- **`ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`** — ReviewMark
  generates a markdown review report file from a definition file, and the report contains
  the configured review-set identifier.
  Requirements: `ReviewMark-OTS-ReviewMark-Scan`, `ReviewMark-OTS-ReviewMark-Report`

### Requirements Coverage

- **ReviewMark-OTS-ReviewMark-Scan**: ReviewMark shall scan file evidence stores and
  produce a report of review status for all governed files.
  - *ReviewMarkReportFlagWithDefinitionFileGeneratesReviewReport*: verifies ReviewMark
    generates a markdown review report from a definition file, confirming scan and
    report-generation capability.
    - `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`
- **ReviewMark-OTS-ReviewMark-Enforce**: ReviewMark shall enforce that all governed files
  have current review records, failing the build when any are missing or outdated.
  - *ReviewMarkEnforceFlagWithNoEvidenceReturnsNonZero*: verifies ReviewMark exits non-zero
    when `--enforce` is supplied and the evidence source contains no matching review records.
    - `ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`
- **ReviewMark-OTS-ReviewMark-Plan**: ReviewMark shall generate a review plan document
  listing all review-sets and the files governed by each.
  - *ReviewMarkPlanFlagWithDefinitionFileGeneratesReviewPlan*: verifies ReviewMark generates
    a markdown review plan containing the configured review-set identifier.
    - `ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`
- **ReviewMark-OTS-ReviewMark-Report**: ReviewMark shall generate a review report document
  summarizing the evidence status for each review-set.
  - *ReviewMarkReportFlagWithDefinitionFileGeneratesReviewReport*: verifies ReviewMark
    generates a markdown review report containing the configured review-set identifier.
    - `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`
