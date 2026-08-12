# CareerOS.Bootstrap — Testing Strategy

## Purpose

This document defines the testing strategy for `CareerOS.Bootstrap`.

The goal is to verify that the application remains safe, predictable, traceable, and maintainable as it evolves from a read-only planning utility into a filesystem provisioning application.

This strategy supports the requirements already established in:

```text
Documentation/Requirements/
├── USER_STORIES.md
├── FUNCTIONAL_REQUIREMENTS.md
├── NON_FUNCTIONAL_REQUIREMENTS.md
└── TRACEABILITY.md
```

Testing is intended to provide evidence that requirements are satisfied. A feature should not be considered complete solely because its implementation compiles or appears to work during one manual run.

---

## Current Testing State

The automated testing foundation is now implemented.

The current verification model is:

```text
Code Change
   |
   v
dotnet build
   |
   v
dotnet test
   |
   v
Targeted Manual Review where needed
   |
   v
Git Diff / Review
```

The test project is:

```text
CareerOS.Bootstrap.Tests
```

At the current M2 checkpoint:

```text
Test framework: xUnit
Target framework: .NET 8
Automated tests: 75 passing
Failed tests: 0
Skipped tests: 0
Service test suites: 4
Shared temporary-filesystem fixture: Implemented and tested
Workflow/integration tests: Implemented for current planning behavior
CI execution: Planned
```

The implemented foundation primarily supports:

```text
US-021
US-022
US-023

FR-041
FR-042
FR-043

NFR-018
NFR-019
NFR-020
NFR-034
NFR-036
```

`FR-044` remains planned because GitHub Actions has not yet been implemented.

---

## Testing Principles

### 1. Safety-critical behavior receives priority

Filesystem behavior can affect long-lived CareerOS data. Testing effort should therefore prioritize:

- No writes during dry-run
- Validation before provisioning
- Preservation of existing content
- Creation of missing directories only
- Idempotent repeat execution
- Clear failure handling
- No implicit destructive deletion

### 2. Test the smallest useful boundary

Pure logic should normally be tested with unit tests rather than requiring filesystem or process execution.

Examples:

```text
Template Resolution      -> Unit Test
Recursive Planning       -> Unit Test
Configuration Validation -> Unit Test
Filesystem Provisioning  -> Integration Test
CLI End-to-End Flow       -> Workflow / Integration Test
```

### 3. Real user directories are never automated test fixtures

Filesystem tests must use isolated temporary roots.

Automated tests must never use an actual CareerOS profile directory as test data.

### 4. Tests should be deterministic

Given the same controlled inputs, a test should produce the same result regardless of prior local application runs.

Tests should not rely unnecessarily on:

- Developer usernames
- Fixed drive letters
- Existing local CareerOS content
- Previous test execution order
- External network availability

### 5. Requirements drive test intent

Important tests should be traceable to one or more `FR-###` or `NFR-###` identifiers.

Test names should describe observable behavior rather than internal implementation details where practical.

### 6. Current and future behavior remain distinct

Tests for future provisioning behavior should be introduced when the corresponding implementation exists.

The test suite must not imply that planned functionality is already available.

---

# Test Levels

## Unit Tests

Unit tests verify focused application behavior in isolation.

Implemented unit-test targets include:

```text
TemplateResolverServiceTests
DirectoryPlanServiceTests
JsonConfigurationServiceTests
PathServiceTests
```

Future unit-test targets may include:

```text
ConfigurationValidationServiceTests
ProvisioningPlanTests
CommandLineOptionTests
ExecutionSummaryTests
```

### Unit-test characteristics

Unit tests should generally:

- Execute quickly
- Avoid shared mutable state
- Avoid real user filesystem locations
- Use controlled input models or fixtures
- Assert one behavioral concern clearly
- Produce useful failure messages

---

## Integration Tests

Integration tests verify collaboration between components or behavior requiring real infrastructure boundaries.

For CareerOS.Bootstrap, the most important integration boundary is the filesystem.

Current integration coverage includes:

```text
Configuration load
Template resolution
Recursive planning
Multiple-profile/template planning
Dry-run/no-write planning behavior
Isolated temporary filesystem use
Temporary repository/path scenarios
```

Future integration coverage includes:

```text
DirectoryProvisioningService
Existing-state inspection
Directory creation
Repeat-run idempotency
Preservation of existing files/directories
Provisioning failure handling
```

