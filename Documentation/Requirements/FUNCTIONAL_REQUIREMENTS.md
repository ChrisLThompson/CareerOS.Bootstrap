# CareerOS.Bootstrap — Functional Requirements

## Purpose

This document defines the testable functional requirements for `CareerOS.Bootstrap`.

The requirements are derived from the user-story catalog in `USER_STORIES.md` and are intended to describe **what the application must do**, without unnecessarily prescribing implementation details.

Quality attributes and system constraints are documented separately in:

```text
NON_FUNCTIONAL_REQUIREMENTS.md
```

Relationships among user stories, requirements, architecture, implementation, and tests are maintained in:

```text
TRACEABILITY.md
```

---

## Requirement Format

Functional requirements use stable identifiers:

```text
FR-### — Requirement Title
```

Each requirement includes:

- **Status** — Implemented, Partially Implemented, Planned, or Future
- **Priority** — High, Medium, or Low
- **Source Stories** — User stories that establish the need
- **Requirement** — Testable system behavior
- **Acceptance Criteria** — Conditions required for satisfaction
- **Notes / Current State** — Current implementation context where appropriate

A requirement may support more than one user story, and a user story may require more than one functional requirement.

---

# Configuration and Profile Requirements

## FR-001 — Load Profile Configuration

**Status:** Implemented
**Priority:** High
**Source Stories:** US-001, US-004

### Requirement

The application shall load CareerOS profile definitions from an external JSON configuration file rather than requiring profile definitions to be compiled into application source code.

### Acceptance Criteria

- The application can locate the configured profile JSON file.
- The file is read successfully when present and valid.
- JSON content is deserialized into the profile configuration model.
- More than one profile can be represented in the configuration.
- Missing profile configuration produces a failure rather than silently using hard-coded profile data.

### Current State

Implemented through `JsonConfigurationService`, `BootstrapConfiguration`, `ProfileConfiguration`, and `bootstrap.json`.

---

## FR-002 — Load Template Configuration

**Status:** Implemented
**Priority:** High
**Source Stories:** US-002, US-003, US-004

### Requirement

The application shall load reusable CareerOS directory templates from an external JSON configuration file.

### Acceptance Criteria

- The application can locate the template JSON file.
- The file is read successfully when present and valid.
- Template JSON is deserialized into strongly typed template models.
- Multiple templates can be represented in one configuration.
- Template structure can be changed through supported JSON edits without changing core application code.

### Current State

Implemented through `JsonConfigurationService`, `TemplateConfiguration`, `CareerTemplate`, `DirectoryNode`, and `templates.json`.

---

## FR-003 — Support Multiple Profiles

**Status:** Implemented
**Priority:** High
**Source Stories:** US-001

### Requirement

The application shall support processing multiple CareerOS profiles from one profile configuration.

### Acceptance Criteria

- At least two profiles can coexist in configuration.
- Each profile is independently identifiable.
- Each configured profile can be processed without requiring a separate application build.
- Adding another valid profile does not require modification of core profile-processing logic.

---

## FR-004 — Associate a Profile With a Template

**Status:** Implemented
**Priority:** High
**Source Stories:** US-002

### Requirement

Each CareerOS profile shall be able to reference a reusable template by name.

### Acceptance Criteria

- A profile contains a template reference.
- The application resolves the reference against configured templates.
- Multiple profiles may reference the same template.
- Template data is not required to be duplicated inside each profile definition.

---

## FR-005 — Resolve Templates Case-Insensitively

**Status:** Implemented
**Priority:** Medium
**Source Stories:** US-002, US-017

### Requirement

The application shall resolve configured template names without requiring exact capitalization matches.

### Acceptance Criteria

- Template names differing only by letter case resolve to the same configured template.
- An unknown template name does not resolve to an arbitrary template.
- A failed resolution produces an actionable failure.

### Current State

Implemented with ordinal case-insensitive comparison in `TemplateResolverService`.

---

## FR-006 — Support Recursive Directory Definitions

**Status:** Implemented
**Priority:** High
**Source Stories:** US-003

### Requirement

The template configuration shall support directory nodes containing child directory nodes recursively.

### Acceptance Criteria

- A directory node may contain zero or more children.
- A child node may itself contain children.
- The application can traverse more than one nesting level.
- Directory depth is not hard-coded into the planning algorithm.

### Current State

