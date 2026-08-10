# CareerOS.Bootstrap --- System Context

## Purpose

This document provides the highest-level system-context view of
`CareerOS.Bootstrap`, identifying current actors, inputs, outputs,
system boundaries, planned provisioning, and future external
integrations.

Status labels used throughout:

-   **CURRENT** --- implemented or actively used.
-   **PLANNED** --- defined architectural direction, not yet fully
    implemented.
-   **FUTURE** --- longer-term extension outside the current
    implementation commitment.

## System Context Diagram

``` mermaid
flowchart LR
    Dev["Developer / Maintainer<br/>CURRENT"]
    User["CareerOS User<br/>CURRENT / PLANNED"]

    subgraph Repo["CareerOS.Bootstrap Repository â€” CURRENT"]
        App["CareerOS.Bootstrap<br/>.NET 8 Console Application"]
        Config["JSON Configuration<br/>bootstrap.json / templates.json"]
        Docs["Version-Controlled Documentation"]
    end

    Console["Console / Terminal<br/>CURRENT"]
    Plan["Read-Only Directory Plan<br/>CURRENT"]
    Workspace["CareerOS User Workspace<br/>PLANNED"]
    FS["Local Filesystem<br/>PLANNED WRITE TARGET"]
    Git["Git / GitHub<br/>CURRENT DEV WORKFLOW<br/>FUTURE OPTIONAL WORKSPACE INTEGRATION"]
    SQL["SQL Server / SSMS<br/>FUTURE"]
    Web["CareerOS Web / Documentation Portal<br/>FUTURE"]

    Dev -->|"maintains"| Repo
    Dev -->|"builds / runs"| App
    User -->|"future profile/options"| App
    Config -->|"profiles + templates"| App
    App -->|"dry-run output"| Console
    App -->|"generates"| Plan
    Plan -->|"describes desired state"| User

    App -.->|"future validated provisioning"| Workspace
    Workspace -.->|"directories / artifacts"| FS

    Dev -->|"branch / commit / review"| Git
    Repo -->|"source controlled in"| Git
    App -.->|"future opt-in Git initialization"| Git

    Docs -.->|"future structured metadata"| SQL
    SQL -.->|"queryable project data"| Web
    Docs -.->|"publishable documentation"| Web
```

## Current System Boundary

The current executable is a local `.NET 8` console application. Its
active flow is:

``` text
Developer
    |
    v
CareerOS.Bootstrap
    |
    +--> Repository Path Discovery
    +--> JSON Configuration
    +--> Profile / Template Resolution
    +--> Recursive Directory Planning
    |
    v
Console Dry-Run Output
```

The current application plans directory structures. It does **not**
currently provision the final CareerOS user workspace. This is a core
safety boundary.

## Current Actors and Resources

### Developer / Maintainer --- CURRENT

The developer maintains source, JSON configuration, documentation,
builds, runtime validation, and Git history. Git branches and commits
are used to isolate and review repository changes before integration
into `main`.

### CareerOS User --- CURRENT / PLANNED

User profiles already exist as configuration data and participate in
directory-plan generation. Planned interaction may allow
profile/template selection, destination selection, preview, explicit
provisioning, and review of execution results.

The final user interface is not yet fixed.

### JSON Configuration --- CURRENT

Repository configuration currently includes:

``` text
Configuration/
â”œâ”€â”€ bootstrap.json
â””â”€â”€ templates.json
```

These provide profile and template definitions. Configuration remains
external to compiled C# so supported profiles and directory structures
can scale without person-specific application logic.

### Console / Terminal --- CURRENT

The console is the current presentation boundary and reports
repository/configuration context, profiles/templates, directory plans,
dry-run status, and top-level success/failure information.

### Local Filesystem --- CURRENT READ CONTEXT / PLANNED WRITE TARGET

The filesystem currently supports repository discovery and configuration
loading. The final CareerOS workspace is a planned write target.

Before write-capable provisioning is enabled, planned safeguards include
destination/configuration validation, explicit execution mode,
existing-state inspection, idempotency, user-content preservation,
structured results, and isolated filesystem tests.

## Git and GitHub Context

Git/GitHub already belong to the development lifecycle:

``` text
Purpose-Specific Branch
        |
        v
Local Validation
        |
        v
Commit / Push
        |
        v
Review / Merge
        |
        v
main
```

This is distinct from a **future application feature** that might
optionally initialize Git inside a generated CareerOS workspace.

If workspace Git integration is implemented, it should be opt-in, detect
and preserve existing repository state, avoid silent initialization, and
clearly report failures.

## Planned CareerOS Workspace

The primary planned external output is a reusable filesystem workspace:

``` text
CareerOS.Bootstrap
        |
        v
Validated Provisioning Plan
        |
        v
CareerOS Root
        |
        +--> User Profile
        |      +--> Resume
        |      +--> Career History
        |      +--> Applications
        |      +--> Supporting Artifacts
        |      +--> Template-Specific Structure
        |
        +--> Additional Profiles
```

