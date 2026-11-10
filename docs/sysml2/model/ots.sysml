package OtsDependencies {
    private import SoftwareItemMetadata::*;

    doc /* Off-the-shelf (OTS) dependencies used by ReviewMark. */

    part def YamlDotNet {
        doc /* OTS: YamlDotNet. */

        metadata info : SoftwareInfo {
            description = "OTS: YamlDotNet.";
            designFiles = ("docs/design/ots/yamldotnet.md");
            verificationFiles = ("docs/verification/ots/yamldotnet.md");
            requirementFiles = ("docs/reqstream/ots/yamldotnet.yaml");
        }
    }

    part def PdfPig {
        doc /* OTS: PdfPig. */

        metadata info : SoftwareInfo {
            description = "OTS: PdfPig.";
            designFiles = ("docs/design/ots/pdfpig.md");
            verificationFiles = ("docs/verification/ots/pdfpig.md");
            requirementFiles = ("docs/reqstream/ots/pdfpig.yaml");
        }
    }

    part def DemaConsultingTestResults {
        doc /* OTS: DemaConsulting.TestResults. */

        metadata info : SoftwareInfo {
            description = "OTS: DemaConsulting.TestResults.";
            designFiles = ("docs/design/ots/dema-consulting-test-results.md");
            verificationFiles = ("docs/verification/ots/dema-consulting-test-results.md");
            requirementFiles = ("docs/reqstream/ots/dema-consulting-test-results.yaml");
        }
    }

    part def FileSystemGlobbing {
        doc /* OTS: Microsoft.Extensions.FileSystemGlobbing. */

        metadata info : SoftwareInfo {
            description = "OTS: Microsoft.Extensions.FileSystemGlobbing.";
            designFiles = ("docs/design/ots/microsoft-extensions-file-system-globbing.md");
            verificationFiles = ("docs/verification/ots/microsoft-extensions-file-system-globbing.md");
            requirementFiles = ("docs/reqstream/ots/microsoft-extensions-file-system-globbing.yaml");
        }
    }
}
