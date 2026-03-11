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
///     Unit tests for the Program class.
/// </summary>
[TestClass]
public class ProgramTests
{
    /// <summary>
    ///     Test that Run with version flag displays version only.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithVersionFlag_DisplaysVersionOnly()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--version"]);

            // Act
            Program.Run(context);

            // Assert — output is exactly the version string; copyright and banner text are absent
            var output = outWriter.ToString();
            Assert.AreEqual(Program.Version, output.Trim());
            Assert.DoesNotContain("Copyright", output);
            Assert.DoesNotContain("ReviewMark version", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with help flag displays usage information.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithHelpFlag_DisplaysUsageInformation()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--help"]);

            // Act
            Program.Run(context);

            // Assert — output contains usage and options sections listing known flags
            var output = outWriter.ToString();
            Assert.Contains("Usage:", output);
            Assert.Contains("Options:", output);
            Assert.Contains("--version", output);
            Assert.Contains("--help", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with validate flag runs validation.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithValidateFlag_RunsValidation()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--validate"]);

            // Act
            Program.Run(context);

            // Assert — output contains the validation summary with a total test count
            var output = outWriter.ToString();
            Assert.Contains("Total Tests:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with no arguments displays default behavior.
    /// </summary>
    [TestMethod]
    public void Program_Run_NoArguments_DisplaysDefaultBehavior()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create([]);

            // Act
            Program.Run(context);

            // Assert — output contains the version banner and copyright notice
            var output = outWriter.ToString();
            Assert.Contains("ReviewMark version", output);
            Assert.Contains("Copyright", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that version property returns non-empty version string.
    /// </summary>
    [TestMethod]
    public void Program_Version_ReturnsNonEmptyString()
    {
        // Act
        var version = Program.Version;

        // Assert — Version is a non-empty, non-whitespace string
        Assert.IsFalse(string.IsNullOrWhiteSpace(version));
    }

    /// <summary>
    ///     Test that Run with --help flag includes --elaborate in the usage information.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithHelpFlag_IncludesElaborateOption()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--help"]);

            // Act
            Program.Run(context);

            // Assert — help text includes the --elaborate option
            var output = outWriter.ToString();
            Assert.Contains("--elaborate", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with --elaborate flag outputs the review set elaboration to the console.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithElaborateFlag_OutputsElaboration()
    {
        // Arrange — create temp directory with a definition file and source file
        var testDirectory = PathHelpers.SafePathCombine(
            Path.GetTempPath(), $"ProgramTests_Elaborate_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(testDirectory);
            var srcDir = PathHelpers.SafePathCombine(testDirectory, "src");
            Directory.CreateDirectory(srcDir);
            File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

            var indexFile = PathHelpers.SafePathCombine(testDirectory, "index.json");
            File.WriteAllText(indexFile, """{"reviews":[]}""");

            var definitionFile = PathHelpers.SafePathCombine(testDirectory, "definition.yaml");
            File.WriteAllText(definitionFile, $"""
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: fileshare
                  location: {indexFile}
                reviews:
                  - id: Core-Logic
                    title: Review of core business logic
                    paths:
                      - "src/**/*.cs"
                """);

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create([
                    "--definition", definitionFile,
                    "--dir", testDirectory,
                    "--elaborate", "Core-Logic"]);

                // Act
                Program.Run(context);

                // Assert — output contains the review set ID and fingerprint heading
                var output = outWriter.ToString();
                Assert.Contains("Core-Logic", output);
                Assert.Contains("Fingerprint", output);
                Assert.Contains("Files", output);
                Assert.AreEqual(0, context.ExitCode);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }

    /// <summary>
    ///     Test that Run with --elaborate and an unknown review-set ID exits with a non-zero code.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithElaborateFlag_UnknownId_ReportsError()
    {
        // Arrange — create temp directory with a definition file
        var testDirectory = PathHelpers.SafePathCombine(
            Path.GetTempPath(), $"ProgramTests_ElaborateUnknown_{Guid.NewGuid()}");
        try
        {
            Directory.CreateDirectory(testDirectory);

            var indexFile = PathHelpers.SafePathCombine(testDirectory, "index.json");
            File.WriteAllText(indexFile, """{"reviews":[]}""");

            var definitionFile = PathHelpers.SafePathCombine(testDirectory, "definition.yaml");
            File.WriteAllText(definitionFile, $"""
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: fileshare
                  location: {indexFile}
                reviews:
                  - id: Core-Logic
                    title: Review of core business logic
                    paths:
                      - "src/**/*.cs"
                """);

            var originalError = Console.Error;
            try
            {
                using var errWriter = new StringWriter();
                Console.SetError(errWriter);
                using var context = Context.Create([
                    "--silent",
                    "--definition", definitionFile,
                    "--elaborate", "Unknown-Id"]);

                // Act
                Program.Run(context);

                // Assert — non-zero exit code when the review-set ID is not found
                Assert.AreEqual(1, context.ExitCode);
            }
            finally
            {
                Console.SetError(originalError);
            }
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }
}
