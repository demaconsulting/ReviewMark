package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def ConfigurationSubsystem {
        doc /* YAML configuration loading, validation, and review-set processing. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Configuration/ConfigurationTests.cs");
            designFiles = ("docs/design/review-mark/configuration.md");
            verificationFiles = ("docs/verification/review-mark/configuration.md");
            requirementFiles = ("docs/reqstream/review-mark/configuration.yaml");
        }

        part reviewMarkConfiguration : ReviewMarkConfiguration;
        part globMatcher : GlobMatcher;
    }
}
