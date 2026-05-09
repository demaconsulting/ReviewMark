## SonarMark

**Component**: DemaConsulting.SonarMark
**Role**: Generates markdown reports from SonarCloud/SonarQube analysis results.
**Acceptance approach**: Established industry use and automated build pipeline verification.

SonarMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is tested implicitly by the CI pipeline succeeding.

No dedicated unit tests are required for SonarMark itself; its correct behaviour is
confirmed by the successful execution of `build.ps1` in CI.

**Requirement coverage**: `ReviewMark-OTS-SonarMark`
