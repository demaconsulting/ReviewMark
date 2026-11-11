package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def ReviewIndex {
        doc /* Review evidence loader and query engine. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReviewMark/Indexing/ReviewIndex.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Indexing/IndexTests.cs");
            designFiles = ("docs/design/review-mark/indexing/review-index.md");
            verificationFiles = ("docs/verification/review-mark/indexing/review-index.md");
            requirementFiles = ("docs/reqstream/review-mark/indexing/review-index.yaml");
        }
    }
}
