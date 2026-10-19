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

using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests.Indexing;

/// <summary>
///     Tests for the PathHelpers class.
/// </summary>
public class PathHelpersTests
{
    /// <summary>
    ///     Test that ParseChangedPaths handles LF, CRLF, blanks and de-duplication, and preserves whitespace.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_NewlineSeparated_PreservesWhitespaceRemovesCrAndSorts()
    {
        // Act
        var result = PathHelpers.ParseChangedPaths("b.cs\r\n\r\n  a.cs  \n\nb.cs\n", "/repo");

        // Assert
        Assert.Equal(["  a.cs  ", "b.cs"], result);
    }

    /// <summary>
    ///     Test that ParseChangedPaths auto-detects NUL separation, keeps newlines inside names and CR in NUL mode.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_NulSeparated_SplitsOnNul()
    {
        // Act
        var result = PathHelpers.ParseChangedPaths("src/a b.cs\0src/c\nd.cs\0src/e\r\0", "/repo");

        // Assert
        Assert.Equal(["src/a b.cs", "src/c\nd.cs", "src/e\r"], result);
    }

    /// <summary>
    ///     Test that empty input yields no paths.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_EmptyInput_ReturnsEmpty()
    {
        // Act
        var empty = PathHelpers.ParseChangedPaths("", "/repo");
        var crlfOnly = PathHelpers.ParseChangedPaths("\r\n\r\n", "/repo");

        // Assert
        Assert.Empty(empty);
        Assert.Empty(crlfOnly);
    }

    /// <summary>
    ///     Test that a leading BOM is removed once and a lone CR entry is dropped.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_BomAndCrOnly_Removed()
    {
        // Act
        var result = PathHelpers.ParseChangedPaths("\uFEFFa.cs\r\n\r\n", "/repo");

        // Assert
        Assert.Equal(["a.cs"], result);
    }

    /// <summary>
    ///     Test that leading and trailing spaces are preserved in names.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_LeadingTrailingSpaces_Preserved()
    {
        // Act
        var result = PathHelpers.ParseChangedPaths(" a.cs \n", "/repo");

        // Assert
        Assert.Equal([" a.cs "], result);
    }

    /// <summary>
    ///     Test that whitespace-only entries are kept as names.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_WhitespaceOnlyEntry_Kept()
    {
        // Act
        var result = PathHelpers.ParseChangedPaths(" \n", "/repo");

        // Assert
        Assert.Equal([" "], result);
    }

    /// <summary>
    ///     Test that different spellings of the same path are de-duplicated.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_DuplicateForms_Deduplicated()
    {
        // Act — the backslash form is a duplicate only where backslash is the directory separator
        var windows = Path.DirectorySeparatorChar == '\\';
        var result = PathHelpers.ParseChangedPaths("a.cs\n./a.cs\n" + (windows ? ".\\a.cs\n" : ""), "/repo");

        // Assert
        Assert.Equal(["a.cs"], result);
    }

    /// <summary>
    ///     Test that EscapeRecordField escapes backslash, TAB, LF and CR reversibly.
    /// </summary>
    [Fact]
    public void PathHelpers_EscapeRecordField_SpecialCharacters_EscapedAndReversible()
    {
        // Arrange
        const string original = "a\\b\tc\nd\re\\t";

        // Act
        var escaped = PathHelpers.EscapeRecordField(original);

        // Assert
        Assert.Equal("a\\\\b\\tc\\nd\\re\\\\t", escaped);
        Assert.Equal("plain/path.cs", PathHelpers.EscapeRecordField("plain/path.cs"));

        var unescaped = System.Text.RegularExpressions.Regex.Replace(
            escaped,
            @"\\(.)",
            m => m.Groups[1].Value switch { "t" => "\t", "n" => "\n", "r" => "\r", var c => c });

        Assert.Equal(original, unescaped);
    }

