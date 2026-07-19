# Quickstart: Permission-Based Authorization

This guide provides instructions on how to verify the authorization migration.

## Prerequisites
- .NET 10 SDK
- Docker (for Testcontainers)

## Verification Steps

### 1. Permission Registration
Check the service logs during startup to ensure synchronous registration completes successfully.
```bash
# Look for logs like:
# [INFO] Registering 14 permissions with IAM...
# [INFO] Registering 4 roles with IAM...
# [INFO] Authorization registration complete.
```

### 2. Integration Tests
Run the updated integration tests to verify enforcement.
```bash
dotnet test Maliev.SupplierService.Tests --filter "Category=Authorization"
```

### 3. Manual Verification (via Swagger/Scalar)
1. Start the service.
2. Obtain a JWT for a user with the `supplier-viewer` role.
3. Attempt to `POST /suppliers`.
4. Verify you receive a `403 Forbidden`.
5. Verify the structured logs contain the failure event:
   ```json
   {
     "Event": "AuthorizationFailed",
     "UserId": "...",
     "Permission": "supplier.suppliers.create",
     "Timestamp": "..."
   }
   ```
