---
title: Getting started
summary: Install the tool, describe the review sets, and write the plan and report.
aliases:
  - start
  - quickstart
  - examples
  - install
tags:
  - tutorial
order: 1
see-also:
  - configuration
  - plan-and-report
  - evidence
  - enforcement
examples:
  - name: install-local
    kind: snippet
    language: shell
    save-as: install-local.sh
    description: Add the tool to the tool manifest of a repository.
    content: |
      dotnet new tool-manifest
      dotnet tool install --local DemaConsulting.ReviewMark
  - name: run-in-ci
    kind: snippet
    language: shell
    save-as: run-in-ci.sh
    description: Restore the manifest tools in CI, then run the tool.
    content: |
      dotnet tool restore
      dotnet {{tool}} --plan plan.md --report report.md --enforce
  - name: version
    kind: command
    language: shell
    description: Print the version and exit.
    content: '{{tool}} --version'
  - name: lint
    kind: command
    language: shell
    description: Check .reviewmark.yaml. Prints nothing when it is valid.
    content: '{{tool}} --lint'
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
  - name: topic
    kind: command
    language: shell
    description: Print a help topic, here the configuration file.
    content: '{{tool}} help configuration'
    expect: 'needs-review'
---
{{tool}} {{version}} is a .NET tool.
It writes only the files that options name, `index.json` for `--index`,
and a temporary folder that `--validate` uses.

1. **Install it.** The tool needs the .NET 10 SDK or later. In CI, set it
   up first when the runner has an older .NET, for example with
   `actions/setup-dotnet` and `dotnet-version: 10.x`.
   For a repository or CI, install it as a local tool. Run the commands
   that [[#install-local]] prints once, without
   `dotnet new tool-manifest` when `.config/dotnet-tools.json` exists, and
   commit that file. In CI, run `dotnet tool restore` first, then run the
   tool as `dotnet {{tool}}`, as in the commands that
   [[#run-in-ci]] prints. For one machine, install it globally and
   run it as `{{tool}}`:

   ```bash
   dotnet tool install -g DemaConsulting.ReviewMark
   ```

2. **Check it.** The command that [[#version]] prints shows the version.
3. **Describe the review sets.** Write `.reviewmark.yaml` in the root of
   the repository: which files need review, how they are grouped, and
   where the review records are; see [[configuration]]. The command
   from [[#lint]] checks it.
4. **Plan and report.** The command that [[#plan]] prints writes the
   review plan, which shows every review set and any file that needs
   review but is in no set. The command from [[#report]] writes the
   review report, the status of every set; see [[plan-and-report]].
5. **Record reviews.** Review a set, save the record as a PDF with its
   fingerprint in the Keywords, and index the records; see [[evidence]].
6. **Enforce in CI.** Add `--enforce` so that a missing or out-of-date
   review fails the build; see [[enforcement]].

Print any help topic with `{{tool}} help <topic>`, as in the command that
[[#topic]] prints. Add `--search <words>` to find a topic, and `--json` to
get any help answer as JSON.
