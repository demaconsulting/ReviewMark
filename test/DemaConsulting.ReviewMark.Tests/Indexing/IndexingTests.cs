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

using System.Net;
using System.Net.Http;
using System.Text;
using DemaConsulting.ReviewMark.Configuration;
using DemaConsulting.ReviewMark.Indexing;
using DemaConsulting.ReviewMark.Tests;

namespace DemaConsulting.ReviewMark.Tests.Indexing;

/// <summary>
///     Subsystem integration tests for the Indexing subsystem
///     (ReviewIndex + PathHelpers working together).
/// </summary>
public sealed class IndexingTests : IDisposable
{
    /// <summary>
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of <see cref="IndexingTests" />.
    /// </summary>
    public IndexingTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(
            Path.GetTempPath(),
            $"IndexingTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Test that SafePathCombine with a subdirectory segment resolves to a valid index path
    ///     that can be loaded by ReviewIndex.
    /// </summary>
    [Fact]
    public void Indexing_SafePathCombine_WithIndexPath_LoadsIndex()
    {
        // Arrange
        var evidenceDir = PathHelpers.SafePathCombine(_testDirectory, "evidence");
        Directory.CreateDirectory(evidenceDir);

        var indexFile = PathHelpers.SafePathCombine(evidenceDir, "index.json");
        File.WriteAllText(indexFile, """
            {
              "reviews": [
                {
                  "id": "Test-Review",
                  "fingerprint": "abc123",
                  "date": "2024-01-01",
                  "result": "pass",
                  "file": "test.pdf"
                }
              ]
            }
            """);

        var combinedPath = PathHelpers.SafePathCombine(_testDirectory, "evidence/index.json");
        var source = new EvidenceSource("fileshare", combinedPath, null, null);

        // Act
        var index = ReviewIndex.Load(source);

        // Assert
        Assert.True(index.HasId("Test-Review"));
        var evidence = index.GetEvidence("Test-Review", "abc123");
        Assert.NotNull(evidence);
    }

    /// <summary>
    ///     Test that a ReviewIndex can be saved and reloaded with all entries preserved.
    /// </summary>
    [Fact]
    public void Indexing_ReviewIndex_SaveAndLoad_RoundTrip()
    {
        // Arrange
        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """
            {
              "reviews": [
                {
                  "id": "Review-Alpha",
                  "fingerprint": "fp001",
                  "date": "2024-06-01",
                  "result": "pass",
                  "file": "alpha.pdf"
                },
                {
                  "id": "Review-Beta",
                  "fingerprint": "fp002",
                  "date": "2024-06-02",
                  "result": "pass",
                  "file": "beta.pdf"
                }
              ]
            }
            """);

        var source = new EvidenceSource("fileshare", indexFile, null, null);

        // Act — load, save to a new file, then reload
        var index1 = ReviewIndex.Load(source);
        var savedFile = PathHelpers.SafePathCombine(_testDirectory, "index-copy.json");
        index1.Save(savedFile);

        var source2 = new EvidenceSource("fileshare", savedFile, null, null);
        var index2 = ReviewIndex.Load(source2);

        // Assert — all entries survive the round-trip
        Assert.True(index2.HasId("Review-Alpha"));
        Assert.True(index2.HasId("Review-Beta"));
        Assert.NotNull(index2.GetEvidence("Review-Alpha", "fp001"));
        Assert.NotNull(index2.GetEvidence("Review-Beta", "fp002"));
    }

    /// <summary>
    ///     Test that Load with a none-type EvidenceSource returns an empty index immediately.
    /// </summary>
    [Fact]
    public void Indexing_ReviewIndex_Load_WithNoneSource_ReturnsEmptyIndex()
    {
        // Arrange
        var source = new EvidenceSource("none", string.Empty, null, null);

        // Act
        var index = ReviewIndex.Load(source);

        // Assert — none source always produces an empty index; no file system access occurs
        Assert.False(index.HasId("any-id"));
    }

