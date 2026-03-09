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
    private readonly Dictionary<string, Dictionary<string, ReviewEvidence>> _byId = new();

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
    internal static ReviewIndex Load(string filePath)
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
            return Load(stream);
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
    internal static ReviewIndex Load(Stream stream)
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
                byFingerprint = new Dictionary<string, ReviewEvidence>();
                index._byId[evidence.Id] = byFingerprint;
            }

            byFingerprint[evidence.Fingerprint] = evidence;
        }

        return index;
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
        catch (ArgumentException)
        {
            // Re-throw validation exceptions as-is
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
    ///     <see cref="ReviewEvidence" /> entry if the required fields are present.
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

        // The 'id' and 'fingerprint' keys are required; skip with a warning if absent
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

        // Build the evidence record from the parsed metadata
        pairs.TryGetValue("date", out var date);
        pairs.TryGetValue("result", out var result);

        var evidence = new ReviewEvidence(
            Id: id,
            Fingerprint: fingerprint,
            Date: date ?? string.Empty,
            Result: result ?? string.Empty,
            File: relativePath);

        // Store the evidence at [id][fingerprint], overwriting any previous entry
        if (!_byId.TryGetValue(evidence.Id, out var byFingerprint))
        {
            byFingerprint = new Dictionary<string, ReviewEvidence>();
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
