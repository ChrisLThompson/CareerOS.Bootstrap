# CareerOS.Bootstrap — Bootstrap Process Flow

## Purpose

This document visualizes the end-to-end execution flow for `CareerOS.Bootstrap`.

It distinguishes the **current implemented validation-first dry-run workflow** from the **planned provisioning workflow** so future functionality is not confused with present behavior.

---

## Status Legend

- **CURRENT** — implemented today.
- **PLANNED** — defined target behavior, not yet implemented.
- **FUTURE** — longer-term capability outside the current execution path.

---

# Current Bootstrap Process Flow

```mermaid
flowchart TD
    A["Application Start<br/>CURRENT"] --> B["Program.Main<br/>CURRENT"]
    B --> C["Instantiate Services<br/>CURRENT"]
    C --> D["PathService<br/>CURRENT"]

    D --> E["Find Repository Root<br/>CURRENT"]
    E --> F["Locate Configuration Directory<br/>CURRENT"]

    F --> G["JsonConfigurationService<br/>CURRENT"]
    G --> H["Load bootstrap.json<br/>CURRENT"]
    G --> I["Load templates.json<br/>CURRENT"]

    H --> J["BootstrapConfiguration<br/>CURRENT"]
    I --> K["TemplateConfiguration<br/>CURRENT"]

    J --> V["ConfigurationValidationService<br/>CURRENT"]
    K --> V
    V --> W{"Configuration Valid?<br/>CURRENT"}

    W -->|"No"| X["Display Structured Validation Errors<br/>CURRENT"]
    X --> Y["Return Exit Code 1<br/>CURRENT"]

    W -->|"Yes"| Z["Validate Preview Destination Root<br/>CURRENT"]
    Z --> L["Enumerate Profiles<br/>CURRENT"]

    L --> M["TemplateResolverService<br/>CURRENT"]
    K --> M

    M --> N["Resolve CareerTemplate<br/>CURRENT"]
    N --> O["DirectoryPlanService<br/>CURRENT"]

    O --> P["Traverse DirectoryNode Tree<br/>CURRENT"]
    P --> Q["Build Planned Paths<br/>CURRENT"]

    Q --> R["Validate Planned-Path Containment<br/>CURRENT"]
    R --> S{"Plan Safe?<br/>CURRENT"}
    S -->|"No"| X
    S -->|"Yes"| T["Display Dry-Run Plan<br/>CURRENT"]
    T --> U["Return Exit Code 0<br/>CURRENT"]

    B -. "Unhandled Exception" .-> AA["Catch at Application Boundary<br/>CURRENT"]
    AA --> AB["Display Error<br/>CURRENT"]
    AB --> Y
```

The current process stops after validating and displaying the read-only plan.

No CareerOS directory provisioning occurs.

---

# Current Safety Boundary

```mermaid
flowchart LR
    A["Configuration"] --> B["Centralized Validation"]
    B --> C["Resolution"]
    C --> D["Planning"]
    D --> E["Path Containment Validation"]
    E --> F["Console Preview"]

    F -. "STOP — current implementation" .-> G["Filesystem Provisioning"]
```

The current workflow is validation-first and intentionally read-only. Blocking
configuration or path-safety failures stop before the future provisioning
boundary.

---

# Current Execution Sequence

```text
Start
  |
  v
Discover Repository
  |
  v
Discover Configuration
  |
  v
Load JSON
  |
  v
Deserialize Models
  |
  v
Validate Configuration
  |
  v
Validate Preview Destination Root
  |
  v
Resolve Profile Template
  |
  v
Traverse Recursive Directory Tree
  |
  v
Build Planned Paths
  |
  v
Validate Planned-Path Containment
  |
  v
Display Dry-Run Output
  |
  v
Exit
```

---

# Planned Bootstrap Process Flow

```mermaid
flowchart TD
    A["Application Start"] --> B["Parse Execution Request<br/>PLANNED"]
    B --> C["Resolve Paths and Environment<br/>CURRENT + PLANNED"]

    C --> D["Load Configuration<br/>CURRENT"]
    D --> E["Validate Configuration + Path Safety<br/>CURRENT FOUNDATION<br/>Future request validation PLANNED"]

    E --> F{"Validation Successful?"}

    F -->|"No"| G["Report Validation Errors<br/>CURRENT FOUNDATION"]
    G --> H["Return Failure Exit Code<br/>PLANNED"]

    F -->|"Yes"| I["Resolve Profile and Template<br/>CURRENT"]
    I --> J["Build Provisioning Plan<br/>CURRENT FOUNDATION / PLANNED EVOLUTION"]

    J --> K{"Execution Mode<br/>PLANNED"}

    K -->|"Dry Run"| L["Render Planned Actions<br/>PLANNED"]
    L --> M["Report Summary<br/>PLANNED"]

    K -->|"Provision"| N["Inspect Existing Filesystem State<br/>PLANNED"]
    N --> O["Classify Actions<br/>PLANNED"]
    O --> P["Create Missing Directories<br/>PLANNED"]
    P --> Q["Verify Result<br/>PLANNED"]
    Q --> M

    M --> R["Logging / Reporting<br/>PLANNED"]
    R --> S["Return Process Exit Result<br/>PLANNED"]
```

---

# Planned Decision Flow

The future workflow introduces explicit gates before filesystem modification.

