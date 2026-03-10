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
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_VersionDisplay");

        try
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "version-test.log");

            // Build command line arguments
            var args = new List<string>
            {
                "--silent",
                "--log", logFile,
                "--version"
            };

            // Run the program
            int exitCode;
            using (var testContext = Context.Create([.. args]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // Check if execution succeeded
            if (exitCode == 0)
            {
                // Read log content
                var logContent = File.ReadAllText(logFile);

                // Verify version string is in log (version contains dots like 0.0.0)
                if (!string.IsNullOrWhiteSpace(logContent) &&
                    logContent.Split('.').Length >= 3)
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine($"✓ ReviewMark_VersionDisplay - Passed");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Version string not found in log";
                    context.WriteError($"✗ ReviewMark_VersionDisplay - Failed: Version string not found in log");
                }
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = $"Program exited with code {exitCode}";
                context.WriteError($"✗ ReviewMark_VersionDisplay - Failed: Exit code {exitCode}");
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_VersionDisplay", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for help display functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunHelpTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_HelpDisplay");

        try
        {
            using var tempDir = new TemporaryDirectory();
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "help-test.log");

            // Build command line arguments
            var args = new List<string>
            {
                "--silent",
                "--log", logFile,
                "--help"
            };

            // Run the program
            int exitCode;
            using (var testContext = Context.Create([.. args]))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // Check if execution succeeded
            if (exitCode == 0)
            {
                // Read log content
                var logContent = File.ReadAllText(logFile);

                // Verify help text is in log
                if (logContent.Contains("Usage:") && logContent.Contains("Options:"))
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine($"✓ ReviewMark_HelpDisplay - Passed");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Help text not found in log";
                    context.WriteError($"✗ ReviewMark_HelpDisplay - Failed: Help text not found in log");
                }
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = $"Program exited with code {exitCode}";
                context.WriteError($"✗ ReviewMark_HelpDisplay - Failed: Exit code {exitCode}");
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_HelpDisplay", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for definition + plan generation functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunDefinitionPlanTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_DefinitionPlan");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create the shared definition fixtures (src file, definition YAML, empty index)
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);

            // Define the plan output file
            var planFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "plan.md");

            // Build command line arguments
            var args = new[]
            {
                "--silent",
                "--definition", definitionFile,
                "--plan", planFile
            };

            // Run the program - all src/**/*.cs files are covered so HasIssues should be false
            int exitCode;
            using (var testContext = Context.Create(args))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // Verify execution succeeded with no coverage issues
            if (exitCode != 0)
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = $"Program exited with code {exitCode}";
                context.WriteError($"✗ ReviewMark_DefinitionPlan - Failed: Exit code {exitCode}");
            }
            else if (!File.Exists(planFile))
            {
                // Verify the plan file was written
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = "Plan file was not created";
                context.WriteError($"✗ ReviewMark_DefinitionPlan - Failed: Plan file was not created");
            }
            else
            {
                // Verify plan file contains expected review coverage heading
                var planContent = File.ReadAllText(planFile);
                if (planContent.Contains("Review Coverage"))
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine($"✓ ReviewMark_DefinitionPlan - Passed");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Plan file does not contain 'Review Coverage'";
                    context.WriteError($"✗ ReviewMark_DefinitionPlan - Failed: Plan file does not contain 'Review Coverage'");
                }
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_DefinitionPlan", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for definition + report generation functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunDefinitionReportTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_DefinitionReport");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create the shared definition fixtures (src file, definition YAML, empty index)
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);

            // Define the report output file
            var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");

            // Build command line arguments
            var args = new[]
            {
                "--silent",
                "--definition", definitionFile,
                "--report", reportFile
            };

            // Run the program - empty index means all reviews are Missing, but without --enforce
            // the tool still exits with code 0 and writes a warning to stdout
            using (var testContext = Context.Create(args))
            {
                Program.Run(testContext);
            }

            // Verify the report file was written regardless of exit code
            if (!File.Exists(reportFile))
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = "Report file was not created";
                context.WriteError($"✗ ReviewMark_DefinitionReport - Failed: Report file was not created");
            }
            else
            {
                // Verify report file contains expected review status heading
                var reportContent = File.ReadAllText(reportFile);
                if (reportContent.Contains("Review Status"))
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine($"✓ ReviewMark_DefinitionReport - Passed");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Report file does not contain 'Review Status'";
                    context.WriteError($"✗ ReviewMark_DefinitionReport - Failed: Report file does not contain 'Review Status'");
                }
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_DefinitionReport", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for index scanning functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunIndexScanTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_IndexScan");

        // Save current directory so it can be restored after the test
        var originalDirectory = Directory.GetCurrentDirectory();

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Change to temp directory so index.json is written there, not to the working directory.
            // The inner try/finally below ensures the directory is restored. It is intentionally
            // placed AFTER this call so the finally block only runs if the directory was actually
            // changed — any exception thrown before this point is caught by the outer catch block.
            Directory.SetCurrentDirectory(tempDir.DirectoryPath);

            try
            {
                // Build command line arguments - glob matches no PDFs so result will be empty
                var args = new[]
                {
                    "--silent",
                    "--index", "**/*.pdf"
                };

                // Run the program
                int exitCode;
                using (var testContext = Context.Create(args))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                // Verify execution succeeded
                if (exitCode != 0)
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = $"Program exited with code {exitCode}";
                    context.WriteError($"✗ ReviewMark_IndexScan - Failed: Exit code {exitCode}");
                }
                else
                {
                    // Verify the index.json file was written to the temp directory
                    var indexJsonPath = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "index.json");
                    if (File.Exists(indexJsonPath))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine($"✓ ReviewMark_IndexScan - Passed");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "index.json was not created";
                        context.WriteError($"✗ ReviewMark_IndexScan - Failed: index.json was not created");
                    }
                }
            }
            finally
            {
                // Always restore the original directory to avoid affecting subsequent tests.
                // The finally block ensures restoration even if an exception occurs during
                // the test, keeping the process state consistent for all subsequent tests
                // which run sequentially on the same thread.
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_IndexScan", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for --enforce flag causing a non-zero exit code when reviews have issues.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunEnforceTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReviewMark_Enforce");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create fixtures: src file, definition YAML, and empty index (no review evidence)
            var (definitionFile, _) = CreateTestDefinitionFixtures(tempDir.DirectoryPath);

            // Define the report output file
            var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");

            // Build command line arguments - empty index means all reviews are Missing
            // so --enforce should cause exit code 1
            var args = new[]
            {
                "--silent",
                "--definition", definitionFile,
                "--report", reportFile,
                "--enforce"
            };

            // Run the program
            int exitCode;
            using (var testContext = Context.Create(args))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // Verify that --enforce caused a non-zero exit code due to missing reviews
            if (exitCode != 0)
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                context.WriteLine($"✓ ReviewMark_Enforce - Passed");
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = "Expected non-zero exit code with --enforce and missing reviews";
                context.WriteError($"✗ ReviewMark_Enforce - Failed: Expected non-zero exit code with --enforce and missing reviews");
            }
        }
        // Generic catch is justified here as this is a test framework - any exception should be
        // recorded as a test failure to ensure robust test execution and reporting.
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReviewMark_Enforce", ex);
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
