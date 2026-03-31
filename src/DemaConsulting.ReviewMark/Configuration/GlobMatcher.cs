// Copyright (c) DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Extensions.FileSystemGlobbing;

namespace DemaConsulting.ReviewMark.Configuration;

/// <summary>
///     Provides glob-based file matching utilities.
/// </summary>
internal static class GlobMatcher
{
    /// <summary>
    ///     Returns all files under <paramref name="baseDirectory" /> that match the
    ///     ordered include/exclude <paramref name="patterns" />.
    /// </summary>
    /// <param name="baseDirectory">The root directory to search within.</param>
    /// <param name="patterns">
    ///     Ordered list of glob patterns. Patterns prefixed with <c>!</c> are treated as
    ///     excludes (the prefix is stripped before matching); all other patterns are includes.
    /// </param>
    /// <returns>
    ///     A sorted list of relative file paths (using forward slashes), relative to
    ///     <paramref name="baseDirectory" />, sorted by <see cref="StringComparer.Ordinal" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="baseDirectory" /> or <paramref name="patterns" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="baseDirectory" /> is empty or whitespace.
    /// </exception>
    internal static IReadOnlyList<string> GetMatchingFiles(string baseDirectory, IReadOnlyList<string> patterns)
    {
        // Validate that neither parameter is null
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentNullException.ThrowIfNull(patterns);

        // Validate that baseDirectory is not empty or whitespace
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ArgumentException("Base directory must not be empty or whitespace.", nameof(baseDirectory));
        }

        // Process patterns in order, maintaining a running set of matched files.
        // Each include pattern adds files; each exclude pattern removes files.
        // This implements the documented ordered semantics from THEORY-OF-OPERATIONS.md,
        // allowing a later include to re-add files removed by an earlier exclude.
        var fileSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pattern in patterns)
        {
            if (pattern.StartsWith('!'))
            {
                // Exclude: build a single-pattern matcher and remove all matching files
                var excludeMatcher = new Matcher();
                excludeMatcher.AddInclude(pattern[1..]);
                foreach (var fullPath in excludeMatcher.GetResultsInFullPath(baseDirectory))
                {
                    fileSet.Remove(Path.GetRelativePath(baseDirectory, fullPath));
                }
            }
            else
            {
                // Include: build a single-pattern matcher and add all matching files
                var includeMatcher = new Matcher();
                includeMatcher.AddInclude(pattern);
                foreach (var fullPath in includeMatcher.GetResultsInFullPath(baseDirectory))
                {
                    fileSet.Add(Path.GetRelativePath(baseDirectory, fullPath));
                }
            }
        }

        // Normalize path separators to forward slashes and sort the results
        var result = fileSet
            .Select(relativePath => relativePath.Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToList();

        return result;
    }
}
