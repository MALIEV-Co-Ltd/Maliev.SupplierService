# Implementation Plan: Permission-Based Authorization Migration

**Branch**: `002-permission-auth-migration` | **Date**: 2025-12-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-permission-auth-migration/spec.md`

## Summary

Migrate the SupplierService from basic role-based authorization to a granular permission-based system. This involves registering 14 granular permissions and 4 predefined roles with the central IAM system during service startup and enforcing these permissions across all API endpoints.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: `Maliev.Aspire.ServiceDefaults`, `Microsoft.AspNetCore.Authorization`  
**Registration Pattern**: `IAMRegistrationService` (from ServiceDefaults), `AddIAMClient` extension
**Storage**: N/A (Permissions/Roles registered with external IAM)  
**Testing**: xUnit with Testcontainers for integration testing  
**Target Platform**: Docker (ASP.NET 10)
**Project Type**: Microservice API  
**ServiceName**: `supplier-service`
**Registration Method**: Inherit `IAMRegistrationService` from ServiceDefaults
**Performance Goals**: < 100ms for local unauthorized rejections  
**Constraints**: Zero warnings, No AutoMapper, No FluentValidation, No FluentAssertions  
**Scale/Scope**: 14 permissions, 4 roles, ~6 controllers

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] Service Autonomy: SupplierService defines its own permission set.
- [x] Explicit Contracts: API changes will be reflected in OpenAPI/Scalar.
- [x] Test-First Development: Integration tests will be updated to verify permission enforcement.
- [x] Real Infrastructure Testing: Testcontainers used for integration tests.
- [x] Auditability: Failed attempts logged as structured JSON.
- [x] Security: Granular permission-based authorization.
- [x] Flat Project Structure: Project maintains company prefix and flat layout.
- [x] No AutoMapper/FluentValidation/FluentAssertions: Explicit mapping and standard validation only.

## Project Structure

### Documentation (this feature)

```text
specs/002-permission-auth-migration/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Technical decisions and rationale
├── data-model.md        # Permission and Role entities
├── quickstart.md        # How to run and verify
├── contracts/           # API contracts (OpenAPI)
└── tasks.md             # Task breakdown
```

### Source Code (repository root)

```text
Maliev.SupplierService.Api/
├── Controllers/         # Updated with [Authorize(Policy = "...")]
├── Services/
│   ├── IAuthorizationRegistrationService.cs
│   └── AuthorizationRegistrationService.cs
├── Constants/
│   ├── Permissions.cs   # Permission string constants
│   └── Roles.cs         # Role definitions
├── Program.cs           # Registration logic integrated
└── appsettings.json     # Log level configuration

Maliev.SupplierService.Data/
└── [Unchanged]

Maliev.SupplierService.Tests/
└── Integration/         # Updated authorization tests
```

**Structure Decision**: Standard Maliev flat structure with business logic in the Api project.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |