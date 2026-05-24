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

namespace DemaConsulting.ReviewMark.Tests.Cli;

/// <summary>
///     Subsystem integration tests for the CLI subsystem (Context + Program).
/// </summary>
public class CliTests
{
    /// <summary>
    ///     Static readonly array for the unknown argument used in CLI error handling tests.
    /// </summary>
    private static readonly string[] UnknownArgArray = ["--unknown-arg-xyz"];

    /// <summary>
    ///     Test that the CLI correctly outputs only the version string when --version is supplied.
    /// </summary>
    [Fact]
    public void Cli_VersionFlag_FlagSupplied_OutputsVersionOnly()
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

            // Assert — output is the version string with no banner or copyright
            var output = outWriter.ToString();
            Assert.Equal(Program.Version, output.Trim());
            Assert.DoesNotContain("Copyright", output);
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the CLI outputs usage information when --help is supplied.
    /// </summary>
    [Fact]
    public void Cli_HelpFlag_FlagSupplied_OutputsUsageInformation()
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

            // Assert — output contains usage and options sections
            var output = outWriter.ToString();
            Assert.Contains("Usage:", output);
            Assert.Contains("Options:", output);
            Assert.Contains("--version", output);
            Assert.Contains("--help", output);
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the CLI runs self-validation when --validate is supplied.
    /// </summary>
    [Fact]
    public void Cli_ValidateFlag_FlagSupplied_RunsValidation()
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

            // Assert — output contains validation summary and exit code is zero
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
    ///     Test that the CLI suppresses all console output when --silent is supplied.
    /// </summary>
    [Fact]
    public void Cli_SilentFlag_FlagSupplied_SuppressesOutput()
    {
        // Arrange
        var originalOut = Console.Out;
        var originalError = Console.Error;
        try
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);
            using var context = Context.Create(["--silent"]);

            // Act
            Program.Run(context);

