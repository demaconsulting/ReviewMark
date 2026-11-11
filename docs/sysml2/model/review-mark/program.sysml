package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def Program {
        doc /* Process entry point; constructs the execution context and dispatches to subsystems. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReviewMark/Program.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/ProgramTests.cs");
            designFiles = ("docs/design/review-mark/program.md");
            verificationFiles = ("docs/verification/review-mark/program.md");
            requirementFiles = ("docs/reqstream/review-mark/program.yaml");
        }
    }
}