    /// <summary>
    ///     Test that EscapeRecordField rejects null.
    /// </summary>
    [Fact]
    public void PathHelpers_EscapeRecordField_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.EscapeRecordField(null!));
    }

    /// <summary>
    ///     Test that NormalizeChangedPath converts backslashes, strips leading ./ segments and does not trim.
    /// </summary>
    [Fact]
    public void PathHelpers_NormalizeChangedPath_BackslashesAndDotSlash_Normalized()
    {
        // Act
        var backslashes = PathHelpers.NormalizeChangedPath(".\\src\\Sub\\A.cs", "/repo");
        var dotSlash = PathHelpers.NormalizeChangedPath("./././src/A.cs", "/repo");
        var whitespace = PathHelpers.NormalizeChangedPath("   \r", "/repo");

        // Assert
        if (Path.DirectorySeparatorChar == '\\')
        {
            Assert.Equal("src/Sub/A.cs", backslashes);
        }
        else
        {
            // Backslash is an ordinary file-name character on POSIX and is preserved
            Assert.Equal(".\\src\\Sub\\A.cs", backslashes);
        }

        Assert.Equal("src/A.cs", dotSlash);
        Assert.Equal("   \r", whitespace);
    }

    /// <summary>
    ///     Test that absolute paths under the directory are made relative and others are kept.
    /// </summary>
    [Fact]
    public void PathHelpers_NormalizeChangedPath_AbsolutePath_MadeRelativeToDirectory()
    {
        // Arrange
        var directory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo-root"));
        var inside = Path.Combine(directory, "src", "A.cs");

        // Act
        var result = PathHelpers.NormalizeChangedPath(inside, directory);

        // Assert
        Assert.Equal("src/A.cs", result);
    }

    /// <summary>
    ///     Test that absolute paths outside the directory are retained unchanged (forward slashes).
    /// </summary>
    [Fact]
    public void PathHelpers_NormalizeChangedPath_AbsoluteOutsideDirectory_RetainedUnchanged()
    {
        // Arrange
        var directory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "repo-root"));
        var outside = Path.GetFullPath(Path.Combine(directory, "..", "elsewhere", "B.cs"));

        // Act
        var result = PathHelpers.NormalizeChangedPath(outside, directory);

        // Assert
        Assert.Equal(Path.DirectorySeparatorChar == '\\' ? outside.Replace('\\', '/') : outside, result);
    }

    /// <summary>
    ///     Test that ParseChangedPaths rejects a null list.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_NullText_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.ParseChangedPaths(null!, "/repo"));
    }

    /// <summary>
    ///     Test that ParseChangedPaths rejects a null directory.
    /// </summary>
    [Fact]
    public void PathHelpers_ParseChangedPaths_NullDirectory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.ParseChangedPaths("a", null!));
    }

    /// <summary>
    ///     Test that NormalizeChangedPath rejects a null path.
    /// </summary>
    [Fact]
    public void PathHelpers_NormalizeChangedPath_NullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.NormalizeChangedPath(null!, "/repo"));
    }

    /// <summary>
    ///     Test that NormalizeChangedPath rejects a null directory.
    /// </summary>
    [Fact]
    public void PathHelpers_NormalizeChangedPath_NullDirectory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.NormalizeChangedPath("a", null!));
    }

    /// <summary>
    ///     Test that SafePathCombine correctly combines valid paths.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "subfolder/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert — result equals the expected combined path
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path traversal with double dots.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "../etc/passwd";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path with double dots in middle.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "subfolder/../../../etc/passwd";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for Unix absolute paths.
    ///     Runs on all platforms.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_AbsoluteUnixPath_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "/etc/passwd";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for Windows absolute paths.
    ///     Runs on Windows only.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_AbsoluteWindowsPath_ThrowsArgumentException()
    {
        // Skip on non-Windows platforms — Windows drive-rooted paths are only absolute on Windows
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows drive-rooted path test runs on Windows only");

        // Arrange
        var basePath = "C:\\Users\\project";
        var relativePath = "C:\\Windows\\System32\\file.txt";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath));
        Assert.Contains("Invalid path component", exception.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles current directory reference.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "./subfolder/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert — current directory reference is preserved in the combined path
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles nested paths.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "level1/level2/level3/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert — nested path segments are combined correctly
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles empty relative path.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert — empty relative path results in the base path unchanged
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentNullException when basePath is null.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException()
    {
        // Arrange
        string? basePath = null;
        var relativePath = "subfolder/file.txt";

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            PathHelpers.SafePathCombine(basePath!, relativePath));
#pragma warning restore CS8604
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentNullException when relativePath is null.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException()
    {
        // Arrange
        var basePath = "/home/user/project";
        string? relativePath = null;

        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument — intentional for this test
        Assert.Throws<ArgumentNullException>(() =>
            PathHelpers.SafePathCombine(basePath, relativePath!));
#pragma warning restore CS8604
    }
}
