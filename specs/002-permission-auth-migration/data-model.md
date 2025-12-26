# Data Model: Permission-Based Authorization

This document describes the logical entities used for the authorization migration. These entities are used for registration with the central Identity Provider and for internal constant definitions.

## Entities

### Permission
Represents a granular action that can be performed within the SupplierService.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Identifier | String | Unique human-readable string (e.g., `supplier.suppliers.create`) | Required, must follow `[service].[resource].[action]` pattern |
| Description | String | Human-readable explanation of the permission | Required |

### Role
Represents a collection of permissions grouped for assignment to users.

| Field | Type | Description | Validation |
|-------|------|-------------|------------|
| Name | String | Unique name of the role (e.g., `supplier-admin`) | Required |
| Permissions | List<String> | List of permission identifiers included in this role | Required, must contain valid Permission identifiers |

## Permission Definitions

### Supplier Resource
- `supplier.suppliers.create`: Create new suppliers
- `supplier.suppliers.read`: Read supplier details
- `supplier.suppliers.update`: Update supplier information
- `supplier.suppliers.delete`: Delete suppliers
- `supplier.suppliers.approve`: Approve new suppliers
- `supplier.suppliers.suspend`: Suspend suppliers
- `supplier.suppliers.export`: Export supplier data

### Contact Resource
- `supplier.contacts.create`: Create supplier contacts
- `supplier.contacts.read`: Read contact details
- `supplier.contacts.update`: Update contacts
- `supplier.contacts.delete`: Delete contacts

### Performance Resource
- `supplier.performance.view`: View supplier performance metrics
- `supplier.performance.rate`: Rate supplier performance
- `supplier.performance.report`: Generate performance reports

## Role Mapping

| Role | Permissions |
|------|-------------|
| `supplier-admin` | All 14 permissions |
| `supplier-manager` | `supplier.suppliers.{create,read,update,approve,suspend}`, all `supplier.contacts.*`, all `supplier.performance.*` |
| `supplier-coordinator` | `supplier.suppliers.{create,read,update}`, all `supplier.contacts.*`, `supplier.performance.view` |
| `supplier-viewer` | `supplier.suppliers.read`, `supplier.contacts.read`, `supplier.performance.view` |
