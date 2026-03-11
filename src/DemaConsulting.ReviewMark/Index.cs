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

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace DemaConsulting.ReviewMark;

// ---------------------------------------------------------------------------
// Internal JSON deserialization models
// These file-local classes mirror the raw JSON structure and are not part of
// the public API. System.Text.Json populates them via JsonPropertyName attributes.
// ---------------------------------------------------------------------------

/// <summary>
///     Raw JSON deserialization model for the top-level index document.
/// </summary>
file sealed class ReviewIndexJson
{
    /// <summary>
    ///     Gets or sets the list of review evidence entries.
    /// </summary>
    [JsonPropertyName("reviews")]
    public List<ReviewEvidenceJson>? Reviews { get; set; }
}

/// <summary>
///     Raw JSON deserialization model for a single review evidence entry.
/// </summary>
file sealed class ReviewEvidenceJson
{
    /// <summary>
    ///     Gets or sets the review identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    ///     Gets or sets the content fingerprint of the reviewed files.
    /// </summary>
    [JsonPropertyName("fingerprint")]
    public string? Fingerprint { get; set; }

    /// <summary>
    ///     Gets or sets the date of the review (e.g. <c>2026-02-14</c>).
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    /// <summary>
    ///     Gets or sets the result of the review (e.g. <c>pass</c> or <c>fail</c>).
    /// </summary>
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    /// <summary>
    ///     Gets or sets the file name of the review evidence PDF.
    /// </summary>
    [JsonPropertyName("file")]
    public string? File { get; set; }
}

// ---------------------------------------------------------------------------
// Public API — internal to the assembly
// ---------------------------------------------------------------------------

/// <summary>
///     Represents a single piece of review evidence linking a review ID and
///     content fingerprint to a dated PDF review record.
/// </summary>
/// <param name="Id">The review-set identifier.</param>
/// <param name="Fingerprint">The content fingerprint of the reviewed files.</param>
/// <param name="Date">The date of the review (e.g. <c>2026-02-14</c>).</param>
/// <param name="Result">The review outcome (e.g. <c>pass</c> or <c>fail</c>).</param>
/// <param name="File">The file name of the review evidence PDF.</param>
internal sealed record ReviewEvidence(
    string Id,
    string Fingerprint,
    string Date,
    string Result,
    string File);

/// <summary>
///     Represents the loaded review-evidence index, keyed by review ID and
///     content fingerprint. Supports loading from and saving to the
///     <c>index.json</c> file, and rebuilding the index from scanned PDF evidence.
/// </summary>
internal sealed class ReviewIndex
{
    // ---------------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Shared JSON serializer options used for both reading and writing
    ///     the index file. Case-insensitive reading allows for flexible JSON
    ///     produced by other tools; indented writing keeps the file readable.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    ///     Maps review IDs to a nested dictionary of fingerprint →
    ///     <see cref="ReviewEvidence" />. This two-level structure allows O(1)
    ///     look-up by both ID and fingerprint.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, ReviewEvidence>> _byId = [];

    // ---------------------------------------------------------------------------
    // Construction
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Private constructor. Use the static factory methods to obtain an
    ///     instance.
    /// </summary>
    private ReviewIndex()
    {
    }

    // ---------------------------------------------------------------------------
    // Static factory methods
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Creates an empty <see cref="ReviewIndex" /> with no entries.
    /// </summary>
    /// <returns>A new, empty <see cref="ReviewIndex" />.</returns>
    internal static ReviewIndex Empty() => new();

