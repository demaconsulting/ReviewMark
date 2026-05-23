## ReviewMark

### Verification Approach

This repository pins DemaConsulting.ReviewMark version 1.2.0 in the local tool
manifest and also verifies the ReviewMark executable as an OTS build tool
within its own compliance pipeline. Fitness for use is shown by a
combination of self-validation, CI execution, and repository integration tests.
The `Run ReviewMark self-validation` step in the `build-docs` job executes
`dotnet reviewmark --validate` and writes
`artifacts/reviewmark-self-validation.trx`. The same job then runs
`dotnet reviewmark --plan ... --report ...` to produce the generated review plan
and review report consumed by the documentation pipeline. In addition,
`test/DemaConsulting.ReviewMark.Tests/IntegrationTests.cs` verifies plan
creation, report creation, enforcement failure behavior, and self-validation
entry points directly. One known project-specific limitation remains: the
`build-docs` workflow does not yet pass `--enforce` during document generation
until the `reviews` branch is populated with production review evidence, so the
repository relies on self-validation and integration tests for enforcement
qualification.

### Test Scenarios

**ReviewMarkSelfValidation**: ReviewMark runs its built-in validation suite when
invoked with `--validate`, proving the installed tool can execute its
self-checks and report results in a form the pipeline can archive. The expected
outcome is a zero exit code and validation summary output, with CI evidence in
`artifacts/reviewmark-self-validation.trx`. This scenario is tested by
`ReviewMark_ValidateFlag_Invoked_RunsValidation`.

**ReviewMarkPlanGeneration**: ReviewMark reads a definition file and generates a
markdown review plan listing the configured review set, proving it can create
the planning artifact required by this repository's code review documentation.
The expected outcome is a generated plan file containing the configured
review-set identifier. This scenario is tested by
`ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`.

**ReviewMarkReportGeneration**: ReviewMark reads a definition file and generates
a markdown review report summarizing review status, proving it can scan the
configured evidence source and render the reporting artifact consumed by the
documentation pipeline. The expected outcome is a generated report file
containing the configured review-set identifier. This scenario is tested by
`ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`.

**ReviewMarkEnforcementFailure**: ReviewMark returns a non-zero exit code when
`--enforce` is used and no current evidence is available, proving the tool can
turn review gaps into a build-breaking condition. The expected outcome is a
failing command when the evidence source cannot satisfy the configured review
set. This scenario is tested by
`ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`.

### Requirements Coverage

- **ReviewMark-OTS-ReviewMark-Scan**: ReviewMark shall scan file evidence stores
  and produce a report of review status for all governed files.
  - *ReviewMarkReportGeneration*: verifies ReviewMark generates the review
    report that expresses scan results for the configured review set.
    - `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`
- **ReviewMark-OTS-ReviewMark-Enforce**: ReviewMark shall enforce that all
  governed files have current review records, failing the build when any are
  missing or outdated.
  - *ReviewMarkEnforcementFailure*: verifies ReviewMark returns a non-zero exit
    code when `--enforce` detects missing current review evidence.
    - `ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero`
- **ReviewMark-OTS-ReviewMark-Plan**: ReviewMark shall generate a review plan
  document listing all review-sets and the files governed by each.
  - *ReviewMarkPlanGeneration*: verifies ReviewMark generates a markdown review
    plan containing the configured review-set identifier.
    - `ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan`
- **ReviewMark-OTS-ReviewMark-Report**: ReviewMark shall generate a review
  report document summarizing the evidence status for each review-set.
  - *ReviewMarkReportGeneration*: verifies ReviewMark generates a markdown
    review report containing the configured review-set identifier.
    - `ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport`
