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
