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

namespace DemaConsulting.ReviewMark.Tests;

/// <summary>
///     Unit tests for <see cref="ReviewMarkConfiguration" />, <see cref="EvidenceSource" />,
///     and <see cref="ReviewSet" />.
/// </summary>
[TestClass]
public class ReviewMarkConfigurationTests
{
    /// <summary>
    ///     Sample minimal YAML used by several parse tests.
    /// </summary>
    private const string MinimalYaml = """
        needs-review:
          - "**/*.cs"
        evidence-source:
          type: url
          location: https://reviews.example.com/
        reviews:
          - id: Core-Logic
            title: Review of core business logic
            paths:
              - "src/**/*.cs"
        """;

    /// <summary>
    ///     Test that passing null yaml throws <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_NullYaml_ThrowsArgumentNullException()
    {
        // Arrange
        string? yaml = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            ReviewMarkConfiguration.Parse(yaml!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that valid YAML is parsed without throwing.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration()
    {
        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert
        Assert.IsNotNull(config);
    }

    /// <summary>
    ///     Test that needs-review patterns are parsed correctly.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly()
    {
        // Arrange
        var yaml = """
            needs-review:
              - "**/*.cs"
              - "**/*.yaml"
              - "!**/obj/**"
            """;

        // Act
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Assert — all three patterns are present and in order
        Assert.AreEqual(3, config.NeedsReviewPatterns.Count);
        Assert.AreEqual("**/*.cs", config.NeedsReviewPatterns[0]);
        Assert.AreEqual("**/*.yaml", config.NeedsReviewPatterns[1]);
        Assert.AreEqual("!**/obj/**", config.NeedsReviewPatterns[2]);
    }

    /// <summary>
    ///     Test that the evidence-source block is parsed correctly.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly()
    {
        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert
        Assert.AreEqual("url", config.EvidenceSource.Type);
        Assert.AreEqual("https://reviews.example.com/", config.EvidenceSource.Location);
        Assert.IsNull(config.EvidenceSource.UsernameEnv);
        Assert.IsNull(config.EvidenceSource.PasswordEnv);
    }

    /// <summary>
    ///     Test that the reviews list is parsed correctly.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly()
    {
        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert — one review set with expected id, title, and path
        Assert.AreEqual(1, config.Reviews.Count);
        var review = config.Reviews[0];
        Assert.AreEqual("Core-Logic", review.Id);
        Assert.AreEqual("Review of core business logic", review.Title);
        Assert.AreEqual(1, review.Paths.Count);
        Assert.AreEqual("src/**/*.cs", review.Paths[0]);
    }

    /// <summary>
    ///     Test that evidence-source credentials are parsed correctly when present.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Parse_EvidenceSourceWithCredentials_ParsedCorrectly()
    {
        // Arrange
        var yaml = """
            evidence-source:
              type: url
              location: https://reviews.example.com/
              credentials:
                username-env: REVIEWMARK_USER
                password-env: REVIEWMARK_TOKEN
            """;

        // Act
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Assert
        Assert.AreEqual("REVIEWMARK_USER", config.EvidenceSource.UsernameEnv);
        Assert.AreEqual("REVIEWMARK_TOKEN", config.EvidenceSource.PasswordEnv);
    }

    /// <summary>
    ///     Test that GetNeedsReviewFiles returns files matching the needs-review patterns.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_GetNeedsReviewFiles_ReturnsMatchingFiles()
    {
        // Arrange — a configuration with a .cs pattern, a temp directory with one .cs and one .txt
        var yaml = """
            needs-review:
              - "**/*.cs"
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Program.cs"), "class Program {}");
            File.WriteAllText(Path.Combine(tempDir, "readme.txt"), "readme");

            // Act
            var files = config.GetNeedsReviewFiles(tempDir);

            // Assert — only the .cs file is returned
            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files.Contains("Program.cs"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    ///     Test that the fingerprint is identical when the same content is present in two directories.
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint()
    {
        // Arrange — two directories with identical file content
        var dir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var dir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        try
        {
            File.WriteAllText(Path.Combine(dir1, "A.cs"), "class A {}");
            File.WriteAllText(Path.Combine(dir1, "B.cs"), "class B {}");
            File.WriteAllText(Path.Combine(dir2, "A.cs"), "class A {}");
            File.WriteAllText(Path.Combine(dir2, "B.cs"), "class B {}");

            var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

            // Act
            var fp1 = reviewSet.GetFingerprint(dir1);
            var fp2 = reviewSet.GetFingerprint(dir2);

            // Assert — identical content produces identical fingerprints
            Assert.AreEqual(fp1, fp2);
        }
        finally
        {
            Directory.Delete(dir1, recursive: true);
            Directory.Delete(dir2, recursive: true);
        }
    }

    /// <summary>
    ///     Test that the fingerprint changes when file content changes.
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint()
    {
        // Arrange — two directories with different file content
        var dir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var dir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        try
        {
            File.WriteAllText(Path.Combine(dir1, "A.cs"), "class A { int x = 1; }");
            File.WriteAllText(Path.Combine(dir2, "A.cs"), "class A { int x = 2; }");

            var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

            // Act
            var fp1 = reviewSet.GetFingerprint(dir1);
            var fp2 = reviewSet.GetFingerprint(dir2);

            // Assert — different content produces different fingerprints
            Assert.AreNotEqual(fp1, fp2);
        }
        finally
        {
            Directory.Delete(dir1, recursive: true);
            Directory.Delete(dir2, recursive: true);
        }
    }

    /// <summary>
    ///     Test that renaming a file does not change the fingerprint (content-based, not path-based).
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint()
    {
        // Arrange — two directories where one file differs only in name but has identical content
        var dir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var dir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        try
        {
            // dir1 has OriginalName.cs; dir2 has the same content under RenamedFile.cs
            const string content = "class SameContent {}";
            File.WriteAllText(Path.Combine(dir1, "OriginalName.cs"), content);
            File.WriteAllText(Path.Combine(dir2, "RenamedFile.cs"), content);

            var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

            // Act
            var fp1 = reviewSet.GetFingerprint(dir1);
            var fp2 = reviewSet.GetFingerprint(dir2);

            // Assert — renaming should not affect the content-based fingerprint
            Assert.AreEqual(fp1, fp2);
        }
        finally
        {
            Directory.Delete(dir1, recursive: true);
            Directory.Delete(dir2, recursive: true);
        }
    }

    /// <summary>
    ///     Test that Load throws when the specified file does not exist.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Load_NonExistentFile_ThrowsException()
    {
        // Arrange — a path that does not exist
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), ".reviewmark.yaml");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ReviewMarkConfiguration.Load(nonExistentPath));
    }
}
