package ReviewMark_HelpCommand {
    private import SoftwareItemMetadata::*;
    private import OtsDependencies::*;

    part def HelpCommand {
        doc /* Built-in help command: answers the help verb from embedded help cards before option parsing. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReviewMark/Cli/HelpCommand.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Cli/HelpCommandTests.cs");
            designFiles = ("docs/design/review-mark/cli/help-command.md");
            verificationFiles = ("docs/verification/review-mark/cli/help-command.md");
            requirementFiles = ("docs/reqstream/review-mark/cli/help-command.yaml");
        }
    }

    // Serves the help command from the cards embedded in the assembly.
    dependency HelpCommand to OtsDependencies::HelpCards;
}
