# CareerOS.Bootstrap â€” Coding Standards

## Purpose

This document defines the coding conventions and engineering practices for `CareerOS.Bootstrap`.

The goal is to keep the codebase readable, maintainable, testable, safe, and consistent as the project grows from a read-only planning utility into a broader provisioning platform.

These standards apply to production code, tests, scripts, configuration-handling code, and supporting development artifacts unless a documented exception is justified.

---

## Guiding Principles

CareerOS.Bootstrap code should prioritize:

- Safety before convenience
- Clarity before cleverness
- Explicit behavior before hidden behavior
- Small focused responsibilities
- Testable component boundaries
- Configuration-driven behavior where appropriate
- Predictable error handling
- Maintainable naming and structure
- Accurate documentation of current versus future behavior

The project should avoid unnecessary abstraction, premature optimization, and dependencies that do not provide clear value.

---

## Language and Platform

Current implementation standards:

```text
Language: C#
Target Framework: .NET 8
Primary IDE: Visual Studio 2022
Configuration: JSON
Source Control: Git / GitHub
```

New code should remain compatible with the project's configured target framework unless a deliberate framework migration is approved and documented.

---

## Entry Point Standard

CareerOS.Bootstrap uses an explicit `Main()` method rather than top-level statements.

Preferred shape:

```csharp
private static int Main(string[] args)
{
    // Application orchestration
}
```

Reasons include:

- Clear application entry point
- Explicit process exit-code behavior
- Easier navigation for reviewers
- Consistency as command-line behavior grows
- Easier future separation into an application runner/orchestrator

Top-level statements should not be introduced without an explicit architectural decision.

---

## File and Type Organization

Each public or significant internal type should normally have a focused responsibility.

Current structure:

```text
CareerOS.Bootstrap/
â”œâ”€â”€ Models/
â”œâ”€â”€ Services/
â”œâ”€â”€ Program.cs
â””â”€â”€ CareerOS.Bootstrap.csproj
```

Future directories should be introduced only when real implementation complexity justifies them.

Avoid creating folders solely to mirror theoretical architecture that does not yet exist in code.

---

## One Primary Responsibility Per Class

Classes should generally have one primary reason to change.

Examples:

```text
PathService
    -> path/repository discovery

JsonConfigurationService
    -> JSON loading/deserialization

TemplateResolverService
    -> template lookup/resolution

DirectoryPlanService
    -> read-only directory planning
```

Do not combine configuration loading, validation, provisioning, logging, and UI behavior into one large service.

---

## Naming Conventions

Use standard .NET/C# naming conventions.

### Types and methods

Use PascalCase:

```text
DirectoryPlanService
TemplateResolverService
BuildPlan
ResolveTemplate
FindRepositoryRoot
```

### Public properties

Use PascalCase:

```text
Name
Directory
Template
Children
```

### Local variables and parameters

Use camelCase:

```text
configurationPath
profile
resolvedTemplate
basePath
```

### Private fields

If private fields are introduced, use a consistent underscore-prefixed camelCase convention:

```csharp
private readonly IFileSystem _fileSystem;
```

Do not mix naming conventions within the same codebase.

---

## Boolean Naming

Boolean names should communicate a true/false state clearly.

Prefer:

```text
isDryRun
hasErrors
shouldCreate
isValid
```

Avoid vague names such as:

```text
flag
value
status
```

when a more specific name is available.

---

## Method Design

Methods should be small enough to understand without excessive mental context.

A method should ideally perform one coherent operation.

Prefer:

```text
Load configuration
Resolve template
Build plan
Validate request
Execute provisioning
```

instead of one method that performs all of those actions.

Long methods should be reviewed for natural responsibility boundaries before being split mechanically.

---

## Parameter Validation

Public/service-layer methods should validate required inputs at the boundary where practical.

Use framework mechanisms such as:

```csharp
ArgumentNullException.ThrowIfNull(value);
```

and explicit checks for invalid strings or collections where appropriate.

Examples of invalid values that may require rejection:

- Null required objects
- Empty template names
- Empty profile directory names
- Empty directory-node names
- Missing required configuration paths

Future provisioning code must validate filesystem destinations before writes occur.

---

## Error Handling

Errors should be explicit and actionable.

Do not silently swallow exceptions unless there is a documented recovery strategy.

Prefer failures that identify:

- What failed
- Which resource or input was involved
- What the user/developer can correct where practical

Top-level application failure should continue to produce a non-success process result.

Do not use exceptions as ordinary branch logic when a normal result model is more appropriate.

---

## Exit Codes

Current convention:

```text
0 = success
1 = failure
```

Future category-specific nonzero codes may be added, but `0` must remain reserved for successful execution.

Exit-code changes should be documented because scripts and CI may depend on them.

---

## Nullability

Nullable reference types should remain enabled where supported by the project configuration.

Code should distinguish intentionally nullable values from values that are required.

