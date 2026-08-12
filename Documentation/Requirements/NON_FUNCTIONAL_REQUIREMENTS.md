# CareerOS.Bootstrap --- Non-Functional Requirements

## Purpose

This document defines the quality attributes, constraints, and
operational characteristics governing how `CareerOS.Bootstrap`
capabilities are designed, implemented, tested, documented, and
released.

Functional requirements define **what** the application shall do.
Non-functional requirements define **how safely, reliably, maintainably,
and effectively** those capabilities shall be delivered.

This document complements `USER_STORIES.md`,
`FUNCTIONAL_REQUIREMENTS.md`, `TRACEABILITY.md`, and the architecture
documentation. Planned requirements are not evidence of implemented
functionality.

## Requirement Convention

Requirements use stable `NFR-###` identifiers and include category,
status, priority, related functional requirements, requirement text, and
verification criteria. Status values are Implemented, Partially
Implemented, Planned, or Future.

------------------------------------------------------------------------

# Safety and Data Preservation

## NFR-001 --- Preview Execution Must Be Non-Destructive

**Category:** Safety\
**Status:** Implemented\
**Priority:** High\
**Related FRs:** FR-014, FR-015, FR-026

Preview/dry-run execution shall not create, modify, rename, move,
overwrite, or delete CareerOS filesystem content.

**Verification:** Current preview performs no directory creation;
`_Preview` is not physically created; existing content remains
unchanged; future explicit dry-run receives automated no-write tests.

## NFR-002 --- Existing User Content Must Be Preserved

**Category:** Safety / Data Integrity\
**Status:** Planned\
**Priority:** High\
**Related FRs:** FR-021, FR-022, FR-023, FR-024, FR-025

Normal provisioning shall preserve valid existing directories and their
contents.

**Verification:** Existing expected directories are not destructively
recreated; contained files remain untouched; repeat execution preserves
content; isolated integration tests verify preservation.

## NFR-003 --- Destructive Behavior Requires Explicit Design

**Category:** Safety\
**Status:** Planned Safety Constraint\
**Priority:** High\
**Related FRs:** FR-025

Deletion, destructive replacement, bulk cleanup, or equivalent behavior
shall not be an implicit consequence of normal provisioning.

**Verification:** Removing configuration does not automatically delete
physical directories; destructive features require separate
requirements, safeguards, documentation, tests, and explicit user
intent.

## NFR-004 --- Validate Before Filesystem Modification

**Category:** Safety / Integrity\
**Status:** Implemented Foundation / Future Provisioning Enforcement\
**Priority:** High\
**Related FRs:** FR-009, FR-019, FR-021, FR-023

No provisioning operation shall begin while blocking configuration,
destination, or plan-validation errors exist.

**Verification:** Centralized configuration validation now rejects
blocking configuration and template-reference errors before planning.
Destination-root and planned-path containment validation are automated.
Current workflow/integration tests remain non-destructive; future
write-capable provisioning must consume the same blocking validation gate.

------------------------------------------------------------------------

# Reliability and Idempotency

## NFR-005 --- Provisioning Must Be Idempotent

**Category:** Reliability\
**Status:** Planned\
**Priority:** High\
**Related FRs:** FR-021, FR-022, FR-023, FR-024

Repeated provisioning using the same valid effective configuration and
target shall converge on the desired structure without duplicate or
destructive side effects.

**Verification:** First run may create missing directories; a second
identical run creates none; compliant existing structures succeed;
integration tests execute provisioning repeatedly.

## NFR-006 --- Failures Must Not Be Reported as Success

**Category:** Reliability / Observability\
**Status:** Partially Implemented\
**Priority:** High\
**Related FRs:** FR-023, FR-034, FR-035

Failed required operations shall not be silently represented as
successful.

**Verification:** Top-level failures return nonzero; future creation
failures differ from success; affected resources are identified where
practical. Current behavior uses `0` for success and `1` for top-level
failure.

## NFR-007 --- Filesystem Outcomes Must Be Observed

**Category:** Reliability\
**Status:** Future\
**Priority:** High\
**Related FRs:** FR-021, FR-023, FR-024, FR-032, FR-033

Provisioning results shall be based on observable filesystem outcomes
rather than assumptions.

**Verification:** Creation failures surface; state inspection occurs;
summaries distinguish created/existing/skipped/failed actions;
integration tests compare expected and actual state.

------------------------------------------------------------------------

# Configuration Integrity

## NFR-008 --- Invalid Configuration Must Fail Clearly

**Category:** Configuration Integrity\
**Status:** Implemented for Current Validation Scope\
**Priority:** High\
**Related FRs:** FR-016, FR-017, FR-018, FR-019, FR-020

