---
title: Elaborate a review set
summary: Print the files, context, and full fingerprint of one review set.
aliases:
  - fingerprint
tags:
  - reference
order: 6
see-also:
  - evidence
  - configuration
examples:
  - name: elaborate
    kind: command
    language: shell
    description: Describe the Core-Logic review set.
    content: '{{tool}} --elaborate Core-Logic'
    expect: 'src/Core/Calculator.cs'
  - name: unknown-id
    kind: command
    language: shell
    description: An id that is not in the definition. Exits 1.
    content: '{{tool}} --elaborate No-Such-Set'
    exit: 1
    expect: "No review set found with ID 'No-Such-Set'"
---
`--elaborate <id>` prints what a reviewer needs to start the review of
one set, as [[#elaborate]] shows. Use it to fill in the review record:
copy the full fingerprint into the Keywords of the PDF; see [[evidence]].

**Output.** After the banner and any lint warnings, the tool prints
Markdown: a heading with the id, a table with the id, the title, and the
full fingerprint, a `Context` list of the context files (when there are
any), and a `Files` list of the files to review. Paths are relative to
the folder and use forward slashes.

**Fingerprint.** The fingerprint is a SHA-256 hash of the sorted
SHA-256 hashes of the files of the set. It does not depend on file names,
so a renamed file keeps it, but it changes when a file is edited, added,
or removed. The plan shows its first 8 characters.

An id that is not in the definition is an error and exits 1, as
[[#unknown-id]] shows.
