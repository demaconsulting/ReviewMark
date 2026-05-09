## Pandoc

**Component**: Pandoc (<https://pandoc.org/>)
**Role**: Converts Markdown and YAML definition files into PDF documentation via the
Pandoc Markdown-to-LaTeX-to-PDF pipeline.
**Acceptance approach**: Established industry use.

Pandoc is a widely adopted open-source universal document converter with over a decade
of active development, extensive automated testing, and broad production usage. Its
output quality is verified by inspecting generated documents during the build process.

ReviewMark does not embed Pandoc; it is an external build dependency. Correct Pandoc
behaviour is confirmed by the successful PDF generation step in `build.ps1`.

**Requirement coverage**: `ReviewMark-OTS-Pandoc`
