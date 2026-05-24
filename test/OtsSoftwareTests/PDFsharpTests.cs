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
using PdfSharp.Pdf.IO;

namespace OtsSoftwareTests;

/// <summary>
///     OTS software tests for the PDFsharp PDF import library.
/// </summary>
public sealed class PDFsharpTests
{
    /// <summary>
    ///     Verifies that <see cref="PdfReader" /> opened in import mode exposes the Keywords
    ///     metadata field with the value that was written to the PDF.
    /// </summary>
    [Fact]
    public void PdfReader_Open_ImportMode_ExposesKeywordsField()
    {
        // Arrange — create a temporary PDF with a known Keywords value
        var pdfPath = Path.Combine(Path.GetTempPath(), $"OtsPdfTest_{Guid.NewGuid()}.pdf");
        try
        {
            using (var doc = new PdfDocument())
            {
                doc.Info.Keywords = "review-id=ABC fingerprint=XYZ";
                doc.AddPage();
                doc.Save(pdfPath);
            }

            // Act — open the PDF in import mode and read Keywords
            using var importedDoc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
            var keywords = importedDoc.Info.Keywords;

            // Assert — the Keywords field contains the expected value
            Assert.Equal("review-id=ABC fingerprint=XYZ", keywords);
        }
        finally
        {
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }
        }
    }

    /// <summary>
    ///     Verifies that opening a PDF that contains no Keywords field in import mode returns a
    ///     null or empty value, confirming graceful handling of absent metadata.
    /// </summary>
    [Fact]
    public void PdfReader_Open_ImportMode_NoKeywords_ReturnsNullOrEmpty()
    {
        // Arrange — create a temporary PDF without setting any Keywords value
        var pdfPath = Path.Combine(Path.GetTempPath(), $"OtsPdfTestNoKw_{Guid.NewGuid()}.pdf");
        try
        {
            using (var doc = new PdfDocument())
            {
                doc.AddPage();
                doc.Save(pdfPath);
            }

            // Act — open the PDF in import mode and read Keywords
            using var importedDoc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
            var keywords = importedDoc.Info.Keywords;

            // Assert — no Keywords field means null or empty is returned
            Assert.True(
                keywords == null || keywords.Length == 0,
                $"Expected null or empty Keywords but got: '{keywords}'");
        }
        finally
        {
            if (File.Exists(pdfPath))
            {
                File.Delete(pdfPath);
            }
        }
    }
}
