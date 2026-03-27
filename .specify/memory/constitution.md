<!--
SYNC IMPACT REPORT
==================
Version change: [TEMPLATE] → 1.0.0 (initial ratification from blank template)

Modified principles:
  [PRINCIPLE_1_NAME] → I. Clean Architecture (NON-NEGOTIABLE)
  [PRINCIPLE_2_NAME] → II. Rich Domain Model
  [PRINCIPLE_3_NAME] → III. Code Quality & .NET 10 Practices
  [PRINCIPLE_4_NAME] → IV. Testing Discipline
  [PRINCIPLE_5_NAME] → V. Observability & Logging

Added sections:
  - Technology Stack (replaces [SECTION_2_NAME])
  - Development Workflow (replaces [SECTION_3_NAME])

Removed sections: none

Templates updated:
  ✅ .specify/memory/constitution.md — this file
  ✅ .specify/templates/plan-template.md — Constitution Check gates updated
  ✅ .specify/templates/spec-template.md — language note added
  ✅ .specify/templates/tasks-template.md — language note added

Follow-up TODOs:
  - None. All fields resolved from project context.
-->

# MyProject Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)

Solution MUST maintain exactly 4 source layers — **Domain**, **Application**,
**Infrastructure**, **API** — with strict unidirectional dependency flow:

```
API → Application → Domain
Infrastructure → Application → Domain
```

- Domain layer MUST contain zero infrastructure or framework dependencies.
- Application layer MUST contain only MediatR commands/queries, validators,
  interfaces, and DTOs. No EF Core, no HTTP concerns.
- Infrastructure layer MUST implement Domain/Application interfaces
  (repositories, external services). No business logic permitted here.
- API layer MUST contain only endpoint registration, middleware, and DI wiring.
  Endpoints MUST be thin (delegate immediately to MediatR).
- Cross-layer references that violate this hierarchy MUST be rejected at review.

**Rationale**: Layer isolation ensures independent testability, replaceability
of infrastructure, and long-term maintainability as the codebase grows.

### II. Rich Domain Model

Domain entities MUST encapsulate their own invariants and business rules.
Anemic domain models (plain data bags with all logic in services) are
prohibited.

- Every entity MUST expose a `static Create(...)` factory method as the sole
  construction path; direct constructor calls from outside the Domain layer
  MUST NOT be used.
- State mutations MUST go through named domain methods that enforce invariants
  (e.g., `Order.AddItem(...)`, `User.ChangeEmail(...)`).
- Domain events SHOULD be raised from within entity methods when meaningful
  state transitions occur.

**Rationale**: Keeping rules inside the entity makes them impossible to bypass
and ensures the model is always in a valid state.

### III. Code Quality & .NET 10 Practices

All C# code MUST follow .NET 10 idiomatic patterns:

- Use **file-scoped namespaces** (`namespace Foo;`) in all new files.
- Prefer **records** for immutable DTOs, and command/query
  request types.
- Use **primary constructors** for services and handlers where all dependencies
  are injected once.
- Use **pattern matching** and **switch expressions** instead of chains of
  `if`/`else if`.
- **Nullable reference types** MUST be enabled (`<Nullable>enable</Nullable>`)
  and all warnings treated as errors in CI.
- Method and class length guidelines:
  - Methods MUST NOT exceed 30 lines; extract to private methods if needed.
  - Classes MUST NOT exceed 200 lines; split by responsibility if needed.
- **YAGNI** — only build what the current feature requires. No speculative
  abstractions for hypothetical future use cases.
- Comments MUST explain *why*, not *what*. Self-documenting names are preferred
  over compensatory comments.

**Rationale**: Consistent use of modern C# features reduces cognitive load,
improves readability, and keeps code idiomatic for the platform.

### IV. Testing Discipline

- **Unit tests** MUST cover Domain entities and Application handlers in
  isolation (xUnit + NSubstitute + FluentAssertions).
- **Integration tests** MUST use a real PostgreSQL database via Testcontainers
  — no in-memory database substitutes.
- Tests MUST follow Arrange-Act-Assert pattern with descriptive method names:
  `MethodName_Scenario_ExpectedBehavior`.
- A feature is NOT considered complete until its happy-path and primary
  error-path are covered by tests.
- Tests for a user story MUST be written (and confirmed failing) before
  implementation begins when following TDD flow.

**Rationale**: Real-database integration tests prevent the class of bugs where
mocked tests pass but production migrations or query behaviour diverge.

### V. Observability & Logging

