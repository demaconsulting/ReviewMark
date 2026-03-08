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

namespace DemaConsulting.ReviewMark;

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

        // Build the glob matcher by iterating patterns in order
        var matcher = new Matcher();
        var hasIncludes = false;
        foreach (var pattern in patterns)
        {
            // Patterns prefixed with '!' are excludes; everything else is an include
            if (pattern.StartsWith('!'))
            {
                matcher.AddExclude(pattern[1..]);
            }
            else
            {
                matcher.AddInclude(pattern);
                hasIncludes = true;
            }
        }

        // Return early if no include patterns were added — the matcher would match nothing anyway
        if (!hasIncludes)
        {
            return [];
        }

        // Execute the match and collect relative paths, normalising separators to forward slashes
        var result = matcher
            .GetResultsInFullPath(baseDirectory)
            .Select(fullPath =>
            {
                // Convert the full path back to a relative path using forward slashes
                var relativePath = Path.GetRelativePath(baseDirectory, fullPath);
                return relativePath.Replace(Path.DirectorySeparatorChar, '/');
            })
            .Order(StringComparer.Ordinal)
            .ToList();

        return result;
    }
}
