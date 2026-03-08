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

namespace DemaConsulting.TemplateDotNetTool.Tests;

/// <summary>
///     Tests for the PathHelpers class.
/// </summary>
[TestClass]
public class PathHelpersTests
{
    /// <summary>
    ///     Test that SafePathCombine correctly combines valid paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "subfolder/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path traversal with double dots.
    /// </summary>
    [TestMethod]
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
    [TestMethod]
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
    ///     Test that SafePathCombine throws ArgumentException for absolute paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_AbsolutePath_ThrowsArgumentException()
    {
        // Test Unix absolute path
        var unixBasePath = "/home/user/project";
        var unixRelativePath = "/etc/passwd";
        var unixException = Assert.Throws<ArgumentException>(() =>
            PathHelpers.SafePathCombine(unixBasePath, unixRelativePath));
        Assert.Contains("Invalid path component", unixException.Message);

        // Test Windows absolute path (only on Windows since Windows paths may not be rooted on Unix)
        if (OperatingSystem.IsWindows())
        {
            var windowsBasePath = "C:\\Users\\project";
            var windowsRelativePath = "C:\\Windows\\System32\\file.txt";
            var windowsException = Assert.Throws<ArgumentException>(() =>
                PathHelpers.SafePathCombine(windowsBasePath, windowsRelativePath));
            Assert.Contains("Invalid path component", windowsException.Message);
        }
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles current directory reference.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_CurrentDirectoryReference_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "./subfolder/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles nested paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_NestedPaths_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "level1/level2/level3/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine correctly handles empty relative path.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_EmptyRelativePath_ReturnsBasePath()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual(Path.Combine(basePath, relativePath), result);
    }
}
