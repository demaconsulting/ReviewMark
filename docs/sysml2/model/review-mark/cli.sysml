package ReviewMark_CliSubsystem {
    private import SoftwareItemMetadata::*;
    public import ReviewMark_Context::*;
    public import ReviewMark_HelpCommand::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing, output channel ownership, and the built-in help command. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/review-mark/cli.md");
            verificationFiles = ("docs/verification/review-mark/cli.md");
            requirementFiles = ("docs/reqstream/review-mark/cli.yaml");
        }

        part context : Context;
        part helpCommand : HelpCommand;
    }
}