    /// <summary>
    ///     Test that Load with a url-type EvidenceSource and a fake HttpClient returns a populated index.
    /// </summary>
    [Fact]
    public void Indexing_ReviewIndex_Load_WithUrlSource_ReturnsPopulatedIndex()
    {
        // Arrange — build a fake handler that returns a fixed JSON index payload
        const string indexJson = """
            {
              "reviews": [
                {
                  "id": "Url-Review",
                  "fingerprint": "fp-url-001",
                  "date": "2026-01-15",
                  "result": "pass",
                  "file": "url-evidence.pdf"
                }
              ]
            }
            """;

        var source = new EvidenceSource("url", "https://example.com/index.json", null, null);
        using var handler = new FakeHttpMessageHandler(indexJson);
        using var httpClient = new HttpClient(handler);

        // Act
        var index = ReviewIndex.Load(source, httpClient);

        // Assert — the entry from the JSON payload is present in the loaded index
        Assert.True(index.HasId("Url-Review"));
        var evidence = index.GetEvidence("Url-Review", "fp-url-001");
        Assert.NotNull(evidence);
        Assert.Equal("Url-Review", evidence.Id);
        Assert.Equal("fp-url-001", evidence.Fingerprint);
    }

    /// <summary>
    ///     Test that SafePathCombine throws for path traversal inputs, preventing directory escapes.
    /// </summary>
    [Fact]
    public void Indexing_SafePathCombine_WithTraversalInputs_Throws()
    {
        // Arrange
        var evidenceDir = PathHelpers.SafePathCombine(_testDirectory, "evidence");
        Directory.CreateDirectory(evidenceDir);

        // Act & Assert — double-dot traversal must be rejected
        Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(evidenceDir, "../../../etc/sensitive"));

        // Act & Assert — absolute path must be rejected
        Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(evidenceDir, Path.GetTempPath()));
    }

    /// <summary>
    ///     Test that Scan with no PDF files in the target directory returns an empty index.
    /// </summary>
    [Fact]
    public void Indexing_ReviewIndex_Scan_WithNoPdfs_ReturnsEmptyIndex()
    {
        // Arrange — create a directory with no PDF files
        var evidenceDir = PathHelpers.SafePathCombine(_testDirectory, "evidence");
        Directory.CreateDirectory(evidenceDir);
        File.WriteAllText(PathHelpers.SafePathCombine(evidenceDir, "notes.txt"), "not a pdf");

        // Act — scan for PDFs; no matches expected
        var index = ReviewIndex.Scan(_testDirectory, ["evidence/**/*.pdf"]);

        // Assert — index is empty because no PDFs are present
        Assert.False(index.HasId("any-id"));
    }

    /// <summary>
    ///     Test that Scan with a PDF containing valid Keywords metadata returns a populated index.
    /// </summary>
    [Fact]
    public void Indexing_ReviewIndex_Scan_WithValidPdf_ReturnsPopulatedIndex()
    {
        // Arrange — create a PDF with all required keyword fields in the Keywords metadata
        var evidenceDir = PathHelpers.SafePathCombine(_testDirectory, "evidence");
        Directory.CreateDirectory(evidenceDir);
        var pdfPath = PathHelpers.SafePathCombine(evidenceDir, "review-evidence.pdf");
        PdfTestHelper.CreateMinimalPdf(pdfPath, "id=Core-Logic fingerprint=abc123 date=2026-04-01 result=pass");

        // Act — scan the evidence directory for PDF files
        var index = ReviewIndex.Scan(_testDirectory, ["evidence/**/*.pdf"]);

        // Assert — the evidence entry is present with all fields correctly extracted
        Assert.True(index.HasId("Core-Logic"));
        var evidence = index.GetEvidence("Core-Logic", "abc123");
        Assert.NotNull(evidence);
        Assert.Equal("Core-Logic", evidence.Id);
        Assert.Equal("abc123", evidence.Fingerprint);
        Assert.Equal("2026-04-01", evidence.Date);
        Assert.Equal("pass", evidence.Result);
    }

    /// <summary>
    ///     Minimal fake HTTP message handler that returns a fixed JSON response body.
    /// </summary>
    private sealed class FakeHttpMessageHandler(string content) : HttpMessageHandler
    {
        /// <inheritdoc />
        protected override HttpResponseMessage Send(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }
    }
}