Integration tests may use real filesystem operations, but only inside isolated temporary roots owned by the test.

---

## Workflow / End-to-End Tests

As command-line and provisioning capabilities mature, selected tests should verify the complete application workflow.

Conceptually:

```text
Execution Request
      |
      v
Configuration Load
      |
      v
Validation
      |
      v
Resolution
      |
      v
Plan
      |
      v
Dry Run or Provision
      |
      v
Summary + Exit Result
```

End-to-end tests should be fewer than focused unit tests and should cover high-value workflows rather than every minor permutation.

---

## Manual Verification

Manual testing remains useful for:

- Human readability of console output
- Documentation accuracy
- Developer workflow validation
- Exploratory testing
- Release smoke testing
- Visual confirmation of GitHub or packaged-release behavior

Manual testing supplements automated tests; it should not be the only protection for critical filesystem behavior once provisioning exists.

---

# Implemented Automated Test Catalog

Stable `TEST-###` identifiers represent behavioral verification categories rather than individual xUnit methods. One identifier may therefore be implemented by multiple `[Fact]` or `[Theory]` cases.

The canonical mapping is maintained in `Documentation/Requirements/TRACEABILITY.md`.

| Test ID | Verification Intent | Primary Test Artifact(s) |
|---|---|---|
| TEST-001 | Load valid bootstrap/profile configuration, including supported JSON options and collection behavior | `JsonConfigurationServiceTests.cs` |
| TEST-002 | Load valid recursive template configuration, including supported JSON options | `JsonConfigurationServiceTests.cs` |
| TEST-003 | Resolve configured template names, including case-insensitive matching and correct selection | `TemplateResolverServiceTests.cs` |
| TEST-004 | Reject missing, invalid, or unknown template resolution requests | `TemplateResolverServiceTests.cs`, `BootstrapPlanningWorkflowTests.cs` |
| TEST-005 | Build top-level and recursive directory plans in deterministic traversal order | `DirectoryPlanServiceTests.cs` |
| TEST-006 | Reject invalid planning inputs, including missing base paths/profile directories and unnamed directory nodes | `DirectoryPlanServiceTests.cs` |
| TEST-007 | Verify planning remains read-only and does not create planned workspace directories | `DirectoryPlanServiceTests.cs`, `BootstrapPlanningWorkflowTests.cs` |
| TEST-008 | Discover the repository root from the default or injected starting directory | `PathServiceTests.cs` |
| TEST-009 | Reject repository discovery when the expected solution root cannot be found | `PathServiceTests.cs` |
| TEST-010 | Resolve the repository `Configuration` directory from default or injected roots | `PathServiceTests.cs` |
| TEST-011 | Reject configuration-directory discovery when the directory is missing | `PathServiceTests.cs` |
| TEST-012 | Execute configuration-load → template-resolution → recursive-planning workflow against isolated files | `BootstrapPlanningWorkflowTests.cs` |
| TEST-013 | Execute multi-profile planning with each profile's assigned template | `BootstrapPlanningWorkflowTests.cs` |
| TEST-014 | Provide and verify isolated temporary filesystem fixtures, including cleanup and path-boundary protection | `TemporaryDirectoryFixture.cs`, `TemporaryDirectoryFixtureTests.cs` |

The current automated suite contains 75 passing xUnit cases at the M2 checkpoint.

---

# Implemented Service Test Coverage

## TemplateResolverService

Implemented coverage includes:

```text
Exact-name resolution
Case-insensitive resolution
Correct selection from multiple templates
Null configuration handling
Missing template-name handling
Unknown-template failure
Empty-template collection failure
```

Primary requirement relationships include:

```text
FR-004
FR-005
FR-018
NFR-008
NFR-016
```

## DirectoryPlanService

Implemented coverage includes:

```text
Profile-root generation
Top-level directory planning
Recursive nested planning
Multiple recursive branches
Missing base-path rejection
Null profile/template rejection
Missing profile-directory rejection
Unnamed directory-node rejection
No filesystem writes during planning
```

Primary requirement relationships include:

```text
FR-006
FR-011
FR-012
FR-014
FR-017
NFR-001
NFR-015
NFR-018
```

## JsonConfigurationService

Implemented coverage includes:

