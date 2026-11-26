---
title: Configuration file
summary: Write .reviewmark.yaml, the review sets and their files, and check it.
aliases:
  - config
  - definition
  - reviewmark-yaml
  - review-sets
  - needs-review
  - lint
  - globs
tags:
  - reference
order: 3
see-also:
  - evidence
  - plan-and-report
  - troubleshooting
examples:
  - name: lint
    kind: command
    language: shell
    description: Check .reviewmark.yaml. Prints nothing when it is valid.
    content: '{{tool}} --lint'
  - name: lint-missing
    kind: command
    language: shell
    description: Check a file that does not exist. Exits 1.
    content: '{{tool}} --lint --definition missing.yaml'
    exit: 1
    expect: 'missing.yaml: error:'
---
The tool reads `.reviewmark.yaml` in the folder it runs in (or `--dir`),
or the file that `--definition <file>` names.

A definition with two review sets, the C# sources and the user guide:

```yaml
---
needs-review:
  - "src/**/*.cs"
  - "docs/guide/**/*.md"
  - "!**/obj/**"
  - "!**/bin/**"

evidence-source:
  type: fileshare
  location: index.json

context:
  - "docs/design.md"

reviews:
  - id: Core-Logic
    title: Review that the core logic is correct
    paths:
      - "src/Core/**/*.cs"

  - id: User-Guide
    title: Review that the user guide is complete
    paths:
      - "docs/guide/**/*.md"
```

**Keys.**

| Key | Meaning |
| --- | --- |
| `needs-review` | Patterns of the files that need review. |
| `evidence-source` | Where the review records are indexed; required. |
| `context` | Files that every review reads but does not review. |
| `reviews` | The review sets, each with `id`, `title`, and `paths`. |

A review set may have its own `context` list as well. Unknown keys are
ignored and are not reported. `evidence-source` is described in
[[evidence]].

**Patterns.** Patterns are matched against paths relative to the folder,
with forward slashes. `**` matches any number of folders. They apply in
order: a pattern that starts with `!` removes the files it matches, and a
later pattern can add them back.

**Review sets.** A set covers the files that its `paths` match and that
also need review; with no `needs-review`, every matched file counts. The
fingerprint of a set changes when the content of any of its files
changes. A file that needs review but is in no set is shown as uncovered
in the plan. `context` files are listed by `--elaborate` only; they are
not part of the fingerprint or the coverage.

**Lint.** `--lint` prints no banner, and nothing at all when the file is
valid; see [[#lint]]. Each issue is one line:

```text
<file>: <error|warning>: <message>
```

A YAML syntax error also gives the line and column. A missing
`evidence-source`, `type`, or (except for type `none`) `location`, an
unknown source type, a set without `id`, `title`, or `paths`, and a
duplicate id are errors. A blank list entry is a warning. The run exits
1 only when there are errors, as [[#lint-missing]] shows. The plan,
report, and other operations check the file the same way first, and stop
on errors.
