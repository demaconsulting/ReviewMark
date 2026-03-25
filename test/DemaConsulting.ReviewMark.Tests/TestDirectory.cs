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
///     Represents a temporary directory that is automatically deleted when disposed.
/// </summary>
internal sealed class TestDirectory : IDisposable
{
    /// <summary>
    ///     Gets the path to the temporary directory.
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestDirectory" /> class.
    /// </summary>
    public TestDirectory()
    {
        DirectoryPath = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reviewmark_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(DirectoryPath);
    }

    /// <summary>
    ///     Deletes the temporary directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return;
        }

        try
        {
            Directory.Delete(DirectoryPath, recursive: true);
        }
        catch (IOException)
        {
            // Ignore cleanup failures in tests (e.g., transient file locks on Windows).
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup failures in tests (e.g., transient access issues on Windows).
        }
    }
}