```text
Valid bootstrap configuration
Valid recursive template configuration
Case-insensitive JSON property names
JSON comments
Trailing commas
Missing-file failure
Malformed-JSON failure
JSON null/deserialization failure
Empty profile/template collections
```

Primary requirement relationships include:

```text
FR-001
FR-002
FR-016
NFR-008
NFR-009
```

## PathService

Implemented coverage includes:

```text
Repository-root discovery from the normal runtime location
Repository-root discovery from an injected nested start directory
Absolute-path behavior
Configuration-directory resolution
Repository discovery failure
Missing Configuration directory failure
Injected repository-root scenarios
Constructor validation for missing start directories
```

The production service retains its default `AppContext.BaseDirectory` behavior while exposing an injected starting-directory constructor for isolated tests.

Primary requirement relationships include:

```text
FR-007
FR-008
NFR-011
NFR-022
NFR-034
```

---

# Implemented Fixture and Integration Foundation

## TemporaryDirectoryFixture

The shared fixture provides:

```text
Unique temporary root per fixture instance
Nested directory creation
UTF-8 file creation
Path composition beneath the fixture root
Path-boundary protection
Deterministic recursive cleanup
Idempotent disposal
Disposed-object protection
```

The fixture itself is covered by automated tests before being relied upon as shared infrastructure.

## BootstrapPlanningWorkflowTests

Current workflow tests exercise:

```text
Temporary configuration files
        |
        v
JsonConfigurationService
        |
        v
TemplateResolverService
        |
        v
DirectoryPlanService
        |
        v
Planned Paths
```

Implemented scenarios include:

```text
Valid recursive planning workflow
Multiple profiles using different templates
No planned workspace directories created
Unknown assigned template failure
```

These tests intentionally stop at planning because filesystem provisioning has not yet been implemented.

---

# Future Validation Test Plan

When a centralized validation service is implemented, tests should include at minimum:

```text
Missing required profile values
Duplicate profile names
Duplicate profile destination directories
Duplicate template names
Missing template references
Invalid filesystem characters
Reserved filesystem names
Empty required collections
Conflicting destination paths
Duplicate sibling directory names
Unsupported schema versions when versioning exists
```

These tests primarily support:

```text
FR-017
FR-019
FR-020
NFR-004
NFR-008
NFR-010
NFR-026
```

Blocking validation errors must be proven to prevent provisioning.

---

# Future Filesystem Provisioning Test Plan

Filesystem provisioning is the highest-risk future test area.

Tests must operate against isolated temporary directories.

## Standard fixture lifecycle

```text
Create Temporary Root
        |
        v
Arrange Existing State
        |
        v
Build Validated Plan
        |
        v
Execute Provisioning
        |
        v
Inspect Filesystem
        |
        v
Assert Result
        |
        v
Repeat If Testing Idempotency
        |
        v
Cleanup Temporary Root
```

## Required provisioning scenarios

### Missing directory creation

Verify that a missing planned directory is created and reported as created.

### Existing directory preservation

Verify that an existing planned directory is preserved and reported as existing rather than recreated.

### Existing file preservation

Place a known file in an expected existing directory before provisioning and verify that its contents remain unchanged afterward.

### Partial structure repair

Create only part of a desired hierarchy and verify that provisioning creates only the missing paths.

### Idempotent second run

Execute the same valid provisioning operation twice and verify that the second execution creates no additional directories and does not alter existing user content.

### No implicit deletion

Include an extra directory not present in current configuration and verify that normal provisioning does not delete it.

### Invalid plan or destination

Verify that invalid input prevents filesystem modification.

### Creation failure

Where a reliable isolated fixture can simulate a permission or filesystem failure, verify that the failure is surfaced and is not reported as success.

Primary requirement relationships include:

```text
FR-021 through FR-026
NFR-002 through NFR-007
NFR-019
NFR-026
NFR-031
```

---

# Dry-Run Testing

Dry-run is a core safety mechanism.

Future explicit dry-run testing should verify both **semantic equivalence** and **non-destructive execution**.

The intended relationship is:

```text
Validated Provisioning Plan
          |
          +-----------> Dry Run
          |
          +-----------> Provision
```

The preview should reflect the same validated intent that provisioning would consume.

Required dry-run assertions include:

- Planned actions are visible.
- Effective profile/template/destination are visible where applicable.
- No directory is created.
- No file is modified.
- No directory is deleted.
- Exit behavior reflects validation or planning success/failure appropriately.

