# Tasks: Permission-Based Authorization Migration

**Feature**: Permission-Based Authorization Migration
**Branch**: `002-permission-auth-migration`
**Plan**: [plan.md](./plan.md)
**Status**: Ready for Implementation

## Implementation Strategy

We will implement this feature using an incremental, user-story-driven approach. 
1. **Phase 1 & 2**: Setup the necessary constants and the registration service.
2. **Phase 3**: Enable full access for the `supplier-admin` role (MVP).
3. **Phase 4 & 5**: Implement restricted and read-only access for other roles.
4. **Phase 6**: Final polish and verification.

## Dependencies

- Foundational tasks (Phase 2) MUST be completed before any User Story phases.
- User stories can be implemented independently after Phase 2, but following priority order (US1 -> US2 -> US3) is recommended.

## Phase 1: Setup

Goal: Initialize the project structure for the migration.

- [ ] T001 Create `Maliev.SupplierService.Api/Constants/` directory
- [ ] T002 Create `Maliev.SupplierService.Api/Services/` directory if it doesn't exist
- [ ] T003 [P] Update `appsettings.json` with mandatory LogLevel configuration (Information for Default/Lifetime, Warning for Microsoft/System)

## Phase 2: Foundational

Goal: Define permissions/roles and implement the registration service.

- [ ] T004 [P] Define permission string constants in `Maliev.SupplierService.Api/Constants/Permissions.cs`
- [ ] T005 [P] Define role name constants and their permission mappings in `Maliev.SupplierService.Api/Constants/Roles.cs`
- [ ] T006 Configure resilient IAM client using `builder.Services.AddIAMClient(builder.Configuration, "supplier-service")` in `Program.cs`
- [ ] T007 Implement `Maliev.SupplierService.Api/Services/SupplierIAMRegistrationService.cs` by inheriting from `IAMRegistrationService` (from ServiceDefaults)
- [ ] T008 Configure standard ASP.NET Core Policy-based authorization in `Maliev.SupplierService.Api/Program.cs` mapping each permission to a policy
- [ ] T009 Integrate `AuthorizationRegistrationService` into the startup sequence in `Maliev.SupplierService.Api/Program.cs` to run before `app.Run()`

## Phase 3: User Story 1 - Full Access for Administrators (Priority: P1)

Goal: Enable and verify full access for `supplier-admin`.

- [ ] T010 [US1] Update `Maliev.SupplierService.Api/Controllers/SuppliersController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T011 [US1] Update `Maliev.SupplierService.Api/Controllers/SupplierContactsController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T012 [US1] Update `Maliev.SupplierService.Api/Controllers/SupplierEvaluationsController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T013 [US1] Update `Maliev.SupplierService.Api/Controllers/SupplierAuditController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T014 [US1] Update `Maliev.SupplierService.Api/Controllers/SupplierCertificationsController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T015 [US1] Update `Maliev.SupplierService.Api/Controllers/SupplierOnboardingController.cs` to use `[Authorize(Policy = "...")]` for all actions
- [ ] T016 [US1] Update integration tests in `Maliev.SupplierService.Tests/Integration/` to verify `supplier-admin` access

## Phase 4: User Story 2 - Restricted Access for Coordinators (Priority: P2)

Goal: Verify restricted access for `supplier-coordinator`.

- [ ] T017 [US2] Add integration tests in `Maliev.SupplierService.Tests/Integration/` for `supplier-coordinator` verifying write access to suppliers/contacts and read-only to performance

## Phase 5: User Story 3 - Read-Only Access for Viewers (Priority: P3)

Goal: Verify read-only access for `supplier-viewer`.

- [ ] T018 [US3] Add integration tests in `Maliev.SupplierService.Tests/Integration/` for `supplier-viewer` verifying read-only access across all controllers

## Phase 6: Polish & Cross-Cutting Concerns

Goal: Final validation and audit logging.

- [ ] T019 Implement structured logging for authorization failures in a global middleware or custom authorization handler
- [ ] T020 Run full test suite and ensure zero warnings in build
- [ ] T021 Final review of Scalar documentation to ensure Security Schemes and Requirements are correctly reflected for all endpoints
