# Maliev SupplierService - Agent Guidelines

This document provides instructions for AI agents operating within the `Maliev.SupplierService` repository.
Follow these guidelines to maintain code quality, consistency, and stability.

## 1. Build, Lint, and Test Commands

Always verify changes by running builds and tests.

### Build
- **Build Solution:** `dotnet build`
- **Build Specific Project:** `dotnet build Maliev.SupplierService.Api/Maliev.SupplierService.Api.csproj`

### Test
- **Run All Tests:** `dotnet test`
- **Run Unit Tests Only:** `dotnet test Maliev.SupplierService.Tests --filter "Category=Unit"` (assuming category usage, otherwise path based)
- **Run Integration Tests Only:** `dotnet test Maliev.SupplierService.Tests --filter "Category=Integration"`
- **Run a Single Test:**
  ```bash
  dotnet test --filter "FullyQualifiedName~Maliev.SupplierService.Tests.Integration.SuppliersControllerTests.CreateSupplier_WithValidData_Returns201AndSupplier"
  ```
  *Note: Replace the fully qualified name with the specific test method you want to run.*

### Lint/Format
- **Check Formatting:** `dotnet format --verify-no-changes`
- **Fix Formatting:** `dotnet format`

## 2. Code Style Guidelines

Adhere strictly to the existing style found in `Maliev.SupplierService.Api` and `Maliev.SupplierService.Data`.

### General Architecture
**Architecture**: Clean Architecture (Api, Application, Domain, Infrastructure, Tests)

- **Api:** Controllers, DTOs, Middleware.
- **Application:** Use cases, handlers, DTOs.
- **Domain:** Entities, interfaces, value objects.
- **Infrastructure:** EF Core, repositories.
- **Tests:** Integration and Unit tests using xUnit.
- **Dependency Injection:** Use constructor injection for all dependencies.
- **Asynchrony:** Use `async/await` for all I/O-bound operations. Avoid `.Result` or `.Wait()`.

### Imports & Namespaces
- **File-Scoped Namespaces:** Use file-scoped namespaces (e.g., `namespace Maliev.SupplierService.Api.Controllers;`).
- **Ordering:** System namespaces first, then third-party, then project namespaces.
- **Cleanliness:** Remove unused `using` directives.

### Formatting
- **Indentation:** Use 4 spaces. No tabs.
- **Braces:** Allman style (opening braces on a new line).
- **Line Length:** Aim for ~120 characters, but don't strictly enforce if it hurts readability.
- **Properties:** Use auto-properties where possible (`public Guid Id { get; set; }`).

### Types & Features
- **Nullable Reference Types:** Enabled. Use `string?` for nullable strings and `required string` for required properties in classes.
- **Records:** Use `record` or `record struct` for DTOs and immutable data structures (e.g., `CreateSupplierRequest`).
- **Pattern Matching:** Encourage use of pattern matching (`is`, `switch` expressions) where appropriate.

### Naming Conventions
- **Classes/Methods/Properties:** PascalCase (e.g., `SupplierService`, `CreateAsync`, `CompanyName`).
- **Variables/Parameters:** camelCase (e.g., `supplierId`, `cancellationToken`).
- **Fields:** Private fields should be camelCase with underscore prefix (e.g., `_context`, `_logger`).
- **Interfaces:** Prefix with 'I' (e.g., `ISupplierService`).
- **Async Methods:** Suffix with 'Async' (e.g., `GetByIdAsync`).

### Error Handling
- **Exceptions:** Use exceptions for exceptional control flow.
- **Controller Handling:** Controllers should catch specific exceptions (like `InvalidOperationException` for business rules) and map them to appropriate HTTP status codes (400, 404, 409).
- **Validation:** Use Data Annotations (`[Required]`, `[MaxLength]`) on Entities and DTOs.
- **Result Types:** Services typically return Entities or specific result tuples/objects, not `IActionResult`.

### Database & Entities
- **Table Names:** Snake_case using `[Table("table_name")]`.
- **Column Names:** Snake_case using `[Column("column_name")]`.
- **Primary Keys:** GUIDs are preferred for IDs.
- **Configuration:** Use `IEntityTypeConfiguration<T>` in `Data/Configurations` rather than `OnModelCreating` bloat if possible, though Attributes are currently used in Entities. Follow the pattern in `Supplier.cs`.

### API Design
- **Versioning:** Use `[ApiVersion("1.0")]` on controllers.
- **Routing:** Kebab-case URLs (e.g., `supplier/v1/suppliers/{id}/eligibility`).
- **Documentation:** Add XML comments (`/// <summary>`) to Controllers and public Service methods for OpenAPI generation.

### Testing
- **Framework:** xUnit.
- **Naming:** `MethodName_StateUnderTesting_ExpectedBehavior`.
- **Integration Tests:** Inherit from `BaseIntegrationTest`. Use `IntegrationTestWebAppFactory`.
- **Assertions:** Use `Assert` class (e.g., `Assert.Equal`, `Assert.NotNull`).

## 3. Constitution Rules (Mandatory)

This service strictly adheres to the following platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **EF Core Design Package**: `Microsoft.EntityFrameworkCore.Design` must ONLY be in the Infrastructure project where migrations are located. NEVER add it to the Api project.
- ✅ **EF Migrations**: Create migrations using Infrastructure as both the project and startup project:
  ```bash
  dotnet ef migrations add <Name> --project Maliev.SupplierService.Infrastructure --startup-project Maliev.SupplierService.Infrastructure
  ```

## 4. Workflow Rules

1. **Safety First:** always check dependencies before modifying a file.
2. **Incremental Changes:** Make small, verifiable changes.
3. **Verification:** Run `dotnet build` after every significant code change.
4. **No Assumptions:** Check `Program.cs` or `Startup.cs` (if it existed) to understand service registration.
5. **Secrets:** Never commit secrets. Use `user-secrets` or environment variables.

---
*Generated by Antigravity for Maliev.SupplierService*


## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project Maliev.<Domain>Service.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
