# CareerOS.Bootstrap â€” Testing Strategy

## Purpose

This document defines the testing strategy for `CareerOS.Bootstrap`.

The goal is to verify that the application remains safe, predictable, traceable, and maintainable as it evolves from a read-only planning utility into a filesystem provisioning application.

This strategy supports the requirements already established in:

```text
Documentation/Requirements/
â”œâ”€â”€ USER_STORIES.md
â”œâ”€â”€ FUNCTIONAL_REQUIREMENTS.md
â”œâ”€â”€ NON_FUNCTIONAL_REQUIREMENTS.md
â””â”€â”€ TRACEABILITY.md
```

Testing is intended to provide evidence that requirements are satisfied. A feature should not be considered complete solely because its implementation compiles or appears to work during one manual run.

---

## Current Testing State

Automated tests are not yet implemented.

Current verification consists primarily of:

```text
Code Change
   |
   v
dotnet build
   |
   v
dotnet run
   |
   v
Manual Output Review
   |
   v
Git Diff / Review
```

This manual process has been useful for establishing the current foundation, but it is not the target long-term testing model.

The planned automated test project is:

```text
CareerOS.Bootstrap.Tests
```

The initial test foundation is associated primarily with:

```text
US-021
US-022
US-023

FR-041
FR-042
FR-043
FR-044

NFR-018
NFR-019
NFR-020
NFR-034
NFR-036
```

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

Expected initial test targets include:

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

Expected future integration coverage includes:

```text
DirectoryProvisioningService
Existing-state inspection
Directory creation
Repeat-run idempotency
Dry-run no-write behavior
Failure handling
Temporary repository / path scenarios where appropriate
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

# Initial Unit-Test Plan

## TemplateResolverService

Initial behaviors to verify:

```text
TEST-001  Resolves a valid template name
TEST-002  Resolves template names case-insensitively
TEST-003  Rejects an unknown template
TEST-004  Does not silently choose another template
```

Primary requirement relationships:

```text
FR-004
FR-005
FR-018
NFR-008
```

---

## DirectoryPlanService

Initial behaviors to verify:

```text
TEST-005  Generates the expected profile root
TEST-006  Includes every top-level directory
TEST-007  Includes nested child directories
TEST-008  Supports deeper recursive nesting
TEST-009  Builds child paths beneath the correct parent
TEST-010  Rejects invalid required planning inputs
TEST-011  Planning performs no filesystem writes
```

Primary requirement relationships:

```text
FR-006
FR-011
FR-012
FR-014
FR-017
NFR-001
NFR-015
```

---

## JsonConfigurationService

Initial behaviors to verify:

```text
TEST-012  Loads valid bootstrap configuration
TEST-013  Loads valid template configuration
TEST-014  Rejects a missing configuration file
TEST-015  Reports invalid JSON as failure
TEST-016  Supports documented JSON parsing options
```

Primary requirement relationships:

```text
FR-001
FR-002
FR-016
NFR-008
NFR-009
```

---

## PathService

Initial behaviors to verify where practical:

```text
TEST-017  Finds the repository root from a nested runtime location
TEST-018  Resolves Configuration from the repository root
TEST-019  Reports repository discovery failure clearly
TEST-020  Reports a missing Configuration directory clearly
```

Primary requirement relationships:

```text
FR-007
FR-008
NFR-011
NFR-022
```

Some path-discovery behavior may require controlled temporary-directory fixtures rather than pure unit tests.

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

`TRACEABILITY.md` currently reserves the concept of `TEST-###` identifiers.

When the automated test project is established, important traceable tests should receive stable identifiers.

Example:

```text
TEST-008
Requirement: FR-012
Test: BuildPlan_DeepNestedStructure_IncludesEveryNode
```

Not every micro-test must necessarily receive a formal requirement ID relationship, but safety-critical and acceptance-driving tests should be traceable.

Identifiers should not replace descriptive test method names.

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

A future test project may begin with a structure similar to:

```text
CareerOS.Bootstrap.Tests/
â”œâ”€â”€ Services/
â”‚   â”œâ”€â”€ DirectoryPlanServiceTests.cs
â”‚   â”œâ”€â”€ JsonConfigurationServiceTests.cs
â”‚   â”œâ”€â”€ PathServiceTests.cs
â”‚   â””â”€â”€ TemplateResolverServiceTests.cs
â”‚
â”œâ”€â”€ Models/
â”œâ”€â”€ Fixtures/
â”œâ”€â”€ Integration/
â”‚   â””â”€â”€ Filesystem/
â””â”€â”€ CareerOS.Bootstrap.Tests.csproj
```

As functionality evolves, additional areas may be introduced for validation, CLI, orchestration, provisioning, logging, and release-related behavior.

The structure should evolve with real testing needs rather than creating empty abstraction layers prematurely.

---

# Test Framework Selection

A specific .NET test framework has not yet been formally selected in the project documentation.

The framework decision should be made when the test project is created and recorded where appropriate.

Selection criteria should include:

- .NET 8 compatibility
- Visual Studio integration
- `dotnet test` support
- Clear assertion behavior
- Maintainability
- CI support
- Minimal unnecessary dependency overhead

A framework should not be documented as adopted until it is actually added to the solution.

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

Before a pull request or merge, developers should eventually run:

```powershell
dotnet restore
dotnet build
dotnet test
```

Until the test project exists, the current minimum remains:

```powershell
dotnet build
dotnet run --project .\CareerOS.Bootstrap\CareerOS.Bootstrap.csproj
```

plus review of relevant Git changes.

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

# Definition of Done â€” Testing Perspective

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

Recommended sequence:

```text
1. Establish CareerOS.Bootstrap.Tests
2. Select .NET test framework
3. Add TemplateResolverService tests
4. Add DirectoryPlanService tests
5. Add JsonConfigurationService tests
6. Add practical PathService tests
7. Assign initial TEST-### identifiers
8. Update TRACEABILITY.md
9. Add validation-service tests when validation is implemented
10. Add isolated filesystem integration fixture
11. Implement provisioning safety/idempotency tests
12. Add dotnet test to GitHub CI
```

This order protects existing behavior before introducing filesystem provisioning.

---

# Summary

The testing strategy evolves CareerOS.Bootstrap from manual validation toward requirements-driven automated verification.

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
