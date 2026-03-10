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

using System.Runtime.InteropServices;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReviewMark;

/// <summary>
///     Provides self-validation functionality for ReviewMark.
/// </summary>
internal static class Validation
{
    /// <summary>
    ///     Runs self-validation tests and optionally writes results to a file.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(context);

        // Print validation header
        PrintValidationHeader(context);

        // Create test results collection
        var testResults = new DemaConsulting.TestResults.TestResults
        {
            Name = "ReviewMark Self-Validation"
        };

        // Run core functionality tests
        RunVersionTest(context, testResults);
        RunHelpTest(context, testResults);
        RunDefinitionPlanTest(context, testResults);
        RunDefinitionReportTest(context, testResults);
        RunIndexScanTest(context, testResults);
        RunDirTest(context, testResults);
        RunEnforceTest(context, testResults);

        // Calculate totals
        var totalTests = testResults.Results.Count;
        var passedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Passed);
        var failedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Failed);

        // Print summary
        context.WriteLine("");
        context.WriteLine($"Total Tests: {totalTests}");
        context.WriteLine($"Passed: {passedTests}");
        if (failedTests > 0)
        {
            context.WriteError($"Failed: {failedTests}");
        }
        else
        {
            context.WriteLine($"Failed: {failedTests}");
        }

        // Write results file if requested
        if (context.ResultsFile != null)
        {
            WriteResultsFile(context, testResults);
        }
    }

    /// <summary>
    ///     Prints the validation header with system information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintValidationHeader(Context context)
    {
        context.WriteLine("# DEMA Consulting ReviewMark");
        context.WriteLine("");
        context.WriteLine("| Information         | Value                                              |");
        context.WriteLine("| :------------------ | :------------------------------------------------- |");
        context.WriteLine($"| Tool Version        | {Program.Version,-50} |");
        context.WriteLine($"| Machine Name        | {Environment.MachineName,-50} |");
        context.WriteLine($"| OS Version          | {RuntimeInformation.OSDescription,-50} |");
        context.WriteLine($"| DotNet Runtime      | {RuntimeInformation.FrameworkDescription,-50} |");
        context.WriteLine($"| Time Stamp          | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC{"",-29} |");
        context.WriteLine("");
    }

    /// <summary>
    ///     Runs a test for version display functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunVersionTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_VersionDisplay", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "version-test.log");

            // Run the program capturing output to a log file
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--log", logFile, "--version"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            // Verify version string is present in the log (version contains at least two dots)
            var logContent = File.ReadAllText(logFile);
            return (!string.IsNullOrWhiteSpace(logContent) && logContent.Split('.').Length >= 3)
                ? null : "Version string not found in log";
        });
    }

    /// <summary>
    ///     Runs a test for help display functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunHelpTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_HelpDisplay", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "help-test.log");

            // Run the program capturing output to a log file
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--log", logFile, "--help"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            // Verify expected help headings are present in the log
            var logContent = File.ReadAllText(logFile);
            return (logContent.Contains("Usage:") && logContent.Contains("Options:"))
                ? null : "Help text not found in log";
        });
    }

    /// <summary>
    ///     Runs a test for definition + plan generation functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunDefinitionPlanTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_DefinitionPlan", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);
            var planFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "plan.md");

            // Run the program to generate the plan file
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--definition", definitionFile, "--plan", planFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            if (!File.Exists(planFile))
            {
                return "Plan file was not created";
            }

            // Verify the plan file contains the expected review coverage heading
            var planContent = File.ReadAllText(planFile);
            return planContent.Contains("Review Coverage") ? null : "Plan file does not contain 'Review Coverage'";
        });
    }

    /// <summary>
    ///     Runs a test for definition + report generation functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunDefinitionReportTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_DefinitionReport", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);
            var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");

            // Run without --enforce so missing reviews only emit a warning; exit code is 0
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--definition", definitionFile, "--report", reportFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Expected exit code 0 but got {exitCode}";
            }

            if (!File.Exists(reportFile))
            {
                return "Report file was not created";
            }

            // Verify the report file contains the expected review status heading
            var reportContent = File.ReadAllText(reportFile);
            return reportContent.Contains("Review Status") ? null : "Report file does not contain 'Review Status'";
        });
    }

    /// <summary>
    ///     Runs a test for index scanning functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunIndexScanTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_IndexScan", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var indexJsonPath = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");

            // Run with --dir so index.json is written to the temporary directory
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--dir", tempDir.DirectoryPath, "--index", "**/*.pdf"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            return File.Exists(indexJsonPath) ? null : "index.json was not created";
        });
    }

    /// <summary>
    ///     Runs a test for the --dir argument overriding the working directory for file operations.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunDirTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_Dir", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);
            var planFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "plan.md");

            // Run with --dir pointing to the temp directory; glob patterns in the definition
            // are resolved under that directory rather than the process working directory
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--dir", tempDir.DirectoryPath, "--definition", definitionFile, "--plan", planFile]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            if (exitCode != 0)
            {
                return $"Program exited with code {exitCode}";
            }

            return File.Exists(planFile) ? null : "Plan file was not created";
        });
    }

    /// <summary>
    ///     Runs a test for --enforce flag causing a non-zero exit code when reviews have issues.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunEnforceTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(context, testResults, "ReviewMark_Enforce", () =>
        {
            using var tempDir = new TemporaryDirectory();
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);
            var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");

            // Run with --enforce: missing reviews should cause non-zero exit code
            int exitCode;
            using (var testContext = Context.Create(["--silent", "--definition", definitionFile, "--report", reportFile, "--enforce"]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            return exitCode != 0 ? null : "Expected non-zero exit code with --enforce and missing reviews";
        });
    }

    /// <summary>
    ///     Runs a single validation test, recording the outcome in the test results collection.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="testBody">
    ///     A function that performs the test logic. Returns <c>null</c> on success, or an error
    ///     message string on failure.
    /// </param>
    private static void RunValidationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        string testName,
        Func<string?> testBody)
    {
        // Record when the test started so duration can be calculated at the end
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult(testName);

        try
        {
            // Execute the test body and interpret null as success, non-null as failure
            var errorMessage = testBody();
            if (errorMessage == null)
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                context.WriteLine($"✓ {testName} - Passed");
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = errorMessage;
                context.WriteError($"✗ {testName} - Failed: {errorMessage}");
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, testName, ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Creates the standard test fixtures for definition-based tests: a <c>src/foo.cs</c>
    ///     source file, a <c>definition.yaml</c> covering <c>src/**/*.cs</c>, and an empty
    ///     <c>index.json</c> evidence file.
    /// </summary>
    /// <param name="directoryPath">The root of the temporary directory to populate.</param>
    /// <returns>
    ///     A tuple containing the path to the created <c>definition.yaml</c> and
    ///     <c>index.json</c> files.
    /// </returns>
    private static (string DefinitionFile, string IndexFile) CreateTestDefinitionFixtures(string directoryPath)
    {
        // Create src subdirectory and a source file to be covered by the review
        var srcDir = PathHelpers.SafePathCombine(directoryPath, "src");
        Directory.CreateDirectory(srcDir);
        File.WriteAllText(PathHelpers.SafePathCombine(srcDir, "foo.cs"), "// test content");

        // Create an empty index file so evidence-source resolves
        var indexFile = PathHelpers.SafePathCombine(directoryPath, "index.json");
        File.WriteAllText(indexFile, """{"reviews":[]}""");

        // Create the definition YAML file referencing the source file glob
        var definitionFile = PathHelpers.SafePathCombine(directoryPath, "definition.yaml");
        var definitionYaml = $"""
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
            """;
        File.WriteAllText(definitionFile, definitionYaml);

        return (definitionFile, indexFile);
    }

    /// <summary>
    ///     Writes test results to a file in TRX or JUnit format.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results to write.</param>
    private static void WriteResultsFile(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        if (context.ResultsFile == null)
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(context.ResultsFile).ToLowerInvariant();
            string content;

            if (extension == ".trx")
            {
                content = TrxSerializer.Serialize(testResults);
            }
            else if (extension == ".xml")
            {
                // Assume JUnit format for .xml extension
                content = JUnitSerializer.Serialize(testResults);
            }
            else
            {
                context.WriteError($"Error: Unsupported results file format '{extension}'. Use .trx or .xml extension.");
                return;
            }

            File.WriteAllText(context.ResultsFile, content);
            context.WriteLine($"Results written to {context.ResultsFile}");
        }
        // Generic catch is justified here as a top-level handler to log file write errors
        catch (Exception ex)
        {
            context.WriteError($"Error: Failed to write results file: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new test result object with common properties.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <returns>A new test result object.</returns>
    private static DemaConsulting.TestResults.TestResult CreateTestResult(string testName)
    {
        return new DemaConsulting.TestResults.TestResult
        {
            Name = testName,
            ClassName = "Validation",
            CodeBase = "ReviewMark"
        };
    }

    /// <summary>
    ///     Finalizes a test result by setting its duration and adding it to the collection.
    /// </summary>
    /// <param name="test">The test result to finalize.</param>
    /// <param name="startTime">The start time of the test.</param>
    /// <param name="testResults">The test results collection to add to.</param>
    private static void FinalizeTestResult(
        DemaConsulting.TestResults.TestResult test,
        DateTime startTime,
        DemaConsulting.TestResults.TestResults testResults)
    {
        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Handles test exceptions by setting failure information and logging the error.
    /// </summary>
    /// <param name="test">The test result to update.</param>
    /// <param name="context">The context for output.</param>
    /// <param name="testName">The name of the test for error messages.</param>
    /// <param name="ex">The exception that occurred.</param>
    private static void HandleTestException(
        DemaConsulting.TestResults.TestResult test,
        Context context,
        string testName,
        Exception ex)
    {
        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
        test.ErrorMessage = $"Exception: {ex.Message}";
        context.WriteError($"✗ {testName} - FAILED: {ex.Message}");
    }

    /// <summary>
    ///     Represents a temporary directory that is automatically deleted when disposed.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        ///     Gets the path to the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            DirectoryPath = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reviewmark_validation_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                throw new InvalidOperationException($"Failed to create temporary directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Deletes the temporary directory and all its contents.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Ignore cleanup errors during disposal
            }
        }
    }
}