Implemented through `DirectoryNode.Children` and recursive traversal in `DirectoryPlanService`.

---

# Path Discovery and Destination Requirements

## FR-007 — Discover Repository Root Dynamically

**Status:** Implemented
**Priority:** High
**Source Stories:** US-005

### Requirement

During repository-based development execution, the application shall locate its repository root without depending on a hard-coded absolute repository path.

### Acceptance Criteria

- Repository discovery does not require a fixed drive letter.
- Repository discovery does not require a fixed Windows username.
- The current repository can be found from the compiled application's runtime path.
- Failure to locate the repository root is reported clearly.

### Current State

Implemented in `PathService` by walking parent directories until `CareerOS.Bootstrap.sln` is located.

---

## FR-008 — Discover Configuration Directory From Repository Root

**Status:** Implemented
**Priority:** High
**Source Stories:** US-005

### Requirement

The application shall derive the repository-level configuration directory from the discovered repository root.

### Acceptance Criteria

- The expected `Configuration` directory is resolved relative to the repository root.
- Missing configuration directory causes an explicit failure.
- No development-machine-specific absolute configuration path is required.

---

## FR-009 — Configure CareerOS Destination Root

**Status:** Planned
**Priority:** High
**Source Stories:** US-006

### Requirement

The application shall allow the target CareerOS root directory to be supplied through a supported configuration or execution mechanism.

### Acceptance Criteria

- A valid destination root can be specified without recompiling the application.
- The effective destination root is displayed before provisioning.
- The destination is validated before filesystem modification.
- An explicitly supplied valid destination is not silently replaced by a machine-specific default.
- Destination-source precedence is documented once multiple sources are supported.

---

## FR-010 — Override Destination Root From Command Line

**Status:** Future
**Priority:** Medium
**Source Stories:** US-006

### Requirement

The application should support a command-line destination-root override when a CLI is implemented.

### Acceptance Criteria

- A supported command-line option can supply the destination root.
- Invalid command-line destinations are rejected before provisioning.
- The effective destination root is shown to the user.
- Configuration files are not permanently modified merely because an execution override is used.

---

# Planning and Preview Requirements

## FR-011 — Generate a Directory Plan for Each Processed Profile

**Status:** Implemented
**Priority:** High
**Source Stories:** US-007

### Requirement

The application shall generate a complete planned directory path collection for each processed profile based on that profile's resolved template.

### Acceptance Criteria

- The profile root is included in the plan.
- Every configured top-level template directory is included.
- Every configured nested directory is included.
- Directory paths reflect the profile's configured directory name.
- Planning can complete without performing directory creation.

---

## FR-012 — Traverse Nested Directory Nodes Recursively

**Status:** Implemented
**Priority:** High
**Source Stories:** US-003, US-007

### Requirement

The directory planning process shall recursively traverse all child directory nodes in the resolved template.

### Acceptance Criteria

- Each node is added to the plan once per occurrence in the configured hierarchy.
- Child paths are constructed beneath the correct parent path.
- Grandchild and deeper nodes are processed without separate depth-specific logic.
- Invalid empty directory-node names are rejected.

---

## FR-013 — Display Planned Directory Paths

**Status:** Implemented
**Priority:** High
**Source Stories:** US-007, US-016, US-020

### Requirement

The application shall display planned directory paths to the user during the current preview workflow.

### Acceptance Criteria

- Planned directories are visible in console output.
- Output identifies the profile being processed.
- Output identifies the template being used.
- A planned-directory count is displayed for each current profile run.

---

## FR-014 — Perform No Filesystem Writes During Current Preview

**Status:** Implemented
**Priority:** High
**Source Stories:** US-007, US-008, US-013

### Requirement

The current preview workflow shall not create, modify, rename, or delete CareerOS directories.

### Acceptance Criteria

- Directory planning can be executed without calling directory-creation behavior.
- The logical `_Preview` location is not physically created by the planning workflow.
- Existing user directories are not modified by preview execution.
- Output clearly states that the current run is non-destructive.

---

## FR-015 — Support Explicit Dry-Run Execution Mode

**Status:** Planned / Partially Implemented
**Priority:** High
**Source Stories:** US-008

### Requirement

Once provisioning exists, the application shall provide an explicit dry-run mode that uses the same validated intent as real execution while performing no provisioning writes.

### Acceptance Criteria

