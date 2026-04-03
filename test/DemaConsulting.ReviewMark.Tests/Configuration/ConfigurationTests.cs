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
[TestClass]
public class ConfigurationTests
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
            $"ConfigurationTests_{Guid.NewGuid()}");
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
    ///     Test that loading a configuration with needs-review glob patterns correctly resolves matching files.
    /// </summary>
    [TestMethod]
    public void Configuration_LoadConfig_ResolvesNeedsReviewFiles()
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
        Assert.IsNotNull(result.Configuration);
        var files = result.Configuration.GetNeedsReviewFiles(_testDirectory);
        Assert.AreEqual(2, files.Count);
    }

    /// <summary>
    ///     Test that modifying a file changes the review-set fingerprint.
    /// </summary>
    [TestMethod]
    public void Configuration_LoadConfig_FingerprintReflectsFileContent()
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
        Assert.IsNotNull(result1.Configuration);
        var fingerprint1 = result1.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        File.WriteAllText(sourceFile, "class Main { void Modified() {} }");

        var result2 = ReviewMarkConfiguration.Load(definitionFile);
        Assert.IsNotNull(result2.Configuration);
        var fingerprint2 = result2.Configuration.Reviews[0].GetFingerprint(_testDirectory);

        // Assert — fingerprints differ after content change
        Assert.AreNotEqual(fingerprint1, fingerprint2);
    }

    /// <summary>
    ///     Test that generating a review plan succeeds and includes the review set ID.
    /// </summary>
    [TestMethod]
    public void Configuration_LoadConfig_PlanGenerationSucceeds()
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
        Assert.IsNotNull(result.Configuration);
        var planResult = result.Configuration.PublishReviewPlan(_testDirectory);

        // Assert
        Assert.Contains("Core-Logic", planResult.Markdown);
    }
}
