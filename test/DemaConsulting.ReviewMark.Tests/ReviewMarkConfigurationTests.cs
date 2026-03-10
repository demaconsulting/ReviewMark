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
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private string _testDirectory = string.Empty;

    /// <summary>
    ///     Creates a fresh GUID-based temporary directory before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"ReviewMarkConfigurationTests_{Guid.NewGuid()}");
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
            evidence-source:
              type: url
              location: https://reviews.example.com/
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
        // Arrange — a configuration with a .cs pattern; one .cs and one .txt file in the test directory
        var yaml = """
            needs-review:
              - "**/*.cs"
            evidence-source:
              type: url
              location: https://reviews.example.com/
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Program.cs"), "class Program {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "readme.txt"), "readme");

        // Act
        var files = config.GetNeedsReviewFiles(_testDirectory);

        // Assert — only the .cs file is returned
        Assert.AreEqual(1, files.Count);
        Assert.IsTrue(files.Contains("Program.cs"));
    }

    /// <summary>
    ///     Test that the fingerprint is identical when the same content is present in two directories.
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_SameContent_ReturnsSameFingerprint()
    {
        // Arrange — two subdirectories with identical file content
        var dir1 = PathHelpers.SafePathCombine(_testDirectory, "dir1");
        var dir2 = PathHelpers.SafePathCombine(_testDirectory, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        File.WriteAllText(PathHelpers.SafePathCombine(dir1, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(dir1, "B.cs"), "class B {}");
        File.WriteAllText(PathHelpers.SafePathCombine(dir2, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(dir2, "B.cs"), "class B {}");

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — identical content produces identical fingerprints
        Assert.AreEqual(fp1, fp2);
    }

    /// <summary>
    ///     Test that the fingerprint changes when file content changes.
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint()
    {
        // Arrange — two subdirectories with different file content
        var dir1 = PathHelpers.SafePathCombine(_testDirectory, "dir1");
        var dir2 = PathHelpers.SafePathCombine(_testDirectory, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        File.WriteAllText(PathHelpers.SafePathCombine(dir1, "A.cs"), "class A { int x = 1; }");
        File.WriteAllText(PathHelpers.SafePathCombine(dir2, "A.cs"), "class A { int x = 2; }");

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — different content produces different fingerprints
        Assert.AreNotEqual(fp1, fp2);
    }

    /// <summary>
    ///     Test that renaming a file does not change the fingerprint (content-based, not path-based).
    /// </summary>
    [TestMethod]
    public void ReviewSet_GetFingerprint_RenameFile_ReturnsSameFingerprint()
    {
        // Arrange — two subdirectories where one file differs only in name but has identical content
        var dir1 = PathHelpers.SafePathCombine(_testDirectory, "dir1");
        var dir2 = PathHelpers.SafePathCombine(_testDirectory, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        // dir1 has OriginalName.cs; dir2 has the same content under RenamedFile.cs
        const string content = "class SameContent {}";
        File.WriteAllText(PathHelpers.SafePathCombine(dir1, "OriginalName.cs"), content);
        File.WriteAllText(PathHelpers.SafePathCombine(dir2, "RenamedFile.cs"), content);

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"]);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — renaming should not affect the content-based fingerprint
        Assert.AreEqual(fp1, fp2);
    }

    /// <summary>
    ///     Test that Load throws when the specified file does not exist.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_Load_NonExistentFile_ThrowsException()
    {
        // Arrange — a path within the test directory that does not exist
        var nonExistentPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ReviewMarkConfiguration.Load(nonExistentPath));
    }

    // -------------------------------------------------------------------------
    // PublishReviewPlan tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that PublishReviewPlan returns no issues and a table row when all
    ///     needs-review files are covered by a review set.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewPlan_AllCovered_NoIssues()
    {
        // Arrange — config whose review set covers every .cs file; create one .cs file
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act
        var result = config.PublishReviewPlan(_testDirectory);

        // Assert — no uncovered files means no issues; the coverage table is present
        Assert.IsFalse(result.HasIssues);
        Assert.IsTrue(result.Markdown.Contains("# Review Coverage"), "Markdown should contain the top-level heading");
        Assert.IsTrue(result.Markdown.Contains("| Core-Logic |"), "Markdown should contain a row for Core-Logic");
        Assert.IsFalse(result.Markdown.Contains("Uncovered Files"), "Markdown should not mention uncovered files when all are covered");
    }

    /// <summary>
    ///     Test that PublishReviewPlan sets HasIssues and lists uncovered files
    ///     when at least one needs-review file is not matched by any review set.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewPlan_UncoveredFiles_HasIssues()
    {
        // Arrange — config covers only src/**/*.cs; Uncovered.cs at the root is not covered
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Uncovered.cs"), "class Uncovered {}");

        // Act
        var result = config.PublishReviewPlan(_testDirectory);

        // Assert — the uncovered file triggers HasIssues and appears in the Markdown
        Assert.IsTrue(result.HasIssues, "HasIssues should be true when uncovered files exist");
        Assert.IsTrue(result.Markdown.Contains("Uncovered Files"), "Markdown should contain an 'Uncovered Files' section");
        Assert.IsTrue(result.Markdown.Contains("`Uncovered.cs`"), "Markdown should list the uncovered file");
    }

    /// <summary>
    ///     Test that PublishReviewPlan honours the markdownDepth parameter when
    ///     building heading levels, including subheadings for uncovered files.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepth_UsedForHeadings()
    {
        // Arrange — depth 2; create an uncovered file so the subheading also appears
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Uncovered.cs"), "class Uncovered {}");

        // Act
        var result = config.PublishReviewPlan(_testDirectory, markdownDepth: 2);

        // Assert — main heading is at depth 2; subheading for uncovered files is at depth 3
        Assert.IsTrue(result.Markdown.StartsWith("## Review Coverage"), "Markdown should start with a depth-2 heading");
        Assert.IsTrue(result.Markdown.Contains("### Uncovered Files"), "Uncovered-files subheading should use depth markdownDepth+1 (###)");
    }

    // -------------------------------------------------------------------------
    // PublishReviewReport tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that PublishReviewReport returns no issues and marks the review as
    ///     current when the index fingerprint matches the computed fingerprint.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewReport_CurrentReview_NoIssues()
    {
        // Arrange — create the source file so the fingerprint can be computed
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Compute the actual fingerprint so the index entry matches
        var fingerprint = config.Reviews[0].GetFingerprint(_testDirectory);

        // Write a JSON index file with the correct fingerprint
        var indexPath = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexPath, $$"""
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "{{fingerprint}}",
                  "date": "2026-02-14",
                  "result": "pass",
                  "file": "CR-2026-014.pdf"
                }
              ]
            }
            """);
        var index = ReviewIndex.Load(new EvidenceSource("fileshare", indexPath, null, null));

        // Act
        var result = config.PublishReviewReport(index, _testDirectory);

        // Assert — matching fingerprint means "Current"; no issues
        Assert.IsFalse(result.HasIssues, "HasIssues should be false when all reviews are current");
        Assert.IsTrue(result.Markdown.Contains("# Review Status"), "Markdown should contain the Review Status heading");
        Assert.IsTrue(result.Markdown.Contains("\u2705 Current"), "Markdown should show the current status indicator");
        Assert.IsTrue(result.Markdown.Contains("CR-2026-014.pdf"), "Markdown should include the evidence PDF filename");
    }

    /// <summary>
    ///     Test that PublishReviewReport sets HasIssues and marks the review as
    ///     stale when the index fingerprint does not match the current fingerprint.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewReport_StaleReview_HasIssues()
    {
        // Arrange — create the source file; write an index with an outdated fingerprint
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        var indexPath = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexPath, """
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "old-fingerprint",
                  "date": "2025-11-03",
                  "result": "pass",
                  "file": "CR-2025-089.pdf"
                }
              ]
            }
            """);
        var index = ReviewIndex.Load(new EvidenceSource("fileshare", indexPath, null, null));

        // Act
        var result = config.PublishReviewReport(index, _testDirectory);

        // Assert — mismatched fingerprint means "Stale"; HasIssues is true
        Assert.IsTrue(result.HasIssues, "HasIssues should be true when a review is stale");
        Assert.IsTrue(result.Markdown.Contains("\u26a0 Stale"), "Markdown should show the stale status indicator");
        Assert.IsTrue(result.Markdown.Contains("CR-2025-089.pdf"), "Markdown should include the stale evidence PDF filename");
    }

    /// <summary>
    ///     Test that PublishReviewReport sets HasIssues and marks the review as
    ///     missing when the index contains no entry for a review set.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewReport_MissingReview_HasIssues()
    {
        // Arrange — config with one review set; empty index has no evidence
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        var index = ReviewIndex.Empty();

        // Act
        var result = config.PublishReviewReport(index, _testDirectory);

        // Assert — no evidence in the index means "Missing"; HasIssues is true
        Assert.IsTrue(result.HasIssues, "HasIssues should be true when a review has no evidence");
        Assert.IsTrue(result.Markdown.Contains("\u274c Missing"), "Markdown should show the missing status indicator");
    }

    /// <summary>
    ///     Test that PublishReviewReport honours the markdownDepth parameter when
    ///     building heading levels.
    /// </summary>
    [TestMethod]
    public void ReviewMarkConfiguration_PublishReviewReport_MarkdownDepth_UsedForHeadings()
    {
        // Arrange — depth 2 should produce "## Review Status"
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        var index = ReviewIndex.Empty();

        // Act
        var result = config.PublishReviewReport(index, _testDirectory, markdownDepth: 2);

        // Assert — heading is at depth 2
        Assert.IsTrue(result.Markdown.StartsWith("## Review Status"), "Markdown should start with a depth-2 heading");
    }
}
