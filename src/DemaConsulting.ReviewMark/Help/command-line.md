---
title: Command-line options
summary: Every option of the tool, the order in which a run handles them, and exit codes.
aliases:
  - options
  - flags
  - usage
  - exit-codes
tags:
  - reference
order: 2
see-also:
  - configuration
  - plan-and-report
  - troubleshooting
examples:
  - name: usage
    kind: command
    language: shell
    description: Print the short usage text. It ends with a pointer to this help.
    content: '{{tool}} --help'
    expect: "Run '{{tool}} help' for topics and examples."
  - name: silent-log
    kind: command
    language: shell
    description: Hide the console output and keep it in a log file.
    content: '{{tool}} --silent --log output.log --version'
  - name: unknown-option
    kind: command
    language: shell
    description: An unknown argument is an error, printed even with --silent. Exits 1.
    content: '{{tool}} --silent --unknown'
    exit: 1
    expect: "Error: Unknown argument '--unknown'"
---
Options start with `--` (or a single letter after `-`) and may be given in
any order.

| Option | Meaning |
| --- | --- |
| `-v`, `--version` | Print only the version. |
| `-?`, `-h`, `--help` | Print the short usage text. |
| `--silent` | Hide console output. Errors still set exit code 1. |
| `--log <file>` | Also write the output to a file. |
| `--definition <file>` | The definition; default `.reviewmark.yaml`. |
| `--lint` | Check the definition; see [[configuration]]. |
| `--plan <file>` | Write the review plan; see [[plan-and-report]]. |
| `--report <file>` | Write the review report. |
| `--depth <n>` | First heading level of the documents, 1 to 5; default 1. |
| `--plan-depth <n>` | Heading level of the plan; default `--depth`. |
| `--report-depth <n>` | Heading level of the report; default `--depth`. |
| `--index <glob>` | Index PDF review records; see [[evidence]]. May repeat. |
| `--dir <folder>` | The folder to work in; default the current folder. |
| `--enforce` | Plan or report issues are errors; see [[enforcement]]. |
| `--elaborate <id>` | Describe one review set; see [[elaborate]]. |
| `--affected <file>` | Review sets that changed files touch; see [[affected]]. |
| `--validate` | Run the self-tests; see [[self-validation]]. |
| `--results <file>` | Write the self-test results as `.trx` or `.xml`. |

`--result` also works for `--results`, and `--affected -` reads standard
input.

**Order.** `--version`, when all arguments are valid, prints only the
version and skips everything else. Without it, the tool prints a banner,
except for `--lint` and for `--affected` without `--help` or `--validate`.
It then handles the first of `--help`, `--validate`, or `--lint`.
Otherwise it writes the index, then the plan, the report, the
elaboration, and the affected list that options ask for. With no
operation it prints a short description. The command that
[[#silent-log]] prints writes the version to the log file only.

**Folder.** `--dir` is where the tool finds `.reviewmark.yaml`, matches
the file patterns, scans `--index` paths, and writes `index.json`. Paths
given to `--definition`, `--plan`, and `--report` are used as given.

**Errors.** An unknown argument, an option without its value, or a depth
out of range prints `Error: ...` and exits 1 ([[#unknown-option]]).
These errors are found before `--silent` and `--log` take effect, so they
are always printed and never logged.

**Exit codes.** 0 is success, also for a run with warnings only. 1 is an
error: a wrong argument, a definition error, an unknown review-set id,
`--enforce` with issues, a failed self-test, a definition, evidence
index, or changed-file list that cannot be read, or a plan, report,
index, results, or log file that cannot be written. Any other failure,
such as an unreadable file under review, prints `Unexpected error: ...`
and ends the run abnormally with a code other than 0 or 1; see
[[troubleshooting]].

**Help.** `help` is a command, not an option, and must be the first
argument. `--silent` and `--log` do not apply to it, and an unknown topic
exits 2.
