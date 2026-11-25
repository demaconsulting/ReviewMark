---
title: Enforce reviews in CI
summary: Fail a build when a file is uncovered or a review is not current.
aliases:
  - enforce
  - ci
  - pipeline
tags:
  - tutorial
order: 8
see-also:
  - plan-and-report
  - evidence
  - getting-started
examples:
  - name: warn
    kind: command
    language: shell
    description: Without --enforce, issues are warnings. Exits 0.
    content: '{{tool}} --report report.md'
    expect: 'Warning: Review report has review issues.'
  - name: enforce
    kind: command
    language: shell
    description: With --enforce, issues are errors. Exits 1.
    content: '{{tool}} --plan plan.md --report report.md --enforce'
    exit: 1
    expect: 'Review report has review issues.'
---
Without `--enforce`, the plan and report are written and their issues
are warnings, as [[#warn]] shows. With `--enforce`, the documents are
still written, but an issue is an error and the exit code is 1, as
[[#enforce]] shows. An issue is a file that needs review but is in no
review set, or a review set that is not Current; see [[plan-and-report]].

1. **Install the tool in CI.** Restore the local tool manifest with
   `dotnet tool restore` and run the tool as `dotnet {{tool}}`; see
   [[getting-started]].
2. **Give it the evidence.** For a `url` evidence source, map the CI
   secrets to the environment variables that `credentials` names, so
   that the index can be downloaded; see [[evidence]].
3. **Write the documents and enforce.** Run
   `{{tool}} --plan plan.md --report report.md --enforce` and publish the
   two files with the other compliance documents of the build.
4. **Fix a failure.** Add a review set or a `!` pattern for an uncovered
   file. For a review set that is not Current, run `--elaborate <id>`,
   review the set, and add its record to the index.

Start with warnings while the first reviews are recorded, and add
`--enforce` once every set is Current.
