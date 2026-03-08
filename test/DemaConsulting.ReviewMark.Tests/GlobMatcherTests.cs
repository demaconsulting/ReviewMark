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
        var baseDirectory = Path.GetTempPath();
        IReadOnlyList<string>? patterns = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            GlobMatcher.GetMatchingFiles(baseDirectory, patterns!));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that an empty patterns list returns an empty result.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList()
    {
        // Arrange
        var baseDirectory = Path.GetTempPath();
        IReadOnlyList<string> patterns = [];

        // Act
        var result = GlobMatcher.GetMatchingFiles(baseDirectory, patterns);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    /// <summary>
    ///     Test that a single include pattern returns all files matching that pattern.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles()
    {
        // Arrange — create a temporary directory with two .cs files and one .txt file
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Alpha.cs"), "class Alpha {}");
            File.WriteAllText(Path.Combine(tempDir, "Beta.cs"), "class Beta {}");
            File.WriteAllText(Path.Combine(tempDir, "readme.txt"), "readme");

            // Act — only match .cs files
            var result = GlobMatcher.GetMatchingFiles(tempDir, ["**/*.cs"]);

            // Assert — both .cs files are returned; the .txt file is not
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("Alpha.cs"));
            Assert.IsTrue(result.Contains("Beta.cs"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    ///     Test that an exclude pattern removes matching files from the result.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles()
    {
        // Arrange — create files in the root and a subdirectory that should be excluded
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var genDir = Path.Combine(tempDir, "Generated");
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(genDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Real.cs"), "class Real {}");
            File.WriteAllText(Path.Combine(genDir, "Generated.cs"), "class Generated {}");

            // Act — include everything but exclude the Generated subdirectory
            var result = GlobMatcher.GetMatchingFiles(tempDir, ["**/*.cs", "!Generated/**"]);

            // Assert — only Real.cs is returned
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains("Real.cs"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    ///     Test that multiple include patterns return all files matching any of the patterns.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching()
    {
        // Arrange — create .cs and .yaml files
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Program.cs"), "class Program {}");
            File.WriteAllText(Path.Combine(tempDir, "config.yaml"), "key: value");
            File.WriteAllText(Path.Combine(tempDir, "readme.txt"), "readme");

            // Act — match both .cs and .yaml files
            var result = GlobMatcher.GetMatchingFiles(tempDir, ["**/*.cs", "**/*.yaml"]);

            // Assert — both .cs and .yaml files are included; .txt is not
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("Program.cs"));
            Assert.IsTrue(result.Contains("config.yaml"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    ///     Test that a combination of include and exclude patterns returns only the filtered files.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles()
    {
        // Arrange — create files in root, src, and obj subdirectories
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var srcDir = Path.Combine(tempDir, "src");
        var objDir = Path.Combine(tempDir, "obj");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(objDir);
        try
        {
            File.WriteAllText(Path.Combine(srcDir, "Main.cs"), "class Main {}");
            File.WriteAllText(Path.Combine(objDir, "Main.obj.cs"), "// generated");

            // Act — include all .cs, exclude obj directory
            var result = GlobMatcher.GetMatchingFiles(tempDir, ["**/*.cs", "!obj/**"]);

            // Assert — only src/Main.cs is returned
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains("src/Main.cs"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    /// <summary>
    ///     Test that a pattern that does not match any files returns an empty list.
    /// </summary>
    [TestMethod]
    public void GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList()
    {
        // Arrange — create a directory with only .txt files
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "notes.txt"), "notes");

            // Act — search for .cs files (none exist)
            var result = GlobMatcher.GetMatchingFiles(tempDir, ["**/*.cs"]);

            // Assert — empty list because no .cs files are present
            Assert.AreEqual(0, result.Count);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
