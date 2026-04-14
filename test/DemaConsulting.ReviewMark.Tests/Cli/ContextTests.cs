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
///     Unit tests for the Context class.
/// </summary>
[TestClass]
public class ContextTests
{
    /// <summary>
    ///     Test creating a context with no arguments.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_ReturnsDefaultContext()
    {
        // Act
        using var context = Context.Create([]);

        // Assert — default context has all flags false and exit code is zero
        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.IsFalse(context.Silent);
        Assert.IsFalse(context.Validate);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the version flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_VersionFlag_SetsVersionTrue()
    {
        // Act
        using var context = Context.Create(["--version"]);

        // Assert — Version is true, Help remains false, and exit code is zero
        Assert.IsTrue(context.Version);
        Assert.IsFalse(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the short version flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_ShortVersionFlag_SetsVersionTrue()
    {
        // Act
        using var context = Context.Create(["-v"]);

        // Assert — short flag also sets Version to true, Help remains false, and exit code is zero
        Assert.IsTrue(context.Version);
        Assert.IsFalse(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the help flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_HelpFlag_SetsHelpTrue()
    {
        // Act
        using var context = Context.Create(["--help"]);

        // Assert — Help is true, Version remains false, and exit code is zero
        Assert.IsFalse(context.Version);
        Assert.IsTrue(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the short help flag -h.
    /// </summary>
    [TestMethod]
    public void Context_Create_ShortHelpFlag_H_SetsHelpTrue()
    {
        // Act
        using var context = Context.Create(["-h"]);

        // Assert — -h flag sets Help to true, Version remains false, and exit code is zero
        Assert.IsFalse(context.Version);
        Assert.IsTrue(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the short help flag -?.
    /// </summary>
    [TestMethod]
    public void Context_Create_ShortHelpFlag_Question_SetsHelpTrue()
    {
        // Act
        using var context = Context.Create(["-?"]);

        // Assert — -? flag sets Help to true, Version remains false, and exit code is zero
        Assert.IsFalse(context.Version);
        Assert.IsTrue(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the silent flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_SilentFlag_SetsSilentTrue()
    {
        // Act
        using var context = Context.Create(["--silent"]);

        // Assert — Silent is true and exit code is zero
        Assert.IsTrue(context.Silent);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the validate flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_ValidateFlag_SetsValidateTrue()
    {
        // Act
        using var context = Context.Create(["--validate"]);

        // Assert — Validate is true and exit code is zero
        Assert.IsTrue(context.Validate);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the results flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultsFlag_SetsResultsFile()
    {
        // Act
        using var context = Context.Create(["--results", "test.trx"]);

        // Assert — ResultsFile is set to the provided path and exit code is zero
        Assert.AreEqual("test.trx", context.ResultsFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with the log flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_LogFlag_OpensLogFile()
    {
        // Arrange
        var logFile = Path.GetTempFileName();
        try
        {
            // Act
            using (var context = Context.Create(["--log", logFile]))
            {
                context.WriteLine("Test message");
                Assert.AreEqual(0, context.ExitCode);
            }

            // Assert — log file exists and contains the written message
            Assert.IsTrue(File.Exists(logFile));
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Test message", logContent);
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
    ///     Test creating a context with an unknown argument throws exception.
    /// </summary>
    [TestMethod]
    public void Context_Create_UnknownArgument_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--unknown"]));
        Assert.Contains("Unsupported argument", exception.Message);
    }

    /// <summary>
    ///     Test creating a context with --log flag but no value throws exception.
    /// </summary>
    [TestMethod]
    public void Context_Create_LogFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--log"]));
        Assert.Contains("--log", exception.Message);
    }

    /// <summary>
    ///     Test creating a context with --results flag but no value throws exception.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultsFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--results"]));
        Assert.Contains("--results", exception.Message);
    }

    /// <summary>
    ///     Test creating a context with the --result alias sets the results file.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultAlias_SetsResultsFile()
    {
        // Act
        using var context = Context.Create(["--result", "test.trx"]);

        // Assert — ResultsFile is set to the provided path and exit code is zero
        Assert.AreEqual("test.trx", context.ResultsFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test creating a context with --result alias but no value throws exception.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultAlias_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--result"]));
        Assert.Contains("--result", exception.Message);
    }

    /// <summary>
    ///     Test WriteLine writes to console output when not silent.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_NotSilent_WritesToConsole()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create([]);

            // Act
            context.WriteLine("Test message");

            // Assert — the message appears in console output when not silent
            var output = outWriter.ToString();
            Assert.Contains("Test message", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test WriteLine does not write to console when silent.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_Silent_DoesNotWriteToConsole()
    {
        // Arrange
        var originalOut = Console.Out;
        try
        {
            using var outWriter = new StringWriter();
            Console.SetOut(outWriter);
            using var context = Context.Create(["--silent"]);

            // Act
            context.WriteLine("Test message");

            // Assert — the message is suppressed from console output when silent
            var output = outWriter.ToString();
            Assert.DoesNotContain("Test message", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test WriteError does not write to console when silent.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_Silent_DoesNotWriteToConsole()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);
            using var context = Context.Create(["--silent"]);

            // Act
            context.WriteError("Test error message");

            // Assert — error output should be suppressed in silent mode
            var output = errWriter.ToString();
            Assert.DoesNotContain("Test error message", output);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test WriteError sets exit code to 1.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_SetsErrorExitCode()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);
            using var context = Context.Create([]);

            // Act
            context.WriteError("Test error message");

            // Assert — exit code is set to 1 after writing an error
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test WriteError writes message to console when not silent.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_NotSilent_WritesToConsole()
    {
        // Arrange
        var originalError = Console.Error;
        try
        {
            using var errWriter = new StringWriter();
            Console.SetError(errWriter);
            using var context = Context.Create([]);

            // Act
            context.WriteError("Test error message");

            // Assert — the error message appears in stderr when not silent
            var output = errWriter.ToString();
            Assert.Contains("Test error message", output);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test WriteError writes message to log file when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_WritesToLogFile()
    {
        // Arrange
        var logFile = Path.GetTempFileName();
        try
        {
            // Act - use silent to avoid console output; verify the error still goes to the log
            using (var context = Context.Create(["--silent", "--log", logFile]))
            {
                context.WriteError("Test error in log");
                Assert.AreEqual(1, context.ExitCode);
            }

            // Assert — log file should contain the error message
            Assert.IsTrue(File.Exists(logFile));
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Test error in log", logContent);
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
    ///     Test that --definition sets DefinitionFile to the provided path.
    /// </summary>
    [TestMethod]
    public void Context_Create_DefinitionFlag_SetsDefinitionFile()
    {
        // Act - create context specifying a definition YAML file
        using var context = Context.Create(["--definition", "spec.yaml"]);

        // Assert — DefinitionFile is set to the provided path and exit code is 0
        Assert.AreEqual("spec.yaml", context.DefinitionFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --definition without a value throws ArgumentException containing "--definition".
    /// </summary>
    [TestMethod]
    public void Context_Create_DefinitionFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert - --definition with no following value should throw and include the flag name in the message
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--definition"]));
        Assert.Contains("--definition", exception.Message);
    }

    /// <summary>
    ///     Test that --plan sets PlanFile to the provided path.
    /// </summary>
    [TestMethod]
    public void Context_Create_PlanFlag_SetsPlanFile()
    {
        // Act - create context specifying a plan output file
        using var context = Context.Create(["--plan", "plan.yaml"]);

        // Assert — PlanFile is set to the provided path and exit code is 0
        Assert.AreEqual("plan.yaml", context.PlanFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --plan-depth sets PlanDepth to the provided integer value.
    /// </summary>
    [TestMethod]
    public void Context_Create_PlanDepthFlag_SetsPlanDepth()
    {
        // Act - create context specifying a heading depth of 3
        using var context = Context.Create(["--plan-depth", "3"]);

        // Assert — PlanDepth is set to the parsed integer value and exit code is 0
        Assert.AreEqual(3, context.PlanDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --plan-depth with a non-numeric value throws ArgumentException because
    ///     the flag requires a positive integer argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_PlanDepthFlag_WithInvalidValue_ThrowsArgumentException()
    {
        // Act & Assert - --plan-depth with a non-numeric value should throw
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--plan-depth", "not-a-number"]));
    }

    /// <summary>
    ///     Test that --plan-depth with zero throws ArgumentException because the flag requires
    ///     a positive integer argument (value must be >= 1).
    /// </summary>
    [TestMethod]
    public void Context_Create_PlanDepthFlag_WithZeroValue_ThrowsArgumentException()
    {
        // Act & Assert - --plan-depth requires a positive integer; zero is not valid
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--plan-depth", "0"]));
    }

    /// <summary>
    ///     Test that --report sets ReportFile to the provided path.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportFlag_SetsReportFile()
    {
        // Act - create context specifying a report output file
        using var context = Context.Create(["--report", "report.md"]);

        // Assert — ReportFile is set to the provided path and exit code is 0
        Assert.AreEqual("report.md", context.ReportFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --report-depth sets ReportDepth to the provided integer value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthFlag_SetsReportDepth()
    {
        // Act - create context specifying a heading depth of 2
        using var context = Context.Create(["--report-depth", "2"]);

        // Assert — ReportDepth is set to the parsed integer value and exit code is 0
        Assert.AreEqual(2, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --report-depth with a non-numeric value throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthFlag_NonNumeric_ThrowsArgumentException()
    {
        // Act & Assert - creating a context with a non-numeric report depth should fail validation
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "abc"]));
    }

    /// <summary>
    ///     Test that --report-depth with a value of 0 throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthFlag_Zero_ThrowsArgumentException()
    {
        // Act & Assert - creating a context with a report depth of 0 should fail validation
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "0"]));
    }

    /// <summary>
    ///     Test that --report-depth with a missing value throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthFlag_MissingValue_ThrowsArgumentException()
    {
        // Act & Assert - creating a context with --report-depth but no value should fail validation
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth"]));
    }

    /// <summary>
    ///     Test that --index adds the provided glob path to IndexPaths.
    /// </summary>
    [TestMethod]
    public void Context_Create_IndexFlag_AddsIndexPath()
    {
        // Act - create context specifying one glob pattern for PDF evidence files
        using var context = Context.Create(["--index", "*.pdf"]);

        // Assert — IndexPaths contains the provided glob pattern and exit code is 0
        Assert.HasCount(1, context.IndexPaths);
        Assert.AreEqual("*.pdf", context.IndexPaths[0]);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that multiple --index flags accumulate all provided paths in IndexPaths.
    /// </summary>
    [TestMethod]
    public void Context_Create_IndexFlag_MultipleTimes_AddsAllPaths()
    {
        // Act - create context with two different --index glob patterns
        using var context = Context.Create(["--index", "*.pdf", "--index", "docs/**/*.md"]);

        // Assert — IndexPaths contains both patterns and exit code is 0
        Assert.HasCount(2, context.IndexPaths);
        Assert.Contains("*.pdf", context.IndexPaths);
        Assert.Contains("docs/**/*.md", context.IndexPaths);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that the default IndexPaths collection is empty when no --index flags are provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_IndexPathsEmpty()
    {
        // Act - create context with no arguments
        using var context = Context.Create([]);

        // Assert — IndexPaths is empty when no --index flags are provided
        Assert.HasCount(0, context.IndexPaths);
    }

    /// <summary>
    ///     Test that the default PlanDepth is 1 when no --plan-depth flag is provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_PlanDepthDefaultsToOne()
    {
        // Act - create context with no arguments
        using var context = Context.Create([]);

        // Assert — PlanDepth defaults to 1
        Assert.AreEqual(1, context.PlanDepth);
    }

    /// <summary>
    ///     Test that the default ReportDepth is 1 when no --report-depth flag is provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_ReportDepthDefaultsToOne()
    {
        // Act - create context with no arguments
        using var context = Context.Create([]);

        // Assert — ReportDepth defaults to 1
        Assert.AreEqual(1, context.ReportDepth);
    }

    /// <summary>
    ///     Test that --enforce sets Enforce to true.
    /// </summary>
    [TestMethod]
    public void Context_Create_EnforceFlag_SetsEnforceTrue()
    {
        // Act - create context with the --enforce flag
        using var context = Context.Create(["--enforce"]);

        // Assert — Enforce is set to true and exit code is 0
        Assert.IsTrue(context.Enforce);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that the default Enforce is false when no --enforce flag is provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_EnforceFalse()
    {
        // Act - create context with no arguments
        using var context = Context.Create([]);

        // Assert — Enforce defaults to false
        Assert.IsFalse(context.Enforce);
    }

    /// <summary>
    ///     Test that --plan-depth with a value greater than 5 throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_PlanDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException()
    {
        // Act & Assert - --plan-depth cannot exceed 5 (max heading depth supported)
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--plan-depth", "6"]));
    }

    /// <summary>
    ///     Test that --report-depth with a value greater than 5 throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthFlag_WithValueGreaterThanFive_ThrowsArgumentException()
    {
        // Act & Assert - --report-depth cannot exceed 5 (max heading depth supported)
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "6"]));
    }

    /// <summary>
    ///     Test that --dir sets WorkingDirectory to the provided path.
    /// </summary>
    [TestMethod]
    public void Context_Create_DirFlag_SetsWorkingDirectory()
    {
        // Act - create context specifying a working directory
        using var context = Context.Create(["--dir", "/evidence"]);

        // Assert — WorkingDirectory is set to the provided path and exit code is 0
        Assert.AreEqual("/evidence", context.WorkingDirectory);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that WorkingDirectory is null when no --dir flag is provided.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_WorkingDirectoryIsNull()
    {
        // Act - create context with no arguments
        using var context = Context.Create([]);

        // Assert — WorkingDirectory is null when --dir is not specified
        Assert.IsNull(context.WorkingDirectory);
    }

    /// <summary>
    ///     Test that --dir with a missing value throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_DirFlag_MissingValue_ThrowsArgumentException()
    {
        // Act & Assert - --dir without a path value should throw
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--dir"]));
    }

    /// <summary>
    ///     Test that --elaborate flag sets ElaborateId to the provided review-set ID.
    /// </summary>
    [TestMethod]
    public void Context_Create_ElaborateFlag_SetsElaborateId()
    {
        // Act
        using var context = Context.Create(["--elaborate", "Core-Logic"]);

        // Assert — ElaborateId is set to the provided review-set ID
        Assert.AreEqual("Core-Logic", context.ElaborateId);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that ElaborateId is null when --elaborate is not specified.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_ElaborateIdIsNull()
    {
        // Act
        using var context = Context.Create([]);

        // Assert — ElaborateId is null when --elaborate is not specified
        Assert.IsNull(context.ElaborateId);
    }

    /// <summary>
    ///     Test that --elaborate without a value throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_ElaborateFlag_WithoutValue_ThrowsArgumentException()
    {
        // Act & Assert - --elaborate without an ID argument should throw
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--elaborate"]));
    }

    /// <summary>
    ///     Test that --lint flag sets Lint to true.
    /// </summary>
    [TestMethod]
    public void Context_Create_LintFlag_SetsLintTrue()
    {
        // Act
        using var context = Context.Create(["--lint"]);

        // Assert — Lint is true, other flags remain false, and exit code is zero
        Assert.IsTrue(context.Lint);
        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Lint is false when --lint is not specified.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_LintIsFalse()
    {
        // Act
        using var context = Context.Create([]);

        // Assert — Lint is false when --lint is not specified
        Assert.IsFalse(context.Lint);
    }

    /// <summary>
    ///     Test that --depth sets Depth, PlanDepth, and ReportDepth to the provided value.
    /// </summary>
    [TestMethod]
    public void Context_Create_DepthFlag_SetsDepth()
    {
        // Act - create context specifying a default heading depth of 3
        using var context = Context.Create(["--depth", "3"]);

        // Assert — Depth, PlanDepth, and ReportDepth are all set to 3 and exit code is 0
        Assert.AreEqual(3, context.Depth);
        Assert.AreEqual(3, context.PlanDepth);
        Assert.AreEqual(3, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --depth sets the default but --plan-depth overrides only PlanDepth.
    /// </summary>
    [TestMethod]
    public void Context_Create_DepthFlag_PlanDepthOverride()
    {
        // Act - create context with --depth 2 and --plan-depth 4
        using var context = Context.Create(["--depth", "2", "--plan-depth", "4"]);

        // Assert — Depth is 2, PlanDepth is 4 (overridden), ReportDepth is 2 (from --depth)
        Assert.AreEqual(2, context.Depth);
        Assert.AreEqual(4, context.PlanDepth);
        Assert.AreEqual(2, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that --depth with a non-numeric value throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_DepthFlag_WithInvalidValue_ThrowsArgumentException()
    {
        // Act & Assert - --depth with a non-numeric value should throw with a message referencing --depth
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--depth", "not-a-number"]));
        Assert.Contains("--depth", exception.Message);
    }

    /// <summary>
    ///     Test that --depth with a value of 0 throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Context_Create_DepthFlag_WithZeroValue_ThrowsArgumentException()
    {
        // Act & Assert - --depth requires a positive integer; zero is not valid
        var exception = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--depth", "0"]));
        Assert.Contains("--depth", exception.Message);
    }
}