    /// <summary>
    ///     Loads a <see cref="ReviewIndex" /> from an <see cref="EvidenceSource" />.
    ///     For <c>fileshare</c> sources the <see cref="EvidenceSource.Location" /> is treated as the
    ///     path to the <c>index.json</c> file. For <c>url</c> sources the location is the base HTTP(S)
    ///     URL (the directory containing <c>index.json</c>); <c>index.json</c> is appended automatically.
    ///     An <see cref="HttpClient" /> with optional pre-emptive Basic-auth credentials read from the
    ///     environment variables named by <see cref="EvidenceSource.UsernameEnv" /> and
    ///     <see cref="EvidenceSource.PasswordEnv" /> is created internally.
    /// </summary>
    /// <param name="evidenceSource">The evidence source to load the index from.</param>
    /// <returns>A populated <see cref="ReviewIndex" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="evidenceSource" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the index cannot be loaded from the evidence source.
    /// </exception>
    internal static ReviewIndex Load(EvidenceSource evidenceSource)
    {
        ArgumentNullException.ThrowIfNull(evidenceSource);

        // Short-circuit for fileshare sources — no HttpClient needed
        if (evidenceSource.Type.Equals("fileshare", StringComparison.OrdinalIgnoreCase))
        {
            return LoadFromFile(evidenceSource.Location);
        }

        // For all other types, create a configured HttpClient and delegate to the testable overload
        using var httpClient = CreateHttpClient(evidenceSource);
        return Load(evidenceSource, httpClient);
    }

    /// <summary>
    ///     Loads a <see cref="ReviewIndex" /> from an <see cref="EvidenceSource" /> using the
    ///     specified <see cref="HttpClient" />. This overload is exposed internally to allow
    ///     unit tests to inject a fake <see cref="HttpMessageHandler" /> when testing URL-based
    ///     evidence sources. For <c>url</c> sources the location is the base HTTP(S) URL (the
    ///     directory containing <c>index.json</c>); <c>index.json</c> is appended automatically.
    /// </summary>
    /// <param name="evidenceSource">The evidence source to load the index from.</param>
    /// <param name="httpClient">The HTTP client to use for <c>url</c>-type evidence sources.</param>
    /// <returns>A populated <see cref="ReviewIndex" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="evidenceSource" /> or <paramref name="httpClient" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the evidence-source type is unsupported or the index cannot be loaded.
    /// </exception>
    internal static ReviewIndex Load(EvidenceSource evidenceSource, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(evidenceSource);
        ArgumentNullException.ThrowIfNull(httpClient);

        // Dispatch to the appropriate loader based on the evidence-source type
        return evidenceSource.Type.ToLowerInvariant() switch
        {
            "fileshare" => LoadFromFile(evidenceSource.Location),
            "url" => LoadFromUrl(GetIndexUrl(evidenceSource.Location), httpClient),
            _ => throw new InvalidOperationException(
                $"Unsupported evidence source type '{evidenceSource.Type}'.")
        };
    }

    // ---------------------------------------------------------------------------
    // Private helpers — load implementation
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Loads a <see cref="ReviewIndex" /> from a JSON file on disk.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the <c>index.json</c> file.</param>
    /// <returns>A populated <see cref="ReviewIndex" /> instance.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="filePath" /> is <c>null</c>, empty, or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <paramref name="filePath" /> cannot be read or the
    ///     JSON content is invalid.
    /// </exception>
    private static ReviewIndex LoadFromFile(string filePath)
    {
        // Validate the path argument
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        }

