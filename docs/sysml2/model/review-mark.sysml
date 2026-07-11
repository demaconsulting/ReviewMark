package ReviewMark {
    import OtsDependencies::*;

    doc
    /*
     * ReviewMark is a .NET command-line tool for automated file-review evidence management
     * in regulated environments.
     */

    part def ReviewMarkSystem {
        doc /* The ReviewMark system as a whole. */

        comment testRef /* Test: test/DemaConsulting.ReviewMark.Tests/IntegrationTests.cs */
        comment designRef /* Design: docs/design/review-mark.md */
        comment verificationRef /* Verification: docs/verification/review-mark.md */
        comment reqRef /* Requirements: docs/reqstream/review-mark.yaml */

        part program : Program;
        part cli : CliSubsystem;
        part configuration : ConfigurationSubsystem;
        part indexing : IndexingSubsystem;
        part selfTest : SelfTestSubsystem;

        part yamlDotNet : YamlDotNet;
        part pdfSharp : PdfSharp;
        part testResults : DemaConsultingTestResults;
        part fileSystemGlobbing : FileSystemGlobbing;
    }
}
