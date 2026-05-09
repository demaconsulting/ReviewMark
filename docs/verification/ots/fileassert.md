## FileAssert

**Component**: DemaConsulting.FileAssert
**Role**: Validates that required files are present and well-formed as part of the CI build.
**Acceptance approach**: Automated build pipeline verification.

FileAssert is run as part of `build.ps1`. If FileAssert exits with a non-zero status,
the build fails. The successful completion of CI therefore provides evidence that
FileAssert is operating correctly for the asserted conditions.

No additional unit tests are written to verify FileAssert itself; it is verified by
its own project test suite and by the build pipeline outcome.

**Requirement coverage**: `ReviewMark-OTS-FileAssert`
