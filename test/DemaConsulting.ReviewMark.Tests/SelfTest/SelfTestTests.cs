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
///     Subsystem integration tests for the SelfTest subsystem.
/// </summary>
[TestClass]
public class SelfTestTests
{
    /// <summary>
    ///     Test that running self-validation passes all tests and exits with code zero.
    /// </summary>
    [TestMethod]
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
            Assert.AreEqual(0, context.ExitCode);
            Assert.Contains("Total Tests:", outWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that running self-validation with --results creates a TRX results file.
    /// </summary>
    [TestMethod]
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
                Assert.IsTrue(File.Exists(resultsFile), "Results file was not created");
                var content = File.ReadAllText(resultsFile);
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
}
