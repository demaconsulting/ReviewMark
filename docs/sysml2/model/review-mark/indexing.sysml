package ReviewMark {
    part def IndexingSubsystem {
        doc /* Review evidence loading, PDF scanning, and safe path utilities. */

        comment testRef /* Test: test/DemaConsulting.ReviewMark.Tests/Indexing/IndexingTests.cs */
        comment designRef /* Design: docs/design/review-mark/indexing.md */
        comment verificationRef /* Verification: docs/verification/review-mark/indexing.md */
        comment reqRef /* Requirements: docs/reqstream/review-mark/indexing.yaml */

        part reviewIndex : ReviewIndex;
        part pathHelpers : PathHelpers;
    }
}
