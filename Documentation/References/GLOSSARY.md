# CareerOS.Bootstrap — Glossary

## Purpose

This glossary defines terminology used throughout the `CareerOS.Bootstrap` source code, configuration, architecture documentation, requirements, diagrams, development guidance, and roadmap.

The goal is to maintain a shared vocabulary so that current behavior, planned capabilities, and future concepts are interpreted consistently.

## Status Terminology

### CURRENT

A capability, component, behavior, or artifact that is implemented or actively used in the current project state.

`CURRENT` does not necessarily mean feature-complete or permanent.

### PLANNED

A capability or architectural direction that has been intentionally identified for implementation but is not yet fully implemented.

Planned concepts must not be described as current application behavior.

### FUTURE

A longer-term extension, integration, or architectural possibility outside the current implementation commitment.

Future concepts may change substantially before implementation.

---

## Project Terminology

### CareerOS

The broader concept and ecosystem for organizing career-related information, workflows, documents, automation, and supporting capabilities.

`CareerOS.Bootstrap` is one component of that broader direction.

### CareerOS.Bootstrap

The .NET 8 application and repository responsible for configuration-driven planning of CareerOS workspace directory structures and, in the planned state, safe provisioning of those structures.

### Bootstrap

The process of establishing an initial CareerOS workspace structure from configuration.

In the current implementation, bootstrap behavior is limited to planning and previewing directory structures.

### Bootstrap Process

The end-to-end application workflow that discovers configuration, loads models, resolves profiles and templates, recursively plans directories, and displays the resulting dry-run plan.

Future versions are expected to extend this process with validation, provisioning, verification, and reporting.

### Repository

The Git-controlled `CareerOS.Bootstrap` project containing application source code, configuration, documentation, and supporting project files.

### Repository Root

The top-level directory containing the solution, project documentation, configuration directory, and Git metadata.

### Workspace

The CareerOS directory structure associated with a profile.

The workspace is currently a planned filesystem output rather than a structure written by the application.

---

## Configuration Terminology

### Configuration

Declarative information that controls application behavior without requiring person-specific or template-specific behavior to be hard-coded into application logic.

### `bootstrap.json`

The current JSON configuration source containing profile definitions and profile-to-template assignments.

### `templates.json`

The current JSON configuration source containing reusable directory template definitions.

### Bootstrap Configuration

The strongly typed application representation of bootstrap configuration data.

The current model is represented by `BootstrapConfiguration`.

### Template Configuration

The strongly typed application representation of template configuration data.

The current model is represented by `TemplateConfiguration`.

### Profile

A configured CareerOS workspace identity or target.

A profile identifies information such as the profile name, destination directory, and assigned template.

### Profile Configuration

The strongly typed representation of a configured profile.

The current model is represented by `ProfileConfiguration`.

### Template

A reusable definition of the directory hierarchy that should be planned for one or more profiles.

Templates allow multiple profiles to share a common structure without duplicating the hierarchy in profile configuration.

### Template Assignment

The logical relationship between a profile and the named template that defines its directory structure.

### Template Resolution

The process of matching a profile's configured template name to the corresponding template definition.

### Directory Node

A single directory definition within a template hierarchy.

The current model is represented by `DirectoryNode`.

### `DirectoryNode.Children`

The recursive child collection that allows a directory node to contain additional directory nodes.

This architecture allows directory structures to extend beyond a fixed nesting depth.

### Recursive Directory Structure

A hierarchy in which directory nodes may contain child directory nodes, which may themselves contain additional children.

### Declarative Configuration

Configuration that describes the desired structure or intent rather than directly specifying procedural application steps.

---

## Application Terminology

### `Program.Main()`

The explicit application entry point used by the current console application.

It currently coordinates application startup, service creation, execution, console output, and top-level exception handling.

### Service

A class responsible for a focused application capability or domain responsibility.

Current services include path discovery, configuration loading, template resolution, and directory planning.

### `PathService`

The current service responsible for repository and configuration path discovery.

### `JsonConfigurationService`

The current service responsible for reading JSON configuration and deserializing it into strongly typed models.

### `TemplateResolverService`

The current service responsible for resolving a profile's assigned template.

### `DirectoryPlanService`

The current service responsible for recursively traversing template directory nodes and producing planned directory paths.

### Model

A strongly typed data structure representing application or configuration data.

### Application Boundary

The outer execution boundary of the console application, including startup and top-level exception handling.

### Orchestrator

A planned application component responsible for coordinating the major execution stages without concentrating all workflow logic in `Program.Main()`.

The final component name is not yet fixed.

---

## Planning Terminology

### Directory Plan

The current read-only result produced from a profile destination and resolved template hierarchy.

It represents directories that would form the CareerOS workspace.

### Planning

The process of transforming configuration and resolved templates into intended directory paths without modifying the target filesystem.

### Dry Run

