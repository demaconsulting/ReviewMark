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

namespace DemaConsulting.TemplateDotNetTool.Tests;

/// <summary>
///     Unit tests for the Program class.
/// </summary>
[TestClass]
public class ProgramTests
{
    /// <summary>
    ///     Test that Run with version flag displays version only.
    /// </summary>
    [TestMethod]
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

            // Assert
            var output = outWriter.ToString();
            Assert.DoesNotContain("Copyright", output);
            Assert.DoesNotContain("Template DotNet Tool version", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with help flag displays usage information.
    /// </summary>
    [TestMethod]
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

            // Assert
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
    [TestMethod]
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

            // Assert
            var output = outWriter.ToString();
            Assert.Contains("Total Tests:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with no arguments displays default behavior.
    /// </summary>
    [TestMethod]
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

            // Assert
            var output = outWriter.ToString();
            Assert.Contains("Template DotNet Tool version", output);
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
    [TestMethod]
    public void Program_Version_ReturnsNonEmptyString()
    {
        // Act
        var version = Program.Version;

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(version));
    }
}
