package ReviewMark {
    private import SoftwareItemMetadata::*;

    part def GlobMatcher {
        doc /* File pattern matching using glob syntax. */

        metadata info : SoftwareInfo {
            description = "File pattern matching using glob syntax.";
            sourceFiles = ("src/DemaConsulting.ReviewMark/Configuration/GlobMatcher.cs");
            testFiles = ("test/DemaConsulting.ReviewMark.Tests/Configuration/GlobMatcherTests.cs");
            designFiles = ("docs/design/review-mark/configuration/glob-matcher.md");
            verificationFiles = ("docs/verification/review-mark/configuration/glob-matcher.md");
            requirementFiles = ("docs/reqstream/review-mark/configuration/glob-matcher.yaml");
        }
    }
}
