## ReqStream

**Component**: DemaConsulting.ReqStream
**Role**: Traces requirements from YAML definition files and validates coverage.
**Acceptance approach**: Automated build pipeline verification.

ReqStream is run as part of `build.ps1`. A non-zero exit from ReqStream fails the build.
The successful completion of CI therefore provides evidence that ReqStream is correctly
processing the requirements defined under `docs/reqstream/`.

No additional unit tests are written to verify ReqStream itself; it is verified by
its own project test suite and by the build pipeline outcome.

**Requirement coverage**: `ReviewMark-OTS-ReqStream`
