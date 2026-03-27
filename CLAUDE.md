# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Project

./README.md

# Skill Auto-Routing

This project has specialized skills in `.claude/skills/`. The routing table below applies to **all speckit phases** — `/speckit.plan`, `/speckit.tasks`, `/speckit.analyze`, and `/speckit.implement` — as well as any manual implementation work. Invoke the matching skill via the Skill tool at the appropriate moment in each phase.

| Artefact / context | Invoke skill |
|---|---|
| Domain entity, aggregate, DDD model, `BaseEntity` subclass | `dotnet-clean-entity` |
| EF Core Fluent config, repository interface/impl, `DbSet`, persistence wiring | `dotnet-clean-repository` |
| MediatR command, query, handler, validator, CQRS feature, use case | `dotnet-clean-feature` |
| Minimal API endpoint, `IEndpoint` class, HTTP route, CRUD wiring | `dotnet-clean-endpoint` |
| xUnit unit tests, NSubstitute mocks, handler/entity/domain tests | `dotnet-clean-unit-test` |
| Integration tests, `WebApplicationFactory`, Testcontainers, Respawn | `dotnet-clean-integration-test` |
| `ILogger<T>` log statements, Serilog structured logging | `dotnet-clean-logging` |
| New solution scaffold, project init, base abstractions, NuGet setup | `dotnet-clean-scaffold` |
| Architecture review, layer guidance, Clean Architecture compliance | `dotnet-clean-architect` |

**When to invoke per phase:**

- **`/speckit.plan`** — invoke skills while designing artifacts: consult `dotnet-clean-architect` for layer decisions, `dotnet-clean-entity` when authoring `data-model.md`, `dotnet-clean-feature` / `dotnet-clean-endpoint` when defining API contracts, `dotnet-clean-repository` when planning persistence.
- **`/speckit.tasks`** — invoke skills before generating tasks for a layer so file paths, class names, and task descriptions match the skill's conventions. Each task description should name the skill that will implement it.
- **`/speckit.analyze`** — invoke `dotnet-clean-architect` when checking layer or dependency-rule violations in the analysis report.
- **`/speckit.implement`** — invoke the matching skill **before writing code** for each task.

Each task maps to at most one skill. If a task creates multiple artefacts (e.g., entity + repository), invoke skills sequentially in dependency order (entity first, then repository).