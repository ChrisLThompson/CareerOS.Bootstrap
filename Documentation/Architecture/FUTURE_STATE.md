# CareerOS.Bootstrap --- Future State

## Purpose

This document describes the **target and planned architecture** for
`CareerOS.Bootstrap`.

Unlike `CURRENT_STATE.md`, this document includes capabilities that do
**not** necessarily exist yet. Its purpose is to provide a controlled
architectural direction for future development without representing
planned functionality as implemented.

The future state should evolve as requirements, testing, and
architectural decisions mature.

------------------------------------------------------------------------

## Target Outcome

CareerOS.Bootstrap is intended to evolve from a read-only directory
planning utility into a safe, configuration-driven provisioning platform
capable of creating and maintaining standardized CareerOS environments.

The target lifecycle is:

``` text
Input
  |
  v
Configuration
  |
  v
Validation
  |
  v
Resolution
  |
  v
Planning
  |
  v
Dry-Run Review
  |
  v
Provisioning
  |
  v
Verification
  |
  v
Logging / Reporting
```

The application should remain safe to rerun and should preserve valid
existing user content.

------------------------------------------------------------------------

## Target Architectural Principles

Future development should preserve the principles already established by
the current implementation.

### Configuration-driven behavior

Profiles, templates, destination settings, and supported options should
remain external to core application logic wherever practical.

### Separation of planning and execution

The component that determines what should happen should remain separate
from the component that changes the filesystem.

### Validate before modifying

Configuration, paths, templates, and execution options should be
validated before provisioning begins.

### Idempotent provisioning

Running the same valid provisioning operation repeatedly should converge
on the desired structure rather than damage or duplicate it.

### Safe defaults

Potentially destructive or irreversible behavior should never be the
default.

### Observable execution

Users should be able to understand what the application planned,
performed, skipped, and failed to perform.

### Testable boundaries

Filesystem, configuration, command-line, and planning behavior should be
separated sufficiently to support focused automated testing.

------------------------------------------------------------------------

## Target High-Level Architecture

``` mermaid
flowchart TD
    A[User / Automation] --> B[Command-Line Interface]
    B --> C[Application Orchestrator]
    C --> D[Path / Environment Resolution]
    C --> E[Configuration Loader]
    E --> F[Configuration Validation]
    D --> F
    F --> G[Profile Resolution]
    G --> H[Template Resolution]
    H --> I[Directory Planning]
    I --> J{Execution Mode}
    J -->|Dry Run| K[Preview / Execution Summary]
    J -->|Provision| L[Filesystem Provisioning]
    L --> M[Existing-State Inspection]
    M --> N[Create Missing Directories]
    N --> O[Verification]
    K --> P[Reporting / Logging]
    O --> P
    P --> Q[Process Exit Result]
```

This diagram represents a target direction, not the current
implementation.

------------------------------------------------------------------------

## Planned Application Layers

### Entry and command-line layer

A future command-line layer should translate user intent into
application options.

Potential capabilities include:

``` text
--dry-run
--profile
--root
--template
--config
--help
--version
```

Exact syntax is not yet finalized. The command-line layer should not
contain provisioning business logic.

### Orchestration layer

As the application grows, `Program.Main()` should remain small.

A future application-level orchestrator may coordinate configuration
loading, validation, profile selection, template resolution, planning,
execution mode, provisioning, reporting, and exit-code mapping.

Conceptually:

``` text
Program.Main
    |
    v
Application Runner / Orchestrator
    |
    +--> Configuration
    +--> Validation
    +--> Planning
    +--> Provisioning
    +--> Reporting
```

The exact class name and implementation should be decided when
complexity justifies the abstraction.

### Configuration layer

Future configuration may expand beyond the current profile/template
definitions.

Potential configuration areas include:

-   Destination root
-   Schema/version information
-   Default execution behavior
-   Profile enablement
-   Template metadata
-   Logging options
-   Future feature flags

Configuration should remain human-readable and version-controlled where
appropriate.

### Validation layer

A dedicated validation service is planned.

Potential validation responsibilities include:

-   Required profile and template values
-   Duplicate profile names or destination directories
-   Duplicate template names
-   Missing template references
-   Invalid path characters or reserved filesystem names
-   Conflicting paths
-   Duplicate sibling directories
-   Empty required collections
-   Unsupported configuration versions

