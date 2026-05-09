## VersionMark

**Component**: DemaConsulting.VersionMark
**Role**: Manages version stamping in build pipelines.
**Acceptance approach**: Established industry use and automated build pipeline verification.

VersionMark is maintained by DemaConsulting and is used to stamp the version string
into the build during CI. Its integration is tested implicitly by the CI pipeline
producing correctly versioned artefacts.

No dedicated unit tests are required for VersionMark itself; its correct behaviour is
confirmed by the successful execution of `build.ps1` in CI.

**Requirement coverage**: `ReviewMark-OTS-VersionMark`
