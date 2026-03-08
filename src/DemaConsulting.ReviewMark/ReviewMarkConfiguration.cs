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

using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.ReviewMark;

// ---------------------------------------------------------------------------
// Internal YAML deserialization models
// These private classes mirror the raw YAML structure and are not part of the
// public API. YamlDotNet populates them via property aliases.
// ---------------------------------------------------------------------------

/// <summary>
///     Raw YAML deserialization model for the top-level .reviewmark.yaml document.
/// </summary>
file sealed class ReviewMarkYaml
{
    /// <summary>
    ///     Gets or sets the list of glob patterns that identify files requiring review.
    /// </summary>
    [YamlMember(Alias = "needs-review")]
    public List<string>? NeedsReview { get; set; }

    /// <summary>
    ///     Gets or sets the evidence-source configuration block.
    /// </summary>
    [YamlMember(Alias = "evidence-source")]
    public EvidenceSourceYaml? EvidenceSource { get; set; }

    /// <summary>
    ///     Gets or sets the list of review set definitions.
    /// </summary>
    [YamlMember(Alias = "reviews")]
    public List<ReviewSetYaml>? Reviews { get; set; }
}

/// <summary>
///     Raw YAML deserialization model for the <c>evidence-source</c> block.
/// </summary>
file sealed class EvidenceSourceYaml
{
    /// <summary>
    ///     Gets or sets the source type (e.g. <c>url</c> or <c>fileshare</c>).
    /// </summary>
    [YamlMember(Alias = "type")]
    public string? Type { get; set; }

    /// <summary>
    ///     Gets or sets the location URL or path for the evidence source.
    /// </summary>
    [YamlMember(Alias = "location")]
    public string? Location { get; set; }

    /// <summary>
    ///     Gets or sets the optional credentials block.
    /// </summary>
    [YamlMember(Alias = "credentials")]
    public EvidenceCredentialsYaml? Credentials { get; set; }
}

/// <summary>
///     Raw YAML deserialization model for the <c>credentials</c> block inside
///     <c>evidence-source</c>.
/// </summary>
file sealed class EvidenceCredentialsYaml
{
    /// <summary>
    ///     Gets or sets the environment-variable name that holds the username.
    /// </summary>
    [YamlMember(Alias = "username-env")]
    public string? UsernameEnv { get; set; }

    /// <summary>
    ///     Gets or sets the environment-variable name that holds the password.
    /// </summary>
    [YamlMember(Alias = "password-env")]
    public string? PasswordEnv { get; set; }
}

/// <summary>
///     Raw YAML deserialization model for an individual entry under <c>reviews</c>.
/// </summary>
file sealed class ReviewSetYaml
{
    /// <summary>
    ///     Gets or sets the unique identifier for the review set.
    /// </summary>
    [YamlMember(Alias = "id")]
    public string? Id { get; set; }

    /// <summary>
    ///     Gets or sets the human-readable title of the review set.
    /// </summary>
    [YamlMember(Alias = "title")]
    public string? Title { get; set; }

    /// <summary>
    ///     Gets or sets the list of glob patterns that make up this review set.
    /// </summary>
    [YamlMember(Alias = "paths")]
    public List<string>? Paths { get; set; }
}

// ---------------------------------------------------------------------------
// Public API — internal to the assembly
// ---------------------------------------------------------------------------

/// <summary>
///     Represents the evidence-source configuration from <c>.reviewmark.yaml</c>.
/// </summary>
/// <param name="Type">The source type, e.g. <c>url</c> or <c>fileshare</c>.</param>
/// <param name="Location">The URL or path for the evidence source.</param>
/// <param name="UsernameEnv">Optional environment-variable name that holds the username credential.</param>
/// <param name="PasswordEnv">Optional environment-variable name that holds the password credential.</param>
internal sealed record EvidenceSource(
    string Type,
    string Location,
    string? UsernameEnv,
    string? PasswordEnv);

/// <summary>
///     Represents a single review-set definition from <c>.reviewmark.yaml</c>.
/// </summary>
internal sealed class ReviewSet
{
    /// <summary>
    ///     Gets the unique identifier for this review set.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Gets the human-readable title of this review set.
    /// </summary>
    public string Title { get; }

