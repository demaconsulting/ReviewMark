# Indexing Subsystem

## Overview

The Indexing subsystem is responsible for loading review evidence from an external index
and for safe file-path manipulation. It provides the lookup engine that determines whether
each review-set is Current, Stale, Missing, or Failed.

## Responsibilities

- Load the evidence index from a `none`, `fileshare`, or `url` source
- Scan a set of PDF files, extract structured metadata from the Keywords field, and
  produce an `index.json` evidence index
- Provide safe path-combination utilities that prevent directory-traversal attacks

## Units

| Unit          | Source File                    | Purpose                                              |
|---------------|--------------------------------|------------------------------------------------------|
| ReviewIndex   | `Indexing/ReviewIndex.cs`      | Review evidence loader and query engine              |
| PathHelpers   | `Indexing/PathHelpers.cs`      | File path utilities (safe path combination)          |