Validation should produce actionable results before filesystem
provisioning is permitted.

A future validation result may conceptually contain:

``` text
Severity
Code
Message
Location
Suggested Resolution
```

The exact model is not yet finalized.

------------------------------------------------------------------------

## Planned Directory Planning Evolution

`DirectoryPlanService` currently produces strings representing intended
directory paths.

Future planning may evolve toward a richer plan model:

``` text
ProvisioningPlan
â””â”€â”€ Actions[]
    â”œâ”€â”€ TargetPath
    â”œâ”€â”€ ActionType
    â”œâ”€â”€ CurrentState
    â”œâ”€â”€ DesiredState
    â””â”€â”€ Reason
```

Possible action types could include:

``` text
Create
Exists
Skip
Invalid
Conflict
```

This would allow dry-run output and actual provisioning to consume the
**same validated plan**, reducing the risk that preview behavior
diverges from execution behavior.

------------------------------------------------------------------------

## Planned Filesystem Provisioning

A dedicated provisioning service should eventually perform filesystem
changes.

Conceptually:

``` text
DirectoryProvisioningService
```

Its responsibilities may include:

-   Receive an already validated plan
-   Inspect existing filesystem state
-   Create missing directories
-   Preserve existing directories
-   Record completed and skipped actions
-   Surface failures
-   Return a structured result

The provisioning service should not decide which template a profile
uses. It should operate on a plan produced earlier in the workflow.

------------------------------------------------------------------------

## Existing-State Inspection and Idempotency

Before creating anything, future provisioning should determine what
already exists.

``` text
Desired: CareerOS\User\Resume
Current: Missing
Result:  Create

Desired: CareerOS\User\Resume\Master
Current: Exists
Result:  Preserve
```

Repeated execution should be safe. A first run might create missing
directories while preserving existing ones; a second identical run
should create nothing and preserve the complete valid structure.

------------------------------------------------------------------------

## Planned Dry-Run Mode

The current application behaves like a dry run but does not yet expose a
true command-line execution mode.

Future behavior should support an explicit option such as:

``` powershell
CareerOS.Bootstrap --dry-run
```

A true dry run should load and validate configuration, resolve
profiles/templates, inspect relevant existing state where appropriate,
build the same plan that execution would use, display intended actions,
and perform no provisioning writes.

Dry-run and real execution should share as much planning logic as
possible.

------------------------------------------------------------------------

## Planned Destination Root Configuration

The current `_Preview` path is temporary.

Future execution should support a configurable CareerOS root rather than
a hard-coded machine-specific location.

Potential precedence may eventually be:

``` text
Command-line override
        |
        v
Configuration value
        |
        v
Documented default
```

The exact precedence rules should be defined through requirements and an
Architecture Decision Record before implementation.

------------------------------------------------------------------------

## Planned Reporting and Logging

Execution should eventually produce a structured summary showing
profiles processed, directories planned, directories created or already
existing, warnings, and errors.

Structured logging is also planned. Potential logged information
includes application version, execution timestamp and mode, selected
profiles, configuration source, destination root, validation results,
planned/completed/skipped actions, warnings, errors, and final execution
result.

Logs should avoid exposing secrets or unnecessary sensitive data. A
logging library should not be selected until requirements justify the
dependency.

------------------------------------------------------------------------

## Planned Error Model

Future development may introduce structured application errors rather
than relying solely on exception messages.

Potential categories include:

``` text
Configuration Error
Validation Error
Path Error
Template Resolution Error
Provisioning Error
Filesystem Permission Error
Unexpected Application Error
```

Future exit codes may distinguish failure categories for automation and
CI usage. The taxonomy is not yet finalized.

------------------------------------------------------------------------

## Planned Testing Architecture

A dedicated automated test project is planned:

``` text
CareerOS.Bootstrap.Tests
```

Expected initial coverage includes:

``` text
TemplateResolverServiceTests
DirectoryPlanServiceTests
JsonConfigurationServiceTests
PathServiceTests
```

Future provisioning will require additional tests such as:

``` text
DirectoryProvisioningServiceTests
ConfigurationValidationServiceTests
ApplicationWorkflowTests
```

Unit tests should verify focused service behavior, while actual
provisioning should use filesystem integration tests against isolated
temporary directories rather than real CareerOS environments.

