package ReviewMark {
    part def ConfigurationSubsystem {
        doc /* YAML configuration loading, validation, and review-set processing. */

        comment testRef /* Test: test/DemaConsulting.ReviewMark.Tests/Configuration/ConfigurationTests.cs */
        comment designRef /* Design: docs/design/review-mark/configuration.md */
        comment verificationRef /* Verification: docs/verification/review-mark/configuration.md */
        comment reqRef /* Requirements: docs/reqstream/review-mark/configuration.yaml */

        part reviewMarkConfiguration : ReviewMarkConfiguration;
        part globMatcher : GlobMatcher;
    }
}
