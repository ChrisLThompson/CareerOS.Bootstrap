# CareerOS.Bootstrap — Development Guide

## Purpose

This guide describes the current developer workflow for `CareerOS.Bootstrap` and establishes a controlled path for future development. It is intended for developers, reviewers, and maintainers who need to understand how to obtain, configure, build, run, change, validate, document, and contribute to the project.

This guide deliberately distinguishes current practices from planned capabilities. Future functionality documented elsewhere must not be treated as implemented until it is coded, tested, reviewed, documented, and merged.

Related documentation:

- `README.md` — repository overview and entry point
- `Documentation/Architecture/ARCHITECTURE.md` — architecture overview
- `Documentation/Architecture/CURRENT_STATE.md` — implemented behavior
- `Documentation/Architecture/FUTURE_STATE.md` — target direction
- `Documentation/Architecture/COMPONENTS.md` — component responsibilities
- `Documentation/Architecture/DATA_FLOW.md` — current and future data flow
- `Documentation/Requirements/USER_STORIES.md` — user-centered requirements
- `Documentation/Requirements/FUNCTIONAL_REQUIREMENTS.md` — functional requirements
- `Documentation/Requirements/NON_FUNCTIONAL_REQUIREMENTS.md` — quality constraints
- `Documentation/Requirements/TRACEABILITY.md` — requirements relationships and verification

---

# Current Development Baseline

## Technology

The current application is a C#/.NET console application targeting .NET 8.

Primary development environment currently used:

```text
Visual Studio Community 2022
.NET 8
Git
GitHub
PowerShell
Windows
```

Cross-platform support must not be assumed merely because .NET is cross-platform. Supported runtime and operating-system expectations should be documented and validated before broader compatibility is claimed.

## Repository Layout

The repository currently follows this high-level structure:

```text
CareerOS.Bootstrap/
├── .github/
├── Configuration/
│   ├── bootstrap.json
│   └── templates.json
├── Documentation/
│   ├── Architecture/
│   ├── Development/
│   ├── Diagrams/
│   ├── References/
│   ├── Requirements/
│   └── Roadmap/
├── CareerOS.Bootstrap/
│   ├── Models/
│   ├── Services/
│   ├── CareerOS.Bootstrap.csproj
│   └── Program.cs
├── .gitignore
├── CHANGELOG.md
├── CareerOS.Bootstrap.sln
├── LICENSE
└── README.md
```

Solution folders displayed in Visual Studio are organizational views and are not the same thing as physical filesystem directories or C# project folders.

---

# Obtaining the Repository

Clone the repository using Git and enter the repository root.

Example:

```powershell
git clone https://github.com/ChrisLThompson/CareerOS.Bootstrap.git
cd CareerOS.Bootstrap
```

A developer should work from the repository root when following commands in this guide unless a command explicitly states otherwise.

The repository root is the directory containing:

```text
CareerOS.Bootstrap.sln
```

---

# Opening the Solution

Open:

```text
CareerOS.Bootstrap.sln
```

in Visual Studio.

Repository-level Markdown and configuration files may be exposed through Visual Studio Solution Folders for convenience. Adding an existing file to a Solution Folder should reference the physical file rather than create an unnecessary duplicate.

The intended high-level Visual Studio organization is conceptually:

```text
Solution 'CareerOS.Bootstrap'
├── Solution Items
├── Documentation
│   ├── Architecture
│   ├── Development
│   ├── Diagrams
│   ├── References
│   ├── Requirements
│   └── Roadmap
└── CareerOS.Bootstrap
```

Empty physical documentation directories may exist even when corresponding Visual Studio Solution Folders have not yet been added.

---

# Configuration

## Repository Configuration Files

Current structured application inputs are:

```text
Configuration/bootstrap.json
Configuration/templates.json
```

`bootstrap.json` defines profiles and associates each profile with a reusable template.

`templates.json` defines reusable recursive directory structures.

Supported profile and template changes should be made through configuration rather than by adding person-specific logic to the C# application.

## Configuration Principles

Developers should preserve these rules:

1. Keep profile definitions external to compiled application logic.
2. Keep reusable directory structures external to compiled application logic.
3. Avoid hard-coded developer usernames, drive letters, and repository locations.
4. Preserve valid JSON syntax.
5. Ensure every configured template reference resolves to a known template.
6. Treat configuration validation as a prerequisite to future filesystem modification.
7. Do not place passwords, API keys, or other secrets in repository configuration.

---

# Building the Application

From the repository root, run:

```powershell
dotnet build
```

A successful development checkpoint should report:

```text
Build succeeded.
```

The current project path is:

```text
CareerOS.Bootstrap\CareerOS.Bootstrap.csproj
```

Build validation should be performed after meaningful source or solution changes and before a branch is considered ready to merge.

