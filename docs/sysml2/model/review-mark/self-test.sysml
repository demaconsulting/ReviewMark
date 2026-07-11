package ReviewMark {
    part def SelfTestSubsystem {
        doc /* Built-in self-validation test suite. */

        comment testRef /* Test: test/DemaConsulting.ReviewMark.Tests/SelfTest/SelfTestTests.cs */
        comment designRef /* Design: docs/design/review-mark/self-test.md */
        comment verificationRef /* Verification: docs/verification/review-mark/self-test.md */
        comment reqRef /* Requirements: docs/reqstream/review-mark/self-test.yaml */

        part validation : Validation;
    }
}