- A user can explicitly request dry-run behavior.
- Dry-run configuration and validation match provision-mode inputs.
- Dry-run produces the same intended action plan that provision mode would consume.
- No provisioning write occurs in dry-run mode.
- The final summary identifies the run as a dry run.

---

# Validation Requirements

## FR-016 — Validate Required Configuration Files

**Status:** Implemented
**Priority:** High
**Source Stories:** US-009, US-017

### Requirement

The application shall verify that required configuration files exist before attempting to deserialize them.

### Acceptance Criteria

- Missing bootstrap configuration is detected.
- Missing template configuration is detected.
- A missing file produces an actionable file-related failure.
- Processing does not continue using invented or hard-coded configuration.

---

## FR-017 — Validate Required Planning Inputs

**Status:** Partially Implemented
**Priority:** High
**Source Stories:** US-009, US-017

### Requirement

The application shall reject invalid required values before producing a directory plan.

### Acceptance Criteria

Current or planned validation shall reject, at minimum:

- Empty base planning path
- Null profile input
- Null template input
- Empty profile destination-directory name
- Empty directory-node name
- Empty template name where resolution is required

### Current State

Several of these checks are already implemented within existing services. A centralized validation service is not yet implemented.

---

## FR-018 — Reject Unknown Template References

**Status:** Implemented
**Priority:** High
**Source Stories:** US-009, US-017

### Requirement

The application shall reject a profile that references a template that cannot be resolved.

### Acceptance Criteria

- No arbitrary fallback template is selected.
- The unresolved template name is represented in the error message.
- Directory planning for the invalid profile does not proceed using an unrelated template.

---

## FR-019 — Perform Comprehensive Configuration Validation Before Provisioning

**Status:** Implemented
**Priority:** High
**Source Stories:** US-009

### Requirement

Before filesystem provisioning is permitted, the application shall validate configuration for structural and filesystem-related conflicts.

### Acceptance Criteria

Validation shall be capable of identifying, as applicable:

- Missing required values
- Duplicate profile names
- Duplicate profile destination directories
- Duplicate template names
- Missing template references
- Invalid filesystem characters
- Reserved filesystem names
- Empty required collections
- Conflicting destination paths
- Duplicate sibling directory names
- Unsupported configuration-schema versions once schema versioning exists

Provisioning shall not begin when blocking validation errors exist.

### Current State

`ConfigurationValidationService` now provides the centralized M3 validation
boundary for configuration, destination-root, and planned-path safety.

Implemented validation includes:

- Required profile and template values
- Empty required profile/template collections
- Duplicate profile names and destination directories
- Duplicate template names
- Missing template references
- Invalid and reserved Windows filesystem names
- Empty directory-node names
- Duplicate sibling directory names
- Destination-root validity
- Planned-path containment beneath the approved destination root

Configuration validation is invoked before template resolution/planning in the
application workflow. Destination-root and planned-path validation are also
performed before the current preview is presented. Validation aggregates
blocking errors rather than failing on the first validation issue.

A duplicate configured profile destination is treated as the current
configuration-level conflicting-destination case. Existing-filesystem-object
conflicts remain part of the later existing-state/provisioning-plan work.

Schema-version validation remains intentionally deferred because explicit
configuration schema versioning has not yet been introduced.

---

## FR-020 — Return Structured Validation Results

**Status:** Partially Implemented
**Priority:** Medium
**Source Stories:** US-009, US-016, US-017, US-020

### Requirement

The validation subsystem should return structured results that can be consumed by console reporting, tests, and automation.

### Acceptance Criteria

Each result should be capable of representing:

- Severity
- Stable or meaningful validation code
- Human-readable message
- Configuration location or affected field where practical
- Suggested resolution where practical

### Current State

M3 introduces `ValidationResult`, `ValidationError`, and `ValidationWarning`.

Current structured results provide:

- Blocking-error versus non-blocking-warning semantics
- Stable validation codes
- Human-readable messages
- Configuration/property locations where practical
- Aggregated error and warning collections

`ValidationResult.IsValid` is false only when blocking errors are present.
Warnings do not invalidate a result. Current console integration renders
blocking validation codes, locations, and messages.

A dedicated suggested-resolution field has not been introduced. Resolution
guidance may remain in human-readable messages unless a later reporting or
automation requirement justifies a separate field.

---