Invalid or unusable configuration shall produce actionable failure
rather than unpredictable behavior or silent fallback.

**Verification:** Missing files, unknown templates, invalid required
values, duplicate configuration, invalid filesystem names, invalid
destination roots, and unsafe planned-path relationships are covered by
automated tests. Structured validation errors identify stable codes,
messages, and affected fields where practical. Blocking validation errors
stop the current workflow before later planning/output stages as applicable.

## NFR-009 --- Supported Configuration Changes Must Not Require Recompilation

**Category:** Maintainability / Configurability\
**Status:** Implemented\
**Priority:** High\
**Related FRs:** FR-001, FR-002, FR-003, FR-004, FR-006

Supported profile/template changes shall remain externally configurable.

**Verification:** Profiles and templates live outside compiled source;
nested structures can change through JSON; adding a supported profile
requires no person-specific C# logic.

## NFR-010 --- Configuration Evolution Must Be Controlled

**Category:** Compatibility / Configuration Integrity\
**Status:** Future\
**Priority:** Medium\
**Related FRs:** FR-019

Incompatible configuration evolution should use an explicit
schema/version strategy.

**Verification:** Unsupported versions fail clearly;
migration/compatibility expectations are documented; version handling is
tested.

------------------------------------------------------------------------

# Portability and Compatibility

## NFR-011 --- Avoid Machine-Specific Development Paths

**Category:** Portability\
**Status:** Implemented\
**Priority:** High\
**Related FRs:** FR-007, FR-008

Repository-based execution shall not depend on a specific username,
drive letter, or hard-coded absolute repository path.

**Verification:** Repository/configuration discovery derives from
runtime/repository location and moving the repository does not require
source changes solely for discovery.

## NFR-012 --- User Destination Must Be Configurable

**Category:** Portability / Usability\
**Status:** Planned\
**Priority:** High\
**Related FRs:** FR-009, FR-010

The final CareerOS destination shall not be permanently tied to the
source repository or developer machine.

**Verification:** Destination can be supplied without recompilation, is
displayed before provisioning, and is validated before use.

## NFR-013 --- Do Not Claim Unvalidated Cross-Platform Support

**Category:** Compatibility\
**Status:** Implemented Documentation Constraint\
**Priority:** Medium

Cross-platform compatibility shall not be claimed until supported
platforms are explicitly defined and validated.

**Verification:** Current Windows assumptions and release platform
support are documented; future claims are backed by platform testing.

------------------------------------------------------------------------

# Maintainability and Architecture

## NFR-014 --- Components Should Have Focused Responsibilities

**Category:** Maintainability\
**Status:** Partially Implemented\
**Priority:** High\
**Related FRs:** FR-004, FR-011, FR-019, FR-021, FR-026

Components should avoid combining unrelated configuration, planning,
provisioning, and presentation responsibilities.

## NFR-015 --- Planning and Provisioning Must Remain Separate

**Category:** Architecture / Safety / Testability\
**Status:** Implemented Foundation / Future Enforcement\
**Priority:** High\
**Related FRs:** FR-011, FR-014, FR-015, FR-023, FR-026

Desired-state planning shall remain separable from filesystem
modification.

**Verification:** Planning runs without provisioning; dry-run consumes
plans without writes; future provisioning consumes validated intent
rather than independently rebuilding template logic.

## NFR-016 --- Core Behavior Must Avoid Person-Specific Logic

**Category:** Scalability / Maintainability\
**Status:** Implemented\
**Priority:** High\
**Related FRs:** FR-003, FR-004, FR-027

Core behavior shall process configured profiles generically.

## NFR-017 --- Documentation Must Distinguish Current and Future State

**Category:** Documentation Quality\
**Status:** Implemented / In Progress\
**Priority:** High\
**Related FRs:** FR-037, FR-038, FR-039, FR-040

Implemented behavior and conceptual/planned behavior shall remain
clearly distinguished in documentation.

------------------------------------------------------------------------

# Testability and Quality Assurance

## NFR-018 --- Core Logic Must Be Automatable in Tests

**Category:** Testability\
**Status:** Planned / Architecture Supports\
**Priority:** High\
**Related FRs:** FR-041, FR-043

Configuration, resolution, validation, and planning behavior shall be
testable without requiring manual console interaction or a real CareerOS
environment.

## NFR-019 --- Filesystem Tests Must Be Isolated

**Category:** Testability / Safety\
**Status:** Planned\
**Priority:** High\
**Related FRs:** FR-042

Provisioning tests shall use temporary isolated filesystem roots, verify
outcomes and idempotency, and never use a real CareerOS user environment
as an automated fixture.

## NFR-020 --- Merge Validation Must Protect Stable Main

