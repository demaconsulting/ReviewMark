---
title: Review evidence
summary: Record a review as a PDF, index the records, and say where the index is.
aliases:
  - review-records
  - records
  - evidence-source
  - index
  - pdf
  - keywords
tags:
  - reference
order: 4
see-also:
  - elaborate
  - plan-and-report
  - configuration
examples:
  - name: index
    kind: command
    language: shell
    description: Index the review records below reviews/.
    content: '{{tool}} --index "reviews/**/*.pdf"'
    expect: 'Index written to'
---
**Review records.** A review is recorded as a PDF, for example the
signed review checklist. Its Keywords document property holds four
`name=value` pairs, separated by spaces:

```text
id=Core-Logic fingerprint=<64 hex digits> date=2026-01-15 result=pass
```

`id` is the review set and `fingerprint` the full fingerprint of its
files when they were reviewed; [[elaborate]] prints both. Write `date`
as `YYYY-MM-DD` and `result` as `pass` or, for example, `fail`. The
names ignore case, and no value may contain a space. A PDF without all
four, or one that cannot be read, is skipped with a warning.

**Index.** `--index <glob>` scans the PDFs that the pattern matches below
the folder (or `--dir`) and writes `index.json` there, as [[#index]]
shows. Repeat `--index` to scan more places. The index holds a `reviews`
list with the `id`, `fingerprint`, `date`, `result`, and `file` of each
record, where `file` is the path of the PDF relative to the folder.
Publish it wherever the review records are kept.

**Evidence source.** `evidence-source` in the definition says where
`--report` finds the index. `type` is one of:

- `none`: no index; every review set is missing.
- `fileshare`: `location` is the path of `index.json`. A relative path
  is relative to the folder of the definition file.
- `url`: `location` is an HTTP or HTTPS address that returns the index.

```yaml
evidence-source:
  type: url
  location: https://reviews.example.com/${PROJECT}/index.json
  credentials:
    username-env: REVIEW_USER
    password-env: REVIEW_TOKEN
```

`${NAME}` in `location` is replaced by that environment variable; an
unset or empty variable is an error, and `$${` writes a literal `${`.
`credentials` names the variables that hold a user name and password
for HTTP Basic authentication. The tool sends them only when both
variables are set and not empty; otherwise the request has no
credentials.

**Status.** The report gives each review set one status. It is Current
when a record has the current fingerprint and the result `pass`, and
Failed when that record has another result. It is Stale when the set has
records but none with the current fingerprint, and Missing when it has no
record. A changed, added, or removed file makes a Current set Stale.