---

# Test Data Strategy

Test data should be intentionally small, readable, and purpose-built.

Recommended fixture categories:

```text
ValidSingleProfile
ValidMultipleProfiles
ValidNestedTemplate
DeepNestedTemplate
UnknownTemplate
MissingRequiredValue
DuplicateProfiles
DuplicateTemplates
InvalidDestination
PartiallyExistingFilesystem
FullyExistingFilesystem
```

Fixtures should demonstrate the smallest configuration needed to test a behavior.

Production CareerOS configuration should not be the only test fixture because changes to personal configuration could then destabilize tests.

---

# Test Naming Convention

A descriptive convention is preferred, for example:

```csharp
ResolveTemplate_ValidName_ReturnsMatchingTemplate()
ResolveTemplate_NameWithDifferentCase_ReturnsMatchingTemplate()
ResolveTemplate_UnknownName_ThrowsExpectedException()
BuildPlan_NestedDirectories_IncludesAllNestedPaths()
Provision_ExistingDirectory_PreservesExistingContents()
Provision_SecondIdenticalRun_CreatesNoDirectories()
```

Exact framework naming conventions can be refined when the test framework is selected.

Test names should communicate:

```text
Operation / Scenario / Expected Outcome
```

---

# Test Identifier Strategy

Stable `TEST-###` identifiers are now implemented and maintained in `TRACEABILITY.md`.

A test identifier represents a durable verification intent, not necessarily one individual xUnit method. This allows methods to be split, renamed, or parameterized without unnecessarily renumbering traceability.

Example:

```text
TEST-005
Intent: Build top-level and recursive directory plans in deterministic traversal order
Implementation: DirectoryPlanServiceTests.cs
```

Not every micro-test requires a separate identifier. Safety-critical, requirement-driving, and acceptance-level behaviors should remain traceable.

Identifiers do not replace descriptive test method names.

---

# Requirement Traceability

The intended chain is:

```text
US-###
   |
   v
FR-### / NFR-###
   |
   v
Component
   |
   v
Implementation
   |
   v
TEST-###
   |
   v
Verification Result
```

When tests are added:

1. Update `TRACEABILITY.md`.
2. Record the applicable requirement relationships.
3. Update requirement status when implementation and verification justify it.
4. Update current-state documentation if a planned capability has become implemented.

---

# Test Project Structure

The current implemented structure is:

```text
CareerOS.Bootstrap.Tests/
├── Fixtures/
│   ├── TemporaryDirectoryFixture.cs
│   └── TemporaryDirectoryFixtureTests.cs
│
├── Integration/
│   └── BootstrapPlanningWorkflowTests.cs
│
├── Services/
│   ├── DirectoryPlanServiceTests.cs
│   ├── JsonConfigurationServiceTests.cs
│   ├── PathServiceTests.cs
│   └── TemplateResolverServiceTests.cs
│
└── CareerOS.Bootstrap.Tests.csproj
```

A `Models/` directory has intentionally not been created because the current model types are simple DTOs without meaningful independent behavior to verify.

As functionality evolves, additional areas may be introduced for validation, CLI, orchestration, provisioning, logging, and release-related behavior.

The structure should continue evolving with real testing needs rather than creating empty abstraction layers prematurely.

---

# Test Framework Selection

xUnit is the adopted test framework for `CareerOS.Bootstrap.Tests`.

The current project uses the standard .NET test tooling and executes through:

```powershell
dotnet test
```

The selected framework satisfies the project criteria for:

- .NET 8 compatibility
- Visual Studio integration
- `dotnet test` support
- Clear assertion behavior
- Maintainability
- Future CI support
- Minimal unnecessary dependency overhead

Framework adoption is now an implemented project decision rather than a planned selection.

---

# Mocking Strategy

Mocks, fakes, or abstractions should be introduced when they improve test isolation or design clarity, not automatically for every dependency.

Prefer simple controlled inputs for pure services.

Filesystem abstraction should be considered only if it materially improves safety and testing without adding unnecessary complexity.

Real filesystem integration tests remain necessary even if a future abstraction is introduced.

---

# Coverage Philosophy

The project should prioritize **meaningful behavioral coverage** over an arbitrary coverage percentage.

High-value areas include:

