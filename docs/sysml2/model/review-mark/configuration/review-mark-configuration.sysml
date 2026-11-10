package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def ReviewMarkConfiguration {
        doc /* YAML configuration parser and review-set processor. */

        metadata info : SoftwareInfo {
            description = "YAML configuration parser and review-set processor.";
            sourceFiles = ("src/DemaConsulting.ReviewMark/Configuration/ReviewMarkConfiguration.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Configuration/ReviewMarkConfigurationTests.cs");
            designFiles = ("docs/design/review-mark/configuration/review-mark-configuration.md");
            verificationFiles = ("docs/verification/review-mark/configuration/review-mark-configuration.md");
            requirementFiles = ("docs/reqstream/review-mark/configuration/review-mark-configuration.yaml");
        }
    }
}
