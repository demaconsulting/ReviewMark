package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def Context {
        doc /* Command-line argument parser and I/O owner. */

        metadata info : SoftwareInfo {
            description = "Command-line argument parser and I/O owner.";
            sourceFiles = ("src/DemaConsulting.ReviewMark/Cli/Context.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Cli/ContextTests.cs");
            designFiles = ("docs/design/review-mark/cli/context.md");
            verificationFiles = ("docs/verification/review-mark/cli/context.md");
            requirementFiles = ("docs/reqstream/review-mark/cli/context.yaml");
        }
    }
}