        // Open the file and delegate to the stream overload for deserialization
        try
        {
            using var stream = File.OpenRead(filePath);
            return LoadFromStream(stream);
        }
        catch (InvalidOperationException)
        {
            // Re-throw exceptions that already carry context
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to read review index file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Loads a <see cref="ReviewIndex" /> from a JSON <see cref="Stream" />.
    /// </summary>
    /// <param name="stream">The stream containing the JSON index document.</param>
    /// <returns>A populated <see cref="ReviewIndex" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="stream" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the stream does not contain valid JSON.
    /// </exception>
    private static ReviewIndex LoadFromStream(Stream stream)
    {
        // Validate the stream argument
        ArgumentNullException.ThrowIfNull(stream);

        // Deserialize the JSON document from the stream
        ReviewIndexJson? raw;
        try
        {
            raw = JsonSerializer.Deserialize<ReviewIndexJson>(stream, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON content in review index: {ex.Message}", nameof(stream), ex);
        }

        // Build and populate a new index from the deserialized model
        var index = new ReviewIndex();
        foreach (var entry in raw?.Reviews ?? [])
        {
            // Skip entries missing required fields
            if (string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.Fingerprint))
            {
                continue;
            }

            // Create and store the evidence record
            var evidence = new ReviewEvidence(
                Id: entry.Id,
                Fingerprint: entry.Fingerprint,
                Date: entry.Date ?? string.Empty,
                Result: entry.Result ?? string.Empty,
                File: entry.File ?? string.Empty);

            // Insert into the two-level dictionary
            if (!index._byId.TryGetValue(evidence.Id, out var byFingerprint))
            {
                byFingerprint = [];
                index._byId[evidence.Id] = byFingerprint;
            }

            byFingerprint[evidence.Fingerprint] = evidence;
        }

        return index;
    }

    /// <summary>
    ///     Computes the full <c>index.json</c> URL from a base URL, appending
    ///     <c>/index.json</c> when necessary.  If the base URL already ends with
    ///     <c>/index.json</c> (case-insensitive) it is returned unchanged, preserving
    ///     backward compatibility for callers that still include the filename.
    /// </summary>
    /// <param name="baseUrl">
    ///     The base HTTP(S) URL of the evidence-source directory, with or without
    ///     a trailing slash and with or without an existing <c>index.json</c> suffix.
    /// </param>
    /// <returns>The full URL pointing directly at the <c>index.json</c> file.</returns>
    private static string GetIndexUrl(string baseUrl)
    {
        // If the URL already ends with /index.json, use it as-is for backward compatibility
        if (baseUrl.EndsWith("/index.json", StringComparison.OrdinalIgnoreCase))
        {
            return baseUrl;
        }

        // Append /index.json with proper trailing-slash handling
        return baseUrl.TrimEnd('/') + "/index.json";
    }

    /// <summary>
    ///     Creates an <see cref="HttpClient" /> configured for the given
    ///     <see cref="EvidenceSource" />. If the source specifies credential environment-variable
    ///     names and those variables are set, a pre-emptive Basic auth header is applied directly
    ///     to <see cref="HttpClient.DefaultRequestHeaders" />.
    /// </summary>
    /// <param name="evidenceSource">The evidence source configuration.</param>
    /// <returns>A configured <see cref="HttpClient" /> instance.</returns>
    private static HttpClient CreateHttpClient(EvidenceSource evidenceSource)
    {
        var client = new HttpClient();

        // Look up credentials from environment variables if names were specified
        var username = evidenceSource.UsernameEnv != null
            ? Environment.GetEnvironmentVariable(evidenceSource.UsernameEnv)
            : null;
        var password = evidenceSource.PasswordEnv != null
            ? Environment.GetEnvironmentVariable(evidenceSource.PasswordEnv)
            : null;

        // Apply pre-emptive Basic auth header when both username and password are available
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", encoded);
        }

        return client;
    }

    /// <summary>
    ///     Downloads the index JSON document from <paramref name="url" /> using
    ///     <paramref name="httpClient" /> and deserializes it into a
    ///     <see cref="ReviewIndex" />.
    /// </summary>
    /// <param name="url">
    ///     The full URL of the <c>index.json</c> file (after <see cref="GetIndexUrl" /> has been
    ///     applied to the base location from the evidence source).
    /// </param>
    /// <param name="httpClient">The HTTP client to use for the request.</param>
    /// <returns>A populated <see cref="ReviewIndex" /> instance.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the HTTP request fails or the response content is not valid JSON.
    /// </exception>
    private static ReviewIndex LoadFromUrl(string url, HttpClient httpClient)
    {
        try
        {
            // Send a synchronous GET request to the index URL
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = httpClient.Send(request);

            // Treat any non-success HTTP status as a load failure
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to download review index from '{url}': HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            // Deserialize the response body; wrap any JSON parse error with URL context
            using var stream = response.Content.ReadAsStream();
            try
            {
                return LoadFromStream(stream);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to parse review index downloaded from '{url}': {ex.Message}", ex);
            }
        }
        catch (InvalidOperationException)
        {
            // Re-throw exceptions that already carry context
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to download review index from '{url}': {ex.Message}", ex);
        }
    }

    // ---------------------------------------------------------------------------
    // Instance methods — persistence
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Saves the index to a JSON file on disk.
    /// </summary>
    /// <param name="filePath">Absolute or relative path where the file should be written.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="filePath" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the file cannot be written.
    /// </exception>
    internal void Save(string filePath)
    {
        // Validate the path argument
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        }

