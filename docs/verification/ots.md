# Off-The-Shelf Components

This section documents the verification strategy applied to each off-the-shelf (OTS)
component used by ReviewMark. For each OTS component, acceptance is based on one of
the following approaches:

- **Automated test coverage** — unit or integration tests exercise the component's
  integration surface and confirm the expected behaviour.
- **Established industry use** — the component is a widely adopted, actively maintained
  open-source project with its own test suite and release process.
- **Vendor assurance** — the component is supplied and maintained by the tool vendor
  with published quality practices.

The subsections below address each component individually. Component version constraints
are defined in the relevant project files and the requirements YAML in
`docs/reqstream/ots/`.
