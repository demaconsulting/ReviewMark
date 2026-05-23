## YamlDotNet

### Verification Approach

**Component**: YamlDotNet (<https://github.com/aaubry/YamlDotNet>)
**Role**: YAML parsing and deserialization library used by the Configuration subsystem to load `.reviewmark.yaml` configuration files.
**Acceptance approach**: Automated test coverage.

YamlDotNet is the de-facto standard .NET YAML library. Its integration surface in ReviewMark is
the `DeserializerBuilder`/`IDeserializer` API used by `ReviewMarkConfiguration` to deserialize
`.reviewmark.yaml` into strongly-typed C# objects. All integration paths — including valid YAML
and malformed YAML — are exercised by `DemaConsulting.ReviewMark.OtsSoftwareTests`, with
additional coverage through `ReviewMarkConfigurationTests.cs`.

### Test Scenarios

#### YamlDotNetDeserialization

Evidence that YamlDotNet correctly deserializes well-formed YAML content into the
expected C# model.

- **`Deserializer_Deserialize_WellFormedYaml_MapsToTypedObject`** — a simple YAML string is
  deserialized directly via `DeserializerBuilder` and the typed object fields match the expected
  values, confirming correct OTS API behavior.
- **`ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration`** — a minimal valid YAML string
  is deserialized without error and returns a non-null configuration object.
- **`ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly`** — the `needs-review` key
  is correctly deserialized into the expected list of file patterns.
- **`ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly`** — the `evidence-source` key,
  including its `type` and `location` sub-fields, is deserialized correctly.
- **`ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly`** — the `reviews` list is deserialized
  correctly, including each review-set `id`, `title`, and `paths`.
- **`ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly`** — a `none`-type evidence
  source is deserialized correctly with no location required.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

#### YamlDotNetErrorHandling

Evidence that YamlDotNet raises a `YamlException` on malformed input, allowing the Configuration
subsystem to report a descriptive error.

- **`Deserializer_Deserialize_MalformedYaml_ThrowsYamlException`** — structurally invalid YAML is
  passed directly to `IDeserializer.Deserialize`, and a `YamlException` is thrown, confirming the
  OTS error contract.
- **`ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue`** — supplying
  malformed YAML to `ReviewMarkConfiguration.Load` causes deserialization to fail, and the returned
  result carries an error issue instead of a configuration object.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

#### YamlDotNetUnknownKeys

Evidence that YamlDotNet silently ignores unrecognized YAML keys when configured with
`IgnoreUnmatchedProperties`.

- **`Deserializer_Deserialize_UnknownKeys_DoesNotThrow`** — YAML containing extra unknown keys is
  deserialized using `IgnoreUnmatchedProperties()` and no exception is thrown; known fields are
  populated correctly.

CI evidence source: `dotnet test` step in the `build` matrix job of `build.yaml`, writing test
result files to `artifacts/`.

### Requirements Coverage

- **ReviewMark-OTS-YamlDotNet-Deserialize**: YamlDotNet shall deserialize `.reviewmark.yaml`
  configuration files into typed C# objects.
  - *YamlDotNetDeserialization*: verifies YamlDotNet correctly maps YAML keys to C# properties,
    including hyphenated keys and nested structures.
    - `Deserializer_Deserialize_WellFormedYaml_MapsToTypedObject`
    - `ReviewMarkConfiguration_Parse_ValidYaml_ReturnsConfiguration`
    - `ReviewMarkConfiguration_Parse_NeedsReviewPatterns_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_EvidenceSource_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_Reviews_ParsedCorrectly`
    - `ReviewMarkConfiguration_Parse_NoneEvidenceSource_ParsedCorrectly`
- **ReviewMark-OTS-YamlDotNet-ErrorHandling**: YamlDotNet shall raise a `YamlException` on
  malformed YAML input.
  - *YamlDotNetErrorHandling*: verifies that malformed YAML input causes deserialization to fail
    with a reportable error.
    - `Deserializer_Deserialize_MalformedYaml_ThrowsYamlException`
    - `ReviewMarkConfiguration_Load_InvalidYaml_ReturnsNullConfigWithErrorIssue`
- **ReviewMark-OTS-YamlDotNet-UnknownKeys**: YamlDotNet shall silently ignore unrecognized YAML
  keys without raising an error.
  - *YamlDotNetUnknownKeys*: verifies that `IgnoreUnmatchedProperties` prevents unknown keys from
    causing deserialization failures.
    - `Deserializer_Deserialize_UnknownKeys_DoesNotThrow`