An execution mode that calculates and displays intended actions without applying filesystem changes.

The current application effectively operates as a dry-run planner.

### Preview

Human-readable output showing the planned directory structure or actions before provisioning.

### Desired State

The filesystem structure defined by validated configuration and planning logic.

### Current State

The observed state of the target filesystem at execution time.

Existing-state inspection is planned functionality.

### State Comparison

The planned process of comparing desired state with observed current state to determine what action, if any, is required.

### Provisioning Plan

A planned richer representation of intended filesystem operations.

Unlike the current path-only directory plan, a future provisioning plan may contain explicit actions, current state, desired state, and reasons.

### Provisioning Action

A planned unit of work describing what should happen to a target path.

Potential classifications include `CREATE`, `PRESERVE`, `SKIP`, `CONFLICT`, and `REJECT`. Exact names remain subject to implementation design.

---

## Provisioning Terminology

### Provisioning

The planned process of applying an approved directory plan to the filesystem.

Provisioning is not part of the current implementation.

### Filesystem Write

An operation that changes filesystem state, such as creating a directory.

Current planning behavior does not perform CareerOS workspace filesystem writes.

### Explicit Provisioning Intent

A planned safety requirement that actual filesystem changes occur only when execution explicitly requests provisioning rather than preview behavior.

### Existing-State Inspection

The planned process of observing target paths before determining provisioning actions.

### Preserve

A planned action in which an existing valid filesystem object is left unchanged.

### Create

A planned action in which a missing required directory is created.

### Conflict

A planned classification indicating that observed filesystem state cannot safely satisfy the desired state without additional handling or user intervention.

### Reject

A planned classification indicating that an invalid or unsafe requested action should not proceed.

### Idempotency

The property that repeated execution against an already-correct workspace converges on the same desired state without unnecessary duplication or destructive changes.

### Verification

The planned process of confirming that actual filesystem state matches the expected result after provisioning.

### Execution Result

A planned structured representation of validation, planning, provisioning, verification, and outcome information.

---

## Validation Terminology

### Validation

The planned process of checking configuration, execution requests, resolved relationships, and other required conditions before write-capable behavior is allowed.

### Validation Error

A condition indicating that input or configuration does not satisfy a requirement necessary for safe execution.

### Blocking Validation Error

A validation failure severe enough to prevent provisioning.

### Validation Result

A planned structured representation of validation success, warnings, or errors.

### Fail Fast

A design principle in which invalid conditions are detected and reported before later processing or filesystem modification occurs.

---

## Architecture Terminology

### Current State Architecture

The architecture that describes components and behavior implemented today.

### Future State Architecture

The target architectural direction for planned and future capabilities.

### Component

A logical application element with a defined responsibility and relationship to other parts of the system.

### Component Boundary

A separation of responsibility intended to reduce coupling and make behavior easier to understand, test, and evolve.

### Separation of Concerns

The principle that configuration loading, resolution, planning, provisioning, verification, reporting, and other responsibilities should remain distinct rather than being combined into a single component.

### Configuration-Driven Architecture

An architecture in which reusable configuration controls structure and behavior instead of requiring code changes for each profile or template.

### Recursive Architecture

An architecture that represents hierarchical structures through self-referencing child collections rather than fixed-depth models.

### Safety Boundary

A point in the workflow beyond which additional validation or explicit intent is required before potentially destructive or write-capable behavior can occur.

### Guardrail

An architectural, coding, testing, or process constraint intended to prevent unsafe, ambiguous, or inconsistent behavior.

### Architectural Decision Record (ADR)

A planned documentation artifact recording an important architectural decision, its context, alternatives, and consequences.

---

## Documentation Terminology

### Documentation Suite

The version-controlled collection under `Documentation/` covering architecture, development, diagrams, references, requirements, and roadmap material.

### Architecture Documentation

Documents describing system structure, current state, future state, components, and data flow.

### Development Documentation

Documents describing development workflow, coding standards, and testing strategy.

### Diagram Documentation

Markdown documents containing Mermaid and text diagrams that visually describe the system.

### Reference Documentation

Documents intended primarily for lookup, navigation, terminology, and configuration reference.

### Requirements Documentation

Documents containing user stories, functional requirements, non-functional requirements, and traceability relationships.

### Roadmap Documentation

Documents describing intended project evolution and milestone progression without treating future work as already implemented.

### Documentation Index

A planned navigation document that maps the documentation suite and explains where different kinds of project information can be found.

### Source of Truth

The authoritative artifact for a particular category of information.

For example, configuration files are intended to remain the declarative source of truth for bootstrap structure, while Git-versioned Markdown remains the readable documentation source.

---

## Requirements Terminology

### User Story

A requirement-oriented statement describing desired value or behavior from a user or stakeholder perspective.

User stories are identified using `US-###` conventions in the documentation.

### Functional Requirement

A statement describing behavior the system must perform.