- Structured logging via **Serilog** is REQUIRED in all handlers, services,
  and endpoint middleware.
- Log levels MUST follow this convention:
  - `Information` — successful business events (order created, user registered).
  - `Warning` — recoverable anomalies (validation failure, cache miss).
  - `Error` — unhandled exceptions or unrecoverable states.
  - `Debug` — developer-facing detail; MUST be stripped in production builds.
- Message templates MUST use structured properties:
  `{@Command}`, `{OrderId}`, not string interpolation.
- Every HTTP request MUST emit an entry/exit log with correlation ID.
- No sensitive data (passwords, tokens, PII) MUST appear in log output.

**Rationale**: Structured logs enable log aggregation, querying, and alerting
in production without requiring code changes.

## Technology Stack

Core technology decisions binding all features in this project:

| Concern            | Technology                                   | Version    |
|--------------------|----------------------------------------------|------------|
| Runtime            | .NET                                         | 10         |
| Language           | C#                                           | 13         |
| Database           | PostgreSQL (Npgsql EF Core provider)         | 10.0.1     |
| ORM (writes)       | EF Core (code-first, Fluent API, snake_case) | 10.x       |
| Queries (reads)    | Dapper (CQRS read side)                      | latest     |
| Mediator           | MediatR                                      | 12.4.1     |
| Validation         | FluentValidation                             | latest     |
| Unit testing       | xUnit + NSubstitute + FluentAssertions       | latest     |
| Integration testing| xUnit + Testcontainers + Respawn             | latest     |
| Logging            | Serilog (structured, console + file sinks)   | latest     |
| API style          | Minimal API (class-per-endpoint pattern)     | .NET 10    |

Adding or replacing a technology in this table MUST be treated as a
constitution amendment.

## Development Workflow

### Language of speckit Outputs

All output produced by speckit commands (`/speckit.specify`, `/speckit.clarify`,
`/speckit.plan`, `/speckit.tasks`, `/speckit.analyze`, `/speckit.implement`)
MUST be written in **Vietnamese**. Code, file paths, identifiers, and
code comments remain in English. Only prose, headings, descriptions,
rationale, and task descriptions are in Vietnamese.

### Feature Lifecycle

Every feature MUST progress through these gates in order:

1. **Specify** (`/speckit.specify`) — Vietnamese spec with user stories and
   acceptance criteria.
2. **Clarify** (`/speckit.clarify`) — resolve ambiguities before design.
3. **Plan** (`/speckit.plan`) — technical design including data model,
   contracts, and architecture decisions.
4. **Tasks** (`/speckit.tasks`) — dependency-ordered task list grouped by
   user story.
5. **Analyze** (`/speckit.analyze`) — consistency check across artifacts.
6. **Implement** (`/speckit.implement`) — execute tasks with constitution
   compliance at each checkpoint.

Skipping gates requires explicit justification documented in `plan.md`.

### CQRS Split

- Commands (writes) MUST use EF Core repositories through the Application layer.
- Queries (reads) MUST use Dapper directly in query handlers for performance.
- Mixing ORM reads into command handlers and raw SQL into write paths is
  prohibited without documented justification.

### Schema Naming

- EF Core Fluent API MUST map all table and column names to **snake_case**.
- Migrations MUST be reviewed for correctness before merging.
- No data annotation attributes (`[Column]`, `[Table]`) in Domain entities —
  all mapping goes in `IEntityTypeConfiguration<T>` classes.

## Governance

This constitution supersedes all other team conventions and README guidance.
Any practice not covered here defaults to the .NET team's official guidelines.

**Amendment Procedure**:
1. Propose change via PR with updated `constitution.md`.
2. Increment version per semantic versioning rules defined below.
3. Update `SYNC IMPACT REPORT` comment at the top of the file.
4. Propagate changes to all dependent templates (see checklist in the report).
5. PR description MUST include migration plan for any existing non-compliant code.

**Versioning Policy**:
- `MAJOR` — backward-incompatible change: principle removed, layer contract
  redefined, technology replacement.
- `MINOR` — new principle added, new mandatory section, tech stack addition.
- `PATCH` — clarification, wording fix, non-semantic refinement.

**Compliance Review**:
- All PRs MUST verify Constitution Check gates defined in `plan.md`.
- Complexity violations MUST be documented in the Complexity Tracking table
  in `plan.md` with explicit justification.
- Violations found during review MUST be resolved before merge; waivers
  require team lead approval and a TODO entry in this file.

**Version**: 1.0.0 | **Ratified**: 2026-03-26 | **Last Amended**: 2026-03-26
