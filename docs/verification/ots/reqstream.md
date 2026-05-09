## ReqStream

**Component**: DemaConsulting.ReqStream
**Role**: Traces requirements from YAML definition files and validates coverage against test evidence.
**Acceptance approach**: Automated build pipeline verification.

ReqStream is invoked in the GitHub Actions CI workflow (`build.yaml`) within the
`build-docs` job. The "Run ReqStream self-validation" step runs `dotnet reqstream
--validate`, producing `artifacts/reqstream-self-validation.trx`. Subsequently, the
"Generate Requirements Report, Justifications, and Trace Matrix" step runs ReqStream
with `--enforce`, which exits non-zero if any requirement lacks test evidence, making
uncovered requirements a build-breaking condition. A successful CI pipeline run therefore
proves both that ReqStream is operational and that all requirements are covered.

### Test scenario coverage

- **`ReqStream_EnforcementMode`** — ReqStream's self-validation confirms enforcement mode
  behaviour: when run with `--enforce`, ReqStream exits non-zero if any requirement lacks
  linked test evidence, making uncovered requirements a build-breaking condition.
  CI Evidence: "Run ReqStream self-validation" step in the `build-docs` job of
  `build.yaml`, writing results to `artifacts/reqstream-self-validation.trx`.

The subsequent `--enforce` run (consuming all previously generated TRX files including
FileAssert, BuildMark, and OTS self-validation results) provides additional runtime
evidence that ReqStream correctly processed `requirements.yaml` and found all requirements
covered by passing tests.

**Requirement coverage**: `ReviewMark-OTS-ReqStream`