Functional requirements use `FR-###` identifiers.

### Non-Functional Requirement

A statement describing a quality attribute, constraint, safety characteristic, maintainability expectation, or other requirement governing how the system behaves.

Non-functional requirements use `NFR-###` identifiers.

### Acceptance Criteria

Conditions used to determine whether a user story or requirement has been satisfactorily implemented.

### Traceability

The ability to connect user stories, requirements, architecture, implementation, tests, and outcomes.

### Traceability Matrix

A structured mapping showing relationships among requirement and implementation artifacts.

### Requirement Identifier

A stable identifier such as `US-###`, `FR-###`, or `NFR-###` used to reference requirements consistently.

---

## Testing Terminology

### Unit Test

An automated test focused on a small component or behavior in isolation.

Unit testing is planned for the project.

### Integration Test

An automated test validating interaction among components or with controlled external boundaries such as a temporary filesystem.

### Filesystem Integration Test

A planned integration test that exercises filesystem behavior against isolated temporary directories rather than real user workspaces.

### Test Fixture

Controlled setup data or infrastructure used to establish repeatable test conditions.

### Test Case

A defined set of conditions, actions, and expected results used to validate behavior.

### Regression

A defect in which previously working behavior stops working after a change.

### Regression Test

A test intended to ensure previously validated behavior remains correct.

### Build Validation

The process of confirming that the solution restores and compiles successfully.

### Continuous Integration (CI)

A planned automated workflow that builds and tests changes as part of repository integration.

---

## Git and Development Workflow Terminology

### `main`

The primary stable branch of the repository.

### Feature Branch

A temporary branch used to develop a scoped change without directly modifying `main`.

### Documentation Branch

A branch focused on documentation work, such as `docs/documentation-v1`.

### Commit

A version-controlled checkpoint representing a coherent set of repository changes.

### Push

The operation that sends local commits to the remote Git repository.

### Pull Request

A GitHub workflow used to review and merge branch changes into another branch, typically `main`.

### Working Tree

The local set of repository files as currently checked out and modified.

### Staging Area

The Git area containing changes selected for inclusion in the next commit.

### Clean Working Tree

A repository state in which no tracked modifications or untracked files remain pending.

### Checkpoint

A deliberate development moment at which changes are built, inspected, committed, and usually pushed before continuing.

---

## Diagram Terminology

### Mermaid

A text-based diagram syntax supported by GitHub Markdown rendering.

### System Context Diagram

A high-level view of the system boundary, users, inputs, outputs, and external systems.

### Component Diagram

A structural view of major application components and their relationships.

### Bootstrap Process Flow

A process-oriented view of execution stages and decisions.

### Data Flow Diagram

A view emphasizing what data moves through the system and how it is transformed.

### Future State Diagram

A visual representation connecting current architecture to planned and longer-term capabilities.

---

## Future Platform Terminology

### SQL Server

A possible future structured persistence platform for searchable project metadata, relationships, execution history, or related CareerOS information.

It is not part of the current bootstrap implementation.

### SQL Server Management Studio (SSMS)

A Microsoft management environment that may be used with a future SQL Server extension.

SSMS itself is not an application datastore.

### Structured Persistence

The future storage of project information in a queryable structured datastore in addition to readable Git-versioned documentation.

### API Layer

A possible future application interface between structured CareerOS data and external consumers such as a web portal.

### CareerOS Web Portal

A possible future web interface for browsing documentation, traceability, project status, diagrams, requirements, releases, or related CareerOS information.

### Search Layer

A possible future capability for querying structured project and documentation information.

---

## Quality Terminology

### Maintainability

The degree to which the project can be understood, modified, tested, and extended safely.

### Testability

The degree to which components and behavior can be validated through repeatable automated tests.

### Reliability

The degree to which the application produces expected outcomes consistently.

### Safety

The degree to which the application prevents unintended or destructive filesystem behavior.

### Observability

The ability to understand application behavior through output, logging, results, and diagnostic information.

### Extensibility

The ability to add capabilities without requiring unnecessary changes to unrelated components.

### Backward Compatibility

The ability for newer application versions to continue supporting previously valid configuration or behavior where compatibility is intentionally maintained.

---

## Naming Conventions

The documentation currently uses the following identifier patterns:

```text
US-###    User Story
FR-###    Functional Requirement
NFR-###   Non-Functional Requirement
ADR-###   Architectural Decision Record (planned)
TEST-###  Test identifier (planned)
```

These identifiers are intended to support traceability as the project evolves.

---

## Core Project Principle

The recurring architectural principle across the documentation can be summarized as:

> **Understand, validate, plan, and verify changes before treating automation as complete.**

For the current implementation, this means the application remains safely on the planning side of the provisioning boundary.

For the planned implementation, it means filesystem writes should occur only after configuration, intent, and desired state have been explicitly understood and validated.
