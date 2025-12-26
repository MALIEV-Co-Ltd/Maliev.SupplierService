# Feature Specification: Permission-Based Authorization Migration

**Feature Branch**: `002-permission-auth-migration`  
**Created**: 2025-12-23  
**Status**: Draft  
**Input**: User description: "use the content in supplier-specify.md as specifications"

## Clarifications

### Session 2025-12-23

- Q: What format should be used for permission identifiers? → A: Human-readable strings (e.g., `supplier.suppliers.create`).
- Q: How should permission enforcement be applied to API endpoints? → A: Individual (every API endpoint checks exactly one specific permission).
- Q: How should the system handle requests if a required permission is unknown or missing? → A: Deny Access (Fail-safe).
- Q: What is the required timing for permission registration? → A: Synchronous (Registration must complete before the service accepts traffic).
- Q: How should failed authorization attempts be audited? → A: Structured Logs (Standard).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Full Access for Administrators (Priority: P1)

As a system administrator, I want full control over all supplier-related operations so that I can manage the entire supply chain data without restrictions.

**Why this priority**: Administrators need to have oversight and recovery capabilities for all aspects of the service.

**Independent Test**: Can be tested by verifying that a user with the `supplier-admin` role can perform any create, read, update, delete, approve, suspend, and export operation across suppliers, contacts, and performance metrics.

**Acceptance Scenarios**:

1. **Given** a user has the `supplier-admin` role, **When** they attempt to create a supplier or delete a contact, **Then** the action is permitted.
2. **Given** a user has the `supplier-admin` role, **When** they attempt to export supplier data, **Then** the export is successful.

---

### User Story 2 - Restricted Access for Coordinators (Priority: P2)

As a supplier coordinator, I want to manage supplier information and contacts but I should only be able to view performance metrics without the ability to modify them.

**Why this priority**: Coordinators are the primary users handling daily operations but shouldn't have authority to change performance ratings or approve/suspend suppliers.

**Independent Test**: Verify that a user with the `supplier-coordinator` role can update supplier info but receives an "Access Denied" error when trying to rate performance or approve a supplier.

**Acceptance Scenarios**:

1. **Given** a user has the `supplier-coordinator` role, **When** they update a supplier's contact details, **Then** the update is successful.
2. **Given** a user has the `supplier-coordinator` role, **When** they attempt to rate a supplier's performance, **Then** the system denies access.

---

### User Story 3 - Read-Only Access for Viewers (Priority: P3)

As a business viewer, I want to see supplier and contact details so that I can find information without risk of accidentally changing data.

**Why this priority**: Many stakeholders need data access for reporting and lookup without operational responsibilities.

**Independent Test**: Verify that a user with the `supplier-viewer` role can view all data but cannot trigger any "create" or "update" actions.

**Acceptance Scenarios**:

1. **Given** a user has the `supplier-viewer` role, **When** they view a supplier profile, **Then** the details are displayed.
2. **Given** a user has the `supplier-viewer` role, **When** they attempt to create a new contact, **Then** the system denies access.

### Edge Cases

- **Unauthorized User**: A user with no supplier roles should be blocked from all supplier endpoints.
- **Role Overlap**: If a user is assigned both `supplier-viewer` and `supplier-coordinator`, they should have the union of permissions (i.e., the coordinator's write access).
- **Unknown Permissions**: Any request requiring a permission that is not recognized or not assigned to the user MUST be denied by default.

## Assumptions & Dependencies

- **Existing Auth Framework**: It is assumed that an underlying authorization framework exists that can handle permission checks.
- **Identity Provider**: Role assignments are managed by an external or shared Identity Provider using the standard `IAMRegistrationService` from ServiceDefaults.
- **Service Name**: The service will identify itself as `supplier-service` during registration with the IAM system.
- **Service Scope**: This migration only affects the SupplierService.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST register 7 distinct supplier permissions: create, read, update, delete, approve, suspend, and export.
- **FR-002**: System MUST register 4 distinct contact permissions: create, read, update, and delete.
- **FR-003**: System MUST register 3 distinct performance permissions: view, rate, and report.
- **FR-004**: System MUST define the `supplier-admin` role with all 14 permissions.
- **FR-005**: System MUST define the `supplier-manager` role with create, read, update, approve, and suspend permissions for suppliers, plus all contact and performance permissions.
- **FR-006**: System MUST define the `supplier-coordinator` role with create, read, and update permissions for suppliers and contacts, plus performance viewing access.
- **FR-007**: System MUST define the `supplier-viewer` role with read-only access to suppliers, contacts, and performance metrics.
- **FR-008**: System MUST enforce granular permission checks on every API endpoint, ensuring each endpoint validates exactly one specific permission corresponding to the operation.
- **FR-009**: System MUST perform permission and role registration synchronously during service startup, blocking traffic until the authorization state is synchronized.
- **FR-010**: System MUST record all failed authorization attempts in structured logs, including User ID, requested Permission, and Timestamp.

### Key Entities *(include if feature involves data)*

- **Permission**: A granular action that can be performed, identified by a unique human-readable string (e.g., `supplier.suppliers.create`).
- **Role**: A collection of permissions assigned to a user group (e.g., `supplier-manager`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Exactly 14 new granular permissions are successfully registered in the authorization system.
- **SC-002**: All 4 predefined roles are available and correctly mapped to their specified permission sets.
- **SC-003**: 100% of API endpoints in the SupplierService enforce the new permission-based authorization.
- **SC-004**: Local unauthorized access attempts (evaluated against the issued JWT) are rejected in under 100ms.