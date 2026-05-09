## SarifMark

**Component**: DemaConsulting.SarifMark
**Role**: Generates markdown reports from SARIF static analysis output files.
**Acceptance approach**: Established industry use and automated build pipeline verification.

SarifMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is tested implicitly by the CI pipeline succeeding.

No dedicated unit tests are required for SarifMark itself; its correct behaviour is
confirmed by the successful execution of `build.ps1` in CI.

**Requirement coverage**: `ReviewMark-OTS-SarifMark`
