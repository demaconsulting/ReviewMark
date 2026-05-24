## YamlDotNet

### Verification Approach

ReviewMark uses YamlDotNet 17.1.0, referenced from `DemaConsulting.ReviewMark.csproj`, to
deserialize `.reviewmark.yaml` content in the Configuration subsystem. The integration surface is
`ReviewMarkConfigurationHelpers.DeserializeRaw()`, which builds a deserializer with
`NullNamingConvention`, `IgnoreUnmatchedProperties()`, and `YamlMember` aliases so hyphenated YAML
keys such as `needs-review` and `evidence-source` bind to the raw model. Malformed YAML is caught
as `YamlException` and converted into user-visible parse diagnostics. Fitness for intended use is
verified by dedicated OTS tests in `test/OtsSoftwareTests/YamlDotNetTests.cs`, companion
configuration tests in `test/DemaConsulting.ReviewMark.Tests/Configuration/ReviewMarkConfigurationTests.cs`,
and the `dotnet test` step in the `build` matrix job of `build.yaml`, which publishes TRX evidence
to `artifacts/`. No project-specific issues have been observed in this validated integration
surface.

### Test Scenarios

**YamlDotNetDeserialization**: Well-formed `.reviewmark.yaml` content is deserialized into the raw
configuration model, including list values, nested objects, and aliased keys, so the Configuration
subsystem can build a usable `ReviewMarkConfiguration`. This scenario is tested by
`Deserializer_Deserialize_WellFormedYaml_MapsToTypedObject`,
`ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration`,
`ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly`,
`ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly`,
`ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly`, and
`ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly`.

**YamlDotNetErrorHandling**: Malformed YAML is rejected with a `YamlException`, and ReviewMark
turns that failure into a deterministic configuration issue instead of accepting invalid input.
This scenario is tested by `Deserializer_Deserialize_MalformedYaml_ThrowsYamlException` and
`ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue`.

**YamlDotNetUnknownKeys**: Unknown YAML keys are ignored when forward-compatible configuration
extensions are present, allowing older ReviewMark versions to continue parsing known fields
correctly. This scenario is tested by `Deserializer_Deserialize_UnknownKeys_DoesNotThrow`.

### Requirements Coverage

- **ReviewMark-OTS-YamlDotNet-Deserialize**: YamlDotNet shall deserialize `.reviewmark.yaml`
  configuration files into typed C# objects.
  - *YamlDotNetDeserialization*
    - `Deserializer_Deserialize_WellFormedYaml_MapsToTypedObject`
    - `ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration`
    - `ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly`
- **ReviewMark-OTS-YamlDotNet-ErrorHandling**: YamlDotNet shall raise a `YamlException` on
  malformed YAML input.
  - *YamlDotNetErrorHandling*
    - `Deserializer_Deserialize_MalformedYaml_ThrowsYamlException`
    - `ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue`
- **ReviewMark-OTS-YamlDotNet-UnknownKeys**: YamlDotNet shall silently ignore unrecognized YAML
  keys without raising an error.
  - *YamlDotNetUnknownKeys*
    - `Deserializer_Deserialize_UnknownKeys_DoesNotThrow`
