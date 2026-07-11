package ReviewMark {
    part def CliSubsystem {
        doc /* Command-line argument parsing and output channel ownership. */

        comment testRef /* Test: test/DemaConsulting.ReviewMark.Tests/Cli/CliTests.cs */
        comment designRef /* Design: docs/design/review-mark/cli.md */
        comment verificationRef /* Verification: docs/verification/review-mark/cli.md */
        comment reqRef /* Requirements: docs/reqstream/review-mark/cli.yaml */

        part context : Context;
    }
}
