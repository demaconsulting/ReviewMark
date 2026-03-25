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
    ///     Log file name used across lint tests.
    /// </summary>
    private const string LintLogFile = "lint.log";
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
        using var tempDir = new TestDirectory();
        var srcDir = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "A.cs"), "class A {}");

        var indexFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
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
                "--dir", tempDir.DirectoryPath,
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

    /// <summary>
    ///     Test that Run with --elaborate and an unknown review-set ID exits with a non-zero code.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithElaborateFlag_UnknownId_ReportsError()
    {
        // Arrange — create temp directory with a definition file
        using var tempDir = new TestDirectory();

        var indexFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
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

    /// <summary>
    ///     Test that Run with --help flag includes --lint in the usage information.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithHelpFlag_IncludesLintOption()
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

            // Assert — help text includes the --lint option
            var output = outWriter.ToString();
            Assert.Contains("--lint", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with --lint flag on a valid definition file reports success.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_ValidConfig_ReportsSuccess()
    {
        // Arrange — create temp directory with a valid definition file
        using var tempDir = new TestDirectory();
        var indexFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
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

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — exit code is zero and log contains success message
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(0, exitCode);
        Assert.Contains("No issues found", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag on a missing definition file reports an error.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_MissingConfig_ReportsError()
    {
        // Arrange — use a non-existent definition file
        using var tempDir = new TestDirectory();
        var nonExistentFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "nonexistent.yaml");
        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", nonExistentFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains an error mentioning the missing file
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("nonexistent.yaml", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag detects duplicate review set IDs and reports an error.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_DuplicateIds_ReportsError()
    {
        // Arrange — create temp directory with a definition file containing duplicate IDs
        using var tempDir = new TestDirectory();
        var indexFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
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
              - id: Core-Logic
                title: Duplicate review set
                paths:
                  - "src/**/*.cs"
            """);

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains a clear duplicate-ID error message
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("duplicate ID", logContent);
        Assert.Contains("Core-Logic", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag detects unknown evidence-source type and reports an error.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_UnknownSourceType_ReportsError()
    {
        // Arrange — create temp directory with a definition file having an unknown source type
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: ftp
              location: ftp://example.com/index.json
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """);

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains a clear unsupported-type error message
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("ftp", logContent);
        Assert.Contains("not supported", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports a clear error for corrupted (invalid) YAML.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_CorruptedYaml_ReportsError()
    {
        // Arrange — create a definition file with invalid YAML syntax
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            {{{this is not valid yaml
            """);

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains an error naming the definition file and a line number
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("definition.yaml:", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports a clear error when required fields are missing.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_MissingEvidenceSource_ReportsError()
    {
        // Arrange — create a definition file with no evidence-source block
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            needs-review:
              - "src/**/*.cs"
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
            """);

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log names the file and the missing field
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("definition.yaml", logContent);
        Assert.Contains("evidence-source", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports ALL errors in one pass when the file has
    ///     multiple detectable issues (missing evidence-source AND duplicate review IDs).
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_MultipleErrors_ReportsAll()
    {
        // Arrange — create a definition file that is missing evidence-source AND has duplicate IDs
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            needs-review:
              - "src/**/*.cs"
            reviews:
              - id: Core-Logic
                title: Review of core business logic
                paths:
                  - "src/**/*.cs"
              - id: Core-Logic
                title: Duplicate review set
                paths:
                  - "src/**/*.cs"
            """);

        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, LintLogFile);

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--lint", "--definition", definitionFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains BOTH the missing evidence-source error
        // AND the duplicate ID error, proving all errors are accumulated in one pass.
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(1, exitCode);
        Assert.Contains("evidence-source", logContent);
        Assert.Contains("duplicate ID", logContent);
        Assert.Contains("Core-Logic", logContent);
    }
}