Future CI is expected to perform clean restore/build/test validation automatically, but that automation is not currently a substitute for the local workflow until implemented.

---

# Running the Application

Because the `.csproj` resides below the repository root, running this command from the repository root is not sufficient:

```powershell
dotnet run
```

Instead use:

```powershell
dotnet run --project .\CareerOS.Bootstrap\CareerOS.Bootstrap.csproj
```

The current application performs configuration loading, profile/template resolution, recursive directory planning, and console preview output.

The current workflow is read-only with respect to CareerOS provisioning. It does not currently create the planned CareerOS directory tree.

A successful run should make it possible to confirm that configured profiles and templates were loaded and that directory plans can be generated.

---

# Current Application Flow

The implemented development-time execution flow is approximately:

```text
Program.Main
    |
    v
PathService
    |
    v
Repository / Configuration Discovery
    |
    v
JsonConfigurationService
    |
    v
Strongly Typed Configuration Models
    |
    v
TemplateResolverService
    |
    v
DirectoryPlanService
    |
    v
Recursive Directory Plan
    |
    v
Console Preview
```

The current flow ends at preview/reporting. Filesystem provisioning is a future capability and must remain separated from planning.

---

# Git Workflow

## Stable Branch

`main` is the stable branch.

Significant development should normally occur on a purpose-specific branch rather than directly on `main`.

Examples:

```text
docs/documentation-v1
feature/configuration-validation
feature/filesystem-provisioning
test/unit-test-foundation
fix/template-resolution
```

Branch names should communicate the purpose of the work.

## Starting Work

Before starting new work, confirm repository state:

```powershell
git status
git branch
```

When appropriate, update local `main` before creating a new branch.

Create a branch using:

```powershell
git switch -c <branch-name>
```

## During Development

Use Git frequently to understand what has changed:

```powershell
git status
git diff
git diff --stat
```

Before committing, inspect staged content:

```powershell
git add .
git status
git diff --cached --stat
```

For higher-risk changes, inspect the full staged diff as well:

```powershell
git diff --cached
```

Do not assume Visual Studio status icons mean a change has already been pushed to GitHub. Git staging, commits, and pushes remain explicit operations.

## Commit Messages

Use concise commit messages that describe the intent of the change.

A conventional style is encouraged:

```text
feat: add configuration validation
fix: handle unknown template names
docs: add development guide
test: add recursive planning tests
refactor: separate orchestration service
chore: update repository configuration
```

A commit should represent a coherent checkpoint rather than an arbitrary collection of unrelated work.

## Pushing

After committing:

```powershell
git push
```

For the first push of a new branch, Git may require:

```powershell
git push -u origin <branch-name>
```

## Pull Requests and Merge Validation

The target workflow is:

```text
Create Branch
    |
    v
Implement / Document
    |
    v
Build + Test + Review
    |
    v
Commit
    |
    v
Push Branch
    |
    v
Pull Request
    |
    v
Automated / Manual Validation
    |
    v
Merge to main
```

GitHub Actions and branch-protection automation are planned capabilities. Until those exist, local build validation and careful review remain required.

---

# Development Change Workflow

For a normal application change, use the following sequence:

```text
1. Understand requirement
2. Confirm current architecture and behavior
3. Create/switch to purpose-specific branch
4. Implement the smallest coherent change
5. Build
6. Run relevant manual verification
7. Add/update automated tests when available
8. Update affected documentation
9. Update traceability when requirement coverage changes
10. Review Git diff
11. Commit
12. Push
13. Open/review pull request
14. Merge only after validation succeeds
```

Future CI should automate repeatable validation steps but should not replace architectural and requirements review.

---

# Complete-File Update Practice

When a source file requires substantial restructuring, prefer producing and reviewing a complete coherent replacement rather than applying a long sequence of fragile manual edits.

This practice is especially useful when:

- method structure changes significantly;
- orchestration is reorganized;
- multiple dependent sections must remain synchronized;
- partial copy/paste changes would increase error risk.

This does not mean every small change requires rewriting a complete file. Small, isolated changes should remain appropriately scoped.

After replacing a complete source file:

1. Save it.
2. Build immediately.
3. Review compiler warnings/errors.
4. Run the relevant application path.
5. Inspect the Git diff to ensure unrelated behavior was not lost.

---

# Source-Code Organization

Current code is organized primarily into:

```text
Models/
Services/
Program.cs
```

Current responsibilities include:

- `Program.Main()` — application entry/orchestration boundary
- `PathService` — repository/configuration path discovery
- `JsonConfigurationService` — JSON loading/deserialization
- `TemplateResolverService` — profile-to-template resolution
- `DirectoryPlanService` — recursive desired-directory planning
- Models — structured configuration and recursive directory data

