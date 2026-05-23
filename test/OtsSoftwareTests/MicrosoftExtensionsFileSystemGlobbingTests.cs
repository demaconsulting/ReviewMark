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

using Microsoft.Extensions.FileSystemGlobbing;

namespace OtsSoftwareTests;

/// <summary>
///     OTS software tests for <see cref="Matcher" /> from
///     Microsoft.Extensions.FileSystemGlobbing.
/// </summary>
public sealed class MicrosoftExtensionsFileSystemGlobbingTests : IDisposable
{
    /// <summary>
    ///     Unique temporary directory created before each test and deleted after.
    /// </summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of <see cref="MicrosoftExtensionsFileSystemGlobbingTests" />.
    /// </summary>
    public MicrosoftExtensionsFileSystemGlobbingTests()
    {
        _testDirectory = Path.Combine(
            Path.GetTempPath(),
            $"OtsFSGlobbingTests_{Guid.NewGuid()}");
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
    ///     Verifies that <c>**/*.cs</c> added via <see cref="Matcher.AddInclude" /> matches
    ///     files located in subdirectories, confirming double-wildcard traversal.
    /// </summary>
    [Fact]
    public void Matcher_GetResultsInFullPath_DoubleWildcard_MatchesFilesInSubdirectories()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "SubFolder");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "Alpha.cs"), "class Alpha {}");

        var matcher = new Matcher();
        matcher.AddInclude("**/*.cs");

        // Act
        var results = matcher.GetResultsInFullPath(_testDirectory).ToList();

        // Assert
        Assert.Single(results);
        Assert.Contains(results, r => r.EndsWith("Alpha.cs", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Verifies that <c>*.cs</c> added via <see cref="Matcher.AddInclude" /> matches only
    ///     files in the root directory and does not traverse subdirectories.
    /// </summary>
    [Fact]
    public void Matcher_GetResultsInFullPath_SingleWildcard_MatchesFilesInDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "Root.cs"), "class Root {}");
        var subDir = Path.Combine(_testDirectory, "SubFolder");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "Sub.cs"), "class Sub {}");

        var matcher = new Matcher();
        matcher.AddInclude("*.cs");

        // Act
        var results = matcher.GetResultsInFullPath(_testDirectory).ToList();

        // Assert — only the root-level file is returned
        Assert.Single(results);
        Assert.Contains(results, r => r.EndsWith("Root.cs", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Verifies that files matching a pattern added via <see cref="Matcher.AddExclude" />
    ///     are absent from the results, confirming exclusion behavior.
    /// </summary>
    [Fact]
    public void Matcher_GetResultsInFullPath_ExcludePattern_OmitsMatchingFiles()
    {
        // Arrange
        var genDir = Path.Combine(_testDirectory, "Generated");
        Directory.CreateDirectory(genDir);
        File.WriteAllText(Path.Combine(_testDirectory, "Real.cs"), "class Real {}");
        File.WriteAllText(Path.Combine(genDir, "Generated.cs"), "class Generated {}");

        var matcher = new Matcher();
        matcher.AddInclude("**/*.cs");
        matcher.AddExclude("Generated/**");

        // Act
        var results = matcher.GetResultsInFullPath(_testDirectory).ToList();

        // Assert — only Real.cs is returned; Generated.cs is excluded
        Assert.Single(results);
        Assert.Contains(results, r => r.EndsWith("Real.cs", StringComparison.Ordinal));
        Assert.DoesNotContain(results, r => r.EndsWith("Generated.cs", StringComparison.Ordinal));
    }
}
