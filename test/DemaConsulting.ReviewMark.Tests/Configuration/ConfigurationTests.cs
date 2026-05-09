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

namespace DemaConsulting.ReviewMark.Tests.Configuration;

/// <summary>
///     Subsystem integration tests for the Configuration subsystem
///     (ReviewMarkConfiguration + GlobMatcher working together).
/// </summary>
public sealed class ConfigurationTests : IDisposable
{
    /// <summary>
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of <see cref="ConfigurationTests" />.
    /// </summary>
    public ConfigurationTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(
            Path.GetTempPath(),
            $"ConfigurationTests_{Guid.NewGuid()}");
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
    ///     Test that loading a configuration with needs-review glob patterns correctly resolves matching files.
    /// </summary>
    [Fact]
    public void Configuration_NeedsReview_ValidConfig_ResolvesFiles()
    {
        // Arrange
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Main.cs"), "class Main {}");
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Helper.cs"), "class Helper {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);

        // Assert
        Assert.NotNull(result.Configuration);
        var files = result.Configuration.GetNeedsReviewFiles(_testDirectory);
        Assert.Equal(2, files.Count);
    }

    /// <summary>
    ///     Test that modifying a file changes the review-set fingerprint.
    /// </summary>
    [Fact]
    public void Configuration_Fingerprinting_ContentModified_FingerprintDiffers()
    {
        // Arrange
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        var sourceFile = PathHelpers.SafePathCombine(srcDir, "Main.cs");
        File.WriteAllText(sourceFile, "class Main {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act — load before and after modifying the source file
        var result1 = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result1.Configuration);
        var fingerprint1 = result1.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        File.WriteAllText(sourceFile, "class Main { void Modified() {} }");

        var result2 = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result2.Configuration);
        var fingerprint2 = result2.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        // Assert — fingerprints differ after content change
        Assert.NotEqual(fingerprint1, fingerprint2);
    }

    /// <summary>
    ///     Test that generating a review plan succeeds and includes the review set ID.
    /// </summary>
    [Fact]
    public void Configuration_PlanGeneration_ValidConfig_Succeeds()
    {
        // Arrange
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Main.cs"), "class Main {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result.Configuration);
        var planResult = result.Configuration.PublishReviewPlan(_testDirectory);

        // Assert
        Assert.Contains("Core-Logic", planResult.Markdown);
    }

    /// <summary>
    ///     Test that generating a review report succeeds and includes the review set ID.
    /// </summary>
    [Fact]
    public void Configuration_ReportGeneration_ValidConfig_Succeeds()
    {
        // Arrange
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Main.cs"), "class Main {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result.Configuration);
        var index = ReviewIndex.Load(result.Configuration.EvidenceSource);
        var reportResult = result.Configuration.PublishReviewReport(index, _testDirectory);

        // Assert
        Assert.Contains("Core-Logic", reportResult.Markdown);
    }

    /// <summary>
    ///     Test that elaborating a review-set succeeds and includes the review set ID, fingerprint, and file list.
    /// </summary>
    [Fact]
    public void Configuration_Elaboration_ValidId_Succeeds()
    {
        // Arrange
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Main.cs"), "class Main {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result.Configuration);
        var elaborateResult = result.Configuration.ElaborateReviewSet("Core-Logic", _testDirectory);

        // Assert — elaborated markdown contains the review ID, a fingerprint, and the file list
        Assert.Contains("Core-Logic", elaborateResult.Markdown);
        Assert.Contains("Fingerprint", elaborateResult.Markdown);
        Assert.Contains("Files", elaborateResult.Markdown);
        Assert.Contains("Main.cs", elaborateResult.Markdown);
    }

    /// <summary>
    ///     Test that elaborating a review-set with an unknown ID throws ArgumentException.
    /// </summary>
    [Fact]
    public void Configuration_LoadConfig_ElaborateUnknownId_ThrowsArgumentException()
    {
        // Arrange
        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result.Configuration);

        // Assert — unknown review-set ID throws ArgumentException
        Assert.Throws<ArgumentException>(() =>
            result.Configuration.ElaborateReviewSet("Unknown-Id", _testDirectory));
    }

    /// <summary>
    ///     Test that renaming a file in a review-set does not change its fingerprint.
    /// </summary>
    [Fact]
    public void Configuration_Fingerprinting_FileRenamed_FingerprintUnchanged()
    {
        // Arrange — create a source file and record its fingerprint
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        Directory.CreateDirectory(srcDir);
        var originalFile = PathHelpers.SafePathCombine(srcDir, "Original.cs");
        File.WriteAllText(originalFile, "class Original {}");

        var indexFile = PathHelpers.SafePathCombine(_testDirectory, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, $"""
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: fileshare
              location: {indexFile}
            reviews:
              - id: Core-Logic
                title: Core logic review
                paths:
                  - "src/**/*.cs"
            """);

        var result1 = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result1.Configuration);
        var fingerprint1 = result1.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        // Act — rename the file (same content, different name)
        var renamedFile = PathHelpers.SafePathCombine(srcDir, "Renamed.cs");
        File.Move(originalFile, renamedFile);

        var result2 = ReviewMarkConfiguration.Load(definitionFile);
        Assert.NotNull(result2.Configuration);
        var fingerprint2 = result2.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        // Assert — fingerprint is the same after rename (content-based, not name-based)
        Assert.Equal(fingerprint1, fingerprint2);
    }

    /// <summary>
    ///     Test that loading a malformed YAML configuration returns a null Configuration
    ///     with at least one issue reported.
    /// </summary>
    [Fact]
    public void Configuration_LoadConfig_MalformedYaml_ReturnsIssues()
    {
        // Arrange — write a YAML file with invalid structure (indentation that breaks parsing)
        var definitionFile = PathHelpers.SafePathCombine(_testDirectory, ".reviewmark.yaml");
        File.WriteAllText(definitionFile, """
            : this is not valid yaml: [
            """);

        // Act
        var result = ReviewMarkConfiguration.Load(definitionFile);

        // Assert — configuration is null and at least one issue was reported
        Assert.Null(result.Configuration);
        Assert.NotEmpty(result.Issues);
    }
}