Developers should preserve separation of concerns. In particular:

```text
Configuration Loading != Template Resolution
Template Resolution   != Directory Planning
Directory Planning    != Filesystem Provisioning
Provisioning          != Presentation / Logging
```

As orchestration grows, future architecture may move coordination out of `Program.Main()` into an application-level orchestrator.

---

# Entry-Point Convention

The project currently uses an explicit `Main()` entry point rather than relying on top-level statements.

Future changes should preserve the explicit entry point unless an intentional architectural decision changes that convention.

Keeping the application boundary explicit supports readability, conventional program structure, exit-code handling, and future orchestration refactoring.

---

# Nullability and Implicit Usings

Project compiler settings should be controlled in the `.csproj` file rather than assumed from Visual Studio project-creation dialogs.

When modifying compiler/project settings:

1. Edit the project file intentionally.
2. Review the resulting Git diff.
3. Rebuild the solution.
4. Address new warnings rather than disabling safety features simply to obtain a clean build.

Exact coding conventions are maintained separately in `CODING_STANDARDS.md`.

---

# Documentation Workflow

Documentation is a maintained part of the product, not an afterthought.

Current documentation categories are:

```text
Documentation/
├── Architecture/
├── Development/
├── Diagrams/
├── References/
├── Requirements/
└── Roadmap/
```

When a planned capability becomes implemented, review at minimum:

1. `CURRENT_STATE.md`
2. `FUTURE_STATE.md`
3. `COMPONENTS.md`
4. `DATA_FLOW.md`
5. Relevant requirements documents
6. `TRACEABILITY.md`
7. Relevant diagrams
8. Roadmap documentation
9. `README.md`
10. `CHANGELOG.md` when appropriate

Documentation must continue to distinguish current behavior from future intent.

## Markdown Encoding

Repository Markdown files should be stored using a Unicode-capable encoding, preferably UTF-8, so diagrams, arrows, tree characters, and other Unicode symbols are preserved.

If Visual Studio warns that characters cannot be saved in the current code page, save the file using an appropriate Unicode/UTF-8 encoding rather than intentionally losing those characters.

## Editing Long Markdown Files

Word Wrap is recommended in Visual Studio for long-form Markdown so horizontal scrolling is not required for normal prose.

Large documentation changes should be reviewed through Git diff as well as visually in the editor.

---

# Requirements and Traceability Workflow

Requirements use stable identifiers:

```text
US-###   User Story
FR-###   Functional Requirement
NFR-###  Non-Functional Requirement
TEST-### Future concrete test identifier
ADR-###  Future architecture decision identifier
```

Do not casually renumber published requirement identifiers.

When behavior changes, developers should determine:

```text
Why does this exist?          -> US-###
What must it do?              -> FR-###
What quality constraints?     -> NFR-###
Where is it implemented?      -> Component / Source
How is it verified?           -> TEST-### / review / build
```

`TRACEABILITY.md` is the central cross-reference and should evolve as implementation and tests mature.

---

# Testing Workflow

The automated test foundation is planned but not yet complete.

Current verification relies on combinations of:

- successful `dotnet build`;
- manual runtime execution;
- code review;
- configuration review;
- negative-path/manual validation where applicable;
- documentation review.

Target verification will include:

- unit tests for configuration, resolution, validation, and planning;
- isolated filesystem integration tests;
- repeat-execution/idempotency tests;
- dry-run no-write tests;
- CLI tests;
- CI build/test checks;
- release validation.

Filesystem tests must use temporary isolated roots and must never use a real user's CareerOS environment as an automated fixture.

The detailed approach belongs in `TESTING_STRATEGY.md`.

---

# Safety Rules for Future Provisioning Work

Filesystem provisioning is a high-risk transition because the application will move from describing desired state to changing real state.

Before provisioning is considered mature, development should demonstrate:

```text
Configuration validation
Valid profile/template resolution
Configurable destination root
Dry-run preview
Existing-directory detection
Safe missing-directory creation
Existing-content preservation
Idempotent repeat execution
Clear execution summary
Actionable error handling
Automated unit tests
Isolated filesystem integration tests
Requirements traceability
Updated documentation
Successful merge validation
```

Developers must not add implicit deletion or destructive replacement to normal provisioning behavior.

Potentially destructive behavior requires separate explicit requirements and safeguards.

---

# Error Handling

The current application boundary reports top-level failure and returns a nonzero exit code.

Developers should preserve the principle:

```text
Failure != Success
```

Errors should be actionable and identify affected configuration, template, path, or operation where practical.

Do not hide a required-operation failure merely to allow execution to continue.

As structured validation and result models are introduced, error reporting should become more precise without coupling business logic to console formatting.

---

# Dependency Management

Prefer .NET platform capabilities where they satisfy project requirements.

