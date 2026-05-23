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

using DemaConsulting.TestResults.IO;

namespace OtsSoftwareTests;

/// <summary>
///     OTS software tests for the DemaConsulting.TestResults serialization library.
/// </summary>
public sealed class DemaConsultingTestResultsTests
{
    /// <summary>
    ///     Verifies that <see cref="TrxSerializer.Serialize" /> produces output containing a
    ///     <c>TestRun</c> element when given a completed test run with at least one result.
    /// </summary>
    [Fact]
    public void TrxSerializer_Serialize_CompletedTestRun_ContainsTestRunElement()
    {
        // Arrange
        var results = new DemaConsulting.TestResults.TestResults
        {
            Name = "Test Run"
        };
        results.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "SampleTest",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.Zero
        });

        // Act
        var xml = TrxSerializer.Serialize(results);

        // Assert — TRX output must contain the TestRun root element
        Assert.False(string.IsNullOrWhiteSpace(xml), "Serialized TRX content must not be empty");
        Assert.Contains("<TestRun", xml, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that <see cref="JUnitSerializer.Serialize" /> produces output containing a
    ///     <c>testsuites</c> element when given a completed test run with at least one result.
    /// </summary>
    [Fact]
    public void JUnitSerializer_Serialize_CompletedTestRun_ContainsTestSuitesElement()
    {
        // Arrange
        var results = new DemaConsulting.TestResults.TestResults
        {
            Name = "Test Run"
        };
        results.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "SampleTest",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.Zero
        });

        // Act
        var xml = JUnitSerializer.Serialize(results);

        // Assert — JUnit output must contain the testsuites root element
        Assert.False(string.IsNullOrWhiteSpace(xml), "Serialized JUnit content must not be empty");
        Assert.Contains("testsuites", xml, StringComparison.Ordinal);
    }
}
