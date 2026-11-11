package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def SelfTestSubsystem {
        doc /* Built-in self-validation test suite. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/SelfTest/SelfTestTests.cs");
            designFiles = ("docs/design/review-mark/self-test.md");
            verificationFiles = ("docs/verification/review-mark/self-test.md");
            requirementFiles = ("docs/reqstream/review-mark/self-test.yaml");
        }

        part validation : Validation;
    }
}
