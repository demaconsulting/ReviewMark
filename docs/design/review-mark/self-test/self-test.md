# SelfTest Subsystem

## Overview

The SelfTest subsystem provides a self-validation framework that allows ReviewMark to
qualify itself as a tool for use in regulated environments. It executes a built-in suite
of integration tests against a temporary working directory and reports the results.

## Responsibilities

- Orchestrate the execution of the built-in validation test suite
- Write test results to a TRX or JUnit XML file for ingestion by CI pipelines
- Output a human-readable summary table to the console
- Set the process exit code to reflect overall pass/fail status

## Units

| Unit       | Source File               | Purpose                                          |
|------------|---------------------------|--------------------------------------------------|
| Validation | `SelfTest/Validation.cs`  | Self-validation test runner                      |
