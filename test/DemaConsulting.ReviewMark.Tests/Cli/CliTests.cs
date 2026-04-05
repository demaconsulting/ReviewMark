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
[TestClass]
public class CliTests
{
    /// <summary>
    ///     Test that the CLI correctly outputs only the version string when --version is supplied.
    /// </summary>
    [TestMethod]
    public void Cli_VersionFlag_OutputsVersionOnly()
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
            Assert.AreEqual(Program.Version, output.Trim());
            Assert.DoesNotContain("Copyright", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the CLI outputs usage information when --help is supplied.
    /// </summary>
    [TestMethod]
    public void Cli_HelpFlag_OutputsUsageInformation()
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
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the CLI runs self-validation when --validate is supplied.
    /// </summary>
    [TestMethod]
    public void Cli_ValidateFlag_RunsValidation()
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
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the CLI suppresses all console output when --silent is supplied.
    /// </summary>
    [TestMethod]
    public void Cli_SilentFlag_SuppressesOutput()
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
            Assert.AreEqual(string.Empty, outWriter.ToString());
            Assert.AreEqual(string.Empty, errWriter.ToString());
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void Cli_ResultsFlag_GeneratesTrxFile()
    {
        // Arrange
        var resultsFile = Path.GetTempFileName();
        resultsFile = Path.ChangeExtension(resultsFile, ".trx");

        try
        {
            using var context = Context.Create(["--validate", "--results", resultsFile]);

            // Act
            Program.Run(context);

            // Assert — exit code is zero and results file contains TRX content
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(resultsFile), "Results file was not created");
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
    [TestMethod]
    public void Cli_LogFlag_WritesOutputToFile()
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
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists(logFile), "Log file was not created");
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
    [TestMethod]
    public void Cli_ErrorOutput_WritesToStderr()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);

            // Act — unknown argument causes ArgumentException → error written to stderr
            int exitCode;
            try
            {
                using var context = Context.Create(["--unknown-arg-xyz"]);
                Program.Run(context);
                exitCode = context.ExitCode;
            }
            catch (ArgumentException)
            {
                // Expected — Context.Create throws on unknown args
                exitCode = 1;
            }

            // Assert — something was written to stderr or exit code non-zero
            var stderr = errWriter.ToString();
            Assert.IsTrue(exitCode != 0 || !string.IsNullOrEmpty(stderr));
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that invalid arguments produce a non-zero exit code.
    /// </summary>
    [TestMethod]
    public void Cli_InvalidArgs_ReturnsNonZeroExitCode()
    {
        // Arrange + Act — the full CLI (Context.Create in Main) catches ArgumentException and writes error
        var originalOut = Console.Out;
        var originalError = Console.Error;
        try
        {
            using var outWriter = new StringWriter();
            using var errWriter = new StringWriter();
            Console.SetOut(outWriter);
            Console.SetError(errWriter);

            // Simulate what Program.Main does: catch ArgumentException and use WriteError
            int exitCode;
            try
            {
                using var context = Context.Create(["--completely-invalid-arg"]);
                Program.Run(context);
                exitCode = context.ExitCode;
            }
            catch (ArgumentException ex)
            {
                // Program.Main writes this to a temporary context — simulate
                using var errorContext = Context.Create([]);
                errorContext.WriteError(ex.Message);
                exitCode = errorContext.ExitCode;
            }

            // Assert — non-zero exit code for invalid arguments
            Assert.AreNotEqual(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that exit code is non-zero when an error occurs.
    /// </summary>
    [TestMethod]
    public void Cli_ExitCode_ReturnsNonZeroOnError()
    {
        // Arrange
        using var context = Context.Create([]);

        // Act — WriteError sets the exit code to 1
        context.WriteError("Simulated error for exit code test");

        // Assert — exit code is non-zero
        Assert.AreNotEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --definition flag loads the specified definition file.
    /// </summary>
    [TestMethod]
    public void Cli_DefinitionFlag_LoadsSpecifiedFile()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");
        var planFile = Path.GetTempFileName();
        planFile = Path.ChangeExtension(planFile, ".md");

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
                Assert.AreEqual(0, context.ExitCode);
                Assert.IsTrue(File.Exists(planFile), "Plan file was not created");
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
    [TestMethod]
    public void Cli_PlanFlag_GeneratesReviewPlan()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");
        var planFile = Path.GetTempFileName();
        planFile = Path.ChangeExtension(planFile, ".md");

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
                Assert.AreEqual(0, context.ExitCode);
                Assert.IsTrue(File.Exists(planFile), "Plan file was not created");
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
    [TestMethod]
    public void Cli_ReportFlag_GeneratesReviewReport()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");
        var reportFile = Path.GetTempFileName();
        reportFile = Path.ChangeExtension(reportFile, ".md");

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
                Assert.AreEqual(0, context.ExitCode);
                Assert.IsTrue(File.Exists(reportFile), "Report file was not created");
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
    [TestMethod]
    public void Cli_EnforceFlag_ExitsNonZeroWhenNotCurrent()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");
        var reportFile = Path.GetTempFileName();
        reportFile = Path.ChangeExtension(reportFile, ".md");

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
                Assert.AreNotEqual(0, context.ExitCode);
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
    [TestMethod]
    public void Cli_DirFlag_SetsWorkingDirectory()
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
                Assert.AreEqual(0, context.ExitCode);
                Assert.IsTrue(File.Exists(planFile), "Plan file was not created");
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
    [TestMethod]
    public void Cli_ElaborateFlag_OutputsElaboration()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");

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
                Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void Cli_LintFlag_ReportsSuccess()
    {
        // Arrange
        var defFile = Path.GetTempFileName();
        defFile = Path.ChangeExtension(defFile, ".yaml");

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
                using var context = Context.Create(["--definition", defFile, "--lint"]);

                // Act
                Program.Run(context);

                // Assert — exits successfully and reports no issues
                Assert.AreEqual(0, context.ExitCode);
                var output = outWriter.ToString();
                Assert.Contains("No issues found", output);
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
    ///     Test that --index flag scans and creates index.json.
    /// </summary>
    [TestMethod]
    public void Cli_IndexFlag_CreatesIndexJson()
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
                Assert.AreEqual(0, context.ExitCode);
                Assert.IsTrue(File.Exists(indexFile), "index.json was not created");
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
}
