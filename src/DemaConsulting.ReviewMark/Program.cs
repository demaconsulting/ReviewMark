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

using System.Reflection;
using DemaConsulting.ReviewMark.Cli;
using DemaConsulting.ReviewMark.Configuration;
using DemaConsulting.ReviewMark.Indexing;
using DemaConsulting.ReviewMark.SelfTest;

namespace DemaConsulting.ReviewMark;

/// <summary>
///     Main program entry point for ReviewMark.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Gets the application version string.
    /// </summary>
    /// <value>
    ///     A semantic version string (e.g., <c>1.2.3</c>); may include a git
    ///     hash suffix when build metadata is present
    ///     (e.g., <c>1.2.3+abc1234</c>).
    /// </value>
    /// <remarks>
    ///     Resolves the version through a fallback chain: tries
    ///     <see cref="AssemblyInformationalVersionAttribute"/> first
    ///     (InformationalVersion, which may include a git hash suffix), then
    ///     falls back to <see cref="System.Reflection.AssemblyName.Version"/>
    ///     (AssemblyVersionAttribute), and finally falls back to
    ///     <c>"0.0.0"</c> when neither attribute is present.  The property is
    ///     stateless and thread-safe.
    /// </remarks>
    public static string Version
    {
        get
        {
            // Get the assembly containing this program
            var assembly = typeof(Program).Assembly;

            // Try to get version from assembly attributes, fallback to AssemblyVersion, or default to 0.0.0
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "0.0.0";
        }
    }

    /// <summary>
    ///     Main entry point for ReviewMark.
    /// </summary>
    /// <remarks>
    ///     Implements a three-tier exception-handling design: (1) expected
    ///     application-level exceptions (<see cref="ArgumentException"/> and
    ///     <see cref="InvalidOperationException"/>) are caught and reported as
    ///     clean user-facing errors, returning exit code 1 without a stack
    ///     trace; (2) any other unexpected exception is caught, its message
    ///     written to <see cref="Console.Error"/>, and then rethrown.
    ///     Unexpected exceptions are rethrown rather than swallowed so that
    ///     the process exits with a non-zero code and the full stack trace is
    ///     preserved for diagnostics.
    /// </remarks>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
    /// <exception cref="Exception">Thrown when an unexpected error occurs in any subsystem; the exception message is written to Console.Error before rethrowing.</exception>
    private static int Main(string[] args)
    {
        try
        {
            // Create context from command-line arguments
            using var context = Context.Create(args);

            // Run the program logic
            Run(context);

            // Return the exit code from the context
            return context.ExitCode;
        }
        catch (ArgumentException ex)
        {
            // Print expected argument exceptions and return error code
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            // Print expected operation exceptions and return error code
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            // Print unexpected exceptions and re-throw to generate event logs
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Runs the program logic based on the provided context.
    /// </summary>
    /// <remarks>
    ///     Implements a priority-ordered dispatch so that diagnostic flags
    ///     (<c>--version</c>, <c>--help</c>, <c>--validate</c>, <c>--lint</c>)
    ///     are always evaluated before any output-generating actions, preventing
    ///     unintended side effects when flags are combined.  The application
    ///     banner is printed between priority-1 (<c>--version</c>) and
    ///     priority-3 (<c>--help</c>) so it appears for every interactive
    ///     invocation but is suppressed for <c>--lint</c> to keep that output
    ///     clean for CI pipelines.
    /// </remarks>
    /// <param name="context">The context containing command line arguments and program state.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a file write operation fails; propagated from
    ///     <see cref="RunDefinitionLogic"/> (plan or report file) or
    ///     <see cref="RunIndexLogic"/> (index file).
    /// </exception>
    public static void Run(Context context)
    {
        // Priority 1: Version query
        if (context.Version)
        {
            context.WriteLine(Version);
            return;
        }

        // Print application banner (suppressed for --lint to show only issues)
        if (!context.Lint)
        {
            PrintBanner(context);
        }

        // Priority 2: Help
        if (context.Help)
        {
            PrintHelp(context);
            return;
        }

        // Priority 3: Self-Validation
        if (context.Validate)
        {
            Validation.Run(context);
            return;
        }

        // Priority 4: Lint
        if (context.Lint)
        {
            RunLintLogic(context);
            return;
        }

        // Priority 5: Main tool functionality
        RunToolLogic(context);
    }

    /// <summary>
    ///     Prints the application banner to the context output.
    /// </summary>
    /// <remarks>
    ///     The banner is printed on every invocation except <c>--version</c>
    ///     and <c>--lint</c>.  Suppressing it for <c>--lint</c> keeps the
    ///     output parseable by linting scripts and CI pipelines (signal, no
    ///     noise).  Suppressing it for <c>--version</c> ensures the output
    ///     contains only the bare version string, which tooling can capture
    ///     directly.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    private static void PrintBanner(Context context)
    {
        context.WriteLine($"ReviewMark version {Version}");
        context.WriteLine("Copyright (c) DEMA Consulting");
        context.WriteLine("");
    }

    /// <summary>
    ///     Prints usage information to the context output.
    /// </summary>
    /// <remarks>
    ///     Called only when <c>--help</c> is set.  All available flags and
    ///     arguments are listed so users can discover the full CLI surface
    ///     without consulting external documentation.  The application banner
    ///     has already been printed before this method is reached, so it is
    ///     not repeated here.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    private static void PrintHelp(Context context)
    {
        context.WriteLine("Usage: reviewmark [options]");
        context.WriteLine("");
        context.WriteLine("Options:");
        context.WriteLine("  -v, --version              Display version information");
        context.WriteLine("  -?, -h, --help             Display this help message");
        context.WriteLine("  --silent                   Suppress console output");
        context.WriteLine("  --validate                 Run self-validation");
        context.WriteLine("  --lint                     Lint the definition file and report issues");
        context.WriteLine("  --results <file>           Write validation results to file (.trx or .xml)");
        context.WriteLine("  --log <file>               Write output to log file");
        context.WriteLine("  --depth <#>                Set the default heading depth for all generated documents (default: 1)");
        context.WriteLine("  --definition <file>        Specify the definition YAML file (default: .reviewmark.yaml)");
        context.WriteLine("  --plan <file>              Write review plan to the specified Markdown file");
        context.WriteLine("  --plan-depth <#>           Set the heading depth for the review plan (default: --depth or 1)");
        context.WriteLine("  --report <file>            Write review report to the specified Markdown file");
        context.WriteLine("  --report-depth <#>         Set the heading depth for the review report (default: --depth or 1)");
        context.WriteLine("  --index <glob-path>        Index PDF evidence files matching the glob path");
        context.WriteLine("  --dir <directory>          Set the working directory (used for default paths and glob scanning)");
        context.WriteLine("                             Note: explicit paths given to --definition/--plan/--report are used as-is");
        context.WriteLine("  --enforce                  Exit with non-zero code if there are review issues");
        context.WriteLine("  --elaborate <id>           Print a Markdown elaboration of the specified review set");
    }

    /// <summary>
    ///     Runs the lint logic to validate the definition file and report issues.
    /// </summary>
    /// <remarks>
    ///     Designed for machine-readable output: no banner, no summary message.
    ///     Silence means the definition file is valid, which keeps the output
    ///     clean for integration with linting scripts and CI pipelines.
    ///     Issue messages are emitted via <see cref="Context.WriteError"/> for
    ///     errors (which also sets the exit code to 1) and
    ///     <see cref="Context.WriteLine"/> for warnings, so the caller can
    ///     distinguish severity without parsing message content.
    /// </remarks>
    /// <param name="context">The context containing command line arguments and program state.</param>
    private static void RunLintLogic(Context context)
    {
        // Determine the definition file path (explicit or default)
        var directory = context.WorkingDirectory ?? Directory.GetCurrentDirectory();
        var definitionFile = context.DefinitionFile ?? PathHelpers.SafePathCombine(directory, ".reviewmark.yaml");

        // Load and lint the file in one pass, collecting all detectable issues.
        var result = ReviewMarkConfiguration.Load(definitionFile);
        result.ReportIssues(context);
    }

    /// <summary>
    ///     Runs the main tool logic.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    /// <remarks>
    ///     Path resolution convention: <c>--dir</c> sets the working directory used for operations
    ///     that do not have an explicit path argument.  Paths that the user explicitly provides via
    ///     <c>--definition</c>, <c>--plan</c>, or <c>--report</c> are used exactly as given and are
    ///     NOT re-rooted under <c>--dir</c>.  This keeps each argument independent — specifying one
    ///     argument's path cannot inadvertently change the resolution of another argument's path.
    /// </remarks>
    private static void RunToolLogic(Context context)
    {
        // The working directory is used for operations without an explicit path argument:
        //   - the default definition file (.reviewmark.yaml) when --definition is omitted
        //   - glob scanning root and index.json output for --index
        //   - file scanning root for plan/report generation
        // Explicit paths provided via --definition, --plan, and --report are used as-is.
        var directory = context.WorkingDirectory ?? Directory.GetCurrentDirectory();

        // Handle --index: scan PDF evidence files and write index.json
        if (context.IndexPaths.Count > 0)
        {
            RunIndexLogic(context, directory);
        }

        // Handle definition-based actions (--plan, --report, or explicit --definition).
        // Use .reviewmark.yaml as the default when --definition is not specified,
        // resolved under the working directory.
        if (context.PlanFile != null || context.ReportFile != null || context.DefinitionFile != null || context.ElaborateId != null)
        {
            var definitionFile = context.DefinitionFile ?? PathHelpers.SafePathCombine(directory, ".reviewmark.yaml");
            RunDefinitionLogic(context, directory, definitionFile);
            return;
        }

        // If neither index nor definition actions are specified, show usage hint
        if (context.IndexPaths.Count == 0)
        {
            context.WriteLine("ReviewMark - File Review Evidence Management");
            context.WriteLine("ReviewMark automates file-review evidence management.");
            context.WriteLine("");
            context.WriteLine("Use --help to see available options.");
        }
    }

    /// <summary>
    ///     Runs the index scanning logic to build an evidence index from PDF files.
    /// </summary>
    /// <remarks>
    ///     Scans all PDFs matched by <c>context.IndexPaths</c> under
    ///     <paramref name="directory"/>, extracts review-evidence metadata from
    ///     each file's PDF Keywords field, and writes the resulting index to
    ///     <c>index.json</c> in the working directory.  Per-file scan failures
    ///     are forwarded as warnings via <c>context.WriteLine</c> so a single
    ///     unreadable PDF does not abort the entire scan.  I/O failures when
    ///     writing <c>index.json</c> are propagated as
    ///     <see cref="InvalidOperationException"/> to <c>Main()</c>, which
    ///     prints the message and returns exit code 1.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    /// <param name="directory">The working directory used as the scan root and index output location.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the index file cannot be written to disk; propagated from
    ///     <see cref="ReviewIndex.Save(string)"/>.
    /// </exception>
    private static void RunIndexLogic(Context context, string directory)
    {
        // Scan PDF evidence files and save the resulting index
        context.WriteLine("Scanning PDF evidence files...");
        var index = ReviewIndex.Scan(directory, context.IndexPaths, onWarning: context.WriteLine);
        var indexFile = PathHelpers.SafePathCombine(directory, "index.json");
        index.Save(indexFile);
        context.WriteLine($"Index written to {indexFile}");
    }

    /// <summary>
    ///     Runs the definition-based logic (plan and/or report generation).
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="directory">The working directory.</param>
    /// <param name="definitionFile">The path to the definition YAML file.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when a plan or report file cannot be written to disk due to an
    ///     I/O failure (e.g., <see cref="IOException"/>,
    ///     <see cref="UnauthorizedAccessException"/>, or
    ///     <see cref="DirectoryNotFoundException"/>).
    /// </exception>
    private static void RunDefinitionLogic(Context context, string directory, string definitionFile)
    {
        // Load the configuration with integrated linting
        var loadResult = ReviewMarkConfiguration.Load(definitionFile);

        // Always report any lint issues found during loading
        loadResult.ReportIssues(context);

        // If the configuration could not be loaded, stop here
        if (loadResult.Configuration == null)
        {
            return;
        }

        var config = loadResult.Configuration;

        // Handle --plan: generate and write the review plan
        if (context.PlanFile != null)
        {
            var planResult = config.PublishReviewPlan(directory, context.PlanDepth);
            try
            {
                File.WriteAllText(context.PlanFile, planResult.Markdown);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
            {
                throw new InvalidOperationException($"Failed to write review plan to '{context.PlanFile}': {ex.Message}", ex);
            }

            context.WriteLine($"Review plan written to {context.PlanFile}");
            HandleIssues(context, planResult.HasIssues, "Review plan has coverage issues.");
        }

        // Handle --report: load index and generate the review report
        if (context.ReportFile != null)
        {
            var index = ReviewIndex.Load(config.EvidenceSource);
            var reportResult = config.PublishReviewReport(index, directory, context.ReportDepth);
            try
            {
                File.WriteAllText(context.ReportFile, reportResult.Markdown);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
            {
                throw new InvalidOperationException($"Failed to write review report to '{context.ReportFile}': {ex.Message}", ex);
            }

            context.WriteLine($"Review report written to {context.ReportFile}");
            HandleIssues(context, reportResult.HasIssues, "Review report has review issues.");
        }

        // Handle --elaborate
        if (context.ElaborateId != null)
        {
            try
            {
                var elaborateResult = config.ElaborateReviewSet(context.ElaborateId, directory);
                context.WriteLine(elaborateResult.Markdown);
            }
            catch (ArgumentException ex)
            {
                context.WriteError($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    ///     Handles review issues by writing an error or warning to the context.
    /// </summary>
    /// <remarks>
    ///     This is the single exit-code decision point for review enforcement.
    ///     When <c>--enforce</c> is set, any detected issues cause
    ///     <see cref="Context.WriteError"/> to be called, which sets the internal
    ///     error flag and drives <see cref="Context.ExitCode"/> to 1.  Without
    ///     <c>--enforce</c>, issues are surfaced as non-fatal warnings so the
    ///     tool can still complete successfully.  Centralizing this logic here
    ///     ensures every plan and report operation observes the same enforcement
    ///     policy.
    /// </remarks>
    /// <param name="context">The context for output.</param>
    /// <param name="hasIssues">Whether there are issues to report.</param>
    /// <param name="message">The issue message.</param>
    private static void HandleIssues(Context context, bool hasIssues, string message)
    {
        // Do nothing if there are no issues
        if (!hasIssues)
        {
            return;
        }

        // With --enforce, exit with non-zero; otherwise emit a non-fatal warning
        if (context.Enforce)
        {
            context.WriteError(message);
        }
        else
        {
            context.WriteLine($"Warning: {message}");
        }
    }
}
