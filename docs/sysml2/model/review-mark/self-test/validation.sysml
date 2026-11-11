package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def Validation {
        doc /* Self-validation test runner. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReviewMark/SelfTest/Validation.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/SelfTest/ValidationTests.cs");
            designFiles = ("docs/design/review-mark/self-test/validation.md");
            verificationFiles = ("docs/verification/review-mark/self-test/validation.md");
            requirementFiles = ("docs/reqstream/review-mark/self-test/validation.yaml");
        }
    }
}