```text
Configuration parsing
Validation boundaries
Template resolution
Recursive planning
Dry-run safety
Filesystem preservation
Idempotency
Error/exit behavior
```

A numeric coverage target may be introduced later if it provides useful governance, but a high percentage alone does not prove filesystem safety or requirement compliance.

---

# Regression Testing

Every confirmed bug that can reasonably be reproduced should receive a regression test before or with the fix.

Preferred workflow:

```text
Reproduce Defect
      |
      v
Create Failing Test
      |
      v
Implement Fix
      |
      v
Test Passes
      |
      v
Run Broader Suite
```

This prevents known defects from returning silently.

---

# CI Testing Strategy

Future GitHub Actions should execute tests automatically for relevant pull requests and branch updates.

Target pipeline:

```text
Checkout
   |
   v
Restore
   |
   v
Build
   |
   v
Unit Tests
   |
   v
Integration Tests
   |
   v
Result
```

Initially, integration tests should remain local and self-contained enough to run on a clean CI runner without relying on developer-specific paths.

CI should report failures visibly and block merge only when branch-protection policy is intentionally enabled.

---

# Local Developer Validation

Before a pull request or merge, developers should run:

```powershell
dotnet restore
dotnet build
dotnet test
```

For checkpoint validation, the current working practice also includes:

```powershell
git status
git diff --check
git diff --stat
```

or the staged equivalents before commit.

Manual `dotnet run` verification remains useful when console behavior or user-facing output changes, but it is no longer the only verification layer.

---

# Documentation Testing

Documentation is also a maintained project artifact.

Documentation review should verify:

- Current behavior is not confused with future behavior.
- Requirement IDs remain stable.
- Links and file references remain valid.
- Architecture diagrams remain consistent with implementation state.
- New functionality updates `CURRENT_STATE.md`.
- Changed architecture updates `FUTURE_STATE.md`, `COMPONENTS.md`, or `DATA_FLOW.md` as appropriate.
- Requirement/test mappings are updated in `TRACEABILITY.md`.

---

# Release Validation

When packaged releases exist, release validation should verify:

- Source builds successfully.
- Automated tests pass.
- Version information matches the release.
- Release artifacts can execute in the documented environment.
- Required configuration behavior is documented.
- Dry-run behavior remains non-destructive.
- Provisioning smoke tests operate only against controlled test locations.

---

# Definition of Done — Testing Perspective

A significant feature should not be considered complete until the applicable items below are satisfied:

```text
Requirement defined
Architecture impact reviewed
Implementation completed
Build succeeds
Relevant unit tests pass
Relevant integration tests pass
Safety behavior verified
Traceability updated
Documentation updated
Pull request reviewed
```

Not every change requires every test level, but filesystem-modifying behavior requires a higher verification standard than documentation-only or presentation changes.

---

# Near-Term Testing Roadmap

Completed M2 foundation:

```text
[x] Establish CareerOS.Bootstrap.Tests
[x] Adopt xUnit
[x] Add TemplateResolverService tests
[x] Add DirectoryPlanService tests
[x] Add JsonConfigurationService tests
[x] Add practical and isolated PathService tests
[x] Add TemporaryDirectoryFixture
[x] Test the shared fixture
[x] Add current-state workflow/integration tests
[x] Assign TEST-001 through TEST-014
[x] Update TRACEABILITY.md
[x] Establish repeatable local dotnet build / dotnet test checkpoints
```

Remaining future testing work:

```text
[ ] Add centralized validation-service tests when validation exists
[ ] Add filesystem provisioning safety/idempotency tests when provisioning exists
[ ] Add CLI/result/exit-code tests when those capabilities exist
[ ] Add dotnet test to GitHub CI
[ ] Add release-validation automation when packaged releases exist
```

This sequence continues to protect implemented behavior without creating tests for functionality that does not yet exist.

---

# Summary

The testing strategy has moved CareerOS.Bootstrap from primarily manual validation into a requirements-driven automated verification foundation.

The target model is:

```text
Requirement
   |
   v
Implementation
   |
   v
Focused Unit Test
   |
   +------> Filesystem / Workflow Integration Test where needed
   |
   v
CI Validation
   |
   v
Traceable Evidence
```

The highest priority is not raw test count. It is confidence that CareerOS.Bootstrap can understand, validate, preview, and eventually modify a user's environment **without silently damaging existing data**.
