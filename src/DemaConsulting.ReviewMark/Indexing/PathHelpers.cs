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

using DemaConsulting.ReviewMark.Configuration;

namespace DemaConsulting.ReviewMark.Indexing;

/// <summary>
///     Helper utilities for safe path operations.
/// </summary>
internal static class PathHelpers
{
    /// <summary>
    ///     Safely combines two paths, ensuring the resolved combined path stays within the base directory.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="relativePath">The relative path to combine.</param>
    /// <returns>The combined path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="basePath"/> or <paramref name="relativePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the resolved combined path escapes the base directory, or when a supplied path is invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">Thrown when a supplied path contains an unsupported format.</exception>
    /// <exception cref="PathTooLongException">Thrown when the combined or resolved path exceeds the system-defined maximum length.</exception>
    /// <remarks>
    ///     This method guards against path-traversal attacks arising from untrusted <c>file</c>
    ///     fields in evidence-index documents. By verifying that the fully-resolved combined path
    ///     remains inside <paramref name="basePath" />, it prevents an attacker-controlled relative
    ///     path (e.g. <c>../../etc/sensitive</c> or an absolute path) from escaping the intended
    ///     directory scope.
    ///     <para>
    ///     The method is stateless and holds no shared mutable state, making it safe for concurrent
    ///     use from multiple threads.
    ///     </para>
    /// </remarks>
    internal static string SafePathCombine(string basePath, string relativePath)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(relativePath);

        // Combine the paths (preserves the caller's relative/absolute style)
        var combinedPath = Path.Combine(basePath, relativePath);

        // Security check: resolve both paths to absolute form and verify the combined
        // path is still inside the base directory. Path.GetRelativePath handles root
        // paths, platform case-sensitivity, and directory-separator normalization natively.
        var absoluteBase = Path.GetFullPath(basePath);
        var absoluteCombined = Path.GetFullPath(combinedPath);
        var checkRelative = Path.GetRelativePath(absoluteBase, absoluteCombined);

        if (string.Equals(checkRelative, "..", StringComparison.Ordinal)
            || checkRelative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || checkRelative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            || Path.IsPathRooted(checkRelative))
        {
            throw new ArgumentException($"Invalid path component: {relativePath}", nameof(relativePath));
        }

        return combinedPath;
    }

    /// <summary>
    ///     Parses the text of a changed-file list into a sorted, de-duplicated list of normalized relative paths.
    /// </summary>
    /// <param name="text">The raw list text, separated by newlines or (when any NUL character is present) NUL characters.</param>
    /// <param name="directory">The directory that absolute paths are made relative to.</param>
    /// <returns>The normalized paths, de-duplicated and sorted by <see cref="StringComparer.Ordinal"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="directory"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     The separator is auto-detected: when the text contains a NUL character the entries are NUL separated
    ///     (as produced by <c>git -z</c>) and newlines are treated as part of the name; otherwise entries are
    ///     separated by <c>\n</c>. A single leading byte-order mark is removed once from the whole text, and in
    ///     newline mode only one trailing <c>\r</c> is removed per entry (in NUL mode CR is part of the name).
    ///     Entries that are empty after that are dropped; whitespace-only entries are kept as names.
    ///     No file system access is performed.
    /// </remarks>
    internal static IReadOnlyList<string> ParseChangedPaths(string text, string directory)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(directory);

        // Remove a single leading byte-order mark (stdin does not strip it)
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        // Detect the separator
        var nulMode = text.Contains('\0');
        var entries = nulMode ? text.Split('\0') : text.Split('\n');

        // Remove one trailing CR per entry in newline mode only
        if (!nulMode)
        {
            entries = entries.Select(entry => entry.EndsWith('\r') ? entry[..^1] : entry).ToArray();
        }

        // Normalize, drop blanks, de-duplicate, and sort for deterministic output
        return entries
            .Where(entry => entry.Length > 0)
            .Select(entry => NormalizeChangedPath(entry, directory))
            .Where(entry => entry.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    ///     Normalizes a single changed-file path for glob matching.
    /// </summary>
    /// <param name="rawPath">The raw path text.</param>
    /// <param name="directory">The directory that absolute paths are made relative to.</param>
    /// <returns>
    ///     The normalized path using forward slashes with no leading <c>./</c>, or an empty string when the
    ///     entry is blank.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rawPath"/> or <paramref name="directory"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     Whitespace is preserved (no trimming). Backslashes become forward slashes only on platforms where
    ///     the backslash is the directory separator (Windows); on other platforms it is an ordinary file-name
    ///     character and is preserved. Absolute paths under <paramref name="directory"/> are made relative to
    ///     it, and leading <c>./</c> segments are removed. Absolute paths outside the directory are retained
    ///     unchanged (apart from the platform-specific separator conversion). Any path containing a <c>..</c>
    ///     segment, absolute or relative, is never canonicalized (an absolute one is not made relative), so
    ///     <c>GlobMatcher.IsMatch</c> rejects it even when the traversal would stay inside the directory.
    /// </remarks>
    internal static string NormalizeChangedPath(string rawPath, string directory)
    {
        ArgumentNullException.ThrowIfNull(rawPath);
        ArgumentNullException.ThrowIfNull(directory);

        // Unify separators (whitespace is significant and preserved)
        // (only where backslash is the directory separator)
        var windowsSeparators = Path.DirectorySeparatorChar == '\\';
        var path = windowsSeparators ? rawPath.Replace('\\', '/') : rawPath;

        // Make absolute paths under the directory relative to it, unless they contain a ".." segment
        // (rejected before canonicalization, exactly like relative paths; it stays rooted and never matches)
        if (path.Length > 0 && Path.IsPathRooted(path) && !GlobMatcher.HasParentSegment(rawPath))
        {
            var relative = Path.GetRelativePath(Path.GetFullPath(directory), path);
            if (windowsSeparators)
            {
                relative = relative.Replace('\\', '/');
            }

            if (relative != ".." && !relative.StartsWith("../", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            {
                path = relative;
            }
        }

        // Strip leading "./" segments
        while (path.StartsWith("./", StringComparison.Ordinal))
        {
            path = path[2..];
        }

        return path;
    }

    /// <summary>
    ///     Escapes a field of a tab-delimited <c>--affected</c> output record.
    /// </summary>
    /// <param name="value">The field value.</param>
    /// <returns>
    ///     The value with <c>\</c> replaced by <c>\\</c>, TAB by <c>\t</c>, LF by <c>\n</c> and CR by <c>\r</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <remarks>
    ///     The escaping is reversible: a consumer scans left to right replacing <c>\\</c>, <c>\t</c>,
    ///     <c>\n</c> and <c>\r</c> with the original characters.
    /// </remarks>
    internal static string EscapeRecordField(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var builder = new System.Text.StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }

        return builder.ToString();
    }
}