        // Open the output file and delegate to the stream overload
        try
        {
            using var stream = File.Create(filePath);
            Save(stream);
        }
        catch (ArgumentException ex) when (ex.ParamName == nameof(filePath))
        {
            // Re-throw our own path-validation exception as-is
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to write review index file '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Saves the index to a JSON <see cref="Stream" />.
    /// </summary>
    /// <param name="stream">The stream to write the JSON index document to.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="stream" /> is <c>null</c>.
    /// </exception>
    internal void Save(Stream stream)
    {
        // Validate the stream argument
        ArgumentNullException.ThrowIfNull(stream);

        // Flatten the nested dictionary into a list of JSON model objects
        var reviews = _byId.Values
            .SelectMany(byFingerprint => byFingerprint.Values)
            .Select(e => new ReviewEvidenceJson
            {
                Id = e.Id,
                Fingerprint = e.Fingerprint,
                Date = e.Date,
                Result = e.Result,
                File = e.File
            })
            .ToList();

        // Serialize the JSON model to the stream
        var json = new ReviewIndexJson { Reviews = reviews };
        JsonSerializer.Serialize(stream, json, JsonOptions);
    }

    // ---------------------------------------------------------------------------
    // Static factory methods — scanning
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Creates a new <see cref="ReviewIndex" /> by scanning PDF files in
    ///     <paramref name="directory" /> that match the given glob
    ///     <paramref name="paths" />.
    /// </summary>
    /// <param name="directory">The root directory to search.</param>
    /// <param name="paths">Ordered include/exclude glob patterns.</param>
    /// <param name="onWarning">
    ///     Optional callback invoked with a human-readable message whenever a PDF
    ///     is skipped (missing required metadata or unreadable).
    /// </param>
    /// <returns>A new <see cref="ReviewIndex" /> populated from the scanned PDFs.</returns>
    internal static ReviewIndex Scan(string directory, IReadOnlyList<string> paths, Action<string>? onWarning = null)
    {
        // Create a fresh index to populate
        var index = new ReviewIndex();

        // Resolve the set of PDF files that match the caller's glob patterns
        var matchedFiles = GlobMatcher.GetMatchingFiles(directory, paths);

        // Process each matched file individually, tolerating per-file errors
        foreach (var relativePath in matchedFiles)
        {
            // Safely combine the base directory with the relative path
            var fullPath = PathHelpers.SafePathCombine(directory, relativePath);

            // Attempt to open and parse each PDF, capturing per-file failures as warnings
            try
            {
                index.ProcessPdf(fullPath, relativePath, onWarning);
            }
            catch (Exception ex)
            {
                onWarning?.Invoke(
                    $"Skipping '{relativePath}': failed to process PDF — {ex.Message}");
            }
        }

        return index;
    }

    /// <summary>
    ///     Opens a single PDF file, reads its Keywords metadata, and adds a
    ///     <see cref="ReviewEvidence" /> entry if all required fields are present.
    ///     All four fields — <c>id</c>, <c>fingerprint</c>, <c>date</c>, and <c>result</c> —
    ///     must be non-empty; any PDF missing one or more is skipped with a warning.
    /// </summary>
    /// <param name="fullPath">The absolute file-system path to the PDF.</param>
    /// <param name="relativePath">
    ///     The relative path used as the <see cref="ReviewEvidence.File" /> value
    ///     and in any warning messages.
    /// </param>
    /// <param name="onWarning">Optional warning callback for skipped entries.</param>
    private void ProcessPdf(string fullPath, string relativePath, Action<string>? onWarning)
    {
        // Open the PDF in import mode (read-only, no modification)
        using var doc = PdfReader.Open(fullPath, PdfDocumentOpenMode.Import);

        // Extract the Keywords metadata field; treat missing as an empty string
        var keywords = doc.Info.Keywords ?? string.Empty;

        // Parse the space-separated name=value pairs into a dictionary
        var pairs = ParseKeywordPairs(keywords);

        // All four keys are required; skip with a warning if any are absent or empty
        if (!pairs.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
        {
            onWarning?.Invoke($"Skipping '{relativePath}': PDF Keywords missing required 'id' field.");
            return;
        }

        if (!pairs.TryGetValue("fingerprint", out var fingerprint) || string.IsNullOrWhiteSpace(fingerprint))
        {
            onWarning?.Invoke($"Skipping '{relativePath}': PDF Keywords missing required 'fingerprint' field.");
            return;
        }

        if (!pairs.TryGetValue("date", out var date) || string.IsNullOrWhiteSpace(date))
        {
            onWarning?.Invoke($"Skipping '{relativePath}': PDF Keywords missing required 'date' field.");
            return;
        }

        if (!pairs.TryGetValue("result", out var result) || string.IsNullOrWhiteSpace(result))
        {
            onWarning?.Invoke($"Skipping '{relativePath}': PDF Keywords missing required 'result' field.");
            return;
        }

        // Build the evidence record from the parsed metadata
        var evidence = new ReviewEvidence(
            Id: id,
            Fingerprint: fingerprint,
            Date: date,
            Result: result,
            File: relativePath);

        // Store the evidence at [id][fingerprint], overwriting any previous entry
        if (!_byId.TryGetValue(evidence.Id, out var byFingerprint))
        {
            byFingerprint = [];
            _byId[evidence.Id] = byFingerprint;
        }

        byFingerprint[evidence.Fingerprint] = evidence;
    }

    /// <summary>
    ///     Parses a string of space-separated <c>name=value</c> pairs into a
    ///     case-insensitive dictionary.
    /// </summary>
    /// <param name="keywords">The raw keywords string from a PDF document.</param>
    /// <returns>
    ///     A case-insensitive dictionary mapping each key to its value.
    ///     Tokens that do not contain <c>=</c> are ignored.
    /// </returns>
    private static Dictionary<string, string> ParseKeywordPairs(string keywords)
    {
        // Split the keywords string on whitespace and parse each name=value token
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in keywords.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            // Only process tokens that contain an '=' separator with a non-empty key.
            // separatorIndex == -1 means no '=' was found; separatorIndex == 0 means the
            // key portion is empty (e.g. '=value'), which is not a valid pair — both are skipped.
            var separatorIndex = token.IndexOf('=', StringComparison.Ordinal);
            if (separatorIndex <= 0)
            {
                continue;
            }

            // Extract the name and value from the token
            var name = token[..separatorIndex].Trim();
            var value = token[(separatorIndex + 1)..].Trim();

            if (!string.IsNullOrEmpty(name))
            {
                result[name] = value;
            }
        }

        return result;
    }

    // ---------------------------------------------------------------------------
    // Instance methods — querying
    // ---------------------------------------------------------------------------

    /// <summary>
    ///     Retrieves the <see cref="ReviewEvidence" /> for the given review ID and
    ///     content fingerprint.
    /// </summary>
    /// <param name="id">The review-set identifier.</param>
    /// <param name="fingerprint">The content fingerprint.</param>
    /// <returns>
    ///     The matching <see cref="ReviewEvidence" />, or <c>null</c> if no entry
    ///     exists for this combination.
    /// </returns>
    internal ReviewEvidence? GetEvidence(string id, string fingerprint)
    {
        // Return null if the id is unknown
        if (!_byId.TryGetValue(id, out var byFingerprint))
        {
            return null;
        }

        // Return null if the fingerprint is unknown for this id
        byFingerprint.TryGetValue(fingerprint, out var evidence);
        return evidence;
    }

    /// <summary>
    ///     Determines whether any evidence has been recorded for the given review ID,
    ///     regardless of fingerprint.
    /// </summary>
    /// <param name="id">The review-set identifier.</param>
    /// <returns>
    ///     <c>true</c> if at least one evidence entry exists for <paramref name="id" />;
    ///     otherwise <c>false</c>.
    /// </returns>
    internal bool HasId(string id) =>
        _byId.TryGetValue(id, out var byFingerprint) && byFingerprint.Count > 0;

    /// <summary>
    ///     Returns all evidence entries recorded for the given review ID.
    /// </summary>
    /// <param name="id">The review-set identifier.</param>
    /// <returns>
    ///     A read-only list of all <see cref="ReviewEvidence" /> entries for
    ///     <paramref name="id" />, or an empty list if none exist.
    /// </returns>
    internal IReadOnlyList<ReviewEvidence> GetAllForId(string id)
    {
        // Return an empty list when there are no entries for this id
        if (!_byId.TryGetValue(id, out var byFingerprint))
        {
            return [];
        }

        return [.. byFingerprint.Values];
    }
}
