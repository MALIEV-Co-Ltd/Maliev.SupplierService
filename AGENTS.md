# Maliev SupplierService - Agent Guidelines

This document provides instructions for AI agents operating within the `Maliev.SupplierService` repository.
Follow these guidelines to maintain code quality, consistency, and stability.

## 1. Build, Test & Lint Commands

All commands run from within this service directory (`B:\maliev\Maliev.SupplierService`).

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.SupplierService.slnx

# Run all tests
dotnet test Maliev.SupplierService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~SuppliersControllerTests.CreateSupplier_WithValidData_Returns201AndSupplier"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~SuppliersControllerTests"

# Run with code coverage
dotnet test Maliev.SupplierService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.SupplierService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.SupplierService.Infrastructure --startup-project Maliev.SupplierService.Infrastructure
```

## 2. Code Style & Conventions

### Architecture

**Architecture**: Clean Architecture (Api, Application, Domain, Infrastructure, Tests)

- **Api:** Controllers, Consumers, Middleware.
- **Application:** Use cases, handlers, DTOs, Interfaces.
- **Domain:** Entities, value objects, domain interfaces.
- **Infrastructure:** EF Core DbContext, repositories, HTTP clients.
- **Tests:** Unit + Integration tests (xUnit).
- **Dependency Injection:** Constructor injection with `private readonly` fields.
- **Asynchrony:** Use `async/await` for all I/O-bound operations. Avoid `.Result` or `.Wait()`.

### C# Naming & Formatting

- **Namespaces**: File-scoped (`namespace Maliev.SupplierService.Api.Controllers;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `GetByIdAsync`)
- **Interfaces**: Prefix with `I` (e.g., `ISupplierService`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `supplier.suppliers.create`, `supplier.eligibility-rules.update`
  - Invalid: `supplier.supplier.create` (singular), `eligibility.update` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns

- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("supplier/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {SupplierId}", supplierId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Check existing conventions in this service for naming policy
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

### Database & Entities

- **Table Names**: Snake_case using `[Table("table_name")]`.
- **Column Names**: Snake_case using `[Column("column_name")]`.
- **Primary Keys**: GUIDs are preferred for IDs.
- **Configuration**: Use `IEntityTypeConfiguration<T>` in `Data/Configurations` rather than `OnModelCreating` bloat if possible, though Attributes are currently used in Entities. Follow the pattern in `Supplier.cs`.

### API Design

- **Versioning**: Use `[ApiVersion("1")]` on controllers.
- **Routing**: Kebab-case URLs (e.g., `supplier/v1/suppliers/{id}/eligibility`).
- **Documentation**: Add XML comments (`/// <summary>`) to Controllers and public Service methods for OpenAPI generation.

## 3. Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/supplier/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

## 4. Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

## 5. Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("supplier.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with service domain (`/supplier`)
- **Scalar docs**: Configured at `/supplier/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

### EF Core Design Package

- `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- It belongs ONLY in the Infrastructure project where migrations live
- Migration commands must target Infrastructure as both project and startup-project:
  ```bash
  dotnet ef migrations add <Name> --project Maliev.SupplierService.Infrastructure --startup-project Maliev.SupplierService.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern

Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- Never use `.Ignore(e => e.Xmin)` — remove the entity property instead

## 6. Git Rules

- Each `Maliev.*` folder is an independent git repo. Work from within this service directory before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked

---

*Generated by Antigravity for Maliev.SupplierService*
