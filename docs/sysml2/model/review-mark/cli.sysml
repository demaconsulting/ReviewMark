package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing and output channel ownership. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/review-mark/cli.md");
            verificationFiles = ("docs/verification/review-mark/cli.md");
            requirementFiles = ("docs/reqstream/review-mark/cli.yaml");
        }

        part context : Context;
    }
}