# Provisioning and Filesystem Requirements

## FR-021 — Inspect Existing Directory State Before Creation

**Status:** Planned
**Priority:** High
**Source Stories:** US-010, US-011, US-012, US-020

### Requirement

Before creating a planned directory, provision mode shall determine whether that directory already exists.

### Acceptance Criteria

- Existing directories are distinguishable from missing directories.
- Inspection occurs before attempting creation.
- Inspection results can be represented in the provisioning result or summary.

---

## FR-022 — Preserve Existing Valid Directories

**Status:** Planned
**Priority:** High
**Source Stories:** US-010, US-012, US-013

### Requirement

Provision mode shall preserve expected directories that already exist.

### Acceptance Criteria

- An existing expected directory is not deleted.
- An existing expected directory is not destructively recreated.
- Files contained within the existing directory remain untouched by normal directory provisioning.
- The result reports the directory as existing/preserved rather than newly created.

---

## FR-023 — Create Missing Planned Directories

**Status:** Planned
**Priority:** High
**Source Stories:** US-011

### Requirement

Provision mode shall create expected directories that are missing from the target CareerOS environment.

### Acceptance Criteria

- Only paths represented in the validated effective plan are candidates for creation.
- Missing parent/child directories can be created in a valid order.
- Successful creation is reported distinctly from existing-directory detection.
- A failed creation is reported rather than silently treated as success.

---

## FR-024 — Support Idempotent Repeat Provisioning

**Status:** Planned
**Priority:** High
**Source Stories:** US-010, US-011, US-012

### Requirement

Provisioning shall be safe to execute repeatedly against the same valid configuration and target environment.

### Acceptance Criteria

- A first execution can create missing directories.
- A second identical execution against the now-correct environment creates no duplicate directories.
- Existing user content remains intact across repeated executions.
- Repeat execution completes successfully when the desired structure already exists.
- Idempotency is validated through automated filesystem integration testing.

---

## FR-025 — Do Not Automatically Delete Removed Configuration Directories

**Status:** Planned Safety Requirement
**Priority:** High
**Source Stories:** US-013

### Requirement

Normal provisioning shall not automatically delete a physical directory merely because that directory is no longer present in configuration.

### Acceptance Criteria

- Removing a directory node from configuration does not cause automatic filesystem deletion.
- Normal provisioning contains no implicit recursive-delete behavior.
- Any future destructive cleanup capability requires separate requirements, explicit user intent, safeguards, and documentation.

---

## FR-026 — Use a Shared Validated Plan for Preview and Provisioning

**Status:** Future
**Priority:** High
**Source Stories:** US-008, US-011, US-020

### Requirement

Dry-run and real provisioning should consume the same validated provisioning-plan representation wherever practical.

### Acceptance Criteria

- The plan used for preview represents the same intended actions supplied to provision mode.
- Provision mode does not independently reinterpret profile/template structure in a way that can diverge from dry-run output.
- Plan actions can distinguish intended operation types such as create, exists, skip, invalid, or conflict.

---

# Profile and Execution Selection Requirements

## FR-027 — Process a Specific Profile on Request

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-014

### Requirement

The application shall provide a supported way to limit execution to a selected configured profile.

### Acceptance Criteria

- A valid profile identifier can be supplied.
- Only the selected profile is processed.
- An unknown profile is rejected clearly.
- Other configured profiles remain unchanged and unprocessed during that run.

---

## FR-028 — Support Controlled Template Override

**Status:** Future
**Priority:** Low
**Source Stories:** US-015

### Requirement

The application may support an explicit temporary template override for a selected execution.

### Acceptance Criteria

- The override requires explicit user input.
- The override template must exist and pass validation.
- The effective template is displayed before execution.
- The persisted profile configuration is not modified by the temporary override.

---

## FR-029 — Provide Help Information

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-018

### Requirement

Once command-line options exist, the application shall provide built-in usage guidance.

### Acceptance Criteria

- A documented help option displays supported commands/options.
- Help can be requested without provisioning directories.
- Help output identifies required and optional arguments where appropriate.

---

## FR-030 — Provide Version Information

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-018, US-028

### Requirement

The application shall provide a supported way to identify the version being executed once formal release versioning is implemented.

### Acceptance Criteria

- A version option or equivalent mechanism reports the application version.
- Requesting version information does not perform provisioning.
- Packaged releases report a version corresponding to the release artifact.

