package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def IndexingSubsystem {
        doc /* Review evidence loading, PDF scanning, and safe path utilities. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Indexing/IndexingTests.cs");
            designFiles = ("docs/design/review-mark/indexing.md");
            verificationFiles = ("docs/verification/review-mark/indexing.md");
            requirementFiles = ("docs/reqstream/review-mark/indexing.yaml");
        }

        part reviewIndex : ReviewIndex;
        part pathHelpers : PathHelpers;
    }
}
