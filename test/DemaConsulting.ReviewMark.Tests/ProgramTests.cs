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
using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests;

/// <summary>
///     Unit tests for the Program class.
/// </summary>
[Collection("ConsoleTests")]
public class ProgramTests
{
    /// <summary>
    ///     Log file name used across lint tests.
    /// </summary>
    private const string LintLogFile = "lint.log";

    /// <summary>
    ///     Definition used by the --affected tests.
    /// </summary>
    private const string AffectedDefinition = """
        needs-review:
          - "src/**/*.cs"
        evidence-source:
          type: none
        reviews:
          - id: Core Logic
            title: Core
            paths:
              - "src/Core/**/*.cs"
        """;

    /// <summary>
    ///     Runs the program with the supplied arguments and optional stdin, capturing output.
    /// </summary>
    private static (string Output, string Error, int ExitCode) RunCaptured(string? stdin, params string[] args)
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        var originalIn = Console.In;
        try
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);
            if (stdin != null)
            {
                Console.SetIn(new StringReader(stdin));
            }

            using var context = Context.Create(args);
            Program.Run(context);
            return (outWriter.ToString(), errWriter.ToString(), context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            Console.SetIn(originalIn);
        }
    }

    /// <summary>
    ///     Test that --help lists the --affected option.
    /// </summary>
    [Fact]
    public void Program_Run_WithHelpFlag_IncludesAffectedOption()
    {
        // Arrange
        string[] args = ["--help"];

        // Act
        var (output, _, _) = RunCaptured(null, args);

        // Assert
        Assert.Contains("--affected", output);
    }

    /// <summary>
    ///     Test that --affected reading stdin prints tab-delimited affected and unreviewed lines
    ///     without a banner, and exits 0 even when unreviewed files exist.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_Stdin_OutputsAffectedAndUnreviewed()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);

        // Act — CRLF input, a leading ./, backslashes (Windows only), a blank line, and a file that does not exist
        var goneLine = Path.DirectorySeparatorChar == '\\' ? ".\\src\\Core\\Gone.cs" : "./src/Core/Gone.cs";
        var (output, _, exitCode) = RunCaptured(
            goneLine + "\r\n\r\nsrc/Other/O.cs\r\nbin/x.dll\r\n",
            "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("ReviewMark version", output);
        Assert.Equal(
            $"affected\tCore Logic\tsrc/Core/Gone.cs{Environment.NewLine}unreviewed\tsrc/Other/O.cs{Environment.NewLine}",
            output);
    }

    /// <summary>
    ///     Test that --affected reading a file works, and empty input reports no affected review-sets.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_EmptyFile_ReportsNoAffectedReviewSets()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);
        var changed = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "changed.txt");
        File.WriteAllText(changed, "");

        // Act
        var (output, _, exitCode) = RunCaptured(
            null, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", changed);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"No affected review-sets{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that --affected honors NUL-separated input and --silent.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_NulInputAndSilent_SuppressesOutput()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);
        var changed = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "changed.bin");
        File.WriteAllText(changed, "src/Core/A.cs\0src/Core/B.cs\0");

        // Act
        var (normal, _, normalExit) = RunCaptured(
            null, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", changed);
        var (silent, _, silentExit) = RunCaptured(
            null, "--silent", "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", changed);

        // Assert
        Assert.Equal(0, normalExit);
        Assert.Equal(0, silentExit);
        Assert.Equal(
            $"affected\tCore Logic\tsrc/Core/A.cs{Environment.NewLine}affected\tCore Logic\tsrc/Core/B.cs{Environment.NewLine}",
            normal);
        Assert.Equal("", silent);
    }

    /// <summary>
    ///     Test that --affected writes its records to the --log file.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_LogFile_WritesRecords()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);
        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "affected.log");

        // Act
        var (_, _, exitCode) = RunCaptured(
            "src/Core/A.cs\n",
            "--silent", "--log", logFile, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("affected\tCore Logic\tsrc/Core/A.cs", File.ReadAllText(logFile));
    }

    /// <summary>
    ///     Test that --affected with a missing definition file exits non-zero.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_MissingDefinition_ReportsError()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var missing = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "nope.yaml");

        // Act
        var (_, _, exitCode) = RunCaptured("src/Core/A.cs\n", "--definition", missing, "--affected", "-");

        // Assert
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    ///     Test that --affected with an unreadable list file throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_UnreadableInput_ThrowsInvalidOperationException()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);
        var missing = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "missing.txt");

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => RunCaptured(
            null, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", missing));

        // Assert
        Assert.StartsWith("Failed to read changed-files list", ex.Message);
    }

    /// <summary>
    ///     Test that --affected with an invalid path (embedded NUL) throws the wrapped InvalidOperationException.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_InvalidPath_ThrowsInvalidOperationException()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);

        // Act — a NUL cannot be passed through argv, so run in-process
        var ex = Assert.Throws<InvalidOperationException>(() => RunCaptured(
            null, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "bad\0name.txt"));

        // Assert
        Assert.StartsWith("Failed to read changed-files list", ex.Message);
    }

    /// <summary>
    ///     Test that --affected with --silent and an invalid definition still writes the error to stderr and exits 1.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_SilentInvalidDefinition_WritesStderrExitsOne()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, "reviews: [");

        // Act
        var (output, error, exitCode) = RunCaptured(
            "src/Core/A.cs\n", "--silent", "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(1, exitCode);
        Assert.NotEqual("", error.Trim());
        Assert.Equal("", output);
    }

    /// <summary>
    ///     Test that --affected reading stdin that fails throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_StdinReadFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);
        var originalIn = Console.In;
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetIn(new ThrowingReader());
            using var context = Context.Create(
                ["--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-"]);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => Program.Run(context));

            // Assert
            Assert.Contains("standard input", ex.Message);
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that lint warnings are not written to the --affected output stream.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_ConfigWarning_NotWrittenToOutput()
    {
        // Arrange — definition with a whitespace-only needs-review entry (lint warning)
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition.Replace(
            "needs-review:\n", "needs-review:\n  - \" \"\n").Replace(
            "needs-review:\r\n", "needs-review:\r\n  - \" \"\r\n"));

        // Act
        var (output, error, exitCode) = RunCaptured(
            "src/Core/A.cs\n", "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal("", error);
        Assert.Equal($"affected\tCore Logic\tsrc/Core/A.cs{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that special characters in names are escaped in records.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_EscapesSpecialCharacters()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: none
            reviews:
              - id: 'Back\Slash'
                title: Core
                paths:
                  - "src/**/*.cs"
            """);
        var changed = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "changed.bin");
        File.WriteAllText(changed, "src/a\tb\nc\rd.cs\0");

        // Act
        var (output, _, exitCode) = RunCaptured(
            null, "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", changed);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"affected\tBack\\\\Slash\tsrc/a\\tb\\nc\\rd.cs{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that a backslash in a changed file path is preserved and escaped on POSIX, and treated as a
    ///     separator on Windows.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_BackslashInPath_EscapedOrConvertedByPlatform()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);

        // Act
        var (output, _, exitCode) = RunCaptured(
            "src/Core/e\\f.cs\n",
            "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        if (Path.DirectorySeparatorChar == '\\')
        {
            Assert.Equal($"affected\tCore Logic\tsrc/Core/e/f.cs{Environment.NewLine}", output);
        }
        else
        {
            Assert.Equal($"affected\tCore Logic\tsrc/Core/e\\\\f.cs{Environment.NewLine}", output);
        }
    }

    /// <summary>
    ///     Test that duplicate paths (including ./ forms) are listed once.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_DuplicatePaths_ListedOnce()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);

        // Act
        var (output, _, exitCode) = RunCaptured(
            "src/Core/A.cs\n./src/Core/A.cs\nsrc/Core/A.cs\n",
            "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"affected\tCore Logic\tsrc/Core/A.cs{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that only unreviewed files produce the sentinel followed by unreviewed records.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_UnreviewedOnly_WritesSentinelAndUnreviewed()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, AffectedDefinition);

        // Act
        var (output, _, exitCode) = RunCaptured(
            "src/Other/O.cs\n",
            "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"No affected review-sets{Environment.NewLine}unreviewed\tsrc/Other/O.cs{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that an absolute path outside --dir is neither affected nor unreviewed, even when
    ///     needs-review is empty (everything is considered).
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_AbsolutePathOutsideDir_Ignored()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            evidence-source:
              type: none
            reviews:
              - id: Core Logic
                title: Core
                paths:
                  - "src/Core/**/*.cs"
            """);
        var outside = Path.GetFullPath(Path.Combine(tempDir.DirectoryPath, "..", "elsewhere", "x.cs")).Replace('\\', '/');

        // Act
        var (output, _, exitCode) = RunCaptured(
            outside + "\n",
            "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"No affected review-sets{Environment.NewLine}", output);
    }

    /// <summary>
    ///     Test that --affected combined with --help still prints the banner, while a normal
    ///     affected run does not.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_Help_PrintsBanner()
    {
        // Act
        var (helpOutput, _, helpExit) = RunCaptured(null, "--affected", "changed.txt", "--help");

        // Assert
        Assert.Equal(0, helpExit);
        Assert.Contains("ReviewMark version", helpOutput);
    }

    /// <summary>
    ///     Test that --affected combined with --validate still prints the banner.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_Validate_PrintsBanner()
    {
        // Act
        var (output, _, _) = RunCaptured(null, "--affected", "changed.txt", "--validate");

        // Assert
        Assert.Contains("ReviewMark version", output);
    }

    /// <summary>
    ///     Test that --affected with an invalid definition reports errors and exits 1.
    /// </summary>
    [Fact]
    public void Program_Run_WithAffectedFlag_InvalidDefinition_ExitsOne()
    {
        // Arrange
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, "reviews: [");

        // Act
        var (output, _, exitCode) = RunCaptured(
            "src/Core/A.cs\n", "--definition", definitionFile, "--dir", tempDir.DirectoryPath, "--affected", "-");

        // Assert
        Assert.Equal(1, exitCode);
        Assert.DoesNotContain("affected\t", output);
    }

    /// <summary>
    ///     A reader that always fails, simulating a broken standard input.
    /// </summary>
    private sealed class ThrowingReader : TextReader
    {
        /// <inheritdoc />
        public override string ReadToEnd() => throw new IOException("simulated failure");
    }

    /// <summary>
    ///     Test that Run with version flag displays version only.
    /// </summary>
    [Fact]
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
            Assert.Equal(Program.Version, output.Trim());
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
    [Fact]
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
    [Fact]
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

            // Assert — output contains the validation summary with a total test count and exit code is 0
            var output = outWriter.ToString();
            Assert.Contains("Total Tests:", output);
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with no arguments displays default behavior.
    /// </summary>
    [Fact]
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
    [Fact]
    public void Program_Version_ReturnsNonEmptyString()
    {
        // Act
        var version = Program.Version;

        // Assert — Version is a non-empty, non-whitespace string
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    /// <summary>
    ///     Test that Run with --help flag includes --elaborate in the usage information.
    /// </summary>
    [Fact]
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
    [Fact]
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
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with --elaborate and an unknown review-set ID exits with a non-zero code.
    /// </summary>
    [Fact]
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
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that Run with --help flag includes --lint in the usage information.
    /// </summary>
    [Fact]
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
    [Fact]
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

        // Assert — exit code is zero and log contains no output (no issues, no banner)
        var logContent = File.ReadAllText(logFile);
        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag does not print banner or copyright text.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlag_ValidConfig_SuppressesBanner()
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

        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--lint", "--definition", definitionFile]);

            // Act
            Program.Run(context);

            // Assert — successful lint output is fully silent
            var output = outWriter.ToString();
            Assert.Equal(string.Empty, output);
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with --lint flag on a missing definition file reports an error.
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("nonexistent.yaml", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag detects duplicate review set IDs and reports an error.
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("duplicate ID", logContent);
        Assert.Contains("Core-Logic", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag detects unknown evidence-source type and reports an error.
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("ftp", logContent);
        Assert.Contains("not supported", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports a clear error for corrupted (invalid) YAML.
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("definition.yaml:", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports a clear error when required fields are missing.
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("definition.yaml", logContent);
        Assert.Contains("evidence-source", logContent);
    }

    /// <summary>
    ///     Test that Run with --lint flag reports ALL errors in one pass when the file has
    ///     multiple detectable issues (missing evidence-source AND duplicate review IDs).
    /// </summary>
    [Fact]
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
        Assert.Equal(1, exitCode);
        Assert.Contains("evidence-source", logContent);
        Assert.Contains("duplicate ID", logContent);
        Assert.Contains("Core-Logic", logContent);
    }

    /// <summary>
    ///     Test that Run with --definition flag pointing to an invalid config reports lint errors and exits with code 1.
    /// </summary>
    [Fact]
    public void Program_Run_WithDefinitionFlag_InvalidConfig_ReportsLintError()
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

        var planFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "plan.md");
        var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "test.log");

        // Act — dispose the context before reading the log to release the file handle on Windows
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--definition", definitionFile, "--plan", planFile]))
        {
            Program.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert — non-zero exit code and log contains error mentioning evidence-source
        var logContent = File.ReadAllText(logFile);
        Assert.Equal(1, exitCode);
        Assert.Contains("error:", logContent);
        Assert.Contains("evidence-source", logContent);
    }

    /// <summary>
    ///     Test that Run sets exit code 1 when --enforce is set and the report has review issues.
    /// </summary>
    [Fact]
    public void Program_HandleIssues_WithEnforce_SetsExitCode1()
    {
        // Arrange — empty index means the report will have review issues (no current evidence)
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

        var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);
            using var context = Context.Create([
                "--silent",
                "--definition", definitionFile,
                "--report", reportFile,
                "--enforce"]);

            // Act
            Program.Run(context);

            // Assert — exit code is 1 when --enforce is set and review-set has no current evidence
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that Run emits a warning but exits with code 0 when review issues exist without --enforce.
    /// </summary>
    [Fact]
    public void Program_HandleIssues_WithoutEnforce_EmitsWarning()
    {
        // Arrange — empty index means the report will have review issues; without --enforce
        // the tool should emit a warning and exit with code 0
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

        var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create([
                "--definition", definitionFile,
                "--report", reportFile]);

            // Act
            Program.Run(context);

            // Assert — exit code is 0 and output contains "Warning:" when --enforce is not set
            Assert.Equal(0, context.ExitCode);
            Assert.Contains("Warning:", outWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run does nothing (exit code 0, no warnings) when there are no review issues —
    ///     both with and without --enforce.
    /// </summary>
    [Fact]
    public void Program_HandleIssues_NoIssues_DoesNothing()
    {
        // Arrange — definition with no needs-review files and no reviews produces HasIssues=false
        // for both plan and report, so HandleIssues is always called with hasIssues=false
        using var tempDir = new TestDirectory();
        var definitionFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "definition.yaml");
        File.WriteAllText(definitionFile, """
            needs-review: []
            evidence-source:
              type: none
            reviews: []
            """);

        var planFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "plan.md");
        var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");

        var originalOut = Console.Out;
        try
        {
            // Act (without --enforce) — no issues present; exit code must stay 0 with no warning
            using var outWriterNoEnforce = new StringWriter();
            Console.SetOut(outWriterNoEnforce);
            int exitCodeNoEnforce;
            using (var context = Context.Create([
                "--definition", definitionFile,
                "--plan", planFile,
                "--report", reportFile]))
            {
                Program.Run(context);
                exitCodeNoEnforce = context.ExitCode;
            }

            // Assert — exit code is 0 and no warning is emitted without --enforce
            Assert.Equal(0, exitCodeNoEnforce);
            Assert.DoesNotContain("Warning:", outWriterNoEnforce.ToString());

            // Act (with --enforce) — no issues present; exit code must still be 0 with no warning
            using var outWriterEnforce = new StringWriter();
            Console.SetOut(outWriterEnforce);
            int exitCodeEnforce;
            using (var context = Context.Create([
                "--definition", definitionFile,
                "--plan", planFile,
                "--report", reportFile,
                "--enforce"]))
            {
                Program.Run(context);
                exitCodeEnforce = context.ExitCode;
            }

            // Assert — exit code is 0 and no warning is emitted even with --enforce when no issues exist
            Assert.Equal(0, exitCodeEnforce);
            Assert.DoesNotContain("Warning:", outWriterEnforce.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with --index flag scans PDF evidence files and writes index.json.
    /// </summary>
    [Fact]
    public void Program_Run_WithIndexFlag_ScansAndWritesIndexFile()
    {
        // Arrange — create a temp directory to use as the working directory (no PDFs)
        using var tempDir = new TestDirectory();
        var indexFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");

        using var context = Context.Create([
            "--dir", tempDir.DirectoryPath,
            "--index", Path.Combine(tempDir.DirectoryPath, "**", "*.pdf")]);

        // Act
        Program.Run(context);

        // Assert — exits with code 0 and writes index.json to the working directory
        Assert.Equal(0, context.ExitCode);
        Assert.True(File.Exists(indexFile), "index.json was not created");
    }
}

/// <summary>
///     Collection definition that disables parallelization for Console-redirecting tests.
///     Ensures tests using Console.SetOut / Console.SetError run sequentially.
/// </summary>
[CollectionDefinition("ConsoleTests", DisableParallelization = true)]
public class ConsoleTestsCollection { }