**Category:** Quality Assurance / Lifecycle\
**Status:** Planned / Process Partially Established\
**Priority:** High\
**Related FRs:** FR-043, FR-044

Changes intended for `main` should undergo appropriate build, test,
review, and documentation validation before merge.

------------------------------------------------------------------------

# Observability and Diagnostics

## NFR-021 --- Execution Context Must Be Understandable

**Category:** Usability / Observability\
**Status:** Partially Implemented\
**Priority:** High\
**Related FRs:** FR-013, FR-031, FR-032, FR-033

Output shall make the execution mode, profile, template, effective
destination, relevant action status, and final outcome understandable as
applicable.

## NFR-022 --- Errors Must Be Actionable

**Category:** Diagnostics / Usability\
**Status:** Partially Implemented\
**Priority:** High\
**Related FRs:** FR-018, FR-020, FR-034, FR-035

Failure information shall provide sufficient context to identify the
affected area and support correction where practical.

## NFR-023 --- Persistent Logs Must Support Diagnosis

**Category:** Observability\
**Status:** Planned\
**Priority:** Medium\
**Related FRs:** FR-036

Future persistent logs shall provide diagnostic history without becoming
the authoritative source for provisioning state.

------------------------------------------------------------------------

# Security and Privacy

## NFR-024 --- Core Repository Configuration Must Not Require Secrets

**Category:** Security\
**Status:** Implemented\
**Priority:** High

Core local planning shall not require credentials, API keys, passwords,
or equivalent secrets in repository JSON configuration.

## NFR-025 --- Logs Must Minimize Sensitive Data

**Category:** Security / Privacy\
**Status:** Planned\
**Priority:** High\
**Related FRs:** FR-036

Persistent logs shall avoid credentials, secrets, and unnecessary
sensitive/personal data.

## NFR-026 --- Destination Paths Must Be Validated Before Writes

**Category:** Security / Safety\
**Status:** Implemented Foundation / Future Write Enforcement\
**Priority:** High\
**Related FRs:** FR-009, FR-019, FR-023

User/configuration-controlled destinations shall be validated before
write operations.

**Verification:** Invalid destination-root syntax, reserved path
segments, relative planned paths, parent-traversal escapes, and paths
outside the approved destination root are covered by automated tests.
Configured duplicate profile destinations are rejected as
configuration-level destination conflicts. Future security review should
still consider existing filesystem state and reparse/symbolic-link behavior
when write-capable provisioning is introduced.

## NFR-027 --- Git Integration Must Preserve Existing Repository State

**Category:** Safety / Security\
**Status:** Future\
**Priority:** Medium\
**Related FRs:** FR-046, FR-047

Future Git integration shall not unexpectedly replace, reinitialize, or
damage an existing repository; initialization requires explicit intent.

------------------------------------------------------------------------

# Performance and Resource Use

## NFR-028 --- Normal Planning Must Complete Without Avoidable Delay

**Category:** Performance\
**Status:** Implemented for Current Scale / Future Monitoring\
**Priority:** Medium\
**Related FRs:** FR-001, FR-002, FR-011, FR-012

Normal local CareerOS configuration loading, resolution, recursive
planning, and preview should complete interactively without material
unnecessary delay. Numeric targets should be introduced only when scale
requirements justify them.

## NFR-029 --- Avoid Unnecessary External Dependencies

**Category:** Maintainability / Supply Chain\
**Status:** Implemented Foundation\
**Priority:** Medium

Third-party dependencies should be introduced only when their value
justifies maintenance, security, licensing, and deployment cost.

------------------------------------------------------------------------

# Usability

## NFR-030 --- Console Output Must Be Human-Readable

**Category:** Usability\
**Status:** Implemented / Future Expansion Planned\
**Priority:** Medium\
**Related FRs:** FR-013, FR-029, FR-031, FR-032, FR-033, FR-035

Technical users shall be able to understand execution information
without reading source code.

## NFR-031 --- Safe Behavior Must Be the Default

**Category:** Usability / Safety\
**Status:** Implemented Foundation / Future Enforcement\
**Priority:** High\
**Related FRs:** FR-014, FR-015, FR-025, FR-047

Where destructive and non-destructive choices exist, the application
shall default to the safer behavior unless requirements explicitly
define otherwise.

------------------------------------------------------------------------

# Release and Build Quality

## NFR-032 --- Runtime and Platform Support Must Be Documented

**Category:** Compatibility\
**Status:** Partially Implemented\
**Priority:** Medium\
**Related FRs:** FR-030, FR-045

Supported .NET runtime/framework and operating-system assumptions shall
be documented for development and releases.

## NFR-033 --- Release Artifacts Must Be Traceable to Source

**Category:** Release Quality / Traceability\
**Status:** Future\
**Priority:** Medium\
**Related FRs:** FR-030, FR-044, FR-045