    /// <summary>
    ///     Gets the ordered list of glob patterns (include and exclude) for this review set.
    /// </summary>
    public IReadOnlyList<string> Paths { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ReviewSet" /> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The human-readable title.</param>
    /// <param name="paths">The ordered list of glob patterns.</param>
    internal ReviewSet(string id, string title, IReadOnlyList<string> paths)
    {
        Id = id;
        Title = title;
        Paths = paths;
    }

    /// <summary>
    ///     Returns all files matched by this review set's glob patterns within
    ///     <paramref name="directory" />.
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <returns>A sorted list of relative file paths.</returns>
    internal IReadOnlyList<string> GetFiles(string directory) =>
        GlobMatcher.GetMatchingFiles(directory, Paths);

    /// <summary>
    ///     Computes a content-based fingerprint for this review set's matched files
    ///     within <paramref name="directory" />.
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <returns>
    ///     A lowercase hex SHA-256 string that is stable across file renames but
    ///     changes when any file's content changes.
    /// </returns>
    /// <remarks>
    ///     Algorithm:
    ///     <list type="number">
    ///         <item>Resolve all files matched by the ordered include/exclude glob patterns.</item>
    ///         <item>Compute a SHA-256 hash of the content of each matched file.</item>
    ///         <item>Sort the resulting SHA-256 hashes lexicographically.</item>
    ///         <item>Concatenate the sorted hashes into a single string.</item>
    ///         <item>Compute a final SHA-256 hash of the concatenated string.</item>
    ///         <item>Return as lowercase hex string.</item>
    ///     </list>
    /// </remarks>
    internal string GetFingerprint(string directory)
    {
        // Resolve all matching files for this review set
        var files = GetFiles(directory);

        // Compute a SHA-256 hash of each file's content and collect as hex strings
        var contentHashes = files
            .Select(relativePath =>
            {
                // Safely combine base directory with relative path to get the full file path
                var fullPath = PathHelpers.SafePathCombine(directory, relativePath);
                var bytes = File.ReadAllBytes(fullPath);
                var hash = SHA256.HashData(bytes);
                return Convert.ToHexString(hash).ToLowerInvariant();
            })
            .ToList();

        // Sort the content hashes lexicographically so that file renames don't change the fingerprint
        contentHashes.Sort(StringComparer.Ordinal);

        // Concatenate the sorted hashes and compute a final SHA-256 hash
        var concatenated = string.Concat(contentHashes);
        var finalBytes = Encoding.UTF8.GetBytes(concatenated);
        var finalHash = SHA256.HashData(finalBytes);
        return Convert.ToHexString(finalHash).ToLowerInvariant();
    }
}

/// <summary>
///     Represents the parsed contents of a <c>.reviewmark.yaml</c> configuration file.
/// </summary>
internal sealed class ReviewMarkConfiguration
{
    /// <summary>
    ///     Gets the ordered list of glob patterns identifying files that need review.
    /// </summary>
    public IReadOnlyList<string> NeedsReviewPatterns { get; }

    /// <summary>
    ///     Gets the evidence-source configuration.
    /// </summary>
    public EvidenceSource EvidenceSource { get; }

    /// <summary>
    ///     Gets the list of review set definitions.
    /// </summary>
    public IReadOnlyList<ReviewSet> Reviews { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ReviewMarkConfiguration" />.
    /// </summary>
    /// <param name="needsReviewPatterns">Glob patterns for files requiring review.</param>
    /// <param name="evidenceSource">Evidence-source configuration.</param>
    /// <param name="reviews">Review set definitions.</param>
    private ReviewMarkConfiguration(
        IReadOnlyList<string> needsReviewPatterns,
        EvidenceSource evidenceSource,
        IReadOnlyList<ReviewSet> reviews)
    {
        NeedsReviewPatterns = needsReviewPatterns;
        EvidenceSource = evidenceSource;
        Reviews = reviews;
    }

    /// <summary>
    ///     Loads and parses a <c>.reviewmark.yaml</c> file from disk.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the configuration file.</param>
    /// <returns>A populated <see cref="ReviewMarkConfiguration" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be read.</exception>
    internal static ReviewMarkConfiguration Load(string filePath)
    {
        // Validate the file path argument
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        }

        // Read the file contents and wrap any file-system exception with useful context.
        // Generic catch is justified here: Expected exceptions include IOException (and its subtypes
        // such as FileNotFoundException, DirectoryNotFoundException, PathTooLongException),
        // UnauthorizedAccessException, ArgumentException (invalid path characters),
        // NotSupportedException, and other file-system exceptions.
        string yaml;
        try
        {
            yaml = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to read configuration file '{filePath}': {ex.Message}", ex);
        }

        // Delegate to Parse for deserialization
        return Parse(yaml);
    }

