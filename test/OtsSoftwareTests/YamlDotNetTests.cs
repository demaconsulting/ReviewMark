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

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OtsSoftwareTests;

/// <summary>
///     Simple configuration class used for YamlDotNet deserialization tests.
/// </summary>
internal sealed class SimpleConfig
{
    /// <summary>Gets or sets the name field.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the value field.</summary>
    public int Value { get; set; }
}

/// <summary>
///     OTS software tests for the YamlDotNet deserialization library.
/// </summary>
public sealed class YamlDotNetTests
{
    /// <summary>
    ///     Verifies that a well-formed YAML string is correctly deserialized into a typed object
    ///     with the expected field values.
    /// </summary>
    [Fact]
    public void Deserializer_Deserialize_WellFormedYaml_MapsToTypedObject()
    {
        // Arrange
        const string yaml = "name: TestName\nvalue: 42\n";
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // Act
        var config = deserializer.Deserialize<SimpleConfig>(yaml);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("TestName", config.Name);
        Assert.Equal(42, config.Value);
    }

    /// <summary>
    ///     Verifies that structurally invalid YAML causes <c>IDeserializer.Deserialize</c>
    ///     to throw a <see cref="YamlException" />.
    /// </summary>
    [Fact]
    public void Deserializer_Deserialize_MalformedYaml_ThrowsYamlException()
    {
        // Arrange — colon in a plain scalar without a space is structurally valid in most cases,
        // so use a tab character which is illegal in YAML indentation.
        const string malformedYaml = "name: valid\n\tinvalid_tab: oops\n";
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Act & Assert
        Assert.ThrowsAny<YamlException>(() => deserializer.Deserialize<SimpleConfig>(malformedYaml));
    }

    /// <summary>
    ///     Verifies that YAML containing unrecognized keys does not throw when the deserializer
    ///     is configured with <c>IgnoreUnmatchedProperties</c>, and that known fields are still
    ///     populated correctly.
    /// </summary>
    [Fact]
    public void Deserializer_Deserialize_UnknownKeys_DoesNotThrow()
    {
        // Arrange
        const string yaml = "name: KnownName\nvalue: 7\nunknownKey: unexpected\n";
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // Act
        var config = deserializer.Deserialize<SimpleConfig>(yaml);

        // Assert — no exception was thrown and known fields are correctly populated
        Assert.NotNull(config);
        Assert.Equal("KnownName", config.Name);
        Assert.Equal(7, config.Value);
    }
}
