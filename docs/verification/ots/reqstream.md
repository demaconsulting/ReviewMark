## ReqStream

### Verification Approach

This repository pins DemaConsulting.ReqStream version 1.10.0 in the local tool
manifest and uses that CLI as the final requirements traceability gate in the
`build-docs` job of `build.yaml`. Fitness for use is
verified by both self-validation and live enforcement. The `Run ReqStream
self-validation` step executes `dotnet reqstream --validate` and writes
`artifacts/reqstream-self-validation.trx`. The subsequent `Generate
Requirements Report, Justifications, and Trace Matrix` step runs
`dotnet reqstream --requirements requirements.yaml --tests "artifacts/**/*.trx"
--report ... --justifications ... --matrix ... --enforce`. Because that command
fails the workflow if any requirement lacks passing test evidence, a successful
run shows that the pinned tool version correctly processes the repository's
requirements set and accumulated TRX inputs. No project-specific qualification
issues are currently recorded for this pinned version.

### Test Scenarios

**ReqStreamEnforcementMode**: ReqStream processes `requirements.yaml` together
with the collected `artifacts/**/*.trx` files and enforces that every
requirement has linked passing evidence, proving the repository can rely on the
tool for release-gating traceability. The expected outcome is a passing
self-validation result and a successful `--enforce` run that generates the
requirements report, justifications, and trace matrix. This scenario is tested
by `ReqStream_EnforcementMode`.

### Requirements Coverage

- **ReviewMark-OTS-ReqStream**: ReqStream shall enforce that every requirement
  is linked to passing test evidence.
  - *ReqStreamEnforcementMode*: verifies ReqStream enforces coverage across the
    repository requirements set and the collected TRX evidence.
    - `ReqStream_EnforcementMode`