---

# Reporting and Error Requirements

## FR-031 — Display Execution Context

**Status:** Partially Implemented
**Priority:** High
**Source Stories:** US-016, US-020

### Requirement

The application shall display enough execution context for a user to understand what is being processed.

### Acceptance Criteria

As applicable to the current or future mode, output should identify:

- Execution mode
- Profile being processed
- Effective template
- Effective destination root
- Configuration source where useful

### Current State

Current output identifies dry-run behavior, profile, template, repository/configuration paths, and planned paths. A true effective CareerOS destination is not yet implemented.

---

## FR-032 — Produce an Execution Summary

**Status:** Planned / Partially Implemented
**Priority:** High
**Source Stories:** US-016, US-020

### Requirement

The application shall provide a concise final summary of planned or completed work.

### Acceptance Criteria

The summary shall distinguish applicable counts such as:

- Profiles processed
- Directories planned
- Directories created
- Directories already existing
- Directories skipped
- Warnings
- Errors

Dry-run summaries shall explicitly state that no provisioning changes were made.

---

## FR-033 — Distinguish Action Statuses

**Status:** Planned
**Priority:** High
**Source Stories:** US-020

### Requirement

The application shall distinguish the status of relevant planned and executed directory actions.

### Acceptance Criteria

- A planned action can be distinguished from an executed creation.
- Existing directories can be distinguished from created directories.
- Skipped, warning, and failed outcomes can be represented when applicable.
- Status terminology is consistent across console output and structured results.

---

## FR-034 — Return Success and Failure Process Exit Codes

**Status:** Implemented / Future Expansion Planned
**Priority:** High
**Source Stories:** US-017, US-029

### Requirement

The application shall return a process exit result indicating whether top-level execution succeeded.

### Acceptance Criteria

Current behavior shall satisfy:

```text
0 = success
1 = failure
```

Future expansion may add category-specific nonzero exit codes, but `0` shall remain reserved for successful execution.

---

## FR-035 — Display Actionable Failure Information

**Status:** Partially Implemented
**Priority:** High
**Source Stories:** US-017

### Requirement

When execution fails, the application shall provide information sufficient to identify the failure area and support correction where practical.

### Acceptance Criteria

- Top-level failure output states that execution failed.
- A meaningful error message is displayed.
- Invalid template references identify the relevant template name.
- Missing configuration resources identify the missing resource/path.
- Filesystem and validation failures shall become distinguishable as those subsystems are implemented.

---

## FR-036 — Record Execution Logs

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-019

### Requirement

The application shall support persistent execution logging when the logging subsystem is introduced.

### Acceptance Criteria

Logs should be capable of recording, as applicable:

- Timestamp
- Application version
- Execution mode
- Profile(s)
- Destination root
- Validation outcome
- Planned/completed/skipped actions
- Warnings
- Errors
- Final execution result

Log content shall comply with non-functional security and privacy requirements.

---

# Documentation, Traceability, and Lifecycle Requirements

## FR-037 — Maintain Current-State Documentation

**Status:** Implemented / In Progress
**Priority:** High
**Source Stories:** US-024

### Requirement

The repository shall maintain documentation that distinguishes currently implemented functionality from planned functionality.

### Acceptance Criteria

- `CURRENT_STATE.md` describes implemented behavior.
- Current limitations are documented.
- Planned features are not represented as already available.
- Current-state documentation is updated when significant planned functionality becomes implemented.

---

## FR-038 — Maintain Future-State Documentation

**Status:** Implemented / In Progress
**Priority:** High
**Source Stories:** US-025

### Requirement

The repository shall maintain future-state architecture documentation separately from current-state implementation documentation.

### Acceptance Criteria

- `FUTURE_STATE.md` documents target direction.
- Conceptual components are labeled as planned/future.
- Future diagrams are distinguishable from current-state diagrams.
- The document is revised when architectural direction changes materially.

---

## FR-039 — Maintain Requirements Traceability

**Status:** Planned / Documentation In Progress
**Priority:** High
**Source Stories:** US-026

### Requirement

The project shall maintain traceability among user stories, functional/non-functional requirements, architecture/components, implementation, and automated tests where applicable.

### Acceptance Criteria