```text
Input
  |
  v
Configuration Loaded?
  |
  +-- No --> Fail
  |
  v
Configuration Valid?
  |
  +-- No --> Fail
  |
  v
Profile / Template Resolved?
  |
  +-- No --> Fail
  |
  v
Plan Valid?
  |
  +-- No --> Fail
  |
  v
Dry Run?
  |
  +-- Yes --> Preview Only
  |
  v
Explicit Provisioning Requested?
  |
  +-- No --> Stop Safely
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

---

# Planned Existing-State Evaluation

```mermaid
flowchart TD
    A["Desired Directory"] --> B["Inspect Target Path<br/>PLANNED"]
    B --> C{"Current State?"}

    C -->|"Missing"| D["Action = CREATE"]
    C -->|"Exists and Valid"| E["Action = PRESERVE"]
    C -->|"Conflict"| F["Action = REPORT / STOP"]
    C -->|"Invalid"| G["Action = REJECT"]

    D --> H["Provisioning Plan"]
    E --> H
    F --> H
    G --> H
```

This classification is central to future idempotent provisioning.

---

# Planned Dry-Run vs Provisioning Branch

```mermaid
flowchart LR
    A["Validated Provisioning Plan"] --> B{"Execution Mode"}

    B -->|"Dry Run"| C["Display Intended Actions"]
    C --> D["No Filesystem Writes"]

    B -->|"Provision"| E["Execute Validated Actions"]
    E --> F["Filesystem"]
    F --> G["Verify Outcomes"]

    D --> H["Execution Summary"]
    G --> H
```

Dry-run and provisioning should consume the same validated plan wherever practical.

---

# Planned Idempotent Repeat Execution

```mermaid
flowchart TD
    A["Run 1"] --> B["Inspect Current State"]
    B --> C["Create Missing Directories"]
    C --> D["Desired State Reached"]

    D --> E["Run 2"]
    E --> F["Inspect Current State"]
    F --> G["All Expected Directories Exist"]
    G --> H["Create Nothing"]
    H --> I["Preserve Existing Content"]
```

The second execution should converge on the same desired structure without duplicate or destructive behavior.

---

# Error Handling Flow

## Current

```text
Exception
  |
  v
Program.Main catch
  |
  +--> Display Error
  |
  +--> Exit Code 1
```

## Planned

```text
Validation Error
Provisioning Error
Filesystem Error
Unexpected Error
        |
        v
Structured Result / Error Category
        |
        v
Reporting / Logging
        |
        v
Defined Exit Code
```

Future error taxonomy remains to be finalized.

---

# Planned Verification Flow

```mermaid
flowchart TD
    A["Provisioning Requested"] --> B["Execute Planned Action"]
    B --> C["Observe Filesystem Result"]
    C --> D{"Expected State Achieved?"}

    D -->|"Yes"| E["Record Success"]
    D -->|"No"| F["Record Failure"]

    E --> G["Execution Summary"]
    F --> G
```

Provisioning outcomes should be verified or reliably observed rather than assumed.

---

# Process Responsibilities

| Process Stage | Current Owner | Status |
| --- | --- | --- |
| Application entry | `Program.Main()` | CURRENT |
| Repository discovery | `PathService` | CURRENT |
| Configuration loading | `JsonConfigurationService` | CURRENT |
| Configuration validation | `ConfigurationValidationService` | CURRENT |
| Destination-root validation | `ConfigurationValidationService` | CURRENT |
| Profile/template resolution | `TemplateResolverService` | CURRENT |
| Recursive directory planning | `DirectoryPlanService` | CURRENT |
| Planned-path containment validation | `ConfigurationValidationService` | CURRENT |
| Console preview | `Program.Main()` | CURRENT |
| CLI request parsing | Future CLI layer | PLANNED |
| Existing-state inspection | Future provisioning layer | PLANNED |
| Filesystem provisioning | Future provisioning service | PLANNED |
| Verification | Future provisioning workflow | PLANNED |
| Structured logging/reporting | Future reporting layer | PLANNED |

---

# Process Invariants

The bootstrap lifecycle should preserve these rules:

1. Configuration must load before dependent behavior executes.
2. Blocking validation errors must prevent provisioning.
3. A profile must resolve to a valid template before planning.
4. Planning must remain independently executable.
5. Dry-run must perform no provisioning writes.
6. Actual provisioning must require explicit execution intent.
7. Existing valid content should be preserved.
8. Repeated provisioning should be idempotent.
9. Filesystem outcomes should be verified.
10. Errors must not be silently reported as success.

---

# Relationship to Existing Documentation

```text
SYSTEM_CONTEXT.md
    |
    v
COMPONENT_DIAGRAM.md
    |
    v
BOOTSTRAP_PROCESS_FLOW.md
    |
    +--> DATA_FLOW_DIAGRAM.md
    |
    +--> FUTURE_STATE_DIAGRAM.md
```

Related detailed documentation:

```text
Documentation/Architecture/CURRENT_STATE.md
Documentation/Architecture/FUTURE_STATE.md
Documentation/Architecture/DATA_FLOW.md
Documentation/Development/TESTING_STRATEGY.md
Documentation/Requirements/TRACEABILITY.md
```

---

# Summary

The current bootstrap process is:

```text
Discover
  |
  v
Load
  |
  v
Resolve
  |
  v
Plan
  |
  v
Preview
```

The planned process extends this to:

```text
Input
  |
  v
Load
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
Preview or Provision
  |
  v
Verify
  |
  v
Report
```

The most important process rule remains:

> **No filesystem modification should occur until the application understands, validates, and explicitly approves the intended change.**
