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

using PdfSharp.Pdf;

namespace DemaConsulting.ReviewMark.Tests;

/// <summary>
///     Helper that generates minimal valid PDF files for testing purposes.
/// </summary>
internal static class PdfTestHelper
{
    /// <summary>
    ///     Creates a minimal valid PDF file with the specified keywords in its Info dictionary.
    /// </summary>
    /// <param name="path">Destination file path.</param>
    /// <param name="keywords">Value to write into the PDF /Keywords entry.</param>
    internal static void CreateMinimalPdf(string path, string keywords)
    {
        // Use PDFsharp in tests because the production project already depends on it at runtime.
        using var document = new PdfDocument();
        document.Info.Keywords = keywords;
        document.AddPage();
        document.Save(path);
    }
}