- User stories use stable `US-###` identifiers.
- Functional requirements use stable `FR-###` identifiers.
- Non-functional requirements use stable `NFR-###` identifiers.
- Architecture decisions use `ADR-###` identifiers when implemented.
- Test identifiers or clearly traceable test names can be mapped to supported requirements.
- `TRACEABILITY.md` records these relationships.

---

## FR-040 — Record Significant Architecture Decisions

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-027

### Requirement

The project shall record significant architectural decisions using Architecture Decision Records when the ADR process is established.

### Acceptance Criteria

Each ADR shall include, at minimum:

- Stable ADR identifier
- Context
- Decision
- Status
- Consequences
- Alternatives considered where relevant

Superseded ADRs should remain available as historical records.

---

# Testing, CI, and Release Requirements

## FR-041 — Provide Automated Unit Tests for Core Services

**Status:** Planned
**Priority:** High
**Source Stories:** US-021

### Requirement

The project shall provide automated unit tests for core behavior that can be isolated from real filesystem modification.

### Acceptance Criteria

Initial automated tests shall cover, at minimum:

- Valid template resolution
- Unknown-template failure
- Case-insensitive template resolution
- Recursive directory planning
- Nested directory-node planning
- Configuration loading behavior
- Relevant invalid-input behavior

---

## FR-042 — Provide Filesystem Integration Tests

**Status:** Planned
**Priority:** High
**Source Stories:** US-012, US-022

### Requirement

Filesystem provisioning behavior shall be validated using automated integration tests against isolated temporary directories.

### Acceptance Criteria

- Tests do not target real CareerOS user directories.
- Tests verify missing-directory creation.
- Tests verify existing-directory preservation.
- Tests verify repeat-run idempotency.
- Cleanup removes only resources created for the test.

---

## FR-043 — Validate Proposed Changes Before Merge

**Status:** Partially Implemented
**Priority:** High
**Source Stories:** US-023, US-029

### Requirement

Significant project changes shall be developed and validated outside the stable `main` branch before merge.

### Acceptance Criteria

- Purpose-specific branches can be used for work in progress.
- Builds are performed before merge.
- Relevant tests are required once automated tests exist.
- Pull-request review is used as the repository workflow matures.
- `main` remains the known-good baseline.

---

## FR-044 — Automatically Build and Test Pull Requests

**Status:** Planned
**Priority:** Medium
**Source Stories:** US-029

### Requirement

The repository shall eventually use GitHub automation to build and test proposed changes before merge.

### Acceptance Criteria

A future CI workflow shall be capable of:

```text
Checkout
Restore
Build
Test
Report Result
```

CI failure shall be visible to maintainers reviewing the proposed change.

---

## FR-045 — Produce Versioned Release Artifacts

**Status:** Future
**Priority:** Medium
**Source Stories:** US-028

### Requirement

The project should produce documented, versioned application artifacts that can be executed without opening the Visual Studio source project.

### Acceptance Criteria

- A packaged artifact is associated with a version.
- Runtime requirements are documented.
- Release artifacts correspond to validated source revisions.
- Distribution does not require manual source-project execution.

---

# Optional Git Integration Requirements

## FR-046 — Detect Existing Git Repository Before Optional Initialization

**Status:** Future
**Priority:** Low
**Source Stories:** US-030

### Requirement

If optional Git initialization is introduced for generated development areas, the application shall detect whether the target is already within an existing Git repository before initialization.

### Acceptance Criteria

- Git initialization remains optional.
- Existing repositories are recognized and preserved.
- The application does not unexpectedly initialize a repository inside an existing repository.
- Git-related failure does not cause unrelated CareerOS data loss.

---

## FR-047 — Initialize Git Only With Explicit User Intent

**Status:** Future
**Priority:** Low
**Source Stories:** US-030

### Requirement

The application shall initialize Git in an eligible target area only when the user explicitly requests or enables that behavior.

### Acceptance Criteria

- Default provisioning does not unexpectedly initialize Git.
- The selected target is shown or otherwise unambiguous.
- Existing repository state is preserved.
- The result reports whether Git initialization occurred.

---

# Functional Requirement Priority Summary

## High Priority

