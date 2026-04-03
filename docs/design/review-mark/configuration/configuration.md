# Configuration Subsystem

## Overview

The Configuration subsystem is responsible for loading, validating, and processing the
ReviewMark YAML configuration file (`.reviewmark.yaml`). It also provides the
file-pattern-matching capability used to resolve glob patterns into concrete file lists.

## Responsibilities

- Deserialize `.reviewmark.yaml` into a strongly-typed configuration model
- Lint the loaded configuration and report any structural errors or warnings
- Resolve `needs-review` and per-review-set `paths` glob patterns into sorted file lists
- Compute SHA-256 fingerprints across resolved file sets
- Generate Review Plan and Review Report markdown documents

## Units

| Unit | Source File | Purpose |
| --- | --- | --- |
| ReviewMarkConfiguration | `Configuration/ReviewMarkConfiguration.cs` | YAML parser and review-set processor |
| GlobMatcher | `Configuration/GlobMatcher.cs` | File pattern matching using glob syntax |
