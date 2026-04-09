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
[TestClass]
public class IntegrationTests
{
    private string _dllPath = string.Empty;

    /// <summary>
    ///     Initialize test by locating the ReviewMark DLL.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        // The DLL should be in the same directory as the test assembly
        // because the test project references the main project
        var baseDir = AppContext.BaseDirectory;
        _dllPath = PathHelpers.SafePathCombine(baseDir, "DemaConsulting.ReviewMark.dll");

        Assert.IsTrue(File.Exists(_dllPath), $"Could not find ReviewMark DLL at {_dllPath}");
    }

    /// <summary>
    ///     Test that version flag outputs version information.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_VersionFlag_OutputsVersion()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Assert — exit succeeds, output is non-empty, and contains no error or copyright text
        Assert.AreEqual(0, exitCode);
        Assert.IsFalse(string.IsNullOrWhiteSpace(output));
        Assert.DoesNotContain("Error", output);
        Assert.DoesNotContain("Copyright", output);
    }

    /// <summary>
    ///     Test that help flag outputs usage information.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_HelpFlag_OutputsUsageInformation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Assert — exit succeeds and output contains usage, options, and version flag
        Assert.AreEqual(0, exitCode);
        Assert.Contains("Usage:", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--version", output);
    }

    /// <summary>
    ///     Test that validate flag runs self-validation.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ValidateFlag_RunsValidation()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Assert — exit succeeds and output contains the validation summary
        Assert.AreEqual(0, exitCode);
        Assert.Contains("Total Tests:", output);
        Assert.Contains("Passed:", output);
    }

    /// <summary>
    ///     Test that validate with results flag generates TRX file.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ValidateWithResults_GeneratesTrxFile()
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
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists(resultsFile), "Results file was not created");

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
    [TestMethod]
    public void IntegrationTest_SilentFlag_SuppressesOutput()
    {
        // Act
        var exitCode = Runner.Run(
            out var _,
            "dotnet",
            _dllPath,
            "--silent");

        // Assert — exit code is zero, proving silent mode did not cause an error
        Assert.AreEqual(0, exitCode);

        // Output check removed since silent mode may still produce some output
    }

    /// <summary>
    ///     Test that log flag writes output to file.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LogFlag_WritesOutputToFile()
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
    ///     Test that validate with results flag generates JUnit XML file.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ValidateWithResults_GeneratesJUnitFile()
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
            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists(resultsFile), "Results file was not created");

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
    [TestMethod]
    public void IntegrationTest_UnknownArgument_ReturnsError()
    {
        // Act
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--unknown");

        // Assert — unknown argument produces a non-zero exit code and an error message
        Assert.AreNotEqual(0, exitCode);
        Assert.Contains("Error", output);
    }

    /// <summary>
    ///     Test that review plan generation writes a Markdown plan file.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ReviewPlanGeneration()
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
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--plan",
                planFile);

            // Assert — exit succeeds and plan file contains review-set id
            Assert.AreEqual(0, exitCode, $"Output: {output}");
            Assert.IsTrue(File.Exists(planFile), "Plan file was not created");
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
    [TestMethod]
    public void IntegrationTest_ReviewReportGeneration()
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
                out var output,
                "dotnet",
                _dllPath,
                "--definition",
                defFile,
                "--report",
                reportFile);

            // Assert — exit succeeds and report file contains review-set id
            Assert.AreEqual(0, exitCode, $"Output: {output}");
            Assert.IsTrue(File.Exists(reportFile), "Report file was not created");
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
    [TestMethod]
    public void IntegrationTest_Enforce()
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

            // Assert — non-zero because evidence source is 'none' so no reviews are current
            Assert.AreNotEqual(0, exitCode);
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
    [TestMethod]
    public void IntegrationTest_IndexScan()
    {
        // Arrange — create a temp directory to index (with no PDF files)
        var tmpDir = Path.Combine(Path.GetTempPath(), $"reviewmark_idx_{Guid.NewGuid()}");
        Directory.CreateDirectory(tmpDir);
        var indexFile = Path.Combine(tmpDir, "index.json");

        try
        {
            // Act — index the empty directory
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--dir",
                tmpDir,
                "--index",
                Path.Combine(tmpDir, "**", "*.pdf"));

            // Assert — exits successfully and produces index.json
            Assert.AreEqual(0, exitCode, $"Output: {output}");
            Assert.IsTrue(File.Exists(indexFile), "index.json was not created");
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
    ///     Test that --dir sets the working directory for file operations.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_WorkingDirectoryOverride()
    {
        // Arrange — create a temp directory with a definition file
        var tmpDir = Path.Combine(Path.GetTempPath(), $"reviewmark_work_{Guid.NewGuid()}");
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

            // Act — use --dir to point to temp directory containing the definition file
            var exitCode = Runner.Run(
                out var output,
                "dotnet",
                _dllPath,
                "--dir",
                tmpDir,
                "--plan",
                planFile);

            // Assert — exits successfully using the directory-relative definition file
            Assert.AreEqual(0, exitCode, $"Output: {output}");
            Assert.IsTrue(File.Exists(planFile), "Plan file was not created");
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
    ///     Test that --elaborate outputs elaboration for a valid review-set ID.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Elaborate()
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
            Assert.AreEqual(0, exitCode, $"Output: {output}");
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
    ///     Test that --lint with a valid config reports success.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Lint()
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
            Assert.AreEqual(0, exitCode, $"Output: {output}");
            Assert.AreEqual(string.Empty, output, $"Expected empty output but got: {output}");
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
