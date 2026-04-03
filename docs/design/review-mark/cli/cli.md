# Cli Subsystem

## Overview

The Cli subsystem is responsible for parsing and owning the command-line interface of
ReviewMark. It exposes a single software unit — Context — that processes the raw
`string[] args` array into a structured set of properties consumed by the rest of the
tool.

## Responsibilities

- Parse all supported command-line flags and arguments into a typed `Context` object
- Validate that no unrecognised arguments are supplied
- Own the output channels (stdout and optional log file) and the process exit code
- Propagate the `--silent` flag to suppress non-error output

## Units

| Unit    | Source File              | Purpose                                      |
|---------|--------------------------|----------------------------------------------|
| Context | `Cli/Context.cs`         | Command-line argument parser and I/O owner   |
