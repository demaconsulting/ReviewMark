### Validation Verification

This document describes the unit-level verification design for the `Validation` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`SelfTest/Validation.cs`.

#### Verification Approach

`Validation` is verified with unit tests in `ValidationTests.cs`. Tests call
`Validation.Validate` with controlled inputs (inline YAML, temporary files, or
constructed objects) and assert on the returned issue list.

#### Dependencies

`Validation` has no dependencies on external processes or network. It accesses the file
system to resolve paths referenced in the YAML definition; tests provide those paths
via temporary files.

#### Test Scenarios

##### Validation_Validate_ValidConfiguration_ReturnsNoIssues

**Scenario**: `Validation.Validate` is called with a fully valid definition.

**Expected**: Returns an empty issue list.

**Requirement coverage**: `ReviewMark-Validation-NoIssues`

##### Validation_Validate_MissingReviewSetFile_ReturnsErrorIssue

**Scenario**: `Validation.Validate` is called with a review-set that references a file
path that does not exist.

**Expected**: Returns at least one error-level issue referencing the missing file.

**Boundary / error path**: Missing referenced file.

**Requirement coverage**: `ReviewMark-Validation-MissingFile`

##### Validation_Validate_DuplicateReviewSetId_ReturnsErrorIssue

**Scenario**: `Validation.Validate` is called with two review-sets sharing the same ID.

**Expected**: Returns at least one error-level issue about the duplicate ID.

**Boundary / error path**: Duplicate ID detection.

**Requirement coverage**: `ReviewMark-Validation-DuplicateId`

##### Validation_Validate_EmptyReviewSetId_ReturnsErrorIssue

**Scenario**: `Validation.Validate` is called with a review-set with an empty ID.

**Expected**: Returns at least one error-level issue.

**Boundary / error path**: Empty ID rejection.

**Requirement coverage**: `ReviewMark-Validation-EmptyId`

#### Requirements Coverage

- **ReviewMark-Validation-NoIssues**: Validation_Validate_ValidConfiguration_ReturnsNoIssues
- **ReviewMark-Validation-MissingFile**: Validation_Validate_MissingReviewSetFile_ReturnsErrorIssue
- **ReviewMark-Validation-DuplicateId**: Validation_Validate_DuplicateReviewSetId_ReturnsErrorIssue
- **ReviewMark-Validation-EmptyId**: Validation_Validate_EmptyReviewSetId_ReturnsErrorIssue
