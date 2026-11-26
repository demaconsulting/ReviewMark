---
title: Affected review sets
summary: List the review sets that a set of changed files touches, for pull requests.
aliases:
  - changed-files
  - git-diff
tags:
  - reference
order: 7
see-also:
  - configuration
  - elaborate
examples:
  - name: affected
    kind: command
    language: shell
    description: List the review sets that the files in changed.txt touch.
    content: '{{tool}} --affected changed.txt'
    expect: |
      Core-Logic
      User-Guide
      unreviewed
---
`--affected <file>` reads a list of changed files and prints the review
sets that they touch, as [[#affected]] shows. Use it in a pull request to
see which reviews a change makes stale. With `-` as the file, the list is
read from standard input.

**Input.** One path per line, as `git diff --name-only` prints, or paths
separated by NUL characters, as `git diff --name-only -z` prints. Paths
are relative to the folder (or `--dir`); absolute paths below it work
too. The files themselves are not read, so deleted files count.

**Output.** One record per line, with fields separated by TAB:

```text
affected<TAB><review-set id><TAB><path>
unreviewed<TAB><path>
```

`affected` names a review set and one of its changed files.
`unreviewed` names a changed file that needs review but is in no review
set. Changed files that need no review are not listed. When no set is
affected, the first line is `No affected review-sets`. A backslash, TAB,
line feed, or carriage return in a field is written as `\\`, `\t`, `\n`,
or `\r`.

There is no banner and no lint warning, so the output can be read by a
script. The exit code is 0 even when files are unreviewed. `--affected`
cannot be combined with `--plan`, `--report`, `--elaborate`, `--index`,
or `--lint`.
