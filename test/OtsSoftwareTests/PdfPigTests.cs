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

using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;

namespace OtsSoftwareTests;

/// <summary>
///     OTS software tests for the PdfPig PDF reading library.
/// </summary>
public sealed class PdfPigTests
{
    /// <summary>
    ///     Verifies that <see cref="PdfDocument" /> opened read-only exposes the Keywords
    ///     metadata field with the value that was written to the PDF.
    /// </summary>
    [Fact]
    public void PdfDocument_Open_ExposesKeywordsField()
    {
        // Arrange — create a temporary PDF with a known Keywords value
        var pdfPath = Path.Combine(Path.GetTempPath(), $"OtsPdfTest_{Guid.NewGuid()}.pdf");
        try
        {
            var builder = new PdfDocumentBuilder();
            builder.DocumentInformation.Keywords = "review-id=ABC fingerprint=XYZ";
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(pdfPath, builder.Build());

            // Act — open the PDF and read Keywords
            using var importedDoc = PdfDocument.Open(pdfPath);
            var keywords = importedDoc.Information.Keywords;

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
    ///     Verifies that opening a PDF that contains no Keywords field returns a
    ///     null or empty value, confirming graceful handling of absent metadata.
    /// </summary>
    [Fact]
    public void PdfDocument_Open_NoKeywords_ReturnsNullOrEmpty()
    {
        // Arrange — create a temporary PDF without setting any Keywords value
        var pdfPath = Path.Combine(Path.GetTempPath(), $"OtsPdfTestNoKw_{Guid.NewGuid()}.pdf");
        try
        {
            var builder = new PdfDocumentBuilder();
            builder.AddPage(PageSize.A4);
            File.WriteAllBytes(pdfPath, builder.Build());

            // Act — open the PDF and read Keywords
            using var importedDoc = PdfDocument.Open(pdfPath);
            var keywords = importedDoc.Information.Keywords;

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
