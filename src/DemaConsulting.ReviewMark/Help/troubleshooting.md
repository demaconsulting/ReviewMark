---
title: Errors and fixes
summary: How to fix the common error messages and review problems.
aliases:
  - errors
  - problems
tags:
  - troubleshooting
order: 10
see-also:
  - command-line
  - configuration
  - evidence
examples:
  - name: help-not-first
    kind: command
    language: shell
    description: help after an option is an unknown argument. Exits 1.
    content: '{{tool}} --plan plan.md help'
    exit: 1
    expect: "Error: Unknown argument 'help'"
  - name: depth-range
    kind: command
    language: shell
    description: A heading depth above 5. Exits 1.
    content: '{{tool}} --plan plan.md --depth 6'
    exit: 1
    expect: '--depth cannot be greater than 5'
  - name: missing-definition
    kind: command
    language: shell
    description: A definition file that does not exist. Exits 1.
    content: '{{tool}} --plan plan.md --definition missing.yaml'
    exit: 1
    expect: 'missing.yaml: error:'
  - name: no-index-definition
    kind: source
    language: yaml
    save-as: no-index.yaml
    description: A definition whose evidence index does not exist.
    content: |
      ---
      evidence-source:
        type: fileshare
        location: no-index.json

      reviews:
        - id: Core-Logic
          title: Review that the core logic is correct
          paths:
            - "src/Core/**/*.cs"
  - name: missing-index
    kind: command
    language: shell
    description: A report whose evidence index is missing. Exits 1.
    content: '{{tool}} --definition no-index.yaml --report report.md'
    exit: 1
    expect: 'Failed to read review index file'
---
Errors are printed to standard error and set exit code 1. A line
`Unexpected error: ...` means a defect in the tool; please report it.

**Wrong arguments.**

- `Unknown argument '<arg>'`: check the spelling in [[command-line]].
  `help` must be the first argument, as [[#help-not-first]] shows.
- `requires a ... argument`: the option needs a value after it.
- `cannot be greater than 5`: depths are 1 to 5 ([[#depth-range]]).
- `--affected cannot be combined with ...`: run `--affected` on its own.

**Definition errors.**

- `Could not find file`: there is no `.reviewmark.yaml` in the folder,
  or the `--definition` path is wrong ([[#missing-definition]]).
- `<file>:<line>:<column>: error:`: a YAML syntax error at that place.
- `is missing a required 'id' field` and similar: add the key; see
  [[configuration]].
- `environment variable '<name>' is not set`: set the variable that
  `location` uses, or write `$${` for a literal `${`.

**Evidence errors.**

- `Failed to read review index file`: the `fileshare` index does not
  exist. A relative `location` is relative to the definition file. The
  command from [[#missing-index]] shows the error, even without
  `--enforce`; run `--index` or fix the path.
- `Failed to download review index`: the `url` cannot be reached or
  returned an error. Check the address and the `credentials` variables;
  without both of them set, no credentials are sent.
- `Skipping '<file>'`: a PDF lacks one of the four Keywords, or cannot be
  read; see [[evidence]].

**Review problems.**

- A set is Stale after any change to the content of one of its files.
  Review it again and record the new fingerprint.
- Files are uncovered: add them to a review set, or exclude them from
  `needs-review` with a `!` pattern.
- `No review set found with ID`: the `--elaborate` id is not in the
  definition; ids are compared exactly.
