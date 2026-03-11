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
///     Unit tests for the <see cref="GlobMatcher" /> class.
/// </summary>
[TestClass]
public class GlobMatcherTests
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
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"GlobMatcherTests_{Guid.NewGuid()}");
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
    ///     Test that passing a null base directory throws <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_NullBaseDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        string? baseDirectory = null;
        IReadOnlyList<string> patterns = ["**/*.cs"];

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            GlobMatcher.GetMatchingFiles(baseDirectory!, patterns));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that passing null patterns throws <see cref="ArgumentNullException" />.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_NullPatterns_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<string>? patterns = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            GlobMatcher.GetMatchingFiles(_testDirectory, patterns!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that an empty patterns list returns an empty result.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList()
    {
        // Arrange
        IReadOnlyList<string> patterns = [];

        // Act
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, patterns);

        // Assert
        Assert.IsEmpty(result);
    }

    /// <summary>
    ///     Test that a single include pattern returns all files matching that pattern.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles()
    {
        // Arrange — create two .cs files and one .txt file in the test directory
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Alpha.cs"), "class Alpha {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Beta.cs"), "class Beta {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "readme.txt"), "readme");

        // Act — only match .cs files
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs"]);

        // Assert — both .cs files are returned; the .txt file is not
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Contains("Alpha.cs"));
        Assert.IsTrue(result.Contains("Beta.cs"));
    }

    /// <summary>
    ///     Test that an exclude pattern removes matching files from the result.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles()
    {
        // Arrange — create files in the root and a subdirectory that should be excluded
        var genDir = PathHelpers.SafePathCombine(_testDirectory, "Generated");
        Directory.CreateDirectory(genDir);
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Real.cs"), "class Real {}");
        File.WriteAllText(PathHelpers.SafePathCombine(genDir, "Generated.cs"), "class Generated {}");

        // Act — include everything but exclude the Generated subdirectory
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "!Generated/**"]);

        // Assert — only Real.cs is returned
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Contains("Real.cs"));
    }

    /// <summary>
    ///     Test that multiple include patterns return all files matching any of the patterns.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching()
    {
        // Arrange — create .cs, .yaml, and .txt files in the test directory
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "Program.cs"), "class Program {}");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "config.yaml"), "key: value");
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "readme.txt"), "readme");

        // Act — match both .cs and .yaml files
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "**/*.yaml"]);

        // Assert — both .cs and .yaml files are included; .txt is not
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Contains("Program.cs"));
        Assert.IsTrue(result.Contains("config.yaml"));
    }

    /// <summary>
    ///     Test that a combination of include and exclude patterns returns only the filtered files.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles()
    {
        // Arrange — create files in src and obj subdirectories
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        var objDir = PathHelpers.SafePathCombine(_testDirectory, "obj");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(objDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Main.cs"), "class Main {}");
        File.WriteAllText(PathHelpers.SafePathCombine(objDir, "Main.obj.cs"), "// generated");

        // Act — include all .cs, exclude obj directory
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "!obj/**"]);

        // Assert — only src/Main.cs is returned
        Assert.HasCount(1, result);
        Assert.IsTrue(result.Contains("src/Main.cs"));
    }

    /// <summary>
    ///     Test that a pattern that does not match any files returns an empty list.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList()
    {
        // Arrange — create a .txt file (no .cs files)
        File.WriteAllText(PathHelpers.SafePathCombine(_testDirectory, "notes.txt"), "notes");

        // Act — search for .cs files (none exist)
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs"]);

        // Assert — empty list because no .cs files are present
        Assert.IsEmpty(result);
    }

    /// <summary>
    ///     Test that an include pattern appearing after an exclude re-adds previously excluded files.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles()
    {
        // Arrange — create files in src and Generated directories
        var srcDir = PathHelpers.SafePathCombine(_testDirectory, "src");
        var genDir = PathHelpers.SafePathCombine(_testDirectory, "Generated");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(genDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "Real.cs"), "class Real {}");
        File.WriteAllText(PathHelpers.SafePathCombine(genDir, "Other.cs"), "class Other {}");
        File.WriteAllText(PathHelpers.SafePathCombine(genDir, "Special.cs"), "class Special {}");

        // Act — include all .cs, exclude Generated/, then re-include Generated/Special.cs
        var result = GlobMatcher.GetMatchingFiles(
            _testDirectory,
            ["**/*.cs", "!Generated/**", "Generated/Special.cs"]);

        // Assert — src/Real.cs and Generated/Special.cs are present; Generated/Other.cs is not
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Contains("src/Real.cs"));
        Assert.IsTrue(result.Contains("Generated/Special.cs"));
        Assert.IsFalse(result.Contains("Generated/Other.cs"));
    }
}