    /// <summary>
    ///     Parses a YAML string into a <see cref="ReviewMarkConfiguration" />.
    /// </summary>
    /// <param name="yaml">The YAML content to parse.</param>
    /// <returns>A populated <see cref="ReviewMarkConfiguration" /> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="yaml" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the YAML is invalid or missing required fields.</exception>
    internal static ReviewMarkConfiguration Parse(string yaml)
    {
        // Validate the yaml input
        ArgumentNullException.ThrowIfNull(yaml);

        // Build a YamlDotNet deserializer that ignores unmatched fields
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // Deserialize the raw YAML into the internal model
        ReviewMarkYaml raw;
        try
        {
            raw = deserializer.Deserialize<ReviewMarkYaml>(yaml)
                  ?? throw new ArgumentException("YAML content is empty or invalid.", nameof(yaml));
        }
        catch (YamlException ex)
        {
            throw new ArgumentException($"Invalid YAML content: {ex.Message}", nameof(yaml), ex);
        }

        // Map needs-review patterns (default to empty list if absent)
        var needsReviewPatterns = (IReadOnlyList<string>)(raw.NeedsReview ?? []);

        // Map evidence-source (required: evidence-source block, type, and location)
        if (raw.EvidenceSource is not { } es)
        {
            throw new ArgumentException("Configuration is missing required 'evidence-source' block.", nameof(yaml));
        }

        if (string.IsNullOrWhiteSpace(es.Type))
        {
            throw new ArgumentException("Configuration 'evidence-source' is missing a required 'type' field.", nameof(yaml));
        }

        if (string.IsNullOrWhiteSpace(es.Location))
        {
            throw new ArgumentException("Configuration 'evidence-source' is missing a required 'location' field.", nameof(yaml));
        }

        var evidenceSource = new EvidenceSource(
            Type: es.Type,
            Location: es.Location,
            UsernameEnv: es.Credentials?.UsernameEnv,
            PasswordEnv: es.Credentials?.PasswordEnv);
        // Map review sets, requiring id, title, and paths for each entry
        var reviews = (raw.Reviews ?? [])
            .Select((r, i) =>
            {
                // Each review set must have an id
                if (string.IsNullOrWhiteSpace(r.Id))
                {
                    throw new ArgumentException($"Review set at index {i} is missing a required 'id' field.");
                }

                // Each review set must have a title
                if (string.IsNullOrWhiteSpace(r.Title))
                {
                    throw new ArgumentException($"Review set '{r.Id}' is missing a required 'title' field.");
                }

                // Each review set must have at least one non-empty path pattern
                var paths = r.Paths;
                if (paths is null || !paths.Any(p => !string.IsNullOrWhiteSpace(p)))
                {
                    throw new ArgumentException(
                        $"Review set '{r.Id}' at index {i} is missing required 'paths' entries.");
                }

                return new ReviewSet(r.Id, r.Title, paths);
            })
            .ToList();

        return new ReviewMarkConfiguration(needsReviewPatterns, evidenceSource, reviews);
    }

    /// <summary>
    ///     Returns all files within <paramref name="directory" /> that match the
    ///     <see cref="NeedsReviewPatterns" /> glob patterns.
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <returns>A sorted list of relative file paths.</returns>
    internal IReadOnlyList<string> GetNeedsReviewFiles(string directory) =>
        GlobMatcher.GetMatchingFiles(directory, NeedsReviewPatterns);
}
