## Microsoft.Extensions.FileSystemGlobbing

Microsoft.Extensions.FileSystemGlobbing is a Microsoft runtime library that provides glob-pattern
file matching against the local file system. It is used by the `GlobMatcher` unit in the
Configuration subsystem.

### Purpose

Microsoft.Extensions.FileSystemGlobbing was chosen because it is the canonical Microsoft-authored
glob library for .NET, distributed as part of the `Microsoft.Extensions` package family. It
supports the `**` double-wildcard pattern required by `.reviewmark.yaml` configurations, handles
path-separator normalization across platforms, and integrates directly with the host file system
without additional abstraction. Using the Microsoft-provided implementation avoids duplicating
glob semantics in application code.

### Features Used

- **`Microsoft.Extensions.FileSystemGlobbing.Matcher`**: the core matching class; accepts one or
  more glob patterns and resolves matching file paths against a base directory
- **`Matcher.AddInclude(string pattern)`**: registers a single glob pattern as an include rule on
  the `Matcher` instance
- **`Matcher.GetResultsInFullPath(string directoryPath)`**: executes the configured patterns
  against the specified directory and returns the full absolute paths of all matching files

### Integration Pattern

`GlobMatcher.GetMatchingFiles()` maintains a `HashSet<string>` of currently matched paths and
processes the ordered list of include/exclude patterns one at a time. For each include pattern
(no `!` prefix) a new `Matcher` is created, `AddInclude` is called with the pattern, and
`GetResultsInFullPath(baseDirectory)` is called; each returned full path is converted to a
repository-relative path and added to the set. For each exclude pattern (prefixed with `!`) a
separate `Matcher` is created for the stripped pattern and the matching paths are removed from the
set. After all patterns are processed, paths are normalized to forward slashes and sorted using
`StringComparer.Ordinal` to produce a deterministic, platform-independent file list.

### Version

Microsoft.Extensions.FileSystemGlobbing 10.0.8 is the required version.