            // Assert — no output written to stdout or stderr; exit code is zero
            Assert.Equal(string.Empty, outWriter.ToString());
            Assert.Equal(string.Empty, errWriter.ToString());
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that --results flag generates a TRX file.
    /// </summary>
    [Fact]
    public void Cli_ResultsFlag_FlagSupplied_GeneratesTrxFile()
    {
        // Arrange
        var resultsFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.trx");

        try
        {
            using var context = Context.Create(["--validate", "--results", resultsFile]);

            // Act
            Program.Run(context);

            // Assert — exit code is zero and results file contains TRX content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(resultsFile), "Results file was not created");
            var content = File.ReadAllText(resultsFile);
            Assert.Contains("<TestRun", content);
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
    ///     Test that --log flag writes output to a log file.
    /// </summary>
    [Fact]
    public void Cli_LogFlag_FlagSupplied_WritesOutputToFile()
    {
        // Arrange
        var logFile = Path.GetTempFileName();

        try
        {
            int exitCode;
            using (var context = Context.Create(["--log", logFile]))
            {
                // Act
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // context is disposed here — log file is closed and safe to read
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
    ///     Test that unknown argument causes error output to stderr.
    /// </summary>
    [Fact]
    public void Cli_ErrorOutput_UnknownArg_WritesToStderr()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            // Note: This uses reflection to invoke the internal Main method. If the method signature changes,
            // mainMethod will be null and Assert.NotNull(mainMethod) will catch the regression.
            var mainMethod = typeof(Program).GetMethod(
                "Main",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(mainMethod);

            // Act — invoke the real CLI entrypoint so invalid args are handled exactly
            // as they are in production, including writing parse errors to stderr.
            var result = mainMethod.Invoke(null, [UnknownArgArray]);
            var exitCode = result is int code ? code : 0;

            // Assert — invalid args should return a failure exit code and write an error to stderr
            var stderr = errWriter.ToString();
            Assert.NotEqual(0, exitCode);
            Assert.Contains("Error:", stderr);
            Assert.Contains("--unknown-arg-xyz", stderr);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that invalid arguments produce a non-zero exit code.
    /// </summary>
    [Fact]
    public void Cli_InvalidArgs_UnknownArgSupplied_ReturnsNonZeroExitCode()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            // Note: This uses reflection to invoke the internal Main method. If the method signature changes,
            // mainMethod will be null and Assert.NotNull(mainMethod) will catch the regression.
            var mainMethod = typeof(Program).GetMethod(
                "Main",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(mainMethod);

            // Act — invoke the real CLI entrypoint with an invalid argument so the exit
            // code is produced by the actual production code path, not a simulation
            var result = mainMethod.Invoke(null, [UnknownArgArray]);
            var exitCode = result is int code ? code : 0;

            // Assert — non-zero exit code for invalid arguments
            Assert.NotEqual(0, exitCode);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that exit code is non-zero when an error occurs.
    /// </summary>
    [Fact]
    public void Cli_ExitCode_ErrorReported_ReturnsNonZeroExitCode()
    {
        // Arrange
        using var context = Context.Create([]);

        // Act — WriteError sets the exit code to 1
        context.WriteError("Simulated error for exit code test");

        // Assert — exit code is non-zero
        Assert.NotEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --definition flag loads the specified definition file.
    /// </summary>
    [Fact]
    public void Cli_DefinitionFlag_FlagSupplied_LoadsSpecifiedFile()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--plan", planFile]);

                // Act
                Program.Run(context);

                // Assert — exits with zero and plan file created from specified definition
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(planFile), "Plan file was not created");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --plan flag generates a review plan file.
    /// </summary>
    [Fact]
    public void Cli_PlanFlag_FlagSupplied_GeneratesReviewPlan()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--plan", planFile]);

                // Act
                Program.Run(context);

                // Assert — plan file exists and contains review-set id
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(planFile), "Plan file was not created");
                var planContent = File.ReadAllText(planFile);
                Assert.Contains("Test-Review", planContent);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --report flag generates a review report file.
    /// </summary>
    [Fact]
    public void Cli_ReportFlag_FlagSupplied_GeneratesReviewReport()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--report", reportFile]);

                // Act
                Program.Run(context);

                // Assert — report file exists and contains review-set id
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(reportFile), "Report file was not created");
                var reportContent = File.ReadAllText(reportFile);
                Assert.Contains("Test-Review", reportContent);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --enforce flag exits with non-zero when reviews are not current.
    /// </summary>
    [Fact]
    public void Cli_EnforceFlag_FlagSupplied_ExitsNonZeroWhenNotCurrent()
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

            var originalOut = Console.Out;
            var originalError = Console.Error;
            try
            {
                using var outWriter = new StringWriter();
                using var errWriter = new StringWriter();
                Console.SetOut(outWriter);
                Console.SetError(errWriter);
                using var context = Context.Create(["--definition", defFile, "--report", reportFile, "--enforce"]);

                // Act
                Program.Run(context);

                // Assert — non-zero exit code because evidence source is 'none'
                Assert.NotEqual(0, context.ExitCode);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
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
    ///     Test that --dir flag sets the working directory for file operations.
    /// </summary>
    [Fact]
    public void Cli_DirFlag_FlagSupplied_SetsWorkingDirectory()
    {
        // Arrange — create a temp directory with a .reviewmark.yaml file
        var tmpDir = Path.Combine(Path.GetTempPath(), $"reviewmark_cli_{Guid.NewGuid()}");
        Directory.CreateDirectory(tmpDir);
        var defFile = Path.Combine(tmpDir, ".reviewmark.yaml");
        var planFile = Path.Combine(tmpDir, "plan.md");

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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--dir", tmpDir, "--plan", planFile]);

                // Act
                Program.Run(context);

                // Assert — exits successfully using directory-relative definition file
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(planFile), "Plan file was not created");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, recursive: true);
            }
        }
    }

    /// <summary>
    ///     Test that --elaborate flag outputs elaboration for a valid review-set.
    /// </summary>
    [Fact]
    public void Cli_ElaborateFlag_ValidId_OutputsElaboration()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--elaborate", "Test-Review"]);

                // Act
                Program.Run(context);

                // Assert — exits successfully and output contains review-set id
                Assert.Equal(0, context.ExitCode);
                var output = outWriter.ToString();
                Assert.Contains("Test-Review", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --lint flag reports success for a valid config.
    /// </summary>
    [Fact]
    public void Cli_LintFlag_ValidConfig_ReportsSuccess()
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

            var originalOut = Console.Out;
            var originalError = Console.Error;
            try
            {
                using var outWriter = new StringWriter();
                using var errWriter = new StringWriter();
                Console.SetOut(outWriter);
                Console.SetError(errWriter);
                using var context = Context.Create(["--definition", defFile, "--lint"]);

                // Act
                Program.Run(context);

                // Assert — exits successfully and produces no output (no issues, no banner)
                Assert.Equal(0, context.ExitCode);
                var output = outWriter.ToString();
                Assert.Equal(string.Empty, output);
                Assert.Equal(string.Empty, errWriter.ToString());
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
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
    ///     Test that --index flag scans and creates index.json.
    /// </summary>
    [Fact]
    public void Cli_IndexFlag_FlagSupplied_CreatesIndexJson()
    {
        // Arrange — create a temp directory to index
        var tmpDir = Path.Combine(Path.GetTempPath(), $"reviewmark_index_{Guid.NewGuid()}");
        Directory.CreateDirectory(tmpDir);
        var indexFile = Path.Combine(tmpDir, "index.json");

        try
        {
            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create([
                    "--dir", tmpDir,
                    "--index", Path.Combine(tmpDir, "**", "*.pdf")]);

                // Act
                Program.Run(context);

                // Assert — exits successfully and index.json was created
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(indexFile), "index.json was not created");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            if (Directory.Exists(tmpDir))
            {
                Directory.Delete(tmpDir, recursive: true);
            }
        }
    }

    /// <summary>
    ///     Test that --plan-depth flag sets the heading depth in the generated review plan.
    /// </summary>
    [Fact]
    public void Cli_PlanDepthFlag_FlagSupplied_SetsHeadingDepth()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--plan", planFile, "--plan-depth", "2"]);

                // Act
                Program.Run(context);

                // Assert — plan file uses ## (depth 2) headings
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(planFile), "Plan file was not created");
                var planContent = File.ReadAllText(planFile);
                Assert.Contains("## Review Coverage", planContent);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --report-depth flag sets the heading depth in the generated review report.
    /// </summary>
    [Fact]
    public void Cli_ReportDepthFlag_FlagSupplied_SetsHeadingDepth()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--report", reportFile, "--report-depth", "2"]);

                // Act
                Program.Run(context);

                // Assert — report file uses ## (depth 2) headings
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(reportFile), "Report file was not created");
                var reportContent = File.ReadAllText(reportFile);
                Assert.Contains("## Review Status", reportContent);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that --depth flag sets the default heading depth for the generated review plan.
    /// </summary>
    [Fact]
    public void Cli_DepthFlag_FlagSupplied_SetsDefaultHeadingDepth()
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

            var originalOut = Console.Out;
            try
            {
                using var outWriter = new StringWriter();
                Console.SetOut(outWriter);
                using var context = Context.Create(["--definition", defFile, "--plan", planFile, "--depth", "2"]);

                // Act
                Program.Run(context);

                // Assert — plan file uses ## (depth 2) headings because --depth 2 sets the default
                Assert.Equal(0, context.ExitCode);
                Assert.True(File.Exists(planFile), "Plan file was not created");
                var planContent = File.ReadAllText(planFile);
                Assert.Contains("## Review Coverage", planContent);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
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
    ///     Test that creating a Context with no arguments returns a context with all default values.
    /// </summary>
    [Fact]
    public void Cli_Context_NoArgs_Parsed()
    {
        // Act — create a context with no arguments
        using var context = Context.Create([]);

        // Assert — all default values are set correctly
        Assert.False(context.Version);
        Assert.False(context.Help);
        Assert.False(context.Silent);
        Assert.False(context.Validate);
        Assert.False(context.Lint);
        Assert.False(context.Enforce);
        Assert.Null(context.PlanFile);
        Assert.Null(context.ReportFile);
        Assert.Null(context.DefinitionFile);
        Assert.Null(context.WorkingDirectory);
        Assert.Null(context.ElaborateId);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --depth with a value below the minimum (0) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_DepthFlag_BelowMinimum_ThrowsArgumentException()
    {
        // Act & Assert — depth 0 is below the minimum of 1
        Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "0"]));
    }

