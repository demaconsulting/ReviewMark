## WeasyPrint

**Component**: WeasyPrint (<https://weasyprint.org/>)
**Role**: Alternative PDF generation backend used via Pandoc for some document outputs.
**Acceptance approach**: Established industry use.

WeasyPrint is a widely adopted open-source HTML/CSS-to-PDF converter used in the build
pipeline as an alternative to LaTeX-based PDF generation. It has an active development
community and its own test suite.

Correct WeasyPrint behaviour is confirmed by the successful PDF generation step in
`build.ps1`. ReviewMark does not embed WeasyPrint; it is an external build dependency.

**Requirement coverage**: `ReviewMark-OTS-WeasyPrint`
