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
using System.Text;
using PdfSharp.Pdf;
using DemaConsulting.ReviewMark.Configuration;
using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests.Indexing;

/// <summary>
///     Unit tests for the <see cref="ReviewIndex" /> class and <see cref="ReviewEvidence" /> record.
/// </summary>
[TestClass]
public class IndexTests
{
    /// <summary>
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private string _testDirectory = string.Empty;

    /// <summary>
    ///     Creates a fresh GUID-based temporary directory before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"IndexTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    ///     Deletes the temporary directory and all its contents after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Writes <paramref name="json" /> to a unique file inside the test temp directory
    ///     and loads it as a <see cref="ReviewIndex" /> via
    ///     <see cref="ReviewIndex.Load(EvidenceSource)" />.
    /// </summary>
    /// <param name="json">The JSON content for the index file.</param>
    /// <returns>The loaded <see cref="ReviewIndex" />.</returns>
    private ReviewIndex LoadIndexFromJson(string json)
    {
        var path = PathHelpers.SafePathCombine(_testDirectory, $"index-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return ReviewIndex.Load(new EvidenceSource("fileshare", path, null, null));
    }

    /// <summary>
    ///     Helper that builds a fake <see cref="HttpMessageHandler" /> returning a canned
    ///     response.  The handler takes ownership of the response and disposes it when
    ///     the handler itself is disposed.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        ///     The response that will be returned for every request.
        /// </summary>
        private readonly HttpResponseMessage _response;

        /// <summary>
        ///     Initializes a new instance of <see cref="FakeHttpMessageHandler" />.
        /// </summary>
        /// <param name="response">The pre-built response to return.</param>
        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        /// <inheritdoc />
        protected override HttpResponseMessage Send(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _response;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _response.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Empty" /> returns an index that reports no
    ///     evidence for any query, proving the factory method creates a truly empty index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Empty_ReturnsEmptyIndex()
    {
        // Act
        var index = ReviewIndex.Empty();

        // Assert — all query operations report empty/no results
        Assert.IsNotNull(index, "Empty() should return a non-null instance.");
        Assert.IsNull(index.GetEvidence("any-id", "any-fingerprint"),
            "GetEvidence should return null on an empty index.");
        Assert.IsFalse(index.HasId("any-id"),
            "HasId should return false on an empty index.");
        Assert.IsEmpty(index.GetAllForId("any-id"),
            "GetAllForId should return an empty list on an empty index.");
    }

    // -------------------------------------------------------------------------
    // Load tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that passing a null <see cref="EvidenceSource" /> to
    ///     <see cref="ReviewIndex.Load(EvidenceSource)" /> throws
    ///     <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_NullSource_ThrowsArgumentNullException()
    {
        // Arrange
        EvidenceSource? nullSource = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            ReviewIndex.Load(nullSource!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that passing an <see cref="EvidenceSource" /> with an unrecognized type to
    ///     <see cref="ReviewIndex.Load(EvidenceSource)" /> throws
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_UnknownType_ThrowsInvalidOperationException()
    {
        // Arrange — a source with an unsupported type value
        var source = new EvidenceSource(
            Type: "unknown-type",
            Location: "/some/path",
            UsernameEnv: null,
            PasswordEnv: null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ReviewIndex.Load(source));
    }

    /// <summary>
    ///     Test that loading a file with invalid JSON via a <c>fileshare</c>
    ///     <see cref="EvidenceSource" /> throws <see cref="InvalidOperationException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange — write non-JSON content to a temp file
        var path = PathHelpers.SafePathCombine(_testDirectory, "invalid.json");
        File.WriteAllText(path, "this is not json {{{");
        var source = new EvidenceSource("fileshare", path, null, null);

        // Act & Assert — invalid JSON content should cause an InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
            ReviewIndex.Load(source));
    }

    /// <summary>
    ///     Test that loading a file with an empty reviews array via a <c>fileshare</c>
    ///     <see cref="EvidenceSource" /> returns an empty index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_EmptyReviews_ReturnsEmptyIndex()
    {
        // Arrange — JSON with an empty reviews array
        const string json = """{"reviews":[]}""";

        // Act
        var index = LoadIndexFromJson(json);

        // Assert — empty index should report no evidence for any id
        Assert.IsNotNull(index);
        Assert.IsFalse(index.HasId("any-id"),
            "HasId should return false on an empty index.");
    }

    /// <summary>
    ///     Test that loading a valid JSON file with two entries via a <c>fileshare</c>
    ///     <see cref="EvidenceSource" /> returns a fully populated index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_ValidJson_ReturnsPopulatedIndex()
    {
        // Arrange — JSON containing two distinct review evidence entries
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "abc123",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "CR-2026-014 Core Logic Review.pdf"
                },
                {
                  "id": "UI-Layer",
                  "fingerprint": "def456",
                  "date": "2026-03-01",
                  "result": "pass",
                  "file": "CR-2026-021 UI Layer Review.pdf"
                }
              ]
            }
            """;

        // Act
        var index = LoadIndexFromJson(json);

        // Assert — both entries are retrievable by their respective ids and fingerprints
        var evidence1 = index.GetEvidence("Core-Logic", "abc123");
        Assert.IsNotNull(evidence1, "First entry should be findable.");
        Assert.AreEqual("Core-Logic", evidence1.Id);
        Assert.AreEqual("abc123", evidence1.Fingerprint);
        Assert.AreEqual("2026-02-14", evidence1.Date);
        Assert.AreEqual("pass", evidence1.Result);
        Assert.AreEqual("CR-2026-014 Core Logic Review.pdf", evidence1.File);

        var evidence2 = index.GetEvidence("UI-Layer", "def456");
        Assert.IsNotNull(evidence2, "Second entry should be findable.");
        Assert.AreEqual("UI-Layer", evidence2.Id);
        Assert.AreEqual("def456", evidence2.Fingerprint);
        Assert.AreEqual("2026-03-01", evidence2.Date);
        Assert.AreEqual("pass", evidence2.Result);
        Assert.AreEqual("CR-2026-021 UI Layer Review.pdf", evidence2.File);
    }

    /// <summary>
    ///     Test that entries missing required fields (id or fingerprint) are silently
    ///     skipped when loading via a <c>fileshare</c> <see cref="EvidenceSource" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_MissingRequiredFields_SkipsInvalidEntries()
    {
        // Arrange — JSON containing three entries:
        //   1. missing 'id'
        //   2. missing 'fingerprint'
        //   3. fully valid
        const string json = """
            {
              "reviews": [
                {
                  "fingerprint": "fp-no-id",
                  "date": "2026-01-01",
                  "result": "pass",
                  "file": "no-id.pdf"
                },
                {
                  "id": "No-Fingerprint",
                  "date": "2026-01-02",
                  "result": "pass",
                  "file": "no-fp.pdf"
                },
                {
                  "id": "Valid-Entry",
                  "fingerprint": "fp-valid",
                  "date": "2026-01-03",
                  "result": "pass",
                  "file": "valid.pdf"
                }
              ]
            }
            """;

        // Act
        var index = LoadIndexFromJson(json);

        // Assert — only the valid entry is present; the two incomplete entries are skipped
        var validEvidence = index.GetEvidence("Valid-Entry", "fp-valid");
        Assert.IsNotNull(validEvidence, "The valid entry should be present in the index.");

        Assert.IsFalse(index.HasId("No-Fingerprint"),
            "Entry missing 'fingerprint' should not appear in the index.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource)" /> with a <c>fileshare</c>
    ///     source loads the index from the path given in
    ///     <see cref="EvidenceSource.Location" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_LoadsFromFile()
    {
        // Arrange — write a valid index JSON file to the temp directory
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "abc123",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var indexPath = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexPath, json);

        var source = new EvidenceSource(
            Type: "fileshare",
            Location: indexPath,
            UsernameEnv: null,
            PasswordEnv: null);

        // Act
        var index = ReviewIndex.Load(source);

        // Assert — the entry written to disk is present in the loaded index
        var evidence = index.GetEvidence("Core-Logic", "abc123");
        Assert.IsNotNull(evidence, "Evidence loaded via fileshare source should be present.");
        Assert.AreEqual("Core-Logic", evidence.Id);
        Assert.AreEqual("abc123", evidence.Fingerprint);
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource)" /> with a <c>fileshare</c>
    ///     source pointing at a non-existent file throws
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Fileshare_NonExistentFile_ThrowsInvalidOperationException()
    {
        // Arrange — a path to a file that does not exist
        var missingPath = PathHelpers.SafePathCombine(_testDirectory, "missing-index.json");
        var source = new EvidenceSource(
            Type: "fileshare",
            Location: missingPath,
            UsernameEnv: null,
            PasswordEnv: null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ReviewIndex.Load(source));
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource, HttpClient)" /> with a
    ///     <c>url</c> source and a 200 OK response correctly deserializes the index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Url_SuccessResponse_LoadsIndex()
    {
        // Arrange — canned JSON served by a fake HTTP handler
        const string json = """
            {
              "reviews": [
                {
                  "id": "UI-Layer",
                  "fingerprint": "def456",
                  "date": "2026-03-01",
                  "result": "pass",
                  "file": "ui-review.pdf"
                }
              ]
            }
            """;

        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var handler = new FakeHttpMessageHandler(fakeResponse);
        using var httpClient = new HttpClient(handler);

        var source = new EvidenceSource(
            Type: "url",
            Location: "https://example.com/evidence/index.json",
            UsernameEnv: null,
            PasswordEnv: null);

        // Act
        var index = ReviewIndex.Load(source, httpClient);

        // Assert
        var evidence = index.GetEvidence("UI-Layer", "def456");
        Assert.IsNotNull(evidence, "Evidence returned via URL source should be present.");
        Assert.AreEqual("UI-Layer", evidence.Id);
        Assert.AreEqual("def456", evidence.Fingerprint);
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource, HttpClient)" /> with a
    ///     <c>url</c> source and a non-success HTTP status code throws
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Url_NotFoundResponse_ThrowsInvalidOperationException()
    {
        // Arrange — fake handler returns HTTP 404
        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        using var handler = new FakeHttpMessageHandler(fakeResponse);
        using var httpClient = new HttpClient(handler);

        var source = new EvidenceSource(
            Type: "url",
            Location: "https://example.com/evidence/index.json",
            UsernameEnv: null,
            PasswordEnv: null);

        // Act & Assert — a 404 should be reported as an InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
            ReviewIndex.Load(source, httpClient));
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource, HttpClient)" /> with a
    ///     <c>url</c> source returning invalid JSON throws
    ///     <see cref="InvalidOperationException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_Url_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange — fake handler returns HTTP 200 with malformed JSON
        var fakeResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json {{{", Encoding.UTF8, "application/json")
        };
        using var handler = new FakeHttpMessageHandler(fakeResponse);
        using var httpClient = new HttpClient(handler);

        var source = new EvidenceSource(
            Type: "url",
            Location: "https://example.com/evidence/index.json",
            UsernameEnv: null,
            PasswordEnv: null);

        // Act & Assert — malformed JSON should produce an InvalidOperationException
        Assert.Throws<InvalidOperationException>(() =>
            ReviewIndex.Load(source, httpClient));
    }

    /// <summary>
    ///     Test that passing a null <see cref="HttpClient" /> to
    ///     <see cref="ReviewIndex.Load(EvidenceSource, HttpClient)" /> throws
    ///     <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new EvidenceSource(
            Type: "url",
            Location: "https://example.com/evidence/index.json",
            UsernameEnv: null,
            PasswordEnv: null);
        HttpClient? nullClient = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            ReviewIndex.Load(source, nullClient!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource)" /> with a <c>none</c>
    ///     source returns an empty <see cref="ReviewIndex" /> without accessing any file
    ///     or network resource.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_None_ReturnsEmptyIndex()
    {
        // Arrange
        var source = new EvidenceSource(
            Type: "none",
            Location: string.Empty,
            UsernameEnv: null,
            PasswordEnv: null);

        // Act
        var index = ReviewIndex.Load(source);

        // Assert — a none source always returns an empty index
        Assert.IsNull(index.GetEvidence("any-id", "any-fingerprint"));
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Load(EvidenceSource, HttpClient)" /> with a <c>none</c>
    ///     source returns an empty <see cref="ReviewIndex" /> without making any HTTP request.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Load_EvidenceSource_None_HttpClientOverload_ReturnsEmptyIndex()
    {
        // Arrange — use a fake handler that fails if actually called
        using var handler = new FakeHttpMessageHandler(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
        using var httpClient = new HttpClient(handler);

        var source = new EvidenceSource(
            Type: "none",
            Location: string.Empty,
            UsernameEnv: null,
            PasswordEnv: null);

        // Act
        var index = ReviewIndex.Load(source, httpClient);

        // Assert — a none source always returns an empty index without touching the handler
        Assert.IsNull(index.GetEvidence("any-id", "any-fingerprint"));
    }

    // -------------------------------------------------------------------------
    // Save tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that passing a null stream to <see cref="ReviewIndex.Save(Stream)" />
    ///     throws <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Save_Stream_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        var index = ReviewIndex.Empty();
        Stream? nullStream = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            index.Save(nullStream!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that passing a null or empty path to <see cref="ReviewIndex.Save(string)" />
    ///     throws <see cref="ArgumentException" />.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Save_File_NullPath_ThrowsArgumentException()
    {
        // Arrange
        var index = ReviewIndex.Empty();
        var emptyPath = string.Empty;

        // Act & Assert — an empty path is invalid and should throw
        Assert.Throws<ArgumentException>(() =>
            index.Save(emptyPath));
    }

    /// <summary>
    ///     Test that saving an index to a <see cref="MemoryStream" /> and reloading it
    ///     produces an index with exactly the same entries.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Save_RoundTrip_PreservesAllEntries()
    {
        // Arrange — build an index from JSON with two entries
        const string json = """
            {
              "reviews": [
                {
                  "id": "Alpha",
                  "fingerprint": "fp-alpha",
                  "date": "2026-04-01",
                  "result": "pass",
                  "file": "alpha.pdf"
                },
                {
                  "id": "Beta",
                  "fingerprint": "fp-beta",
                  "date": "2026-04-02",
                  "result": "fail",
                  "file": "beta.pdf"
                }
              ]
            }
            """;
        var original = LoadIndexFromJson(json);

        // Act — save the index to a temp file, then reload it via an EvidenceSource
        var savedPath = PathHelpers.SafePathCombine(_testDirectory, "saved-index.json");
        original.Save(savedPath);
        var reloaded = ReviewIndex.Load(new EvidenceSource("fileshare", savedPath, null, null));

        // Assert — every original entry is present in the reloaded index with identical field values
        var alpha = reloaded.GetEvidence("Alpha", "fp-alpha");
        Assert.IsNotNull(alpha, "Alpha entry should survive the round-trip.");
        Assert.AreEqual("Alpha", alpha.Id);
        Assert.AreEqual("fp-alpha", alpha.Fingerprint);
        Assert.AreEqual("2026-04-01", alpha.Date);
        Assert.AreEqual("pass", alpha.Result);
        Assert.AreEqual("alpha.pdf", alpha.File);

        var beta = reloaded.GetEvidence("Beta", "fp-beta");
        Assert.IsNotNull(beta, "Beta entry should survive the round-trip.");
        Assert.AreEqual("Beta", beta.Id);
        Assert.AreEqual("fp-beta", beta.Fingerprint);
        Assert.AreEqual("2026-04-02", beta.Date);
        Assert.AreEqual("fail", beta.Result);
        Assert.AreEqual("beta.pdf", beta.File);
    }

    // -------------------------------------------------------------------------
    // Scan tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that scanning an empty directory with a PDF glob pattern returns an
    ///     empty index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_NoMatchingFiles_LeavesIndexEmpty()
    {
        // Act — scan with a PDF glob pattern; no files exist so nothing should be indexed
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"]);

        // Assert — the index should be empty when no PDFs are found
        Assert.IsFalse(index.HasId("any-id"),
            "Index should be empty when no PDFs are found.");
    }

    /// <summary>
    ///     Test that scanning a directory containing a PDF with valid Keywords metadata
    ///     populates the index with the corresponding evidence entry.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithValidMetadata_PopulatesIndex()
    {
        // Arrange — create a PDF with all required keywords
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "valid-review.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = "id=Core-Logic fingerprint=abc123 date=2026-03-08 result=pass";
            document.Save(pdfPath);
        }

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"]);

        // Assert — the entry is retrievable and all fields match the PDF keywords
        var evidence = index.GetEvidence("Core-Logic", "abc123");
        Assert.IsNotNull(evidence, "Evidence should be present after scanning a valid PDF.");
        Assert.AreEqual("Core-Logic", evidence.Id);
        Assert.AreEqual("abc123", evidence.Fingerprint);
        Assert.AreEqual("2026-03-08", evidence.Date);
        Assert.AreEqual("pass", evidence.Result);
        // GlobMatcher returns forward-slash paths; the File field reflects that
        Assert.AreEqual("valid-review.pdf", evidence.File);
    }

    /// <summary>
    ///     Test that a PDF with a fingerprint keyword but no id keyword is skipped
    ///     and the warning callback is invoked.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithMissingId_SkipsWithWarning()
    {
        // Arrange — create a PDF that has fingerprint but no id
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "missing-id.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = "fingerprint=abc123 date=2026-03-08 result=pass";
            document.Save(pdfPath);
        }

        var warnings = new List<string>();

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"], onWarning: msg => warnings.Add(msg));

        // Assert — the file is skipped and at least one warning is emitted; no entry in the index
        Assert.IsNotEmpty(warnings, "A warning should be emitted for a PDF missing 'id'.");
        Assert.IsFalse(index.HasId("any-id"),
            "No entry should be added when the 'id' keyword is missing.");
    }

    /// <summary>
    ///     Test that a PDF with an id keyword but no fingerprint keyword is skipped
    ///     and the warning callback is invoked.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithMissingFingerprint_SkipsWithWarning()
    {
        // Arrange — create a PDF that has id but no fingerprint
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "missing-fingerprint.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = "id=Core-Logic date=2026-03-08 result=pass";
            document.Save(pdfPath);
        }

        var warnings = new List<string>();

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"], onWarning: msg => warnings.Add(msg));

        // Assert — the file is skipped and at least one warning is emitted; no entry in the index
        Assert.IsNotEmpty(warnings, "A warning should be emitted for a PDF missing 'fingerprint'.");
        Assert.IsFalse(index.HasId("Core-Logic"),
            "No entry should be added when the 'fingerprint' keyword is missing.");
    }

    /// <summary>
    ///     Test that a PDF with no keywords at all is skipped and the warning callback
    ///     is invoked.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithNoKeywords_SkipsWithWarning()
    {
        // Arrange — create a PDF with an empty Keywords field
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "no-keywords.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = string.Empty;
            document.Save(pdfPath);
        }

        var warnings = new List<string>();

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"], onWarning: msg => warnings.Add(msg));

        // Assert — the file is skipped and at least one warning is emitted; index remains empty
        Assert.IsNotEmpty(warnings, "A warning should be emitted for a PDF with no keywords.");
        Assert.IsFalse(index.HasId("any-id"),
            "No entry should be added when a PDF has no keywords.");
    }

    /// <summary>
    ///     Test that a PDF with id and fingerprint but no date keyword is skipped
    ///     and the warning callback is invoked.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithMissingDate_SkipsWithWarning()
    {
        // Arrange — create a PDF that has id and fingerprint but no date
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "missing-date.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = "id=Core-Logic fingerprint=abc123 result=pass";
            document.Save(pdfPath);
        }

        var warnings = new List<string>();

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"], onWarning: msg => warnings.Add(msg));

        // Assert — the file is skipped and at least one warning is emitted; no entry in the index
        Assert.IsNotEmpty(warnings, "A warning should be emitted for a PDF missing 'date'.");
        Assert.IsFalse(index.HasId("Core-Logic"),
            "No entry should be added when the 'date' keyword is missing.");
    }

    /// <summary>
    ///     Test that a PDF with id, fingerprint, and date but no result keyword is skipped
    ///     and the warning callback is invoked.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_PdfWithMissingResult_SkipsWithWarning()
    {
        // Arrange — create a PDF that has id, fingerprint, and date but no result
        var pdfPath = PathHelpers.SafePathCombine(_testDirectory, "missing-result.pdf");
        using (var document = new PdfDocument())
        {
            document.AddPage();
            document.Info.Keywords = "id=Core-Logic fingerprint=abc123 date=2026-03-08";
            document.Save(pdfPath);
        }

        var warnings = new List<string>();

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"], onWarning: msg => warnings.Add(msg));

        // Assert — the file is skipped and at least one warning is emitted; no entry in the index
        Assert.IsNotEmpty(warnings, "A warning should be emitted for a PDF missing 'result'.");
        Assert.IsFalse(index.HasId("Core-Logic"),
            "No entry should be added when the 'result' keyword is missing.");
    }

    /// <summary>
    ///     Test that scanning a directory with two PDFs, each with distinct metadata,
    ///     populates the index with both entries.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_MultiplePdfs_PopulatesAllEntries()
    {
        // Arrange — create two PDFs with different ids and fingerprints
        var pdf1Path = PathHelpers.SafePathCombine(_testDirectory, "review-alpha.pdf");
        using (var doc1 = new PdfDocument())
        {
            doc1.AddPage();
            doc1.Info.Keywords = "id=Alpha fingerprint=fp-alpha date=2026-05-01 result=pass";
            doc1.Save(pdf1Path);
        }

        var pdf2Path = PathHelpers.SafePathCombine(_testDirectory, "review-beta.pdf");
        using (var doc2 = new PdfDocument())
        {
            doc2.AddPage();
            doc2.Info.Keywords = "id=Beta fingerprint=fp-beta date=2026-05-02 result=pass";
            doc2.Save(pdf2Path);
        }

        // Act
        var index = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"]);

        // Assert — both entries are present in the index
        var alpha = index.GetEvidence("Alpha", "fp-alpha");
        Assert.IsNotNull(alpha, "Alpha entry should be indexed after scanning.");
        Assert.AreEqual("Alpha", alpha.Id);

        var beta = index.GetEvidence("Beta", "fp-beta");
        Assert.IsNotNull(beta, "Beta entry should be indexed after scanning.");
        Assert.AreEqual("Beta", beta.Id);
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.Scan" /> always returns a fresh index
    ///     that does not include entries from any separately loaded index.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_Scan_ClearsExistingEntries()
    {
        // Arrange — load an index with a pre-existing entry
        const string json = """
            {
              "reviews": [
                {
                  "id": "Old-Entry",
                  "fingerprint": "fp-old",
                  "date": "2025-01-01",
                  "result": "pass",
                  "file": "old.pdf"
                }
              ]
            }
            """;
        var existingIndex = LoadIndexFromJson(json);
        Assert.IsTrue(existingIndex.HasId("Old-Entry"), "Pre-condition: Old-Entry should be present in the loaded index.");

        // Act — scan is a static factory; it always creates a fresh index independent of any prior index
        var scannedIndex = ReviewIndex.Scan(_testDirectory, ["**/*.pdf"]);

        // Assert — the scanned index does not contain entries from the separately loaded index
        Assert.IsFalse(scannedIndex.HasId("Old-Entry"),
            "Scan should return a fresh index containing only scanned PDFs.");
    }

    // -------------------------------------------------------------------------
    // Query tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that <see cref="ReviewIndex.GetEvidence" /> returns the correct
    ///     <see cref="ReviewEvidence" /> when the id and fingerprint both match an
    ///     existing entry.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_GetEvidence_ExistingEntry_ReturnsEvidence()
    {
        // Arrange
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "abc123",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act
        var evidence = index.GetEvidence("Core-Logic", "abc123");

        // Assert — the returned evidence has every field populated from the JSON
        Assert.IsNotNull(evidence, "Evidence should be found for a matching id and fingerprint.");
        Assert.AreEqual("Core-Logic", evidence.Id);
        Assert.AreEqual("abc123", evidence.Fingerprint);
        Assert.AreEqual("2026-02-14", evidence.Date);
        Assert.AreEqual("pass", evidence.Result);
        Assert.AreEqual("review.pdf", evidence.File);
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.GetEvidence" /> returns <c>null</c> when the
    ///     id exists in the index but the fingerprint does not match any entry for that id.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_GetEvidence_WrongFingerprint_ReturnsNull()
    {
        // Arrange
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "abc123",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act — use the correct id but a fingerprint that was never stored
        var evidence = index.GetEvidence("Core-Logic", "wrong-fingerprint");

        // Assert — no match means null is returned
        Assert.IsNull(evidence, "GetEvidence should return null when the fingerprint does not match.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.GetEvidence" /> returns <c>null</c> when the
    ///     given id is not present in the index at all.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_GetEvidence_UnknownId_ReturnsNull()
    {
        // Arrange — an index with one entry
        const string json = """
            {
              "reviews": [
                {
                  "id": "Known-Id",
                  "fingerprint": "fp-known",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act — query with an id that was never loaded
        var evidence = index.GetEvidence("Unknown-Id", "fp-known");

        // Assert — unknown id always returns null
        Assert.IsNull(evidence, "GetEvidence should return null for an unknown id.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.HasId" /> returns <c>true</c> when at least
    ///     one evidence entry exists for the given id.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_HasId_ExistingId_ReturnsTrue()
    {
        // Arrange
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "abc123",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act
        var result = index.HasId("Core-Logic");

        // Assert — the id was loaded so HasId must return true
        Assert.IsTrue(result, "HasId should return true for an id that exists in the index.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.HasId" /> returns <c>false</c> when no entry
    ///     exists for the given id.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_HasId_UnknownId_ReturnsFalse()
    {
        // Arrange — an index with one known entry
        const string json = """
            {
              "reviews": [
                {
                  "id": "Known-Id",
                  "fingerprint": "fp-known",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act
        var result = index.HasId("Unknown-Id");

        // Assert — the id was never loaded so HasId must return false
        Assert.IsFalse(result, "HasId should return false for an id that is not in the index.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.GetAllForId" /> returns all evidence entries
    ///     when the same id has multiple associated fingerprints.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_GetAllForId_ExistingId_ReturnsAllEntries()
    {
        // Arrange — two entries share the same id but have different fingerprints
        const string json = """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "fp-v1",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review-v1.pdf"
                },
                {
                  "id": "Core-Logic",
                  "fingerprint": "fp-v2",
                  "date": "2026-04-01",
                  "result": "pass",
                  "file": "review-v2.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act
        var entries = index.GetAllForId("Core-Logic");

        // Assert — both entries for "Core-Logic" are returned
        Assert.HasCount(2, entries,
            "GetAllForId should return exactly two entries for the id with two fingerprints.");

        var fingerprints = entries.Select(e => e.Fingerprint).ToHashSet();
        Assert.Contains("fp-v1", fingerprints, "fp-v1 entry should be included.");
        Assert.Contains("fp-v2", fingerprints, "fp-v2 entry should be included.");
    }

    /// <summary>
    ///     Test that <see cref="ReviewIndex.GetAllForId" /> returns an empty list when
    ///     no entries exist for the given id.
    /// </summary>
    [TestMethod]
    public void ReviewIndex_GetAllForId_UnknownId_ReturnsEmptyList()
    {
        // Arrange — an index with a single known entry
        const string json = """
            {
              "reviews": [
                {
                  "id": "Known-Id",
                  "fingerprint": "fp-known",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "review.pdf"
                }
              ]
            }
            """;
        var index = LoadIndexFromJson(json);

        // Act
        var entries = index.GetAllForId("Unknown-Id");

        // Assert — an unknown id produces an empty list, not null
        Assert.IsNotNull(entries, "GetAllForId should never return null.");
        Assert.IsEmpty(entries,
            "GetAllForId should return an empty list for an id that is not in the index.");
    }
}