```text
FR-001  Load profile configuration
FR-002  Load template configuration
FR-003  Support multiple profiles
FR-004  Associate profile with template
FR-006  Recursive directory definitions
FR-007  Dynamic repository discovery
FR-008  Configuration-directory discovery
FR-009  Configurable CareerOS destination
FR-011  Generate directory plans
FR-012  Recursive directory traversal
FR-013  Display planned paths
FR-014  No filesystem writes during preview
FR-015  Explicit dry-run mode
FR-016  Validate required configuration files
FR-017  Validate planning inputs
FR-018  Reject unknown templates
FR-019  Comprehensive pre-provision validation
FR-021  Inspect existing state
FR-022  Preserve existing directories
FR-023  Create missing directories
FR-024  Idempotent repeat provisioning
FR-025  No automatic destructive deletion
FR-026  Shared plan for preview/provisioning
FR-031  Display execution context
FR-032  Execution summary
FR-033  Action statuses
FR-034  Process exit codes
FR-035  Actionable failures
FR-037  Current-state documentation
FR-038  Future-state documentation
FR-039  Requirements traceability
FR-041  Core automated unit tests
FR-042  Filesystem integration tests
FR-043  Validate before merge
```

## Medium Priority

```text
FR-005  Case-insensitive template resolution
FR-010  CLI destination override
FR-020  Structured validation results
FR-027  Specific profile selection
FR-029  Help information
FR-030  Version information
FR-036  Execution logging
FR-040  Architecture Decision Records
FR-044  GitHub CI
FR-045  Versioned release artifacts
```

## Low Priority / Future Optional

```text
FR-028  Controlled template override
FR-046  Detect existing Git before optional initialization
FR-047  Explicit optional Git initialization
```

Priorities may be revised as project goals evolve.

---

# Current Implementation Coverage

The following requirements are substantially implemented in the current codebase:

```text
FR-001
FR-002
FR-003
FR-004
FR-005
FR-006
FR-007
FR-008
FR-011
FR-012
FR-013
FR-014
FR-016
FR-018
FR-034
```

The following are partially implemented or documentation work is currently establishing them:

```text
FR-015
FR-017
FR-031
FR-032
FR-035
FR-037
FR-038
FR-039
FR-043
```

Remaining requirements are planned or future-state capabilities.

---

# Requirement Dependencies

Several planned requirements depend on earlier foundations.

```text
FR-009 Destination Root
      |
      v
FR-019 Comprehensive Validation
      |
      v
FR-021 Existing-State Inspection
      |
      +-------------------+
      v                   v
FR-022 Preserve       FR-023 Create
      |                   |
      +---------+---------+
                v
          FR-024 Idempotency
                |
                v
          FR-032 Summary
```

Similarly:

```text
FR-041 Unit Tests
      |
      +--> validates current planning behavior

FR-042 Integration Tests
      |
      +--> required before provisioning maturity

FR-044 GitHub CI
      |
      +--> executes build/tests automatically
```

Dependencies do not necessarily dictate exact implementation order, but they provide a logical development sequence.

---

# Acceptance and Verification Strategy

Functional requirements should be verified using the most appropriate mechanism.

Possible verification methods include:

```text
Code Review
Manual Runtime Test
Unit Test
Integration Test
Git / CI Validation
Documentation Review
```

`TRACEABILITY.md` will identify the planned or actual verification mechanism for each requirement as the test foundation is established.

A requirement should not be marked fully implemented solely because a corresponding class or method exists. Its acceptance criteria must be satisfied by observable application behavior.

---

# Relationship to Non-Functional Requirements

Functional requirements define application capabilities.

`NON_FUNCTIONAL_REQUIREMENTS.md` will constrain how those capabilities are delivered, including areas such as:

- Safety
- Reliability
- Idempotency
- Maintainability
- Portability
- Testability
- Security
- Performance appropriate to a bootstrap utility
- Usability
- Documentation quality
- Compatibility

For example:

```text
FR-023
Create Missing Directories

        +

NFR-Safety / Reliability constraints

        =

Safe, validated directory provisioning
```

---

## Summary

These functional requirements translate the project's user stories into explicit, testable system behavior.

The current application already satisfies much of the configuration, resolution, recursive planning, and read-only preview foundation.

The next major functional transition will move from:

```text
Configuration
   |
   v
Resolution
   |
   v
Planning
   |
   v
Preview
```

toward:

```text
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
Preview or Provision
   |
   v
Verification
   |
   v
Reporting
```

Requirements should be marked implemented only when their acceptance criteria are supported by the actual codebase and appropriate verification evidence.
