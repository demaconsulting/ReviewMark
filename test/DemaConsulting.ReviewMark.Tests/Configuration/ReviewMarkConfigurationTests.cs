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

using DemaConsulting.ReviewMark.Cli;
using DemaConsulting.ReviewMark.Configuration;
using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests.Configuration;

/// <summary>
///     Unit tests for <see cref="ReviewMarkConfiguration" />, <see cref="EvidenceSource" />,
///     and <see cref="ReviewSet" />.
/// </summary>
public sealed class ReviewMarkConfigurationTests : IDisposable
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
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of <see cref="ReviewMarkConfigurationTests" />.
    /// </summary>
    public ReviewMarkConfigurationTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"ReviewMarkConfigurationTests_{Guid.NewGuid()}");
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
    ///     Test that passing null yaml throws <see cref="ArgumentNullException" />.
    /// </summary>
    [Fact]
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
    [Fact]
    public void ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration()
    {
        // Arrange — uses MinimalYaml constant defined at class level

        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert — a non-null configuration is returned from valid YAML
        Assert.NotNull(config);
    }

    /// <summary>
    ///     Test that needs-review patterns are parsed correctly.
    /// </summary>
    [Fact]
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
        Assert.Equal(3, config.NeedsReviewPatterns.Count());
        Assert.Equal("**/*.cs", config.NeedsReviewPatterns[0]);
        Assert.Equal("**/*.yaml", config.NeedsReviewPatterns[1]);
        Assert.Equal("!**/obj/**", config.NeedsReviewPatterns[2]);
    }

    /// <summary>
    ///     Test that the evidence-source block is parsed correctly.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly()
    {
        // Arrange — uses MinimalYaml constant defined at class level

        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert — evidence-source type, location, and absent credentials are parsed correctly
        Assert.Equal("url", config.EvidenceSource.Type);
        Assert.Equal("https://reviews.example.com/", config.EvidenceSource.Location);
        Assert.Null(config.EvidenceSource.UsernameEnv);
        Assert.Null(config.EvidenceSource.PasswordEnv);
    }

    /// <summary>
    ///     Test that the reviews list is parsed correctly.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly()
    {
        // Arrange — uses MinimalYaml constant defined at class level

        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert — one review set with expected id, title, and path
        Assert.Single(config.Reviews);
        var review = config.Reviews[0];
        Assert.Equal("Core-Logic", review.Id);
        Assert.Equal("Review of core business logic", review.Title);
        Assert.Single(review.Paths);
        Assert.Equal("src/**/*.cs", review.Paths[0]);
    }

    /// <summary>
    ///     Test that evidence-source credentials are parsed correctly when present.
    /// </summary>
    [Fact]
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

        // Assert — credential environment variable names are parsed correctly
        Assert.Equal("REVIEWMARK_USER", config.EvidenceSource.UsernameEnv);
        Assert.Equal("REVIEWMARK_TOKEN", config.EvidenceSource.PasswordEnv);
    }

    /// <summary>
    ///     Test that GetNeedsReviewFiles returns files matching the needs-review patterns.
    /// </summary>
    [Fact]
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
        Assert.Single(files);
        Assert.Contains("Program.cs", files);
    }

    /// <summary>
    ///     Test that the fingerprint is identical when the same content is present in two directories.
    /// </summary>
    [Fact]
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

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"], []);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — identical content produces identical fingerprints
        Assert.Equal(fp1, fp2);
    }

    /// <summary>
    ///     Test that the fingerprint changes when file content changes.
    /// </summary>
    [Fact]
    public void ReviewSet_GetFingerprint_DifferentContent_ReturnsDifferentFingerprint()
    {
        // Arrange — two subdirectories with different file content
        var dir1 = PathHelpers.SafePathCombine(_testDirectory, "dir1");
        var dir2 = PathHelpers.SafePathCombine(_testDirectory, "dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        File.WriteAllText(PathHelpers.SafePathCombine(dir1, "A.cs"), "class A { int x = 1; }");
        File.WriteAllText(PathHelpers.SafePathCombine(dir2, "A.cs"), "class A { int x = 2; }");

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"], []);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — different content produces different fingerprints
        Assert.NotEqual(fp1, fp2);
    }

    /// <summary>
    ///     Test that renaming a file does not change the fingerprint (content-based, not path-based).
    /// </summary>
    [Fact]
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

        var reviewSet = new ReviewSet("Test", "Test Review", ["**/*.cs"], []);

        // Act
        var fp1 = reviewSet.GetFingerprint(dir1);
        var fp2 = reviewSet.GetFingerprint(dir2);

        // Assert — renaming should not affect the content-based fingerprint
        Assert.Equal(fp1, fp2);
    }

    /// <summary>
    ///     Test that Load returns null configuration with an error issue when the file does not exist.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_NonExistentFile_ReturnsNullConfigWithErrorIssue()
    {
        // Arrange — a path within the test directory that does not exist
        var nonExistentPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");

        // Act
        var result = ReviewMarkConfiguration.Load(nonExistentPath);

        // Assert — configuration is null and one error issue is reported
        Assert.Null(result.Configuration);
        Assert.Single(result.Issues);
        Assert.Equal(LintSeverity.Error, result.Issues[0].Severity);
    }

    /// <summary>
    ///     Test that Load returns null configuration with an error issue naming file and line when YAML is invalid.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue()
    {
        // Arrange — write a configuration file with invalid YAML syntax
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, "{{{invalid yaml");

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — configuration is null, one error issue naming file and line
        Assert.Null(result.Configuration);
        Assert.Single(result.Issues);
        Assert.Equal(LintSeverity.Error, result.Issues[0].Severity);
        Assert.Contains(".reviewmark.yaml", result.Issues[0].Location);
        Assert.Contains("at line", result.Issues[0].Description);
    }

    /// <summary>
    ///     Test that Load returns null configuration with an error issue naming the file and missing field
    ///     when required fields are missing.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_MissingEvidenceSource_ReturnsNullConfigWithErrorIssue()
    {
        // Arrange — write a valid YAML file that is missing the required evidence-source block
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            needs-review:
              - "src/**/*.cs"
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — configuration is null and error mentions evidence-source
        Assert.Null(result.Configuration);
        Assert.Single(result.Issues);
        Assert.Equal(LintSeverity.Error, result.Issues[0].Severity);
        Assert.Contains("evidence-source", result.Issues[0].Description);
    }

    /// <summary>
    ///     Test that Load returns all issues from a file with multiple detectable errors
    ///     (missing evidence-source AND duplicate review IDs) without stopping at the first.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_MultipleErrors_ReturnsAllIssues()
    {
        // Arrange — write a YAML file missing evidence-source and containing duplicate IDs
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            needs-review:
              - "src/**/*.cs"
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
              - id: Core-Logic
                title: Duplicate review set
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — configuration is null and both errors are reported
        Assert.Null(result.Configuration);
        Assert.Equal(2, result.Issues.Count());
        Assert.DoesNotContain(result.Issues, (LintIssue i) => i.Severity != LintSeverity.Error);
        Assert.Contains(result.Issues, (LintIssue i) => i.Description.Contains("evidence-source"));
        Assert.Contains(result.Issues, (LintIssue i) => i.Description.Contains("duplicate ID") && i.Description.Contains("Core-Logic"));
    }

    /// <summary>
    ///     Test that Load resolves a relative fileshare location against the config file's directory.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_FileshareRelativeLocation_ResolvesToAbsolutePath()
    {
        // Arrange — write a config file with a relative fileshare location
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            needs-review:
              - "**/*.cs"
            evidence-source:
              type: fileshare
              location: index.json
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """);

        // Act - load the configuration
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — relative location is resolved to an absolute path under the config directory
        Assert.NotNull(result.Configuration);
        Assert.True(Path.IsPathRooted(result.Configuration.EvidenceSource.Location));
        Assert.Equal(PathHelpers.SafePathCombine(_testDirectory, "index.json"), result.Configuration.EvidenceSource.Location);
    }

    /// <summary>
    ///     Test that an evidence-source with type <c>none</c> is parsed correctly
    ///     and produces an empty <see cref="EvidenceSource.Location" />.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly()
    {
        // Arrange
        var yaml = """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """;

        // Act
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Assert — type is 'none' and location is empty
        Assert.Equal("none", config.EvidenceSource.Type);
        Assert.Equal(string.Empty, config.EvidenceSource.Location);
    }

    /// <summary>
    ///     Test that an evidence-source with type <c>none</c> does not require a
    ///     <c>location</c> field.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_NoneEvidenceSource_NoLocationRequired()
    {
        // Arrange — YAML with a none source and no location field
        var yaml = """
            evidence-source:
              type: none
            """;

        // Act & Assert — parsing must succeed without throwing
        var config = ReviewMarkConfiguration.Parse(yaml);
        Assert.Equal("none", config.EvidenceSource.Type);
    }

    /// <summary>
    ///     Test that Load does not report an issue when the evidence-source type is <c>none</c>
    ///     and no <c>location</c> field is present.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_NoneEvidenceSource_NoIssues()
    {
        // Arrange — write a valid config with a none evidence source
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — no issues and configuration is non-null for a valid none source
        Assert.NotNull(result.Configuration);
        Assert.Empty(result.Issues);
    }

    // -------------------------------------------------------------------------
    // PublishReviewPlan tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that PublishReviewPlan returns no issues and a table row when all
    ///     needs-review files are covered by a review set.
    /// </summary>
    [Fact]
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
        Assert.False(result.HasIssues);
        Assert.Contains("# Review Coverage", result.Markdown);
        Assert.Contains("| Core-Logic |", result.Markdown);
        Assert.Contains("All files requiring review are covered by a review-set.", result.Markdown);
        Assert.DoesNotContain("⚠", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewPlan sets HasIssues and lists uncovered files
    ///     when at least one needs-review file is not matched by any review set.
    /// </summary>
    [Fact]
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
        Assert.True(result.HasIssues, "HasIssues should be true when uncovered files exist");
        Assert.Contains("Coverage", result.Markdown);
        Assert.Contains("`Uncovered.cs`", result.Markdown);
    }

    /// <summary>
    ///     Test that a file listed only in context: is still reported as uncovered by
    ///     PublishReviewPlan when it matches the needs-review pattern.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_PublishReviewPlan_ContextOnlyFile_StillReportedAsUncovered()
    {
        // Arrange — src/MyFile.cs matches needs-review; it appears in context: but NOT in paths:
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "MyFile.cs"), "class MyFile {}");

        var yaml = """
            needs-review:
              - "**/*.cs"
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                context:
                  - "src/**/*.cs"
                paths:
                  - "other/**/*.cs"
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Act
        var result = config.PublishReviewPlan(_testDirectory);

        // Assert — the context-only file is not covered, so HasIssues must be true
        Assert.True(result.HasIssues, "HasIssues should be true when a needs-review file appears only in context:");
        Assert.Contains("Coverage", result.Markdown);
        Assert.Contains("`src/MyFile.cs`", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewPlan honors the markdownDepth parameter when
    ///     building heading levels, including subheadings for uncovered files.
    /// </summary>
    [Fact]
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

        // Assert — main heading is at depth 2; subheading for coverage is at depth 3
        Assert.StartsWith("## Review Coverage", result.Markdown);
        Assert.Contains("### Coverage", result.Markdown);
    }

    // -------------------------------------------------------------------------
    // PublishReviewReport tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that PublishReviewReport returns no issues and marks the review as
    ///     current when the index fingerprint matches the computed fingerprint.
    /// </summary>
    [Fact]
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
        Assert.False(result.HasIssues, "HasIssues should be false when all reviews are current");
        Assert.Contains("# Review Status", result.Markdown);
        Assert.Contains("\u2705 Current", result.Markdown);
        Assert.Contains("Referenced Documents", result.Markdown);
        Assert.Contains("CR-2026-014.pdf", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewReport sets HasIssues and marks the review as
    ///     stale when the index fingerprint does not match the current fingerprint.
    /// </summary>
    [Fact]
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
        Assert.True(result.HasIssues, "HasIssues should be true when a review is stale");
        Assert.Contains("\u26a0 Stale", result.Markdown);
        Assert.Contains("CR-2025-089.pdf", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewReport sets HasIssues and marks the review as
    ///     failed when the index has a matching fingerprint but a non-passing result.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_PublishReviewReport_FailedReview_HasIssues()
    {
        // Arrange — create the source file so the fingerprint can be computed
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Compute the actual fingerprint so the index entry matches the current code
        var fingerprint = config.Reviews[0].GetFingerprint(_testDirectory);

        // Write a JSON index file with a matching fingerprint but a failing result
        var indexPath = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexPath, $$"""
            {
              "reviews": [
                {
                  "id": "Core-Logic",
                  "fingerprint": "{{fingerprint}}",
                  "date": "2026-02-14",
                  "result": "fail",
                  "file": "CR-2026-014.pdf"
                }
              ]
            }
            """);
        var index = ReviewIndex.Load(new EvidenceSource("fileshare", indexPath, null, null));

        // Act
        var result = config.PublishReviewReport(index, _testDirectory);

        // Assert — matching fingerprint with a failing result means "Failed"; HasIssues is true
        Assert.True(result.HasIssues, "HasIssues should be true when a review has failed");
        Assert.Contains("\u274c Failed", result.Markdown);
        Assert.Contains("CR-2026-014.pdf", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewReport sets HasIssues and marks the review as
    ///     missing when the index contains no entry for a review set.
    /// </summary>
    [Fact]
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
        Assert.True(result.HasIssues, "HasIssues should be true when a review has no evidence");
        Assert.Contains("\u274c Missing", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewReport honours the markdownDepth parameter when
    ///     building heading levels.
    /// </summary>
    [Fact]
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
        Assert.StartsWith("## Review Status", result.Markdown);
    }

    /// <summary>
    ///     Test that PublishReviewPlan throws when markdownDepth exceeds 5,
    ///     since subheadings at depth+1 would exceed the maximum Markdown heading level of 6.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_PublishReviewPlan_MarkdownDepthAbove5_Throws()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act & Assert — depth 6 should throw because subheadings would require level 7
        Assert.Throws<ArgumentOutOfRangeException>(
            () => config.PublishReviewPlan(_testDirectory, markdownDepth: 6));
    }

    /// <summary>
    ///     Test that PublishReviewReport throws when markdownDepth exceeds 5,
    ///     since subheadings at depth+1 would exceed the maximum Markdown heading level of 6.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_PublishReviewReport_MarkdownDepthAbove5_Throws()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        var index = ReviewIndex.Empty();

        // Act & Assert — depth 6 should throw because subheadings would require level 7
        Assert.Throws<ArgumentOutOfRangeException>(
            () => config.PublishReviewReport(index, _testDirectory, markdownDepth: 6));
    }

    // -------------------------------------------------------------------------
    // ElaborateReviewSet tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that ElaborateReviewSet returns the review ID, fingerprint, and file list
    ///     when given a valid review-set ID.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_ValidId_ReturnsElaboration()
    {
        // Arrange — create a source file so files and fingerprint can be computed
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — result contains the review ID, title, a Fingerprint field, and the file listing
        Assert.NotNull(result);
        Assert.Contains("# Core-Logic", result.Markdown);
        Assert.Contains("| ID | Core-Logic |", result.Markdown);
        Assert.Contains("| Title | Review of core business logic |", result.Markdown);
        Assert.Contains("| Fingerprint |", result.Markdown);
        Assert.Contains("## Files", result.Markdown);
        Assert.Contains("`src/A.cs`", result.Markdown);
    }

    /// <summary>
    ///     Test that ElaborateReviewSet throws ArgumentException when the
    ///     review-set ID does not exist in the configuration.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_UnknownId_ThrowsArgumentException()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act & Assert — an unknown review-set ID should throw ArgumentException
        Assert.Throws<ArgumentException>(() =>
            config.ElaborateReviewSet("NonExistent", _testDirectory));
    }

    /// <summary>
    ///     Test that ElaborateReviewSet throws ArgumentException when the
    ///     review-set ID is null or whitespace.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Act & Assert — null review-set ID should throw
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type — intentional
        Assert.Throws<ArgumentNullException>(() =>
            config.ElaborateReviewSet(null!, _testDirectory));
#pragma warning restore CS8625
    }

    /// <summary>
    ///     Test that ElaborateReviewSet throws ArgumentException when the
    ///     review-set ID is whitespace-only.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_WhitespaceId_ThrowsArgumentException()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Act & Assert — whitespace-only review-set ID should throw
        Assert.Throws<ArgumentException>(() =>
            config.ElaborateReviewSet("   ", _testDirectory));
    }

    /// <summary>
    ///     Test that ElaborateReviewSet honours the markdownDepth parameter for
    ///     both the main heading and the Files subheading.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepth_UsedForHeadings()
    {
        // Arrange — depth 2; create a source file
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory, markdownDepth: 2);

        // Assert — main heading at depth 2; files subheading at depth 3
        Assert.StartsWith("## Core-Logic", result.Markdown);
        Assert.Contains("### Files", result.Markdown);
    }

    /// <summary>
    ///     Test that ElaborateReviewSet throws when markdownDepth exceeds 5.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_MarkdownDepthAbove5_Throws()
    {
        // Arrange
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act & Assert — depth 6 should throw
        Assert.Throws<ArgumentOutOfRangeException>(
            () => config.ElaborateReviewSet("Core-Logic", _testDirectory, markdownDepth: 6));
    }

    /// <summary>
    ///     Test that ElaborateReviewSet includes the full (non-abbreviated) fingerprint.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_ContainsFullFingerprint()
    {
        // Arrange — create a source file so the fingerprint can be computed
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Compute the expected fingerprint independently
        var expectedFingerprint = config.Reviews[0].GetFingerprint(_testDirectory);

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — the full 64-character hex fingerprint appears in the Markdown (not abbreviated)
        Assert.Contains(expectedFingerprint, result.Markdown);
        Assert.Equal(64, expectedFingerprint.Length);
    }

    /// <summary>
    ///     Test that Load on a valid file returns configuration and no issues.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_ValidFile_ReturnsConfigurationAndNoIssues()
    {
        // Arrange — write a valid configuration file
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, MinimalYaml);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — configuration is non-null and no issues are reported
        Assert.NotNull(result.Configuration);
        Assert.Empty(result.Issues);
    }

    /// <summary>
    ///     Test that ReportIssues routes errors to WriteError and warnings to WriteLine via Context.
    /// </summary>
    [Fact]
    public void ReviewMarkLoadResult_ReportIssues_RoutesIssuesToContext()
    {
        // Arrange — a result with one warning and one error; capture output via a log file
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "report.log");
        var issues = new List<LintIssue>
        {
            new("file.yaml", LintSeverity.Warning, "A warning message"),
            new("file.yaml", LintSeverity.Error, "An error message")
        };
        var result = new ReviewMarkLoadResult(null, issues);

        // Act — dispose context before reading log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            result.ReportIssues(context);
            exitCode = context.ExitCode;
        }

        // Assert — error sets exit code; both messages appear in the log
        Assert.Equal(1, exitCode);
        var log = File.ReadAllText(logFile);
        Assert.Contains("warning", log);
        Assert.Contains("A warning message", log);
        Assert.Contains("error", log);
        Assert.Contains("An error message", log);
    }

    /// <summary>
    ///     Test that Load returns a lint error when a review set has only whitespace entries in its paths list.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_WhitespaceOnlyPaths_ReturnsLintError()
    {
        // Arrange — write a config with a review set whose paths list contains only a whitespace string
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "   "
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — whitespace-only paths list should produce a lint error naming the review set
        Assert.Null(result.Configuration);
        Assert.Single(result.Issues);
        Assert.Equal(LintSeverity.Error, result.Issues[0].Severity);
        Assert.Contains("paths", result.Issues[0].Description);
    }

    // -------------------------------------------------------------------------
    // Context file tests
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Test that Load returns a lint warning when a review set has whitespace-only context entries.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Load_WhitespaceOnlyContextEntries_ReturnsLintWarning()
    {
        // Arrange — write a config with a review set whose context list contains a whitespace-only string
        var configPath = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(configPath, """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
                context:
                  - "   "
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(configPath);

        // Assert — whitespace-only context entry should produce a lint warning; configuration is still returned
        Assert.NotNull(result.Configuration);
        Assert.Single(result.Issues);
        Assert.Equal(LintSeverity.Warning, result.Issues[0].Severity);
        Assert.Contains("context", result.Issues[0].Description);
    }

    /// <summary>
    ///     Test that a top-level context: list is parsed into GlobalContext.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_GlobalContext_ParsedCorrectly()
    {
        // Arrange
        var yaml = """
            context:
              - "docs/design/introduction.md"
              - "docs/design/system.md"
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """;

        // Act
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Assert — both global context entries are present and in order
        Assert.Equal(2, config.GlobalContext.Count);
        Assert.Equal("docs/design/introduction.md", config.GlobalContext[0]);
        Assert.Equal("docs/design/system.md", config.GlobalContext[1]);
    }

    /// <summary>
    ///     Test that a per-review-set context: list is parsed into ReviewSet.Context.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_ReviewSetContext_ParsedCorrectly()
    {
        // Arrange
        var yaml = """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
                context:
                  - "docs/design/core.md"
                  - "docs/design/core-api.md"
            """;

        // Act
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Assert — both per-review-set context entries are present and in order
        Assert.Single(config.Reviews);
        Assert.Equal(2, config.Reviews[0].Context.Count);
        Assert.Equal("docs/design/core.md", config.Reviews[0].Context[0]);
        Assert.Equal("docs/design/core-api.md", config.Reviews[0].Context[1]);
    }

    /// <summary>
    ///     Test that omitting context: in YAML results in empty GlobalContext and ReviewSet.Context.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_Parse_NoContext_DefaultsToEmpty()
    {
        // Arrange — uses MinimalYaml constant which has no context: entries

        // Act
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);

        // Assert — both GlobalContext and the review set's Context are empty
        Assert.Empty(config.GlobalContext);
        Assert.Single(config.Reviews);
        Assert.Empty(config.Reviews[0].Context);
    }

    /// <summary>
    ///     Test that context patterns do not affect the review set fingerprint.
    /// </summary>
    [Fact]
    public void ReviewSet_GetFingerprint_ContextNotIncluded()
    {
        // Arrange — one source file; two review sets with identical paths but different context
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        var reviewSetNoContext = new ReviewSet("Test", "Test Review", ["src/**/*.cs"], []);
        var reviewSetWithContext = new ReviewSet("Test", "Test Review", ["src/**/*.cs"], ["docs/**/*.md"]);

        // Act
        var fp1 = reviewSetNoContext.GetFingerprint(_testDirectory);
        var fp2 = reviewSetWithContext.GetFingerprint(_testDirectory);

        // Assert — different context patterns must not change the fingerprint
        Assert.Equal(fp1, fp2);
    }

    /// <summary>
    ///     Test that ElaborateReviewSet includes a Context subsection with [global] labels
    ///     when global context files resolve.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_GlobalContext_AppearsInOutput()
    {
        // Arrange — create a source file and a context file; configure global context
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        var docsDir = PathHelpers.SafePathCombine(_testDirectory, "docs");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(docsDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(docsDir, "design.md"), "# Design");

        var yaml = """
            context:
              - "docs/**/*.md"
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — context subsection is present and the file is labeled [global]
        Assert.Contains("## Context", result.Markdown);
        Assert.Contains("[global]", result.Markdown);
        Assert.Contains("docs/design.md", result.Markdown);
    }

    /// <summary>
    ///     Test that ElaborateReviewSet includes a Context subsection with [local] labels
    ///     when per-review-set context files resolve.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_LocalContext_AppearsInOutput()
    {
        // Arrange — create a source file and a context file; configure per-review-set context
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        var docsDir = PathHelpers.SafePathCombine(_testDirectory, "docs");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(docsDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(docsDir, "design.md"), "# Design");

        var yaml = """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
                context:
                  - "docs/**/*.md"
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — context subsection is present and the file is labeled [local]
        Assert.Contains("## Context", result.Markdown);
        Assert.Contains("[local]", result.Markdown);
        Assert.Contains("docs/design.md", result.Markdown);
    }

    /// <summary>
    ///     Test that ElaborateReviewSet omits the Context subsection when no context files resolve.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_NoContext_ContextSectionOmitted()
    {
        // Arrange — config with no context entries; one source file present
        var config = ReviewMarkConfiguration.Parse(MinimalYaml);
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — the Context heading must not appear when there are no context files
        Assert.DoesNotContain("## Context", result.Markdown);
    }

    /// <summary>
    ///     Test that context files do not appear in the Files subsection of ElaborateReviewSet output.
    /// </summary>
    [Fact]
    public void ReviewMarkConfiguration_ElaborateReviewSet_ContextNotUnderReview()
    {
        // Arrange — source file in src/, context file in docs/; context not in paths:
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        var docsDir = PathHelpers.SafePathCombine(_testDirectory, "docs");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(docsDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");
        File.WriteAllText(PathHelpers.SafePathCombine(docsDir, "design.md"), "# Design");

        var yaml = """
            evidence-source:
              type: none
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
                context:
                  - "docs/**/*.md"
            """;
        var config = ReviewMarkConfiguration.Parse(yaml);

        // Act
        var result = config.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — Files subsection contains only the source file, not the context file
        Assert.Contains("## Files", result.Markdown);
        Assert.Contains("`src/A.cs`", result.Markdown);

        // Extract just the Files section content to verify the context file is absent there
        var filesIndex = result.Markdown.IndexOf("## Files", StringComparison.Ordinal);
        var filesSection = result.Markdown[filesIndex..];
        Assert.DoesNotContain("`docs/design.md`", filesSection);
    }
}
