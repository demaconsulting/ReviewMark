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

using System.Text;

namespace DemaConsulting.ReviewMark.Tests;

/// <summary>
///     Helper that generates minimal valid PDF files for testing purposes without
///     requiring the PDFsharp library.
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
        // Build PDF objects as strings.
        var catalog = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n";
        var pages = "2 0 obj\n<< /Type /Pages /Kids [] /Count 0 >>\nendobj\n";
        var escaped = keywords.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var info = $"3 0 obj\n<< /Keywords ({escaped}) >>\nendobj\n";

        // Lay out the file body and record byte offsets for the xref table.
        var header = "%PDF-1.4\n";
        int offset1 = header.Length;
        int offset2 = offset1 + catalog.Length;
        int offset3 = offset2 + pages.Length;
        int xrefOffset = offset3 + info.Length;

        // Build xref table.  Each entry MUST be exactly 20 bytes:
        // "NNNNNNNNNN GGGGG X \n"  (10 digits, space, 5 digits, space, 1 char, space, \n)
        var xref = new StringBuilder();
        xref.Append("xref\n");
        xref.Append("0 4\n");
        xref.Append("0000000000 65535 f \n");
        xref.AppendFormat("{0:D10} 00000 n \n", offset1);
        xref.AppendFormat("{0:D10} 00000 n \n", offset2);
        xref.AppendFormat("{0:D10} 00000 n \n", offset3);
        xref.Append($"trailer\n<< /Size 4 /Root 1 0 R /Info 3 0 R >>\nstartxref\n{xrefOffset}\n%%EOF\n");

        File.WriteAllText(path, header + catalog + pages + info + xref, Encoding.Latin1);
    }
}
