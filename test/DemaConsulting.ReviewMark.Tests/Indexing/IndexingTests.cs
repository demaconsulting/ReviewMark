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
using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests.Indexing;

/// <summary>
///     Subsystem integration tests for the Indexing subsystem
///     (ReviewIndex + PathHelpers working together).
/// </summary>
[TestClass]
public class IndexingTests
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
        _testDirectory = PathHelpers.SafePathCombine(
            Path.GetTempPath(),
            $"IndexingTests_{Guid.NewGuid()}");
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

    /// <summary>
    ///     Test that SafePathCombine with a subdirectory segment resolves to a valid index path
    ///     that can be loaded by ReviewIndex.
    /// </summary>
    [TestMethod]
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
        Assert.IsTrue(index.HasId("Test-Review"));
        var evidence = index.GetEvidence("Test-Review", "abc123");
        Assert.IsNotNull(evidence);
    }

    /// <summary>
    ///     Test that a ReviewIndex can be saved and reloaded with all entries preserved.
    /// </summary>
    [TestMethod]
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
        Assert.IsTrue(index2.HasId("Review-Alpha"));
        Assert.IsTrue(index2.HasId("Review-Beta"));
        Assert.IsNotNull(index2.GetEvidence("Review-Alpha", "fp001"));
        Assert.IsNotNull(index2.GetEvidence("Review-Beta", "fp002"));
    }
}
