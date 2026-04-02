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
using DemaConsulting.ReviewMark.Cli;
using DemaConsulting.ReviewMark.Indexing;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.ReviewMark.Configuration;

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
// File-local helpers — use file-local YAML types
// ---------------------------------------------------------------------------

/// <summary>
///     File-local static helper that encapsulates YAML deserialization and model validation
///     on behalf of <see cref="ReviewMarkConfiguration" />.  Because both this class and
///     <see cref="ReviewMarkYaml" /> are file-local, C# allows them to appear in the
///     method signatures here.
/// </summary>
file static class ReviewMarkConfigurationHelpers
{
    /// <summary>
    ///     Returns <c>true</c> when <paramref name="type" /> is a recognized evidence-source
    ///     type (<c>none</c>, <c>url</c>, or <c>fileshare</c>, case-insensitive).
    /// </summary>
    /// <param name="type">The type string to test.</param>
    /// <returns><c>true</c> if the type is supported; <c>false</c> otherwise.</returns>
    public static bool IsSupportedEvidenceSourceType(string type) =>
        string.Equals(type, "none", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(type, "url", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(type, "fileshare", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///     Deserializes a YAML string into the raw <see cref="ReviewMarkYaml" /> model.
    /// </summary>
    /// <param name="yaml">YAML content to parse.</param>
    /// <param name="filePath">
    ///     Optional file path used to produce actionable error messages.  When <c>null</c>,
    ///     YAML errors are thrown as <see cref="ArgumentException" /> (preserving the
    ///     <see cref="ReviewMarkConfiguration.Parse" /> contract).  When non-<c>null</c>,
    ///     they are thrown as <see cref="InvalidOperationException" /> and include the
    ///     file name, line, and column.
    /// </param>
    /// <returns>The deserialized <see cref="ReviewMarkYaml" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="filePath" /> is <c>null</c> and the YAML is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <paramref name="filePath" /> is set and the YAML is invalid.
    /// </exception>
    public static ReviewMarkYaml DeserializeRaw(string yaml, string? filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        try
        {
            if (filePath != null)
            {
                return deserializer.Deserialize<ReviewMarkYaml>(yaml)
                       ?? throw new InvalidOperationException(
                           $"Configuration file '{filePath}' is empty or null.");
            }

            return deserializer.Deserialize<ReviewMarkYaml>(yaml)
                   ?? throw new ArgumentException("YAML content is empty or invalid.", nameof(yaml));
        }
        catch (YamlException ex)
        {
            if (filePath != null)
            {
                throw new InvalidOperationException(
                    $"Failed to parse '{filePath}' at line {ex.Start.Line}, column {ex.Start.Column}: {ex.Message}",
                    ex);
            }

            throw new ArgumentException($"Invalid YAML content: {ex.Message}", nameof(yaml), ex);
        }
    }

    /// <summary>
    ///     Validates a raw <see cref="ReviewMarkYaml" /> model and builds a
    ///     <see cref="ReviewMarkConfiguration" /> from it.
    /// </summary>
    /// <param name="raw">The deserialized raw model to validate.</param>
    /// <returns>A validated <see cref="ReviewMarkConfiguration" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when required fields are absent or malformed.
    /// </exception>
    public static ReviewMarkConfiguration BuildConfiguration(ReviewMarkYaml raw)
    {
        // Map needs-review patterns (default to empty list if absent)
        var needsReviewPatterns = (IReadOnlyList<string>)(raw.NeedsReview ?? []);

        // Map evidence-source (required: evidence-source block, type, and location)
        if (raw.EvidenceSource is not { } es)
        {
            throw new ArgumentException("Configuration is missing required 'evidence-source' block.");
        }

        if (string.IsNullOrWhiteSpace(es.Type))
        {
            throw new ArgumentException("Configuration 'evidence-source' is missing a required 'type' field.");
        }

        if (!IsSupportedEvidenceSourceType(es.Type))
        {
            throw new ArgumentException(
                $"Configuration 'evidence-source' type '{es.Type}' is not supported (must be 'none', 'url', or 'fileshare').");
        }

        if (string.IsNullOrWhiteSpace(es.Location) && !string.Equals(es.Type, "none", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Configuration 'evidence-source' is missing a required 'location' field.");
        }

        var evidenceSource = new EvidenceSource(
            Type: es.Type,
            Location: es.Location ?? string.Empty,
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
}

// ---------------------------------------------------------------------------
// Public API — internal to the assembly
// ---------------------------------------------------------------------------

/// <summary>
///     Represents the evidence-source configuration from <c>.reviewmark.yaml</c>.
/// </summary>
/// <param name="Type">The source type, e.g. <c>none</c>, <c>url</c>, or <c>fileshare</c>.</param>
/// <param name="Location">
///     The URL or path for the evidence source; required for <c>url</c> and <c>fileshare</c> types,
///     and optional/ignored when <paramref name="Type" /> is <c>none</c>.
/// </param>
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
                using var stream = File.OpenRead(fullPath);
                var hash = SHA256.HashData(stream);
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
///     Represents the result of publishing a review plan.
/// </summary>
/// <param name="Markdown">The generated Markdown content.</param>
/// <param name="HasIssues">
///     <c>true</c> if any files requiring review are not covered by any review-set;
///     otherwise <c>false</c>.
/// </param>
internal sealed record ReviewPlanResult(string Markdown, bool HasIssues);

/// <summary>
///     Represents the result of publishing a review report.
/// </summary>
/// <param name="Markdown">The generated Markdown content.</param>
/// <param name="HasIssues">
///     <c>true</c> if any reviews are failed, stale, or missing; otherwise <c>false</c>.
/// </param>
internal sealed record ReviewReportResult(string Markdown, bool HasIssues);

/// <summary>
///     Represents the result of elaborating a review set.
/// </summary>
/// <param name="Markdown">The generated Markdown content.</param>
internal sealed record ElaborateResult(string Markdown);

/// <summary>
///     Severity level of a lint issue.
/// </summary>
internal enum LintSeverity
{
    /// <summary>Informational warning — does not prevent configuration use.</summary>
    Warning,

    /// <summary>Fatal error — prevents configuration use.</summary>
    Error
}

/// <summary>
///     A single lint issue detected when loading or validating a <c>.reviewmark.yaml</c> file.
/// </summary>
/// <param name="Location">
///     The file path (and optionally <c>:line:column</c>) where the issue was detected.
/// </param>
/// <param name="Severity">The severity of the issue.</param>
/// <param name="Description">A human-readable description of the issue.</param>
internal sealed record LintIssue(string Location, LintSeverity Severity, string Description)
{
    /// <inheritdoc />
    public override string ToString() =>
        $"{Location}: {Severity.ToString().ToLowerInvariant()}: {Description}";
}

/// <summary>
///     The result of <see cref="ReviewMarkConfiguration.Load" />.
/// </summary>
/// <param name="Configuration">
///     The loaded configuration, or <c>null</c> if any error-level lint issues were detected.
/// </param>
/// <param name="Issues">
///     All lint issues (errors and warnings) detected during loading. May be empty when the
///     file is valid.
/// </param>
internal sealed record ReviewMarkLoadResult(
    ReviewMarkConfiguration? Configuration,
    IReadOnlyList<LintIssue> Issues)
{
    /// <summary>
    ///     Reports all lint issues to the supplied <paramref name="context" />, routing errors
    ///     to <see cref="Context.WriteError" /> and warnings to <see cref="Context.WriteLine" />.
    /// </summary>
    /// <param name="context">The context to report issues to.</param>
    internal void ReportIssues(Context context)
    {
        foreach (var issue in Issues)
        {
            if (issue.Severity == LintSeverity.Error)
            {
                context.WriteError(issue.ToString());
            }
            else
            {
                context.WriteLine(issue.ToString());
            }
        }
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
    internal ReviewMarkConfiguration(
        IReadOnlyList<string> needsReviewPatterns,
        EvidenceSource evidenceSource,
        IReadOnlyList<ReviewSet> reviews)
    {
        NeedsReviewPatterns = needsReviewPatterns;
        EvidenceSource = evidenceSource;
        Reviews = reviews;
    }

    /// <summary>
    ///     Loads and lints a <c>.reviewmark.yaml</c> file, returning both the parsed
    ///     configuration and all detected issues in a single pass.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the configuration file.</param>
    /// <returns>
    ///     A <see cref="ReviewMarkLoadResult" /> containing the configuration (or <c>null</c> if
    ///     any error-level issues were detected) and the complete list of lint issues.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath" /> is null or empty.</exception>
    internal static ReviewMarkLoadResult Load(string filePath)
    {
        // Validate the file path argument
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        }

        var issues = new List<LintIssue>();

        // Try to read the file; if this fails we cannot continue.
        string yaml;
        try
        {
            yaml = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            issues.Add(new LintIssue(filePath, LintSeverity.Error, ex.Message));
            return new ReviewMarkLoadResult(null, issues.ToArray());
        }

        // Try to parse the raw YAML model; if this fails we cannot do semantic checks.
        // When the inner exception is a YamlException, format the location as "file:line:col"
        // to match the standard linting output convention.
        ReviewMarkYaml raw;
        try
        {
            raw = ReviewMarkConfigurationHelpers.DeserializeRaw(yaml, filePath);
        }
        catch (InvalidOperationException ex) when (ex.InnerException is YamlException yamlEx)
        {
            issues.Add(new LintIssue(
                $"{filePath}:{yamlEx.Start.Line}:{yamlEx.Start.Column}",
                LintSeverity.Error,
                $"at line {yamlEx.Start.Line}, column {yamlEx.Start.Column}: {yamlEx.Message}"));
            return new ReviewMarkLoadResult(null, issues.ToArray());
        }
        catch (InvalidOperationException ex)
        {
            issues.Add(new LintIssue(filePath, LintSeverity.Error, ex.Message));
            return new ReviewMarkLoadResult(null, issues.ToArray());
        }

        // Validate the evidence-source block, collecting all field-level errors.
        var es = raw.EvidenceSource;
        if (es == null)
        {
            issues.Add(new LintIssue(
                filePath,
                LintSeverity.Error,
                "Configuration is missing required 'evidence-source' block."));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(es.Type))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    "'evidence-source' is missing a required 'type' field."));
            }
            else if (!ReviewMarkConfigurationHelpers.IsSupportedEvidenceSourceType(es.Type))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"'evidence-source' type '{es.Type}' is not supported (must be 'none', 'url', or 'fileshare')."));
            }

            if (string.IsNullOrWhiteSpace(es.Location) && !string.Equals(es.Type, "none", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    "'evidence-source' is missing a required 'location' field."));
            }
        }

        // Validate each review set, accumulating all structural and uniqueness errors.
        // Review IDs are treated as case-sensitive identifiers (Ordinal), which is intentional:
        // "Core-Logic" and "core-logic" are distinct IDs. Evidence-source type uses OrdinalIgnoreCase
        // because YAML convention allows any casing for keyword values like "url" or "fileshare".
        var seenIds = new Dictionary<string, int>(StringComparer.Ordinal);
        var reviews = raw.Reviews ?? [];
        for (var i = 0; i < reviews.Count; i++)
        {
            var r = reviews[i];

            if (r is null)
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"Review set at index {i} is null (for example, from an empty '-' entry in 'reviews') and will be ignored."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(r.Id))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"Review set at index {i} is missing a required 'id' field."));
            }
            else if (seenIds.TryGetValue(r.Id, out var firstIndex))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"reviews[{i}] has duplicate ID '{r.Id}' (first defined at reviews[{firstIndex}])."));
            }
            else
            {
                seenIds[r.Id] = i;
            }

            if (string.IsNullOrWhiteSpace(r.Title))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"Review set at index {i} is missing a required 'title' field."));
            }

            if (r.Paths == null || !r.Paths.Any(p => !string.IsNullOrWhiteSpace(p)))
            {
                issues.Add(new LintIssue(
                    filePath,
                    LintSeverity.Error,
                    $"Review set at index {i} is missing required 'paths' entries."));
            }
        }

        // If any error-level issues were found, return null configuration
        if (issues.Any(i => i.Severity == LintSeverity.Error))
        {
            return new ReviewMarkLoadResult(null, issues.ToArray());
        }

        // Build configuration from the validated raw model
        var config = ReviewMarkConfigurationHelpers.BuildConfiguration(raw);

        // Determine the base directory for resolving relative fileshare locations.
        var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (baseDirectory == null)
        {
            issues.Add(new LintIssue(
                filePath,
                LintSeverity.Error,
                $"Cannot determine base directory for configuration file '{filePath}'."));
            return new ReviewMarkLoadResult(null, issues.ToArray());
        }

        // Resolve relative fileshare locations against the config file's directory so that
        // a relative location (e.g., "index.json") works correctly regardless of the process
        // working directory.
        if (string.Equals(config.EvidenceSource.Type, "fileshare", StringComparison.OrdinalIgnoreCase) &&
            !Path.IsPathRooted(config.EvidenceSource.Location))
        {
            var absoluteLocation = Path.GetFullPath(config.EvidenceSource.Location, baseDirectory);
            config = new ReviewMarkConfiguration(
                config.NeedsReviewPatterns,
                config.EvidenceSource with { Location = absoluteLocation },
                config.Reviews);
        }

        return new ReviewMarkLoadResult(config, issues.ToArray());
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

        // Deserialize without a file path so YAML errors are wrapped as ArgumentException (not
        // InvalidOperationException) which is what callers of Parse (unit tests) expect.
        var raw = ReviewMarkConfigurationHelpers.DeserializeRaw(yaml, filePath: null);

        return ReviewMarkConfigurationHelpers.BuildConfiguration(raw);
    }

    /// <summary>
    ///     Returns all files within <paramref name="directory" /> that match the
    ///     <see cref="NeedsReviewPatterns" /> glob patterns.
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <returns>A sorted list of relative file paths.</returns>
    internal IReadOnlyList<string> GetNeedsReviewFiles(string directory) =>
        GlobMatcher.GetMatchingFiles(directory, NeedsReviewPatterns);

    /// <summary>
    ///     Generates a Markdown "Review Coverage" section listing all review sets
    ///     and any uncovered files.
    /// </summary>
    /// <param name="directory">The root directory to search for files.</param>
    /// <param name="markdownDepth">
    ///     The heading depth for the section title (1 = <c>#</c>, 2 = <c>##</c>, etc.).
    /// </param>
    /// <returns>
    ///     A <see cref="ReviewPlanResult" /> containing the Markdown text and a flag
    ///     indicating whether any files requiring review are uncovered.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory" /> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="markdownDepth" /> is less than 1 or greater than 5
    ///     (subheadings at depth+1 would exceed the maximum Markdown heading level of 6).
    /// </exception>
    internal ReviewPlanResult PublishReviewPlan(string directory, int markdownDepth = 1)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(markdownDepth);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(markdownDepth, 5);

        // Build the section heading at the requested depth
        var sb = new StringBuilder();
        var heading = new string('#', markdownDepth);
        sb.AppendLine($"{heading} Review Coverage");
        sb.AppendLine();

        // Emit the review-set coverage table
        sb.AppendLine("| Review ID | Title | Files | Fingerprint |");
        sb.AppendLine("| :--- | :--- | ---: | :--- |");

        // Collect the set of all files covered by at least one review set
        var coveredFiles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var review in Reviews)
        {
            // Resolve matched files and compute the fingerprint for this review set
            var files = review.GetFiles(directory);
            var fingerprint = review.GetFingerprint(directory);

            // Abbreviate the fingerprint to first 8 characters followed by an ellipsis
            var abbreviatedFingerprint = $"`{fingerprint[..8]}\u2026`";

            // Emit the table row for this review set
            sb.AppendLine($"| {review.Id} | {review.Title} | {files.Count} | {abbreviatedFingerprint} |");

            // Track all files as covered
            foreach (var file in files)
            {
                coveredFiles.Add(file);
            }
        }

        sb.AppendLine();

        // Identify files that require review but are not covered by any review set
        var needsReviewFiles = GetNeedsReviewFiles(directory);
        var uncoveredFiles = needsReviewFiles
            .Where(f => !coveredFiles.Contains(f))
            .ToList();

        // Always emit a "Coverage" subsection reporting whether all files are covered
        var subHeading = new string('#', markdownDepth + 1);
        sb.AppendLine($"{subHeading} Coverage");
        sb.AppendLine();

        if (uncoveredFiles.Count == 0)
        {
            // All files requiring review are covered — state it positively
            sb.AppendLine("All files requiring review are covered by a review-set.");
        }
        else
        {
            // List uncovered files as a bullet list
            sb.AppendLine($"\u26a0 {uncoveredFiles.Count} file(s) require review but are not covered by any review-set:");
            foreach (var file in uncoveredFiles)
            {
                sb.AppendLine($"- `{file}`");
            }
        }

        sb.AppendLine();

        return new ReviewPlanResult(sb.ToString(), uncoveredFiles.Count > 0);
    }

    /// <summary>
    ///     Generates a Markdown "Review Status" section reporting the currency of
    ///     review evidence for every review set.
    /// </summary>
    /// <param name="index">The loaded review-evidence index to query.</param>
    /// <param name="directory">The root directory to search for files.</param>
    /// <param name="markdownDepth">
    ///     The heading depth for the section title (1 = <c>#</c>, 2 = <c>##</c>, etc.).
    /// </param>
    /// <returns>
    ///     A <see cref="ReviewReportResult" /> containing the Markdown text and a flag
    ///     indicating whether any reviews are failed, stale, or missing.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="index" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory" /> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="markdownDepth" /> is less than 1 or greater than 5
    ///     (subheadings at depth+1 would exceed the maximum Markdown heading level of 6).
    /// </exception>
    internal ReviewReportResult PublishReviewReport(ReviewIndex index, string directory, int markdownDepth = 1)
    {
        // Validate the required index argument
        ArgumentNullException.ThrowIfNull(index);

        // Validate remaining input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(markdownDepth);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(markdownDepth, 5);

        // Build the section heading at the requested depth
        var sb = new StringBuilder();
        var heading = new string('#', markdownDepth);
        sb.AppendLine($"{heading} Review Status");
        sb.AppendLine();

        // Emit the review-status table header (Evidence PDF filenames are listed separately below)
        sb.AppendLine("| Review ID | Status | Date | Result |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        // Track whether any reviews are failed, stale, or missing
        var hasIssues = false;

        // Collect referenced documents (review ID and file) while iterating
        var referencedDocuments = new List<(string Id, string File)>();

        foreach (var review in Reviews)
        {
            // Compute the current content fingerprint for this review set
            var fingerprint = review.GetFingerprint(directory);

            // Check if there is evidence with a matching fingerprint for this review set
            var currentEvidence = index.GetEvidence(review.Id, fingerprint);
            if (currentEvidence != null &&
                string.Equals(currentEvidence.Result, "pass", StringComparison.OrdinalIgnoreCase))
            {
                // Current: evidence exists with a matching fingerprint and a passing result
                sb.AppendLine(
                    $"| {review.Id} | \u2705 Current | {currentEvidence.Date} | {FormatResult(currentEvidence.Result)} |");
                referencedDocuments.Add((review.Id, currentEvidence.File));
            }
            else if (currentEvidence != null)
            {
                // Failed: evidence exists with a matching fingerprint but the result is not passing
                hasIssues = true;
                sb.AppendLine(
                    $"| {review.Id} | \u274c Failed | {currentEvidence.Date} | {FormatResult(currentEvidence.Result)} |");
                referencedDocuments.Add((review.Id, currentEvidence.File));
            }
            else if (index.HasId(review.Id))
            {
                // Stale: there is evidence for this review ID but none matches the current fingerprint
                hasIssues = true;

                // Pick the most recent evidence entry by sorting on Date descending (ISO 8601 dates sort lexicographically)
                var mostRecent = index.GetAllForId(review.Id)
                    .OrderByDescending(e => e.Date, StringComparer.Ordinal)
                    .First();

                sb.AppendLine(
                    $"| {review.Id} | \u26a0 Stale | {mostRecent.Date} | {FormatResult(mostRecent.Result)} |");
                referencedDocuments.Add((review.Id, mostRecent.File));
            }
            else
            {
                // Missing: no evidence at all for this review ID
                hasIssues = true;
                sb.AppendLine($"| {review.Id} | \u274c Missing | | |");
            }
        }

        sb.AppendLine();

        // Emit the referenced-documents subsection when any evidence was found
        if (referencedDocuments.Count > 0)
        {
            var subHeading = new string('#', markdownDepth + 1);
            sb.AppendLine($"{subHeading} Referenced Documents");
            sb.AppendLine();
            foreach (var (id, file) in referencedDocuments)
            {
                sb.AppendLine($"- {id}: {file}");
            }

            sb.AppendLine();
        }

        return new ReviewReportResult(sb.ToString(), hasIssues);
    }

    /// <summary>
    ///     Generates a Markdown elaboration of a specific review set, showing its ID,
    ///     fingerprint, and the full list of files to review.
    /// </summary>
    /// <param name="reviewSetId">The ID of the review set to elaborate.</param>
    /// <param name="directory">The root directory to search for files.</param>
    /// <param name="markdownDepth">
    ///     The heading depth for the section title (1 = <c>#</c>, 2 = <c>##</c>, etc.).
    /// </param>
    /// <returns>
    ///     An <see cref="ElaborateResult" /> containing the Markdown text.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="reviewSetId" /> is null or whitespace,
    ///     when <paramref name="directory" /> is null or whitespace,
    ///     or when no review set with the specified ID exists.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="markdownDepth" /> is less than 1 or greater than 5
    ///     (subheadings at depth+1 would exceed the maximum Markdown heading level of 6).
    /// </exception>
    internal ElaborateResult ElaborateReviewSet(string reviewSetId, string directory, int markdownDepth = 1)
    {
        // Validate input parameters
        ArgumentException.ThrowIfNullOrWhiteSpace(reviewSetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(markdownDepth);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(markdownDepth, 5);

        // Find the review set with the specified ID (case-sensitive match)
        var review = Reviews.FirstOrDefault(r =>
            string.Equals(r.Id, reviewSetId, StringComparison.Ordinal));
        if (review == null)
        {
            throw new ArgumentException($"No review set found with ID '{reviewSetId}'.");
        }

        // Build the section heading at the requested depth
        var sb = new StringBuilder();
        var heading = new string('#', markdownDepth);
        sb.AppendLine($"{heading} {review.Id}");
        sb.AppendLine();

        // Emit the review metadata table
        var fingerprint = review.GetFingerprint(directory);
        sb.AppendLine("| Field | Value |");
        sb.AppendLine("| :--- | :--- |");
        sb.AppendLine($"| ID | {review.Id} |");
        sb.AppendLine($"| Title | {review.Title} |");
        sb.AppendLine($"| Fingerprint | `{fingerprint}` |");
        sb.AppendLine();

        // Emit the files subsection
        var subHeading = new string('#', markdownDepth + 1);
        sb.AppendLine($"{subHeading} Files");
        sb.AppendLine();
        var files = review.GetFiles(directory);
        foreach (var file in files)
        {
            sb.AppendLine($"- `{file}`");
        }

        sb.AppendLine();

        return new ElaborateResult(sb.ToString());
    }

    /// <summary>
    ///     Formats a result string by capitalizing its first letter.
    /// </summary>
    /// <param name="result">The raw result string (e.g. <c>"pass"</c>).</param>
    /// <returns>
    ///     The result with its first character upper-cased (e.g. <c>"Pass"</c>),
    ///     or an empty string if <paramref name="result" /> is empty.
    /// </returns>
    private static string FormatResult(string result)
    {
        // Return empty string unchanged to avoid index-out-of-range on empty input
        if (result.Length == 0)
        {
            return result;
        }

        // Capitalize just the first character and append the remainder unchanged
        return char.ToUpperInvariant(result[0]) + result[1..];
    }
}
