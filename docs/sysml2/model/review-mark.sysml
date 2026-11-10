package ReviewMark {
    private import SoftwareItemMetadata::*;
    import OtsDependencies::*;

    doc
    /*
     * ReviewMark is a .NET command-line tool for automated file-review evidence management
     * in regulated environments.
     */

    part def ReviewMarkSystem {
        doc /* The ReviewMark system as a whole. */

        metadata info : SoftwareInfo {
            description = "The ReviewMark system as a whole.";
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/IntegrationTests.cs");
            designFiles = ("docs/design/review-mark.md");
            verificationFiles = ("docs/verification/review-mark.md");
            requirementFiles = ("docs/reqstream/review-mark.yaml");
        }

        part program : Program;
        part cli : CliSubsystem;
        part configuration : ConfigurationSubsystem;
        part indexing : IndexingSubsystem;
        part selfTest : SelfTestSubsystem;

        part yamlDotNet : YamlDotNet;
        part pdfPig : PdfPig;
        part testResults : DemaConsultingTestResults;
        part fileSystemGlobbing : FileSystemGlobbing;
    }
}
