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

using System.Xml.Linq;
using DemaConsulting.ReviewMark.Cli;
using DemaConsulting.ReviewMark.SelfTest;

namespace DemaConsulting.ReviewMark.Tests.SelfTest;

/// <summary>
///     Subsystem integration tests for the SelfTest subsystem.
/// </summary>
public class SelfTestTests
{
    /// <summary>
    ///     Test that running self-validation passes all tests and exits with code zero.
    /// </summary>
    [Fact]
    public void SelfTest_Run_AllTestsPass_ExitCodeIsZero()
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

            // Assert
            Assert.Equal(0, context.ExitCode);
            var outString = outWriter.ToString();
            Assert.Contains("Total Tests:", outString);
            Assert.Contains("Passed:", outString);
            Assert.Contains("Failed:", outString);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that running self-validation with --results creates a TRX results file.
    /// </summary>
    [Fact]
    public void SelfTest_Run_GeneratesResultsFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"reviewmark-selftest-{Guid.NewGuid()}.trx");
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

                // Assert
                Assert.True(File.Exists(resultsFile), "Results file was not created");
                var content = File.ReadAllText(resultsFile);
                var doc = XDocument.Parse(content);
                Assert.Equal("TestRun", doc.Root?.Name.LocalName);
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
    ///     Test that the process exit code is non-zero when self-validation encounters an error.
    ///     Since all built-in validation tests pass in a correctly functioning environment, this
    ///     test uses an unsupported results-file format (.csv) to trigger a controlled WriteError
    ///     within the validation run, exercising the same exit-code mechanism as a test failure.
    /// </summary>
    [Fact]
    public void SelfTest_Run_UnsupportedResultsFormat_ExitCodeIsNonZero()
    {
        // Arrange — an unsupported results file extension causes WriteResultsFile to call
        // context.WriteError, which sets the exit code to 1 via the same path used for test failures.
        var originalOut = Console.Out;
        var originalError = Console.Error;
        try
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);
            using var context = Context.Create(["--validate", "--results", "unsupported-format.csv"]);

            // Act
            Validation.Run(context);

            // Assert — exit code is non-zero when the validation process calls WriteError
            Assert.NotEqual(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }
}
