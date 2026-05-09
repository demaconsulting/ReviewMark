## BuildMark

**Component**: DemaConsulting.BuildMark
**Role**: Provides the `buildmark` CLI tool used in the build pipeline.
**Acceptance approach**: Established industry use and automated build pipeline verification.

BuildMark is maintained by DemaConsulting and is used as a build tool in the CI/CD
pipeline. Its integration is tested implicitly by the CI pipeline succeeding.

No dedicated unit tests are required for BuildMark itself; its correct behaviour is
confirmed by the successful execution of `build.ps1` in CI.

**Requirement coverage**: `ReviewMark-OTS-BuildMark`