Avoid using the null-forgiving operator (`!`) merely to suppress warnings without proving the value is safe.

When null is a legitimate state, document or model that state clearly.

---

## Implicit Usings

Implicit usings may remain enabled when configured in the project.

Explicit `using` directives should still be used when they improve clarity or are required by project structure.

Do not add unused imports.

---

## Collection Initialization

Prefer clear collection initialization that matches the project's target language version.

Use empty collections rather than null collections for model properties where an empty collection accurately represents the state.

This reduces unnecessary null checks during traversal.

---

## Recursive Models and Algorithms

Recursive directory structure is a deliberate design choice.

`DirectoryNode` may contain child `DirectoryNode` values.

Recursive traversal code should:

- Preserve parent-child relationships
- Avoid hard-coded nesting depth
- Reject invalid node names before path construction
- Remain independently testable

Any future recursion-depth or cycle concerns should be addressed through validation rather than replacing the recursive model without need.

---

## Planning Versus Provisioning

This is a critical coding boundary.

Planning logic must remain separable from filesystem modification.

Planning code should determine intended state.

Provisioning code should apply already validated intent.

Do not place calls such as:

```csharp
Directory.CreateDirectory(...)
```

inside planning logic.

Future dry-run and real execution should consume the same validated plan representation wherever practical.

---

## Filesystem Safety

Future filesystem code must follow conservative behavior.

Normal provisioning should:

- Inspect before creating
- Preserve existing valid directories
- Create only missing validated directories
- Avoid automatic deletion
- Avoid overwriting unrelated user content
- Surface failures clearly
- Support repeated safe execution

Potentially destructive functionality requires separate requirements and safeguards.

---

## Path Handling

Use `System.IO.Path` APIs rather than manual string concatenation for filesystem paths.

Prefer:

```csharp
Path.Combine(root, profile.Directory, node.Name)
```

Avoid:

```csharp
root + "\\" + profile.Directory + "\\" + node.Name
```

Do not hard-code developer-specific drive letters, usernames, or absolute repository locations into core logic.

---

## Configuration-Driven Behavior

Supported profile/template structure should remain external to compiled C# code.

Prefer configuration for:

- Profile definitions
- Template selection
- Directory hierarchy
- Future destination settings where appropriate

Do not introduce person-specific branches such as:

```csharp
if (profile.Name == "Chris")
```

when the behavior can be represented by configuration or reusable rules.

---

## JSON Handling

Use `System.Text.Json` unless requirements justify another library.

Configuration loading should remain centralized rather than scattered across the application.

Future schema evolution should be explicit and version-aware if incompatible configuration changes are introduced.

Do not silently reinterpret invalid configuration.

---

## Dependency Policy

Avoid unnecessary third-party dependencies.

Before adding a package, evaluate:

- Whether .NET already provides the capability
- Maintenance burden
- Security implications
- Licensing
- Deployment impact
- Testability

Dependencies should solve a real problem rather than add abstraction for its own sake.

---

## Comments

Comments should explain **why**, not restate obvious code.

Useful:

```csharp
// Keep planning read-only so the same plan can be reviewed before provisioning.
```

Not useful:

```csharp
// Increment i by 1.
i++;
```

Complex safety behavior, non-obvious edge cases, and intentional architectural constraints should be documented.

Avoid stale comments that no longer match implementation.

---

## XML Documentation

XML documentation comments should be considered for public APIs, important services, and behavior where IntelliSense documentation adds value.

Example:

```csharp
/// <summary>
/// Builds a read-only directory plan for the supplied profile and template.
/// </summary>
```

Do not add low-value XML comments that simply repeat the method name.

---

## Complete-File Update Practice

For substantial updates, prefer replacing a complete class/file with a validated version rather than manually applying many small disconnected edits when doing so reduces integration risk.

This is especially useful during guided development where copy/paste errors could leave a partially updated implementation.

However, complete-file replacement must not bypass review. Always inspect the diff before committing.

---

## Formatting

Use Visual Studio/.NET formatting conventions.

General expectations:

- Four-space indentation
- Braces on new lines for type/method/control blocks
- One statement per line
- Consistent blank-line separation between logical sections
- No trailing whitespace
- Files end with a newline

Avoid manual alignment that becomes fragile during edits.

---

## Line Length and Readability

There is no rigid short line-length limit for C# code, but excessively long statements should be reformatted for readability.

Markdown documentation should use normal prose wrapping where practical and rely on editor word wrap for display.

Do not sacrifice clarity merely to meet an arbitrary line count.

---

## Encoding

Repository text files should use a Unicode-safe encoding, preferably UTF-8.

Avoid saving documentation or source files using legacy encodings that cannot preserve required characters.

When Visual Studio warns that Unicode characters cannot be represented in the current code page, save using a Unicode/UTF-8-compatible encoding.

---

## Line Endings

