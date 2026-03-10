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

namespace DemaConsulting.ReviewMark;

/// <summary>
///     Main program entry point for ReviewMark.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Gets the application version string.
    /// </summary>
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
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
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
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Priority 1: Version query
        if (context.Version)
        {
            context.WriteLine(Version);
            return;
        }

        // Print application banner
        PrintBanner(context);

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

        // Priority 4: Main tool functionality
        RunToolLogic(context);
    }

    /// <summary>
    ///     Prints the application banner.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintBanner(Context context)
    {
        context.WriteLine($"ReviewMark version {Version}");
        context.WriteLine("Copyright (c) DEMA Consulting");
        context.WriteLine("");
    }

    /// <summary>
    ///     Prints usage information.
    /// </summary>
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
        context.WriteLine("  --results <file>           Write validation results to file (.trx or .xml)");
        context.WriteLine("  --log <file>               Write output to log file");
        context.WriteLine("  --definition <file>        Specify the definition YAML file");
        context.WriteLine("  --plan <file>              Write review plan to the specified Markdown file");
        context.WriteLine("  --plan-depth <#>           Set the heading depth for the review plan (default: 1)");
        context.WriteLine("  --report <file>            Write review report to the specified Markdown file");
        context.WriteLine("  --report-depth <#>         Set the heading depth for the review report (default: 1)");
        context.WriteLine("  --index <glob-path>        Index PDF evidence files matching the glob path");
    }

    /// <summary>
    ///     Runs the main tool logic.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    private static void RunToolLogic(Context context)
    {
        var directory = Directory.GetCurrentDirectory();

        // Handle --index: scan PDF evidence files and write index.json
        if (context.IndexPaths.Count > 0)
        {
            RunIndexLogic(context, directory);
        }

        // Handle --definition: load configuration and process plan/report.
        // Return early after RunDefinitionLogic so the usage hint below is not
        // shown when a definition was successfully processed (even if --index was
        // also provided and already handled above).
        if (context.DefinitionFile != null)
        {
            RunDefinitionLogic(context, directory);
            return;
        }

        // If neither is specified, show usage hint
        if (context.IndexPaths.Count == 0)
        {
            context.WriteLine("ReviewMark - File Review Evidence Management");
            context.WriteLine("ReviewMark automates file-review evidence management.");
            context.WriteLine("");
            context.WriteLine("Use --help to see available options.");
        }
    }

    /// <summary>
    ///     Runs the index scanning logic.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="directory">The working directory.</param>
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
    private static void RunDefinitionLogic(Context context, string directory)
    {
        // Load the configuration from the definition file
        var config = ReviewMarkConfiguration.Load(context.DefinitionFile!);

        // Handle --plan: generate and write the review plan
        if (context.PlanFile != null)
        {
            var planResult = config.PublishReviewPlan(directory, context.PlanDepth);
            File.WriteAllText(context.PlanFile, planResult.Markdown);
            context.WriteLine($"Review plan written to {context.PlanFile}");
            if (planResult.HasIssues)
            {
                context.WriteError("Review plan has coverage issues.");
            }
        }

        // Handle --report: load index and generate the review report
        if (context.ReportFile != null)
        {
            var index = ReviewIndex.Load(config.EvidenceSource);
            var reportResult = config.PublishReviewReport(index, directory, context.ReportDepth);
            File.WriteAllText(context.ReportFile, reportResult.Markdown);
            context.WriteLine($"Review report written to {context.ReportFile}");
            if (reportResult.HasIssues)
            {
                context.WriteError("Review report has review issues.");
            }
        }
    }
}
