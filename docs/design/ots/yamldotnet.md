## YamlDotNet

YamlDotNet is the YAML parsing and serialization library for .NET used by the Configuration
subsystem to deserialize `.reviewmark.yaml` configuration files.

### Purpose

YamlDotNet was chosen because it is the de-facto standard .NET YAML library with broad ecosystem
adoption, comprehensive YAML 1.1/1.2 support, and a stable public API. It provides the
deserialization path from raw YAML text to strongly-typed C# objects, removing the need to
hand-write a YAML parser for the configuration file format. No alternative .NET YAML library
offers equivalent ecosystem support or API maturity.

### Features Used

- **`DeserializerBuilder`**: fluent builder for constructing an `IDeserializer` instance;
  configured with naming convention and node-type resolver settings before use
- **`NullNamingConvention`**: preserves YAML key names exactly as written, relying on
  `YamlMember(Alias = "...")` attributes for all hyphenated or camelCase key mappings
- **`YamlMember(Alias = "...")` attribute**: maps hyphenated YAML key names (such as
  `needs-review` and `evidence-source`) to their C# property counterparts
- **`IDeserializer.Deserialize<T>(string)`**: strongly-typed deserialization from a YAML string
  into a target C# model class
- **`YamlDotNet.Core.YamlException`**: exception type thrown on malformed or structurally invalid
  YAML input; caught at the Configuration subsystem boundary

### Integration Pattern

`ReviewMarkConfigurationHelpers.DeserializeRaw()` constructs a `DeserializerBuilder` configured with
`NullNamingConvention` and `IgnoreUnmatchedProperties` to tolerate forward-compatible
configuration extensions. The `.reviewmark.yaml` content is read from disk as a string and passed
directly to `Deserialize<ReviewMarkYaml>()`. The deserialization models are private
`file sealed class` types declared inside `ReviewMarkConfiguration.cs` that mirror the raw YAML
structure and form no part of the public API. A `YamlException` from the deserialization step is
caught and reported as a descriptive error message via `Context.WriteError`, resulting in exit
code 1.

### Version

YamlDotNet 17.1.0 is the required version.