Before introducing a third-party package, consider:

- Is it necessary?
- Does the .NET platform already provide the capability?
- What maintenance burden does it add?
- What security/supply-chain risk does it introduce?
- What license applies?
- Does it complicate deployment or testing?

Dependencies should be intentional rather than incidental.

---

# Secrets and Sensitive Data

Do not commit:

```text
Passwords
API keys
Access tokens
Private keys
Connection-string secrets
Personal secrets
```

Current core bootstrap behavior does not require secrets.

If future database, cloud, website, or external-service integrations require credentials, a secure configuration strategy must be defined before implementation.

Future logs must also avoid unnecessarily capturing personal or sensitive information.

---

# Database / Website Extension Boundary

A future SQL Server-backed project catalog may store structured representations of requirements, documentation metadata, components, scripts, tests, relationships, and release information for querying and eventual website presentation.

That extension is not part of the current application implementation.

If pursued, it should be designed as an explicit future subsystem with requirements covering:

- schema design and versioning;
- SQL Server connectivity;
- secure connection configuration;
- migration/deployment strategy;
- source-of-truth rules between Markdown/Git and database records;
- search/query behavior;
- traceability relationships;
- API or website boundaries;
- authentication/authorization if exposed remotely;
- backup/recovery;
- privacy and sensitive-data handling.

Until such an architecture is approved, Markdown and Git remain the authoritative documentation/requirements artifacts.

---

# Definition of Done for a Development Change

A normal change should not be considered complete solely because it compiles.

Depending on scope, Definition of Done should include:

- Requirement/intent understood.
- Implementation is focused and consistent with architecture.
- `dotnet build` succeeds.
- Relevant manual verification succeeds.
- Applicable automated tests pass once available.
- New tests are added when behavior warrants them.
- Existing content/safety expectations are preserved.
- Documentation is updated where behavior changed.
- Requirements/traceability are updated where coverage changed.
- Git diff contains no unintended changes.
- Commit message accurately describes the change.
- Branch is pushed and reviewed before merge.
- Future CI checks pass once implemented.

Higher-risk filesystem, security, release, or data changes require correspondingly stronger verification.

---

# Developer Checklists

## Before Starting

```text
[ ] Confirm current branch
[ ] Confirm git status
[ ] Understand relevant US/FR/NFR requirements
[ ] Review current-state/architecture documentation if needed
[ ] Create or switch to an appropriate branch
```

## Before Commit

```text
[ ] Save all intended files
[ ] Build successfully
[ ] Run relevant verification
[ ] Review git status
[ ] Review git diff / staged diff
[ ] Update documentation
[ ] Update traceability if applicable
[ ] Confirm no secrets or unintended generated files are staged
```

## Before Merge

```text
[ ] Branch is pushed
[ ] Build succeeds
[ ] Applicable tests succeed
[ ] Documentation matches behavior
[ ] Requirement status is accurate
[ ] Current vs future state remains accurate
[ ] Review is complete
[ ] CI succeeds once available
```

---

# Planned Development-Process Evolution

The development process is expected to evolve approximately as follows:

```text
Current
  |
  +--> Purpose-specific Git branches
  +--> Manual build validation
  +--> Manual runtime verification
  +--> Architecture documentation
  +--> Requirements catalog
  +--> Traceability foundation
  |
  v
Next
  |
  +--> Unit-test project
  +--> Testing standards
  +--> Configuration validation
  +--> Filesystem integration-test fixtures
  |
  v
Later
  |
  +--> GitHub Actions
  +--> Pull-request checks
  +--> Branch protection
  +--> Versioned releases
  +--> Automated packaging
```

This evolution should remain requirements-driven rather than adding tooling solely for complexity's sake.

---

# Quick Command Reference

From the repository root:

```powershell
# Repository state
git status
git branch

# Build
dotnet build

# Run
dotnet run --project .\CareerOS.Bootstrap\CareerOS.Bootstrap.csproj

# Inspect changes
git diff
git diff --stat

# Create a branch
git switch -c <branch-name>

# Stage and inspect
git add .
git status
git diff --cached --stat

# Commit
git commit -m "<type>: <description>"

# Push a new branch
git push -u origin <branch-name>
```

---

# Summary

The CareerOS.Bootstrap development workflow is designed around a few core principles:

```text
Understand intent
      |
      v
Work on an isolated branch
      |
      v
Keep configuration external
      |
      v
Preserve architectural boundaries
      |
      v
Build and verify frequently
      |
      v
Treat filesystem changes conservatively
      |
      v
Keep documentation and requirements synchronized
      |
      v
Review before merge
```

The project should remain understandable enough that another developer or reviewer can determine what exists today, what is planned, why a capability exists, where it belongs architecturally, and how it is expected to be validated.