A typical filesystem test lifecycle should be:

``` text
Create isolated temporary test root
        |
        v
Execute provisioning
        |
        v
Verify filesystem state
        |
        v
Execute provisioning again
        |
        v
Verify idempotency
        |
        v
Remove temporary test root
```

------------------------------------------------------------------------

## Requirements Traceability

Future requirements documentation should connect business intent to
implementation.

``` text
Business Need
      |
      v
User Story
      |
      v
Functional / Non-Functional Requirement
      |
      v
Architecture Decision
      |
      v
Component / Implementation
      |
      v
Automated Test
      |
      v
Acceptance Result
```

Stable identifiers should include:

``` text
US-###
FR-###
NFR-###
ADR-###
```

------------------------------------------------------------------------

## Planned Architecture Decision Records

Major future decisions should receive ADRs. Likely subjects include:

-   Explicit Main Entry Point
-   JSON-Driven Configuration
-   Recursive Directory Model
-   Planning / Provisioning Separation
-   Repository Root Discovery
-   Multi-Profile Template Model
-   Destination Root Precedence
-   CLI Framework Selection
-   Logging Strategy
-   Provisioning Plan Model
-   Filesystem Safety Policy

ADRs should record decisions rather than merely restating
implementation.

------------------------------------------------------------------------

## Planned CI/CD and Branch Protection

GitHub-based continuous integration is planned.

A future workflow may perform:

``` text
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
Validation Result
```

As the repository matures, `main` may be protected with rules such as
required pull requests, successful build/tests, and protection from
accidental force pushes or deletion.

The process should match project maturity rather than add unnecessary
overhead.

------------------------------------------------------------------------

## Planned Release and Distribution Model

Future releases may produce a packaged executable rather than requiring
users to run from source.

Potential lifecycle:

``` text
Source
  |
  v
Build
  |
  v
Test
  |
  v
Package
  |
  v
Version
  |
  v
GitHub Release
```

Potential distribution options include framework-dependent or
self-contained executables and packaged release archives.

Semantic Versioning (`MAJOR.MINOR.PATCH`) is a possible versioning
strategy but should be formally decided before releases begin.

Cross-platform distribution should not be promised until explicitly
tested and supported.

------------------------------------------------------------------------

## Planned Configuration Versioning

As configuration evolves, future JSON schemas may require version
information:

``` json
{
  "schemaVersion": 1
}
```

Versioning could allow the application to detect unsupported
configurations, provide migration guidance, preserve compatibility where
practical, and avoid silently interpreting incompatible structures.

This feature is planned, not implemented.

------------------------------------------------------------------------

## Planned Backup and Rollback

Backup or rollback functionality may become appropriate if future
versions modify more than missing directories.

If future capabilities include file generation/replacement, directory
renaming, configuration migration, or content movement, backup and
rollback requirements should be defined before those operations are
implemented.

------------------------------------------------------------------------

## Planned Git Integration

Optional Git initialization may eventually be supported for generated
CareerOS development areas.

Potential functionality could detect existing repositories, optionally
initialize a new repository, generate appropriate `.gitignore` content,
and report repository status.

Git operations should remain optional and should never unexpectedly
modify an existing repository.

------------------------------------------------------------------------

## Future Application Flow

``` mermaid
sequenceDiagram
    actor User
    participant CLI
    participant App as Application Orchestrator
    participant Config as Configuration Service
    participant Validator as Validation Service
    participant Resolver as Template Resolver
    participant Planner as Directory Planner
    participant Provisioner as Provisioning Service
    participant FS as Filesystem
    participant Logger as Reporting / Logging

    User->>CLI: Start application with options
    CLI->>App: Parsed execution request
    App->>Config: Load configuration
    Config-->>App: Configuration models
    App->>Validator: Validate request + configuration
    Validator-->>App: Validation result
    App->>Resolver: Resolve selected profile/template
    Resolver-->>App: Resolved template
    App->>Planner: Build provisioning plan
    Planner-->>App: Provisioning plan

    alt Dry-run mode
        App->>Logger: Report planned actions
        Logger-->>User: Dry-run summary
    else Provision mode
        App->>Provisioner: Execute validated plan
        Provisioner->>FS: Inspect/create directories
        FS-->>Provisioner: Filesystem results
        Provisioner-->>App: Provisioning result
        App->>Logger: Report execution
        Logger-->>User: Provisioning summary
    end
```

