# CareerOS.Bootstrap — GitHub Copilot Instructions

## Project Purpose

CareerOS.Bootstrap is a configuration-driven .NET console application used to provision and maintain standardized CareerOS directory structures.

The application is intended to support multiple CareerOS profiles, reusable directory templates, safe repeatable execution, dry-run previews, validation, logging, documentation, testing, and future extensibility.

## Core Development Principles

- Use C# and .NET 8 unless the project requirements are intentionally changed.
- Prefer an explicit `Program` class with an explicit `Main(string[] args)` entry point.
- Do not convert the application to top-level statements unless explicitly requested.
- Keep `Main()` focused on application orchestration rather than business logic.
- Follow separation of concerns and single-responsibility principles.
- Do not hard-code machine-specific paths, usernames, profile names, or drive letters.
- Keep CareerOS profiles and directory structures configuration-driven.
- Use JSON configuration for profile and template definitions.
- Keep directory templates reusable and independent from specific users.
- Preserve recursive `DirectoryNode` support for nested directory structures.
- Prefer complete-file replacements when modifying an existing source file so updates remain internally consistent.
- Avoid unnecessary abstractions until they provide clear architectural value.

## Filesystem Safety

- Treat filesystem modification as a privileged operation.
- Planning, validation, and template resolution must occur before filesystem writes.
- Dry-run behavior must never create, delete, rename, or modify directories.
- Actual provisioning must be idempotent.
- Existing directories must be preserved unless explicit future functionality states otherwise.
- Do not implement destructive behavior without explicit requirements and safeguards.
- Never delete user data automatically.

## Configuration

- `bootstrap.json` defines CareerOS profiles.
- `templates.json` defines reusable directory templates.
- Configuration loading must validate that required files exist.
- Template names should be resolved case-insensitively.
- Invalid profile or template configuration should fail before filesystem modification.
- Future configuration schema changes should preserve backward compatibility where practical.

## Code Quality

- Use meaningful class, method, variable, and property names.
- Prefer readable code over clever code.
- Enable and respect nullable reference types.
- Add XML documentation where it explains architectural intent, public behavior, assumptions, or non-obvious logic.
- Do not add comments that merely restate obvious code.
- Throw meaningful exceptions with actionable messages.
- Keep services focused on one primary responsibility.

## Current Architecture

Current implemented components include:

- `Program`
  - Application entry point and orchestration.

- `PathService`
  - Locates the repository root and configuration directory.

- `JsonConfigurationService`
  - Loads and deserializes JSON configuration.

- `TemplateResolverService`
  - Resolves profile template names to configured templates.

- `DirectoryPlanService`
  - Recursively creates a read-only provisioning plan without modifying the filesystem.

- Configuration models
  - `BootstrapConfiguration`
  - `ProfileConfiguration`
  - `TemplateConfiguration`
  - `CareerTemplate`
  - `DirectoryNode`

## Current Capabilities

- Discover repository root without hard-coded paths.
- Load `bootstrap.json`.
- Load `templates.json`.
- Support multiple profiles.
- Resolve profile templates.
- Support nested directory definitions through recursive `DirectoryNode.Children`.
- Generate dry-run directory plans.
- Run without modifying the filesystem.

## Planned Capabilities

The following features are planned and must not be treated as already implemented:

- Configurable installation root.
- Command-line arguments.
- True `--dry-run` CLI mode.
- Configuration validation service.
- Directory creation/provisioning service.
- Existing-directory reporting.
- Structured execution summaries.
- File logging.
- Error logging.
- Unit tests.
- Integration tests.
- User stories and requirements traceability.
- Architecture and process diagrams.
- Current-state and future-state architecture documentation.
- Release packaging.
- GitHub Actions / CI.
- Versioned releases.
- Optional Git repository initialization.
- Additional CareerOS profile templates.
- Additional configuration schema validation.
- Backup or rollback capabilities where appropriate.

## Testing Philosophy

Future tests should verify behavior at the service level whenever practical.

Examples include:

- Template resolution succeeds for valid templates.
- Template resolution fails safely for unknown templates.
- Directory planning correctly traverses nested directory nodes.
- Dry-run planning never modifies the filesystem.
- JSON configuration loads valid files correctly.
- Invalid configuration fails before provisioning.
- Repeated provisioning does not damage existing directory structures.

## Documentation Standard

Documentation should allow a new developer or reviewer to understand:

1. What CareerOS.Bootstrap is intended to accomplish.
2. What functionality exists today.
3. What functionality is planned.
4. How configuration flows through the application.
5. Why major architectural decisions were made.
6. How to build, run, test, and extend the application.

Clearly distinguish current implementation from planned/future functionality.

## AI Assistance

AI tools may assist development but should not replace understanding.

Any generated code must:

- Be understood before acceptance.
- Follow this repository's architecture.
- Build successfully.
- Preserve existing validated behavior unless intentionally changed.
- Be tested appropriately before commit.