---
title: Review plan and report
summary: What the review plan and the review report show, and how to write them.
aliases:
  - plan
  - report
  - review-plan
  - review-report
  - status
  - coverage
tags:
  - reference
order: 5
see-also:
  - configuration
  - evidence
  - enforcement
examples:
  - name: plan
    kind: command
    language: shell
    description: Write the review plan to plan.md.
    content: '{{tool}} --plan plan.md'
    expect: 'Review plan written to plan.md'
  - name: report
    kind: command
    language: shell
    description: Write the review report to report.md.
    content: '{{tool}} --report report.md'
    expect: 'Review report written to report.md'
  - name: both-depth
    kind: command
    language: shell
    description: Write both, starting at heading level 2.
    content: '{{tool}} --plan plan.md --report report.md --depth 2'
    expect: |
      Review plan written to plan.md
      Review report written to report.md
---
Both documents are Markdown, written to build compliance documents such
as a code review plan and a code review report.

**Plan.** `--plan <file>` writes the review plan, as [[#plan]] shows. Its
heading `Review Coverage` is followed by a table with one row per review
set: the id, the title, the number of files, and the first 8 characters
of the fingerprint. A `Coverage` section then says that all files that
need review are covered, or lists those that are in no review set.

**Report.** `--report <file>` loads the evidence index and writes the
review report, as [[#report]] shows. Its heading `Review Status` is
followed by a table with one row per review set: the id, the status
(Current, Stale, Missing, or Failed; see [[evidence]]), and the date and
result of the record. A `Referenced Documents` list then names the PDF
of each record in the table.

**Issues.** Uncovered files are issues of the plan, and a status other
than Current is an issue of the report. The tool prints
`Warning: Review plan has coverage issues.` or
`Warning: Review report has review issues.` and exits 0. With
`--enforce` they are errors and the exit code is 1; see [[enforcement]].

**Headings.** `--depth <n>` sets the level of the first heading of both
documents, from 1 to 5, as [[#both-depth]] shows. `--plan-depth` and
`--report-depth` set one document each.

The files that need review are found below the folder the tool runs in,
or `--dir`.