Exact structure remains configuration-driven.

## Future SQL Server / SSMS Extension

A future extension may introduce Microsoft SQL Server as a structured
project-information and traceability store. This is **not part of the
current Bootstrap runtime**.

Potential structured entities include:

``` text
User Stories
Functional Requirements
Non-Functional Requirements
Acceptance Criteria
Components
Architecture Decisions
Tests
Scripts
Documentation Metadata
Releases
Traceability Relationships
```

This could support searchable requirements, requirement-to-test
coverage, component impact analysis, documentation indexing, reporting,
API access, and a future website.

For example:

``` text
Which functional requirements do not yet have an automated test?
```

A relational traceability model could answer this without manually
inspecting multiple Markdown documents.

Markdown and Git should remain human-readable, source-controlled
documentation even if a database layer is introduced.

## Future Web / Documentation Portal

A future browser experience could consume version-controlled
documentation and structured project data:

``` text
Git / Markdown
      |
      +----------+
                 |
SQL Server ------+
                 |
                 v
          Application / API
                 |
                 v
        Web / Documentation Portal
```

Potential capabilities include documentation navigation, requirements
search, traceability views, architecture diagrams, test coverage status,
release history, and roadmap information.

This remains a future extension and should not prematurely shape the
Bootstrap engine.

## Trust and Safety Boundaries

### Configuration to Application

``` text
Configuration
     |
     v
Load
     |
     v
Validate
     |
     X  Blocking Error
     |
     v
Planning / Execution
```

Invalid configuration must not reach future filesystem modification.

### Planning to Provisioning

``` text
Desired-State Planning
        |
        v
Validated Plan
        |
        +--> Dry Run --------> No Writes
        |
        +--> Explicit Provisioning
                    |
                    v
               Filesystem
```

Planning must remain independently executable.

### Repository to User Workspace

``` text
CareerOS.Bootstrap Repository
          |
          | generates / provisions
          v
CareerOS User Workspace
```

Application-development artifacts and user workspace content are
separate concerns.

### Local Application to Future External Systems

Future SQL Server, API, or web integrations introduce additional
security and operational concerns, including authentication,
authorization, secrets management, data classification, privacy, network
security, availability, backup/recovery, database migration, and API
contracts.

These concerns remain deferred until those integrations enter
implementation scope.

## Current vs Target Context

### Current

``` text
Developer
   |
   v
Git-Controlled Repository
   |
   +--> JSON Configuration
   |
   v
CareerOS.Bootstrap
   |
   v
Read-Only Plan
   |
   v
Console
```

### Planned

``` text
Developer / User
       |
       v
Execution Request
       |
       v
CareerOS.Bootstrap
       |
       v
Validation
       |
       v
Provisioning Plan
       |
       +--> Dry Run
       |
       +--> Explicit Provisioning
                 |
                 v
          CareerOS Workspace
```

### Future Extension

``` text
Git / Markdown
      |
      +----------+
                 |
SQL Server ------+
                 |
                 v
          Project Data / API
                 |
                 v
        Web / Documentation Portal
```

## Context Principles

1.  Profiles and templates remain configuration-driven.
2.  Safe defaults, preview, and validation precede filesystem
    modification.
3.  Planning, provisioning, persistence, and presentation remain
    separate concerns.
4.  Existing valid user content is preserved.
5.  Requirements, implementation, and tests become increasingly
    traceable.
6.  Git remains authoritative history for source and Markdown
    documentation.
7.  Future SQL Server storage enhances queryability rather than
    unnecessarily replacing readable repository documentation.
8.  A future website consumes well-defined interfaces rather than
    pushing web concerns into the Bootstrap engine.
9.  Implemented and conceptual behavior remain clearly labeled.
10. External integrations require explicit security design before
    implementation.

## Related Documentation

``` text
Documentation/Architecture/ARCHITECTURE.md
Documentation/Architecture/CURRENT_STATE.md
Documentation/Architecture/FUTURE_STATE.md
Documentation/Architecture/COMPONENTS.md
Documentation/Architecture/DATA_FLOW.md

Documentation/Requirements/USER_STORIES.md
Documentation/Requirements/FUNCTIONAL_REQUIREMENTS.md
Documentation/Requirements/NON_FUNCTIONAL_REQUIREMENTS.md
Documentation/Requirements/TRACEABILITY.md

Documentation/Development/DEVELOPMENT_GUIDE.md
Documentation/Development/CODING_STANDARDS.md
Documentation/Development/TESTING_STRATEGY.md
```

## Summary

The current context is intentionally small:

``` text
Configuration
     |
     v
Bootstrap Application
     |
     v
Read-Only Plan
```

The planned context adds controlled provisioning:

``` text
Configuration + User Intent
            |
            v
       Validation
            |
            v
      Bootstrap Plan
            |
            v
    CareerOS Workspace
```

Longer-term extensions may add SQL Server-backed traceability and a
web/documentation experience, but those systems remain outside the
current runtime boundary.

> **Understand and validate desired state before changing actual
> state.**