Formal release artifacts shall be identifiable by version and traceable
to a known source-control state.

## NFR-034 --- Builds Must Be Validatable From the Repository

**Category:** Build Quality\
**Status:** Implemented Manually / Future Automation Planned\
**Priority:** High\
**Related FRs:** FR-043, FR-044, FR-045

A clean checkout with documented prerequisites shall restore/build
without undocumented machine-specific source changes.

------------------------------------------------------------------------

# Documentation and Traceability Quality

## NFR-035 --- Requirement Identifiers Must Remain Stable

**Category:** Traceability / Maintainability\
**Status:** Implemented Foundation\
**Priority:** High\
**Related FRs:** FR-039

Published `US-###`, `FR-###`, and `NFR-###` identifiers should remain
stable. New requirements receive new identifiers; retired requirements
should not trigger broad renumbering where practical.

## NFR-036 --- Significant Requirements Must Be Verifiable

**Category:** Quality Assurance / Traceability\
**Status:** Planned / Documentation In Progress\
**Priority:** High\
**Related FRs:** FR-039, FR-041, FR-042, FR-043

Requirements shall be specific enough to evaluate through review,
runtime observation, automated testing, or another documented
verification method.

------------------------------------------------------------------------

# Priority Summary

## High Priority

`NFR-001`--`NFR-009` except `NFR-010`; `NFR-011`, `NFR-012`,
`NFR-014`--`NFR-022`, `NFR-024`--`NFR-026`, `NFR-031`,
`NFR-034`--`NFR-036`.

## Medium Priority

`NFR-010`, `NFR-013`, `NFR-023`, `NFR-027`--`NFR-030`, `NFR-032`,
`NFR-033`.

No initial requirement is Low priority because this catalog focuses on
quality attributes relevant to the planned bootstrap architecture.

------------------------------------------------------------------------

# Current Implementation Coverage

Substantially implemented or established today:

``` text
NFR-001
NFR-009
NFR-011
NFR-016
NFR-024
NFR-028
NFR-029
NFR-030
NFR-035
```

Partially implemented, process-based, or currently being established:

``` text
NFR-006
NFR-008
NFR-013
NFR-014
NFR-015
NFR-017
NFR-018
NFR-020
NFR-021
NFR-022
NFR-031
NFR-032
NFR-034
NFR-036
```

Remaining requirements primarily constrain planned provisioning,
validation, logging, security, testing, Git integration, and release
capabilities.

------------------------------------------------------------------------

# Cross-Cutting Relationship to Functional Requirements

Non-functional requirements constrain multiple functional capabilities.
For example:

``` text
FR-023 — Create Missing Directories
            |
            +--> NFR-002 Preserve Existing Content
            +--> NFR-004 Validate Before Modification
            +--> NFR-005 Idempotency
            +--> NFR-006 Accurate Failure Reporting
            +--> NFR-007 Verify Outcomes
            +--> NFR-026 Destination Validation
            +--> NFR-031 Safe Defaults
```

And:

``` text
FR-036 — Record Execution Logs
            |
            +--> NFR-023 Diagnostic Logging
            +--> NFR-025 Sensitive Data Minimization
```

The complete mapping among user stories, functional requirements,
non-functional requirements, components, and tests belongs in
`TRACEABILITY.md`.

------------------------------------------------------------------------

# Verification Strategy

Verification mechanisms may include:

``` text
Documentation Review
Code Review
Static Inspection
Manual Runtime Test
Unit Test
Filesystem Integration Test
Build Validation
Git / CI Validation
Release Validation
```

Safety-critical filesystem behavior should rely on automated
verification wherever practical rather than manual review alone.

------------------------------------------------------------------------

# Requirement Governance

When an NFR changes:

1.  Preserve its identifier if the requirement remains conceptually the
    same.
2.  Update status and verification criteria.
3.  Review related functional requirements.
4.  Review affected architecture documentation.
5.  Review associated automated tests.
6.  Update `TRACEABILITY.md`.
7.  Update roadmap/changelog material where appropriate.

A planned requirement should become Implemented only after observable
behavior and verification support that status.

------------------------------------------------------------------------

# Summary

CareerOS.Bootstrap's non-functional requirements establish the quality
boundaries around its functional behavior.

The central quality progression is:

``` text
Safety
  |
  v
Validation
  |
  v
Reliability
  |
  v
Idempotency
  |
  v
Testability
  |
  v
Observability
  |
  v
Maintainability
  |
  v
Traceability
```

The project should remain conservative around filesystem changes: **plan
first, validate before writing, preserve existing content, make
destructive behavior explicit, verify outcomes, and keep the system
understandable enough to test and maintain confidently.**