    /// <summary>
    ///     Test that --depth with a value above the maximum (6) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_DepthFlag_AboveMaximum_ThrowsArgumentException()
    {
        // Act & Assert — depth 6 exceeds the maximum of 5
        Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "6"]));
    }

    /// <summary>
    ///     Test that --plan-depth with a value below the minimum (0) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_PlanDepthFlag_BelowMinimum_ThrowsArgumentException()
    {
        // Act & Assert — plan-depth 0 is below the minimum of 1
        Assert.Throws<ArgumentException>(() => Context.Create(["--plan-depth", "0"]));
    }

    /// <summary>
    ///     Test that --plan-depth with a value above the maximum (6) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_PlanDepthFlag_AboveMaximum_ThrowsArgumentException()
    {
        // Act & Assert — plan-depth 6 exceeds the maximum of 5
        Assert.Throws<ArgumentException>(() => Context.Create(["--plan-depth", "6"]));
    }

    /// <summary>
    ///     Test that --report-depth with a value below the minimum (0) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_ReportDepthFlag_BelowMinimum_ThrowsArgumentException()
    {
        // Act & Assert — report-depth 0 is below the minimum of 1
        Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth", "0"]));
    }

    /// <summary>
    ///     Test that --report-depth with a value above the maximum (6) throws ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_ReportDepthFlag_AboveMaximum_ThrowsArgumentException()
    {
        // Act & Assert — report-depth 6 exceeds the maximum of 5
        Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth", "6"]));
    }

    /// <summary>
    ///     Test that --lint flag with an invalid config reports issue messages.
    /// </summary>
    [Fact]
    public void Cli_LintFlag_InvalidConfig_ReportsIssueMessages()
    {
        // Arrange — create a definition file with a malformed YAML structure
        var defFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".yaml"));

        try
        {
            // Write a YAML file that is syntactically valid but missing evidence-source
            File.WriteAllText(defFile, """
                needs-review:
                  - "src/**/*.cs"
                reviews:
                  - id: Test-Review
                    title: Test review
                    paths:
                      - "src/**/*.cs"
                """);

            var originalOut = Console.Out;
            var originalError = Console.Error;
            try
            {
                using var outWriter = new StringWriter();
                using var errWriter = new StringWriter();
                Console.SetOut(outWriter);
                Console.SetError(errWriter);
                using var context = Context.Create(["--definition", defFile, "--lint"]);

                // Act
                Program.Run(context);

                // Assert — exits with non-zero exit code and issue messages appear in error output
                Assert.NotEqual(0, context.ExitCode);
                var stderr = errWriter.ToString();
                Assert.Contains("evidence-source", stderr);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        finally
        {
            if (File.Exists(defFile))
            {
                File.Delete(defFile);
            }
        }
    }
}
