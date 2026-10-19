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

namespace DemaConsulting.ReviewMark.Tests.Configuration;

/// <summary>
///     Unit tests for the <see cref="GlobMatcher" /> class.
/// </summary>
public sealed class GlobMatcherTests : IDisposable
{
    /// <summary>
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of <see cref="GlobMatcherTests" />.
    /// </summary>
    public GlobMatcherTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"GlobMatcherTests_{Guid.NewGuid()}");
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
    ///     Test that IsMatch includes a path matching an include pattern.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_IncludePattern_MatchesPath()
    {
        // Act & Assert — deleted (non-existent) files match because no file system is consulted
        Assert.True(GlobMatcher.IsMatch("src/Sub/Deleted.cs", ["src/**/*.cs"]));
        Assert.False(GlobMatcher.IsMatch("docs/readme.md", ["src/**/*.cs"]));
    }

    /// <summary>
    ///     Test that IsMatch applies excludes and later re-includes in order.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_ExcludeAndReInclude_AppliesOrder()
    {
        // Arrange
        IReadOnlyList<string> excluded = ["src/**/*.cs", "!src/Gen/**"];
        IReadOnlyList<string> reIncluded = ["src/**/*.cs", "!src/Gen/**", "src/Gen/Keep.cs"];

        // Act & Assert
        Assert.True(GlobMatcher.IsMatch("src/A.cs", excluded));
        Assert.False(GlobMatcher.IsMatch("src/Gen/B.cs", excluded));
        Assert.True(GlobMatcher.IsMatch("src/Gen/Keep.cs", reIncluded));
        Assert.False(GlobMatcher.IsMatch("src/Gen/B.cs", reIncluded));
    }

    /// <summary>
    ///     Test that IsMatch with no patterns matches nothing.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_EmptyPatterns_ReturnsFalse()
    {
        Assert.False(GlobMatcher.IsMatch("src/A.cs", []));
    }

    /// <summary>
    ///     Test that IsMatch never matches paths outside the base directory.
    /// </summary>
    [Theory]
    [InlineData("/x/a.cs")]
    [InlineData("C:/x/a.cs")]
    [InlineData("C:\\x\\a.cs")]
    [InlineData("\\x\\a.cs")]
    [InlineData("../a.cs")]
    [InlineData("../../a.cs")]
    [InlineData("..")]
    [InlineData("src/../../outside.cs")]
    [InlineData("src/../a.cs")]
    [InlineData("src/..")]
    [InlineData("a/b/../../../c.cs")]
    public void GlobMatcher_IsMatch_OutsidePath_ReturnsFalse(string path)
    {
        Assert.False(GlobMatcher.IsMatch(path, ["**"]));
        Assert.False(GlobMatcher.IsMatch(path, ["**/*.cs"]));
    }

    /// <summary>
    ///     Test that names merely containing dots are still matched (negative control).
    /// </summary>
    [Theory]
    [InlineData("..foo/a.cs")]
    [InlineData("a..b/x.cs")]
    [InlineData("src/a..b.cs")]
    [InlineData("src/foo../x.cs")]
    public void GlobMatcher_IsMatch_DotDotPrefixedName_StillMatches(string path)
    {
        Assert.True(GlobMatcher.IsMatch(path, ["**/*.cs"]));
    }

    /// <summary>
    ///     Test that a backslash is a separator only on Windows.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_BackslashDotDot_PlatformSpecific()
    {
        var isWindows = Path.DirectorySeparatorChar == '\\';
        Assert.Equal(!isWindows, GlobMatcher.IsMatch("a\\..\\b.cs", ["**/*.cs"]));
        Assert.Equal(!isWindows, GlobMatcher.IsMatch("..\\b.cs", ["**/*.cs"]));
    }

    /// <summary>
    ///     Test that IsMatch rejects null arguments.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_NullArguments_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GlobMatcher.IsMatch(null!, ["*"]));
        Assert.Throws<ArgumentNullException>(() => GlobMatcher.IsMatch("a", null!));
    }

    /// <summary>
    ///     Test that IsMatch agrees with file-system resolution for the same patterns.
    /// </summary>
    [Fact]
    public void GlobMatcher_IsMatch_AgreesWithGetMatchingFiles()
    {
        // Arrange — a fixture tree and an ordered pattern list using ** , exclude and re-include
        string[] files = ["src/A.cs", "src/Sub/B.cs", "src/Gen/C.cs", "src/Gen/Keep.cs", "docs/d.md", "top.cs"];
        foreach (var file in files)
        {
            var full = Path.Combine(_testDirectory, file);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, "x");
        }

        IReadOnlyList<string> patterns = ["**/*.cs", "!src/Gen/**", "src/Gen/Keep.cs"];

        // Act
        var resolved = GlobMatcher.GetMatchingFiles(_testDirectory, patterns);
        var matched = files.Where(f => GlobMatcher.IsMatch(f, patterns)).Order(StringComparer.Ordinal).ToList();

        // Assert
        Assert.Equal(resolved, matched);
    }

    /// <summary>
    ///     Test that passing a null base directory throws <see cref="ArgumentNullException" />.
    /// </summary>
    [Fact]
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
    [Fact]
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
    ///     Test that passing an empty base directory throws <see cref="ArgumentException" />.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_EmptyBaseDirectory_ThrowsArgumentException()
    {
        // Arrange
        var baseDirectory = string.Empty;
        IReadOnlyList<string> patterns = ["**/*.cs"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            GlobMatcher.GetMatchingFiles(baseDirectory, patterns));
    }

    /// <summary>
    ///     Test that passing a whitespace-only base directory throws <see cref="ArgumentException" />.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_WhitespaceBaseDirectory_ThrowsArgumentException()
    {
        // Arrange
        var baseDirectory = "   ";
        IReadOnlyList<string> patterns = ["**/*.cs"];

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            GlobMatcher.GetMatchingFiles(baseDirectory, patterns));
    }

    /// <summary>
    ///     Test that an empty patterns list returns an empty result.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_EmptyPatterns_ReturnsEmptyList()
    {
        // Arrange
        IReadOnlyList<string> patterns = [];

        // Act
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, patterns);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    ///     Test that a single include pattern returns all files matching that pattern.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_SingleIncludePattern_ReturnsMatchingFiles()
    {
        // Arrange — create two .cs files and one .txt file in the test directory
        File.WriteAllText(Path.Combine(_testDirectory, "Alpha.cs"), "class Alpha {}");
        File.WriteAllText(Path.Combine(_testDirectory, "Beta.cs"), "class Beta {}");
        File.WriteAllText(Path.Combine(_testDirectory, "readme.txt"), "readme");

        // Act — only match .cs files
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs"]);

        // Assert — both .cs files are returned; the .txt file is not
        Assert.Equal(2, result.Count);
        Assert.Contains("Alpha.cs", result);
        Assert.Contains("Beta.cs", result);
    }

    /// <summary>
    ///     Test that an exclude pattern removes matching files from the result.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_ExcludePattern_ExcludesMatchingFiles()
    {
        // Arrange — create files in the root and a subdirectory that should be excluded
        var genDir = Path.Combine(_testDirectory, "Generated");
        Directory.CreateDirectory(genDir);
        File.WriteAllText(Path.Combine(_testDirectory, "Real.cs"), "class Real {}");
        File.WriteAllText(Path.Combine(genDir, "Generated.cs"), "class Generated {}");

        // Act — include everything but exclude the Generated subdirectory
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "!Generated/**"]);

        // Assert — only Real.cs is returned
        Assert.Single(result);
        Assert.Contains("Real.cs", result);
    }

    /// <summary>
    ///     Test that multiple include patterns return all files matching any of the patterns.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_MultipleIncludePatterns_ReturnsAllMatching()
    {
        // Arrange — create .cs, .yaml, and .txt files in the test directory
        File.WriteAllText(Path.Combine(_testDirectory, "Program.cs"), "class Program {}");
        File.WriteAllText(Path.Combine(_testDirectory, "config.yaml"), "key: value");
        File.WriteAllText(Path.Combine(_testDirectory, "readme.txt"), "readme");

        // Act — match both .cs and .yaml files
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "**/*.yaml"]);

        // Assert — both .cs and .yaml files are included; .txt is not
        Assert.Equal(2, result.Count);
        Assert.Contains("Program.cs", result);
        Assert.Contains("config.yaml", result);
    }

    /// <summary>
    ///     Test that a combination of include and exclude patterns returns only the filtered files.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_IncludeAndExclude_ReturnsFilteredFiles()
    {
        // Arrange — create files in src and obj subdirectories
        var srcDir = Path.Combine(_testDirectory, "src");
        var objDir = Path.Combine(_testDirectory, "obj");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(objDir);
        File.WriteAllText(Path.Combine(srcDir, "Main.cs"), "class Main {}");
        File.WriteAllText(Path.Combine(objDir, "Main.obj.cs"), "// generated");

        // Act — include all .cs, exclude obj directory
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs", "!obj/**"]);

        // Assert — only src/Main.cs is returned
        Assert.Single(result);
        Assert.Contains("src/Main.cs", result);
    }

    /// <summary>
    ///     Test that a pattern that does not match any files returns an empty list.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_NoMatchingFiles_ReturnsEmptyList()
    {
        // Arrange — create a .txt file (no .cs files)
        File.WriteAllText(Path.Combine(_testDirectory, "notes.txt"), "notes");

        // Act — search for .cs files (none exist)
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs"]);

        // Assert — empty list because no .cs files are present
        Assert.Empty(result);
    }

    /// <summary>
    ///     Test that an include pattern appearing after an exclude re-adds previously excluded files.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_ReIncludeAfterExclude_ReturnsReIncludedFiles()
    {
        // Arrange — create files in src and Generated directories
        var srcDir = Path.Combine(_testDirectory, "src");
        var genDir = Path.Combine(_testDirectory, "Generated");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(genDir);
        File.WriteAllText(Path.Combine(srcDir, "Real.cs"), "class Real {}");
        File.WriteAllText(Path.Combine(genDir, "Other.cs"), "class Other {}");
        File.WriteAllText(Path.Combine(genDir, "Special.cs"), "class Special {}");

        // Act — include all .cs, exclude Generated/, then re-include Generated/Special.cs
        var result = GlobMatcher.GetMatchingFiles(
            _testDirectory,
            ["**/*.cs", "!Generated/**", "Generated/Special.cs"]);

        // Assert — src/Real.cs and Generated/Special.cs are present; Generated/Other.cs is not
        Assert.Equal(2, result.Count);
        Assert.Contains("src/Real.cs", result);
        Assert.Contains("Generated/Special.cs", result);
        Assert.DoesNotContain("Generated/Other.cs", result);
    }

    /// <summary>
    ///     Test that returned relative paths use forward slashes as separators,
    ///     regardless of the host operating system's directory separator.
    /// </summary>
    [Fact]
    public void GlobMatcher_GetMatchingFiles_FileInSubdirectory_UsesForwardSlashSeparator()
    {
        // Arrange — create a file inside a subdirectory so the result contains a separator
        var subDir = Path.Combine(_testDirectory, "SubFolder");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "Alpha.cs"), "class Alpha {}");

        // Act
        var result = GlobMatcher.GetMatchingFiles(_testDirectory, ["**/*.cs"]);

        // Assert — path uses a forward slash, not the platform directory separator
        Assert.Single(result);
        Assert.Equal("SubFolder/Alpha.cs", result[0]);
    }
}