This sequence represents the intended direction and may change as
implementation decisions are made.

------------------------------------------------------------------------

## Future Component Direction

Potential future components include:

``` text
ApplicationRunner
CommandLineOptions
ConfigurationValidationService
ValidationResult
ProvisioningPlan
ProvisioningAction
DirectoryProvisioningService
ProvisioningResult
ExecutionSummary
Logging / Reporting abstraction
```

These names are conceptual. They should not be created merely because
they appear in this document. New abstractions should be introduced only
when requirements and implementation complexity justify them.

------------------------------------------------------------------------

## Non-Goals Unless Requirements Change

The following are not current architectural priorities:

-   Full desktop GUI
-   Web application
-   Cloud-hosted service
-   Database-backed configuration
-   Real-time multi-user collaboration
-   Enterprise identity integration
-   Remote filesystem administration

These may be reconsidered only if future CareerOS requirements create a
legitimate need.

------------------------------------------------------------------------

## Security Evolution

Future features should trigger corresponding security review.

Filesystem provisioning should consider path traversal, permissions,
unexpected target roots, reparse-point behavior where relevant, and
unsafe overwrite behavior.

Logging should consider personal or sensitive data exposure.

Git integration should consider credential handling, remote URLs, and
preservation of existing repositories.

Remote/cloud features, if ever introduced, should define authentication,
authorization, data protection, and network-security requirements before
implementation.

------------------------------------------------------------------------

## Future Documentation Expectations

When a planned capability becomes implemented:

1.  Update `CURRENT_STATE.md`.
2.  Update this document if the target architecture changes.
3.  Update component documentation.
4.  Update relevant diagrams.
5.  Update requirements traceability.
6.  Add or update tests.
7.  Update the roadmap.
8.  Update `CHANGELOG.md` when appropriate.

This prevents documentation from becoming a historical snapshot that no
longer matches the application.

------------------------------------------------------------------------

## Target Definition of Done for Provisioning

Filesystem provisioning should not be considered complete merely because
directories can be created.

A mature initial provisioning feature should demonstrate:

-   Configuration validation
-   Valid profile/template resolution
-   Configurable destination root
-   Dry-run preview
-   Existing-directory detection
-   Safe missing-directory creation
-   Idempotent repeat execution
-   Clear execution summary
-   Error handling
-   Automated unit testing
-   Filesystem integration testing
-   Updated documentation
-   Requirements traceability
-   Successful pull-request validation

------------------------------------------------------------------------

## Evolution Strategy

Preferred sequence:

``` text
Documentation Foundation
        |
        v
Requirements Foundation
        |
        v
Testing Foundation
        |
        v
Configuration Validation
        |
        v
Provisioning Plan Evolution
        |
        v
Filesystem Provisioning
        |
        v
Logging / Reporting
        |
        v
CLI Expansion
        |
        v
CI / Release Automation
```

This sequence may change when requirements justify a different priority.

------------------------------------------------------------------------

## Relationship to Current State

The future architecture builds directly on existing components rather
than discarding the current foundation.

``` text
CURRENT
PathService
JsonConfigurationService
TemplateResolverService
DirectoryPlanService

        |
        | evolve
        v

FUTURE
Path / Environment Resolution
Configuration + Validation
Profile / Template Resolution
Rich Provisioning Planning
Filesystem Provisioning
Reporting / Logging
Automated Testing
CI / Releases
```

Existing components may be refactored as requirements mature, but
validated behavior should be preserved unless intentionally changed.

------------------------------------------------------------------------

## Summary

The target state of CareerOS.Bootstrap is a safe, testable,
configuration-driven provisioning application capable of moving from
declared CareerOS structure to verified filesystem state.

The intended future lifecycle is:

``` text
Declare
  |
  v
Validate
  |
  v
Resolve
  |
  v
Plan
  |
  v
Preview
  |
  v
Provision
  |
  v
Verify
  |
  v
Report
```

The architecture should continue to prioritize **safety, traceability,
maintainability, idempotency, and clear separation between intent and
execution**.

Planned functionality documented here should become current
functionality only after it is implemented, tested, reviewed,
documented, and merged into the stable codebase.