Windows development may use CRLF line endings.

Git line-ending normalization warnings should be understood rather than treated automatically as corruption.

A future `.gitattributes` file may be introduced if the project needs explicit repository-wide line-ending policy.

---

## Source-Control Practices

Do not develop significant changes directly on `main`.

Use purpose-specific branches such as:

```text
feat/<name>
fix/<name>
docs/<name>
test/<name>
refactor/<name>
```

Before committing:

```powershell
git status
git diff
git diff --stat
```

Before merge, validate the build and applicable tests.

---

## Commit Messages

Use concise conventional-style commit messages where practical.

Examples:

```text
feat: implement multi-profile dry-run scaffolding
docs: establish project documentation foundation
test: add template resolver coverage
fix: reject invalid destination paths
refactor: separate application orchestration
```

A commit should represent one coherent change set whenever practical.

---

## Pull Requests

Significant work should move through a pull request before reaching `main` as the repository workflow matures.

A pull request should explain:

- What changed
- Why it changed
- Relevant requirements
- Testing performed
- Documentation impact
- Known limitations or follow-up work

Future CI should validate builds and tests automatically.

---

## Testing Standards

New behavior should be designed with testability in mind.

Unit tests should focus on deterministic business behavior.

Filesystem integration tests must use isolated temporary directories.

Tests must not operate against real CareerOS user directories.

When bugs are fixed, add regression tests where practical.

Detailed testing policy is maintained in:

```text
Documentation/Development/TESTING_STRATEGY.md
```

---

## Requirement Traceability

Significant implementation should be traceable to the requirements catalog when applicable.

References may include:

```text
US-###
FR-###
NFR-###
ADR-###
TEST-###
```

Do not rename existing requirement identifiers casually once they are referenced elsewhere.

---

## Architecture Decision Records

Significant architectural decisions should receive ADRs when the decision would otherwise be difficult for future maintainers to understand.

Examples include:

- CLI framework selection
- Logging framework selection
- Provisioning-plan model
- Destination-root precedence
- Database/documentation integration
- Cross-platform support strategy

Do not create an ADR for trivial implementation details.

---

## Documentation Updates

When behavior changes materially, review at minimum:

```text
README.md
CURRENT_STATE.md
FUTURE_STATE.md
COMPONENTS.md
DATA_FLOW.md
Requirements documents
Roadmap
CHANGELOG.md
```

Only update documents actually affected by the change.

Planned functionality must not be described as implemented until it exists and has been validated.

---

## Security-Conscious Coding

Future code that accepts user-controlled paths, remote inputs, credentials, or external integrations must be reviewed for security implications.

Filesystem code should consider:

- Invalid paths
- Reserved names
- Path traversal
- Unexpected destination roots
- Permissions
- Reparse/symbolic-link behavior where relevant

Logging must not expose secrets or unnecessary personal data.

---

## Performance Guidance

CareerOS.Bootstrap is an orchestration/provisioning utility, not a high-throughput service.

Optimize for correctness and clarity first.

Performance work should be driven by observed need or requirements.

Avoid introducing complex caching, parallelism, or asynchronous behavior unless it provides measurable value and preserves safety.

---

## Refactoring Standard

Refactoring should preserve validated behavior unless the change intentionally modifies requirements.

Before larger refactors:

1. Confirm current behavior.
2. Establish or improve tests where practical.
3. Make the structural change.
4. Rebuild and rerun tests.
5. Review documentation and traceability impact.

Do not combine major refactoring with unrelated new features unless there is a clear reason.

---

## Future Database Integration

A future SQL Server/SSMS-backed documentation or requirements repository may be introduced.

If implemented, database access should be isolated behind dedicated components rather than embedded throughout the application.

Potential concerns include:

- Connection configuration
- Schema migrations
- Parameterized T-SQL
- Data integrity
- Search/query performance
- Separation between Markdown source artifacts and database representations
- Website/API consumption

The repository documentation remains the current source of truth until a future architecture decision explicitly changes that model.

---

## Definition of Code-Review Readiness

A change is generally ready for review when:

- The code builds successfully.
- Applicable tests pass.
- New behavior matches documented requirements.
- Safety boundaries are preserved.
- Names and responsibilities are understandable.
- No accidental machine-specific paths or secrets were introduced.
- Relevant documentation is updated.
- `git diff` contains only intended changes.

---

## Summary

CareerOS.Bootstrap coding standards are centered on disciplined, safe evolution.

The preferred engineering sequence is:

```text
Understand Requirement
        |
        v
Design Focused Change
        |
        v
Implement Clearly
        |
        v
Validate Inputs
        |
        v
Build / Test
        |
        v
Review Diff
        |
        v
Update Documentation
        |
        v
Pull Request / Review
        |
        v
Merge to Stable Main
```

The most important rule is that code should remain easy to understand, safe to execute, and straightforward to verify as the project grows.
