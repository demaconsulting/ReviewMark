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

using DemaConsulting.ReviewMark.Cli;
using DemaConsulting.ReviewMark.SelfTest;

namespace DemaConsulting.ReviewMark.Tests.SelfTest;

/// <summary>
///     Unit tests for the <see cref="Validation" /> class.
/// </summary>
[TestClass]
public class ValidationTests
{
    /// <summary>
    ///     Test that Run throws ArgumentNullException when context is null.
    /// </summary>
    [TestMethod]
    public void Validation_Run_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Validation.Run(null!));
    }

    /// <summary>
    ///     Test that Run writes a validation header containing system information.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WritesValidationHeader()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--validate"]);

            // Act
            Validation.Run(context);

            // Assert — output contains the markdown header and table headings
            var output = outWriter.ToString();
            Assert.Contains("DEMA Consulting ReviewMark", output);
            Assert.Contains("Tool Version", output);
            Assert.Contains("Machine Name", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run writes a summary with a total test count.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WritesSummaryWithTotalTests()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--validate"]);

            // Act
            Validation.Run(context);

            // Assert — output contains the summary section
            var output = outWriter.ToString();
            Assert.Contains("Total Tests:", output);
            Assert.Contains("Passed:", output);
            Assert.Contains("Failed:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run returns a zero exit code when all tests pass.
    /// </summary>
    [TestMethod]
    public void Validation_Run_AllTestsPass_ExitCodeIsZero()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--validate"]);

            // Act
            Validation.Run(context);

            // Assert — exit code is zero (no errors)
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run writes results to a TRX file when --results is provided with a .trx extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithTrxResultsFile_WritesFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"reviewmark-validation-{Guid.NewGuid()}.trx");
        try
        {
            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--validate", "--results", resultsFile]);

                // Act
                Validation.Run(context);

                // Assert — results file exists and has content
                Assert.IsTrue(File.Exists(resultsFile), "TRX results file was not created");
                var content = File.ReadAllText(resultsFile);
                Assert.IsFalse(string.IsNullOrWhiteSpace(content), "TRX results file is empty");
                Assert.Contains("TestRun", content);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }

    /// <summary>
    ///     Test that Run writes results to a JUnit XML file when --results is provided with a .xml extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithXmlResultsFile_WritesFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"reviewmark-validation-{Guid.NewGuid()}.xml");
        try
        {
            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--validate", "--results", resultsFile]);

                // Act
                Validation.Run(context);

                // Assert — results file exists and has content
                Assert.IsTrue(File.Exists(resultsFile), "XML results file was not created");
                var content = File.ReadAllText(resultsFile);
                Assert.IsFalse(string.IsNullOrWhiteSpace(content), "XML results file is empty");
                Assert.Contains("testsuites", content);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }
}
