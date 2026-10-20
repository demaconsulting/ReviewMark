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

using DemaConsulting.ReviewMark.Indexing;

namespace DemaConsulting.ReviewMark.Tests;

/// <summary>
///     Integration tests that run the ReviewMark application through dotnet.
/// </summary>
public class IntegrationTests
{
    private readonly string _dllPath;

    /// <summary>
    ///     Initializes a new instance of <see cref="IntegrationTests" />.
    /// </summary>
    public IntegrationTests()
    {
        var baseDir = AppContext.BaseDirectory;
        _dllPath = PathHelpers.SafePathCombine(baseDir, "DemaConsulting.ReviewMark.dll");
        Assert.True(File.Exists(_dllPath), $"Could not find ReviewMark DLL at {_dllPath}");
    }

    /// <summary>
    ///     Test that version flag outputs version information.
    /// </summary>
    [Fact]
    public void ReviewMark_VersionFlag_Invoked_OutputsVersion()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Assert — exit succeeds, output is non-empty, and contains no error or copyright text
        Assert.Equal(0, exitCode);
        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.DoesNotContain("Error", output);
        Assert.DoesNotContain("Copyright", output);
    }

    /// <summary>
    ///     Test that help flag outputs usage information.
    /// </summary>
    [Fact]
    public void ReviewMark_HelpFlag_Invoked_OutputsUsageInformation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Assert — exit succeeds and output contains usage, options, and version flag
        Assert.Equal(0, exitCode);
        Assert.Contains("Usage:", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--version", output);
    }

    /// <summary>
    ///     Test that validate flag runs self-validation.
    /// </summary>
    [Fact]
    public void ReviewMark_ValidateFlag_Invoked_RunsValidation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Assert — exit succeeds and output contains the validation summary
        Assert.Equal(0, exitCode);
        Assert.Contains("Total Tests:", output);
        Assert.Contains("Passed:", output);
    }

    /// <summary>
    ///     Test that validate with results flag generates TRX file.
    /// </summary>
    [Fact]
    public void ReviewMark_ValidateFlag_WithTrxResultsPath_GeneratesTrxFile()
    {
        // Arrange
        var resultsFile = Path.GetTempFileName();
        resultsFile = Path.ChangeExtension(resultsFile, ".trx");

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results",
                resultsFile);

            // Assert — exit succeeds, results file is created, and contains valid TRX XML
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "Results file was not created");

            var trxContent = File.ReadAllText(resultsFile);
            Assert.Contains("<TestRun", trxContent);
            Assert.Contains("</TestRun>", trxContent);
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
    ///     Test that silent flag suppresses output.
    /// </summary>
    [Fact]
    public void ReviewMark_SilentFlag_Invoked_SuppressesOutput()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--silent");

        // Assert — exit code is zero and console output is empty
        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, output.Trim());
    }

    /// <summary>
    ///     Test that log flag writes output to file.
    /// </summary>
    [Fact]
    public void ReviewMark_LogFlag_Invoked_WritesOutputToFile()
    {
        // Arrange
        var logFile = Path.GetTempFileName();

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--log",
                logFile);

            // Assert — exit succeeds, log file is created, and contains the version banner
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(logFile), "Log file was not created");

            var logContent = File.ReadAllText(logFile);
            Assert.Contains("ReviewMark version", logContent);
        }
        finally
        {
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
    }

    /// <summary>
    ///     Test that validate with results flag generates JUnit XML file.
    /// </summary>
    [Fact]
    public void ReviewMark_ValidateFlag_WithXmlResultsPath_GeneratesJUnitFile()
    {
        // Arrange
        var resultsFile = Path.GetTempFileName();
        resultsFile = Path.ChangeExtension(resultsFile, ".xml");

        try
        {
            // Act
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results",
                resultsFile);

            // Assert — exit succeeds, results file is created, and contains JUnit XML root element
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "Results file was not created");

            var xmlContent = File.ReadAllText(resultsFile);
            Assert.Contains("<testsuites", xmlContent);
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
    ///     Test that unknown argument returns error.
    /// </summary>
    [Fact]
    public void ReviewMark_UnknownArgument_Provided_ReturnsNonZeroAndError()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--unknown");

        // Assert — unknown argument produces exit code 1 and an error message
        Assert.Equal(1, exitCode);
        Assert.Contains("Error", output);
    }

    /// <summary>
    ///     Test that review plan generation writes a Markdown plan file.
    /// </summary>
    [Fact]
    public void ReviewMark_PlanFlag_WithDefinitionFile_GeneratesReviewPlan()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var planFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--plan",
                planFile);

            // Assert — exit succeeds and plan file contains review-set id
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(planFile), "Plan file was not created");
            var planContent = File.ReadAllText(planFile);
            Assert.Contains("Test-Review", planContent);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(planFile))
            {
                File.Delete(planFile);
            }
        }
    }

    /// <summary>
    ///     Test that review report generation writes a Markdown report file.
    /// </summary>
    [Fact]
    public void ReviewMark_ReportFlag_WithDefinitionFile_GeneratesReviewReport()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var reportFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--report",
                reportFile);

            // Assert — exit succeeds and report file contains review-set id
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(reportFile), "Report file was not created");
            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("Test-Review", reportContent);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that --enforce returns non-zero when reviews are not current.
    /// </summary>
    [Fact]
    public void ReviewMark_EnforceFlag_WithNoEvidence_ReturnsNonZero()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var reportFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act — enforce with no evidence returns non-zero exit code
            var exitCode = Runner.Run(
                out var _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--report",
                reportFile,
                "--enforce");

            // Assert — exit code is 1 because evidence source is 'none' so no reviews are current
            Assert.Equal(1, exitCode);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that --index scans a directory and creates an index.json.
    /// </summary>
    [Fact]
    public void ReviewMark_IndexFlag_OnEmptyDirectory_CreatesIndexJson()
    {
        // Arrange — create a temp directory to index (with no PDF files)
        using var tempDir = new TestDirectory();
        var indexFile = Path.Combine(tempDir.DirectoryPath, "index.json");

        // Act — index the empty directory
        var exitCode = Runner.Run(
            out _,
            "dotnet",
            _dllPath,
            "--dir",
            tempDir.DirectoryPath,
            "--index",
            Path.Combine(tempDir.DirectoryPath, "**", "*.pdf"));

        // Assert — exits successfully and produces index.json
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(indexFile), "index.json was not created");
    }

    /// <summary>
    ///     Test that --dir sets the working directory for file operations.
    /// </summary>
    [Fact]
    public void ReviewMark_DirFlag_Invoked_OverridesWorkingDirectory()
    {
        // Arrange — create a temp directory with a definition file
        using var tempDir = new TestDirectory();
        var defFile = Path.Combine(tempDir.DirectoryPath, ".reviewmark.yaml");
        var planFile = Path.Combine(tempDir.DirectoryPath, "plan.md");

        File.WriteAllText(defFile, """
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: none
            reviews:
              - id: Test-Review
                title: Test review
                paths:
                  - "src/**/*.cs"
            """);

        // Act — use --dir to point to temp directory containing the definition file
        var exitCode = Runner.Run(
            out _,
            "dotnet",
            _dllPath,
            "--dir",
            tempDir.DirectoryPath,
            "--plan",
            planFile);

        // Assert — exits successfully using the directory-relative definition file
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(planFile), "Plan file was not created");
    }

    /// <summary>
    ///     Test that --elaborate outputs elaboration for a valid review-set ID.
    /// </summary>
    [Fact]
    public void ReviewMark_ElaborateFlag_WithValidId_OutputsElaboration()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--elaborate",
                "Test-Review");

            // Assert — exits successfully and output contains the review-set id
            Assert.Equal(0, exitCode);
            Assert.Contains("Test-Review", output);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
        }
    }

    /// <summary>
    ///     Test that --affected lists the review sets affected by the files in a changed-files list.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_WithChangedFile_ListsAffectedReviewSets()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var listFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/Core/**/*.cs"
                """);
            File.WriteAllText(listFile, "src/Core/Deleted.cs\nsrc/Other.cs\n");

            // Act
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--affected",
                listFile);

            // Assert — exits 0 (unreviewed files are not an error) and reports both kinds of line
            Assert.Equal(0, exitCode);
            Assert.Contains("affected\tTest-Review\tsrc/Core/Deleted.cs", output);
            Assert.Contains("unreviewed\tsrc/Other.cs", output);
        }
        finally
        {
            File.Delete(defFile);
            File.Delete(listFile);
        }
    }

    /// <summary>
    ///     Writes a definition file and runs --affected in a process with optional stdin.
    /// </summary>
    private int RunAffectedProcess(
        string? stdin, string[] extraArgs, out string stdout, out string stderr, string? definition = null)
    {
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        try
        {
            File.WriteAllText(defFile, definition ?? """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/Core/**/*.cs"
                """);
            return Runner.Run(
                out stdout, out stderr, stdin, "dotnet", [_dllPath, "--definition", defFile, .. extraArgs]);
        }
        finally
        {
            File.Delete(defFile);
        }
    }

    /// <summary>
    ///     Test that --affected - reads the list from standard input.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_Stdin_ListsAffectedReviewSets()
    {
        // Arrange
        const string stdin = "src/Core/A.cs\r\n";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out var stderr);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"affected\tTest-Review\tsrc/Core/A.cs{Environment.NewLine}", stdout);
        Assert.Equal("", stderr);
    }

    /// <summary>
    ///     Test that --affected accepts a CRLF-separated named file without CR leaking into the output.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_NamedFileCrlf_ListsAffectedReviewSets()
    {
        // Arrange
        var listFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));
        try
        {
            File.WriteAllText(listFile, "src/Core/A.cs\r\nsrc/Other/O.cs\r\n");

            // Act
            var exitCode = RunAffectedProcess(null, ["--affected", listFile], out var stdout, out var stderr);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.DoesNotContain("\r\t", stdout);
            Assert.Equal(
                $"affected\tTest-Review\tsrc/Core/A.cs{Environment.NewLine}unreviewed\tsrc/Other/O.cs{Environment.NewLine}",
                stdout);
            Assert.Equal("", stderr);
        }
        finally
        {
            File.Delete(listFile);
        }
    }

    /// <summary>
    ///     Test that --affected accepts NUL-separated input from standard input.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_NulSeparatedInput_ListsAffectedReviewSets()
    {
        // Arrange
        const string stdin = "src/Core/A b.cs\0src/Core/C.cs\0";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(
            $"affected\tTest-Review\tsrc/Core/A b.cs{Environment.NewLine}affected\tTest-Review\tsrc/Core/C.cs{Environment.NewLine}",
            stdout);
    }

    /// <summary>
    ///     Test that a deleted (non-existent) path is still reported as affected.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_DeletedPath_ListsAffectedReviewSets()
    {
        // Arrange
        const string stdin = "src/Core/Deleted.cs\n";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("affected\tTest-Review\tsrc/Core/Deleted.cs", stdout);
    }

    /// <summary>
    ///     Test that an unreviewed path is reported and the exit code is zero.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_UnreviewedPath_ReportsUnreviewedExitZero()
    {
        // Arrange
        const string stdin = "src/Other/O.cs\n";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Contains("unreviewed\tsrc/Other/O.cs", stdout);
    }

    /// <summary>
    ///     Test that empty input reports no affected review-sets.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_EmptyInput_ReportsNoAffected()
    {
        // Arrange
        const string stdin = "";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"No affected review-sets{Environment.NewLine}", stdout);
    }

    /// <summary>
    ///     Test that non-matching input reports no affected review-sets.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_NoMatch_ReportsNoAffected()
    {
        // Arrange
        const string stdin = "docs/a.md\n";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"No affected review-sets{Environment.NewLine}", stdout);
    }

    /// <summary>
    ///     Test the exact stdout format: no banner, empty stderr.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_ExactOutputFormat_NoBannerNoStderr()
    {
        // Arrange
        const string stdin = "src/Core/A.cs\nsrc/Other/O.cs\n";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out var stderr);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(
            $"affected\tTest-Review\tsrc/Core/A.cs{Environment.NewLine}unreviewed\tsrc/Other/O.cs{Environment.NewLine}",
            stdout);
        Assert.Equal("", stderr);
    }

    /// <summary>
    ///     Test that backslash, TAB, LF and CR in names are escaped.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_EscapedNames_AreEscaped()
    {
        // Arrange — review-set ID with a backslash; path with TAB, LF and CR
        const string definition = """
            needs-review:
              - "src/**/*.cs"
            evidence-source:
              type: none
            reviews:
              - id: 'Back\Slash'
                title: Test review
                paths:
                  - "src/Core/**/*.cs"
            """;
        const string stdin = "src/Core/a\tb\nc\rd.cs\0";

        // Act
        var exitCode = RunAffectedProcess(stdin, ["--affected", "-"], out var stdout, out _, definition);

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal($"affected\tBack\\\\Slash\tsrc/Core/a\\tb\\nc\\rd.cs{Environment.NewLine}", stdout);
    }

    /// <summary>
    ///     Test that a missing input file exits 1 with an error on stderr.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_MissingInputFile_ExitsOne()
    {
        // Arrange
        var missing = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".txt");

        // Act
        var exitCode = RunAffectedProcess(null, ["--affected", missing], out var stdout, out var stderr);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.StartsWith("Error:", stderr);
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that --affected combined with each exclusive operation exits 1 with an error on stderr.
    /// </summary>
    [Theory]
    [InlineData("--plan", "x.md")]
    [InlineData("--report", "x.md")]
    [InlineData("--elaborate", "X")]
    [InlineData("--index", "*.pdf")]
    [InlineData("--lint", null)]
    public void ReviewMark_AffectedFlag_ProhibitedCombination_ExitsOne(string option, string? value)
    {
        // Arrange
        string[] extra = value == null ? ["--affected", "-", option] : ["--affected", "-", option, value];

        // Act
        var exitCode = RunAffectedProcess("", extra, out var stdout, out var stderr);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("--affected cannot be combined", stderr);
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that an option-like --affected value exits 1 with a usage error on stderr.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_OptionLikeValue_ExitsOne()
    {
        // Arrange
        string[] extra = ["--affected", "--lint"];

        // Act
        var exitCode = RunAffectedProcess("", extra, out var stdout, out var stderr);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("--affected requires", stderr);
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that an empty or whitespace-only --affected value exits 1 with a usage error on stderr.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReviewMark_AffectedFlag_EmptyOrWhitespaceValue_ExitsOne(string value)
    {
        // Arrange
        string[] extra = ["--affected", value];

        // Act
        var exitCode = RunAffectedProcess("", extra, out var stdout, out var stderr);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Contains("--affected requires", stderr);
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that an invalid definition without --silent reports an error on stderr and exits 1.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_InvalidDefinition_ReportsStderr()
    {
        // Arrange
        const string definition = "reviews: [";

        // Act
        var exitCode = RunAffectedProcess(
            "src/A.cs\n", ["--affected", "-"], out var stdout, out var stderr, definition);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.NotEqual("", stderr.Trim());
        Assert.Contains(": error:", stderr);
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that a missing definition file exits 1.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_MissingDefinition_ExitsOne()
    {
        // Arrange
        var missing = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".yaml");

        // Act
        var exitCode = Runner.Run(
            out var stdout, out var stderr, "src/A.cs\n", "dotnet", _dllPath, "--definition", missing, "--affected", "-");

        // Assert
        Assert.Equal(1, exitCode);
        Assert.Equal("", stdout);
        Assert.NotEqual("", stderr);
    }

    /// <summary>
    ///     Test that --silent does not hide an invalid-definition error in --affected mode.
    /// </summary>
    [Fact]
    public void ReviewMark_AffectedFlag_Silent_InvalidDefinition_ReportsStderr()
    {
        // Arrange
        const string definition = "reviews: [";

        // Act
        var exitCode = RunAffectedProcess(
            "src/A.cs\n", ["--silent", "--affected", "-"], out var stdout, out var stderr, definition);

        // Assert
        Assert.Equal(1, exitCode);
        Assert.NotEqual("", stderr.Trim());
        Assert.Equal("", stdout);
    }

    /// <summary>
    ///     Test that --depth flag sets the default heading depth across all generated documents.
    /// </summary>
    [Fact]
    public void ReviewMark_DepthFlag_Invoked_SetsDefaultHeadingDepth()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var planFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));
        var reportFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--plan",
                planFile,
                "--report",
                reportFile,
                "--depth",
                "2");

            // Assert — exit succeeds, plan and report both use ## (depth 2) headings
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(planFile), "Plan file was not created");
            Assert.True(File.Exists(reportFile), "Report file was not created");
            var planContent = File.ReadAllText(planFile);
            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("## Review Coverage", planContent);
            Assert.Contains("## Review Status", reportContent);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(planFile))
            {
                File.Delete(planFile);
            }
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that --plan-depth flag overrides the heading depth for the plan document only,
    ///     leaving the report at the global --depth.
    /// </summary>
    /// <remarks>Links to ReviewMark-System-PlanDepth.</remarks>
    [Fact]
    public void ReviewMark_PlanDepthFlag_Invoked_OverridesPlanHeadingOnly()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var planFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));
        var reportFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act — global depth is 1, plan-depth overrides to 2
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--plan",
                planFile,
                "--report",
                reportFile,
                "--depth",
                "1",
                "--plan-depth",
                "2");

            // Assert — exit succeeds; plan uses ## (depth 2), report stays at # (depth 1)
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(planFile), "Plan file was not created");
            Assert.True(File.Exists(reportFile), "Report file was not created");
            var planContent = File.ReadAllText(planFile);
            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("## Review Coverage", planContent);
            Assert.DoesNotContain("## Review Status", reportContent);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(planFile))
            {
                File.Delete(planFile);
            }
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that --report-depth flag overrides the heading depth for the report document only,
    ///     leaving the plan at the global --depth.
    /// </summary>
    /// <remarks>Links to ReviewMark-System-ReportDepth.</remarks>
    [Fact]
    public void ReviewMark_ReportDepthFlag_Invoked_OverridesReportHeadingOnly()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));
        var planFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));
        var reportFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".md"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act — global depth is 1, report-depth overrides to 2
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--plan",
                planFile,
                "--report",
                reportFile,
                "--depth",
                "1",
                "--report-depth",
                "2");

            // Assert — exit succeeds; report uses ## (depth 2), plan stays at # (depth 1)
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(planFile), "Plan file was not created");
            Assert.True(File.Exists(reportFile), "Report file was not created");
            var planContent = File.ReadAllText(planFile);
            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("## Review Status", reportContent);
            Assert.DoesNotContain("## Review Coverage", planContent);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
            if (File.Exists(planFile))
            {
                File.Delete(planFile);
            }
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that --depth flag sets the heading depth in the self-validation report.
    /// </summary>
    [Fact]
    public void ReviewMark_DepthFlag_WithValidate_SetsValidationHeadingDepth()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate",
            "--depth",
            "2");

        // Assert — exit succeeds and validation output uses ## (depth 2) heading
        Assert.Equal(0, exitCode);
        Assert.Contains("## DEMA Consulting ReviewMark", output);
    }

    /// <summary>
    ///     Test that --lint with a valid config reports success.
    /// </summary>
    [Fact]
    public void ReviewMark_LintFlag_WithValidConfig_ProducesNoOutput()
    {
        // Arrange
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: none
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--lint");

            // Assert — exits successfully and output is empty (no issues, no banner)
            Assert.Equal(0, exitCode);
            Assert.Equal(string.Empty, output);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
        }
    }

    /// <summary>
    ///     Test that --lint accepts a definition whose evidence-source location references a set
    ///     environment variable, and reports an error naming the variable when it is unset.
    /// </summary>
    [Fact]
    public void ReviewMark_LintFlag_EvidenceLocationEnvironmentVariable_ExpandsOrReportsError()
    {
        // Arrange — a definition file whose url location references an environment variable
        const string variable = "RM_INTEG_LOCATION_A1";
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));

        try
        {
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                evidence-source:
                  type: url
                  location: ${RM_INTEG_LOCATION_A1}/index.json
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            // Act — lint with the variable set, then unset
            Environment.SetEnvironmentVariable(variable, "https://evidence.example.com");
            var setExitCode = Runner.Run(out var setOutput, "dotnet", _dllPath, "--definition", defFile, "--lint");
            Environment.SetEnvironmentVariable(variable, null);
            var unsetExitCode = Runner.Run(out var unsetOutput, "dotnet", _dllPath, "--definition", defFile, "--lint");

            // Assert — success is silent; the unset variable fails lint and is named in the output
            Assert.Equal(0, setExitCode);
            Assert.Equal(string.Empty, setOutput);
            Assert.Equal(1, unsetExitCode);
            Assert.Contains(variable, unsetOutput);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, null);
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
        }
    }

    /// <summary>
    ///     Test that --lint with an invalid config file reports issue messages and no version banner.
    /// </summary>
    [Fact]
    public void ReviewMark_LintFlag_WithInvalidConfig_ReportsOnlyIssueMessages()
    {
        // Arrange — create a temp file with malformed YAML (invalid content triggers lint errors)
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));

        try
        {
            File.WriteAllText(defFile, """
                {{{this is not valid yaml
                """);

            // Act
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--lint");

            // Assert — non-zero exit code because the config is invalid
            Assert.Equal(1, exitCode);

            // Assert — at least one issue message appears in output
            Assert.Contains("error:", output);

            // Assert — version banner is NOT present (lint mode suppresses the banner)
            Assert.DoesNotContain("ReviewMark version", output);
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
        }
    }

    /// <summary>
    ///     Test that an invalid log file path causes Main() to return a non-zero exit code.
    /// </summary>
    [Fact]
    public void ReviewMark_LogFlag_WithInvalidPath_ReturnsNonZero()
    {
        // Arrange — construct a log path whose parent directory does not exist
        var nonExistentDir = Path.Combine(Path.GetTempPath(), $"reviewmark_missing_{Guid.NewGuid()}");
        var invalidLogPath = Path.Combine(nonExistentDir, "log.txt");

        // Act — Context.Create fails to open the log file and throws InvalidOperationException,
        //       which Main() catches and converts to exit code 1
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--log",
            invalidLogPath);

        // Assert — non-zero exit code and error message on stderr (captured by Runner)
        Assert.NotEqual(0, exitCode);
        Assert.Contains("Error", output);
    }

    /// <summary>
    ///     Test that multiple --index flags accumulate evidence across all specified paths.
    /// </summary>
    [Fact]
    public void ReviewMark_IndexFlag_WithRepeat_ScansAllPaths()
    {
        // Arrange — create a temp directory to index (with no PDF files)
        using var tempDir = new TestDirectory();
        var indexFile = Path.Combine(tempDir.DirectoryPath, "index.json");

        // Act — supply two --index glob patterns pointing to the same empty directory
        var exitCode = Runner.Run(
            out _,
            "dotnet",
            _dllPath,
            "--dir",
            tempDir.DirectoryPath,
            "--index",
            Path.Combine(tempDir.DirectoryPath, "sub1", "**", "*.pdf"),
            "--index",
            Path.Combine(tempDir.DirectoryPath, "sub2", "**", "*.pdf"));

        // Assert — exits successfully and produces a single merged index.json
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(indexFile), "index.json was not created");
    }
}
