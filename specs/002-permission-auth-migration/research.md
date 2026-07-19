# Research: Permission-Based Authorization Migration

## Decision 1: Authorization Framework Integration
- **Decision**: Leverage standard ASP.NET Core Policy-Based Authorization.
- **Rationale**: It aligns with .NET best practices and the requirement for granular, per-endpoint checks. Each permission string (e.g., `supplier.suppliers.create`) will map to a named Policy.
- **Alternatives considered**: 
    - Manual checks in every controller method (Rejected: Too verbose and error-prone).
    - Middleware-based generic check (Rejected: Hard to map specific endpoints to granular permissions without metadata).

## Decision 2: Synchronous Registration Timing
- **Decision**: Implement registration logic in `Program.cs` after the `WebApplication` is built but before `app.Run()` is called.
- **Rationale**: The specification requires registration to be synchronous and block traffic until complete. Executing it during the startup sequence ensures the service is ready before the first request arrives.
- **Alternatives considered**:
    - `IHostedService` (Rejected: Runs in the background and doesn't naturally block the startup of the web host in a way that prevents initial requests).
    - `BackgroundService` (Rejected: Asynchronous).

## Decision 3: IAM Client Communication
- **Decision**: Use a typed `HttpClient` or a dedicated `IIamClient` to communicate with the central Identity Provider for permission/role registration.
- **Rationale**: Follows the "Explicit Contracts" and "Service Autonomy" principles of the constitution.
- **Alternatives considered**:
    - Direct database access to IAM service (Rejected: Violates Service Autonomy).

## Decision 4: Audit Logging Implementation
- **Decision**: Use the built-in `ILogger` with structured logging (JSON) to record failed authorization attempts.
- **Rationale**: Complies with Constitution Principle V (Auditability & Observability) and Clarification Q5.
- **Alternatives considered**:
    - Custom `IAuditService` writing to a database (Rejected: Spec specifically asked for structured logs).

## Decision 5: Role Assignment Assumption
- **Decision**: The SupplierService will define the *meaning* of roles (which permissions they contain) and register these definitions with the Identity Provider. User-to-Role assignments remain the responsibility of the central Identity Provider.
- **Rationale**: Aligns with the Assumption in the specification regarding the Identity Provider.
