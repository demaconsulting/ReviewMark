# GlobMatcher

## Purpose

The `GlobMatcher` software unit resolves an ordered list of glob patterns into a
concrete, sorted list of file paths relative to a base directory. It provides the
file enumeration primitive used by the Configuration subsystem to expand the
`needs-review` and `review-set` file lists defined in `.reviewmark.yaml`.

## Algorithm

`GlobMatcher.GetMatchingFiles(baseDirectory, patterns)` processes patterns in the
order they are declared. Patterns prefixed with `!` are exclusion patterns; all
others are inclusion patterns. Each inclusion pattern adds matching paths to the
result set; each exclusion pattern removes matching paths from the result set.
Because patterns are applied in declaration order, a later pattern can re-include
files excluded by an earlier one, or exclude files included by an earlier one. The
`**` wildcard matches any number of path segments, enabling recursive matching.
After all patterns are processed, the result set is sorted and returned.

## Return Value

The method returns a sorted list of relative file paths. Path separators are
normalized to forward slashes regardless of the host operating system, ensuring
consistent fingerprint computation across platforms.

## Usage

`GlobMatcher.GetMatchingFiles()` is called by `ReviewMarkConfiguration` to resolve:

- The `needs-review` file list, which represents all files subject to review
- Each `review-set` file list, which represents the files covered by a specific review record
