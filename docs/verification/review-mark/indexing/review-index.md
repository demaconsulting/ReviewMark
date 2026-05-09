### ReviewIndex Verification

This document describes the unit-level verification design for the `ReviewIndex` unit.
It defines the test scenarios, dependency usage, and requirement coverage for
`Indexing/ReviewIndex.cs`.

#### Verification Approach

`ReviewIndex` is verified with unit tests in `IndexTests.cs`. Tests exercise all source
types (none, fileshare, url), JSON round-trip serialization, PDF metadata extraction
(via the `Scan` method), and query operations (`GetEvidence`, `HasId`, `GetAllForId`).

#### Dependencies

| Mock / Stub             | Reason                                                     |
| ----------------------- | ---------------------------------------------------------- |
| Temporary JSON files    | Controlled fileshare evidence without real review PDFs     |
| `FakeHttpMessageHandler`| Returns fixed JSON for URL source tests                    |
| Temporary PDF files     | Real minimal PDF fixtures used for Scan metadata tests     |

#### Test Scenarios

##### ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Load` is called with `EvidenceSource` type `none`.

**Expected**: Returns an empty index with no entries.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`, `ReviewMark-EvidenceSource-None`

##### ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex

**Scenario**: `ReviewIndex.Load` is called with a fileshare source pointing to a valid
index JSON file.

**Expected**: Returns a populated index with all entries from the file.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException

**Scenario**: `ReviewIndex.Load` is called with a fileshare path that does not exist.

**Expected**: `InvalidOperationException` is thrown.

**Boundary / error path**: Missing file handling.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex

**Scenario**: `ReviewIndex.Load` is called with a url source; the fake HTTP client returns
a 200 OK with valid index JSON.

**Expected**: Returns a populated index.

**Requirement coverage**: `ReviewMark-Index-EvidenceSource`

##### ReviewIndex_Empty_ReturnsEmptyIndex

**Scenario**: `ReviewIndex.Empty` is called.

**Expected**: Returns an index with no entries.

**Requirement coverage**: `ReviewMark-Index-Empty`

##### ReviewIndex_Save_RoundTrip_PreservesAllEntries

**Scenario**: A populated index is saved to a stream and reloaded.

**Expected**: All entries are preserved after the round-trip.

**Requirement coverage**: `ReviewMark-Index-Save`

##### ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence

**Scenario**: `GetEvidence` is called with an ID and fingerprint that exist in the index.

**Expected**: Returns the matching evidence record.

**Requirement coverage**: `ReviewMark-Index-GetEvidence`

##### ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull

**Scenario**: `GetEvidence` is called with a known ID but wrong fingerprint.

**Expected**: Returns null.

**Boundary / error path**: Fingerprint mismatch.

**Requirement coverage**: `ReviewMark-Index-GetEvidence`

##### ReviewIndex_HasId_ExistingId_ReturnsTrue

**Scenario**: `HasId` is called with an ID that exists in the index.

**Expected**: Returns true.

**Requirement coverage**: `ReviewMark-Index-HasId`

##### ReviewIndex_HasId_UnknownId_ReturnsFalse

**Scenario**: `HasId` is called with an ID that does not exist.

**Expected**: Returns false.

**Boundary / error path**: Unknown ID lookup.

**Requirement coverage**: `ReviewMark-Index-HasId`

#### Requirements Coverage

- **ReviewMark-Index-EvidenceSource**: ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex,
  ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex,
  ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException,
  ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex
- **ReviewMark-EvidenceSource-None**: ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex
- **ReviewMark-Index-Empty**: ReviewIndex_Empty_ReturnsEmptyIndex
- **ReviewMark-Index-Save**: ReviewIndex_Save_RoundTrip_PreservesAllEntries
- **ReviewMark-Index-GetEvidence**: ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence,
  ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull
- **ReviewMark-Index-HasId**: ReviewIndex_HasId_ExistingId_ReturnsTrue,
  ReviewIndex_HasId_UnknownId_ReturnsFalse
