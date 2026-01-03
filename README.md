# Maliev.SupplierService

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/MALIEV-Co-Ltd/Maliev.SupplierService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/PostgreSQL-18-336791.svg)](https://www.postgresql.org/)
[![Tests](https://img.shields.io/badge/tests-36%2F36%20passing-brightgreen.svg)](https://github.com/MALIEV-Co-Ltd/Maliev.SupplierService)
[![License](https://img.shields.io/badge/license-Proprietary-red.svg)](LICENSE)

Comprehensive supplier lifecycle management system for the MALIEV platform. Handles supplier onboarding, qualification, performance evaluation, certification tracking, and relationship management with full IAM integration and event-driven communication via MessagingContracts.

---

## Architecture & Tech Stack

### Technology Stack
- **.NET 10.0**: ASP.NET Core Web API with C# 13
- **PostgreSQL 18**: Primary database with Entity Framework Core 10.x
- **Redis**: Distributed caching for frequently accessed supplier data
- **RabbitMQ**: Event-driven messaging via MassTransit 8.5.7
- **OpenTelemetry**: Structured logging, metrics, and distributed tracing
- **Testcontainers**: Integration testing with real PostgreSQL, Redis, RabbitMQ

### Project Structure
```
Maliev.SupplierService/
├── Maliev.SupplierService.Api/          # Presentation layer
│   ├── Controllers/                     # REST API endpoints
│   ├── Services/                        # Business logic
│   ├── Models/                          # DTOs (Request/Response)
│   └── Consumers/                       # MassTransit event consumers
├── Maliev.SupplierService.Data/         # Data access layer
│   ├── Entities/                        # EF Core entities
│   ├── Configurations/                  # Entity configurations
│   ├── Repositories/                    # Repository implementations
│   └── Migrations/                      # Database migrations
└── Maliev.SupplierService.Tests/        # Integration tests
    ├── Integration/                     # API integration tests
    └── Testing/                         # Test infrastructure
```

### Dependencies

**Databases:**
- **PostgreSQL 18**: Supplier master data, evaluations, certifications, contacts, audit trail
- **Redis**: Caching for frequently accessed supplier information and evaluation scores

**Messaging:**
- **RabbitMQ**: Event publishing (supplier lifecycle events) and consumption (purchase order updates)

**External Services:**
- **IAM Service**: Authentication, authorization, and permission management
- **Purchase Order Service**: Supplier order history and performance metrics
- **Accounting Service**: Financial records, payment history, and credit status

---

## ⚠️ Constitution Rules

These rules are **non-negotiable** and apply to ALL Maliev microservices:

### Banned Libraries
| ❌ BANNED | ✅ USE INSTEAD |
|-----------|----------------|
| AutoMapper | Explicit manual mapping |
| FluentValidation | Data Annotations (`[Required]`, `[StringLength]`, etc.) |
| FluentAssertions | xUnit `Assert.*` methods |
| In-memory test DB | Testcontainers (real PostgreSQL) |
| `/src` or `/tests` folders | Flat project structure at repo root |

### Mandatory Practices
- **No Secrets in Code**: All secrets injected via Google Secret Manager (environment variables)
- **TreatWarningsAsErrors**: Enabled in all `.csproj` files - zero warnings tolerated
- **XML Documentation**: Required on ALL public methods, properties, and classes
- **MessagingContracts Only**: ALL events use `Maliev.MessagingContracts` package (no local events)
- **ServiceDefaults Integration**: Use `Maliev.Aspire.ServiceDefaults` for infrastructure patterns

---

## Key Features

### Supplier Lifecycle Management
- **Registration**: Capture supplier basic information, tax ID, legal entity details
- **Onboarding Workflow**: Multi-stage qualification process (Application → Review → Approval)
- **Due Diligence**: Background checks, financial verification, compliance validation
- **Approval Process**: Multi-level approval based on supplier category and risk level
- **Deactivation**: Controlled supplier suspension and termination with audit trail

### Supplier Onboarding Workflow
```
Application → Under Review → Background Check → Financial Review → Approved/Rejected
                                                                         ↓
                                                                    Active/Blocked
```

**Status Transitions:**
- **Application**: Supplier submitted registration
- **Under Review**: Initial document verification
- **Background Check**: Legal and compliance verification
- **Financial Review**: Credit and financial stability assessment
- **Approved**: Qualified for procurement
- **Rejected**: Did not meet qualification criteria
- **Active**: Currently doing business
- **Blocked**: Temporarily or permanently suspended

### Performance Evaluation System
- **Multi-Criteria Scorecards**: Delivery performance, quality, pricing competitiveness, service responsiveness
- **Weighted Scoring**: Configurable weights for different evaluation criteria
- **Trend Analysis**: Track supplier performance over time
- **Risk Assessment**: Automatic risk categorization based on evaluation scores
- **Corrective Actions**: Issue and track supplier improvement plans

### Certification Management
- **Quality Certifications**: ISO 9001, ISO 14001, industry-specific standards
- **Compliance Documents**: Business licenses, tax registrations, insurance certificates
- **Expiration Tracking**: Automated alerts for certificate renewals
- **Document Storage**: Integration with UploadService for certificate files
- **Audit History**: Track all certification changes and renewals

### Contact Management
- **Multiple Contacts**: Sales, technical support, finance, compliance contacts per supplier
- **Role Assignment**: Define contact roles and responsibilities
- **Communication Log**: Track all interactions with supplier contacts
- **Primary Contact**: Designated main point of contact for each category

### Event-Driven Integration
- **Events Published** (via MessagingContracts):
  - `SupplierCreatedEvent` - New supplier registered
  - `SupplierApprovedEvent` - Supplier approved for procurement
  - `SupplierUpdatedEvent` - Supplier information modified
  - `SupplierEvaluatedEvent` - Performance evaluation completed
  - `SupplierDeactivatedEvent` - Supplier suspended or terminated
  - `SupplierCertificationExpiredEvent` - Certification nearing expiration

- **Events Consumed**:
  - `PurchaseOrderCreatedEvent` - Track supplier order volume
  - `GoodsReceivedEvent` - Update delivery performance metrics
  - `PaymentCompletedEvent` - Update financial relationship status

---

## Quick Start

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 18 (local or via Kubernetes port-forward)
- Redis (optional, for caching)
- RabbitMQ (optional, for event messaging)

### Local Development

1. **Clone Repository**
   ```bash
   git clone https://github.com/MALIEV-Co-Ltd/Maliev.SupplierService.git
   cd Maliev.SupplierService
   ```

2. **Configure Database Connection**
   ```bash
   # Set connection string environment variable
   export ConnectionStrings__SupplierDbContext="Host=localhost;Port=5432;Database=supplier_app_db;Username=postgres;Password=<password>;"
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update --project Maliev.SupplierService.Data
   ```

4. **Run the Service**
   ```bash
   cd Maliev.SupplierService.Api
   dotnet run
   ```

5. **Access API Documentation**
   - Scalar UI: http://localhost:5000/supplier/scalar
   - OpenAPI Spec: http://localhost:5000/supplier/openapi/v1.json
   - Health Check: http://localhost:5000/supplier/readiness

### Docker Deployment

```bash
# Build image
docker build -t maliev/supplier-service:latest .

# Run container
docker run -p 8080:8080 \
  -e ConnectionStrings__SupplierDbContext="Host=postgres;Port=5432;Database=supplier_app_db;..." \
  -e Jwt__PublicKey="<base64-encoded-public-key>" \
  maliev/supplier-service:latest
```

---

## API Endpoints

All endpoints are prefixed with `/supplier` (configured via `UsePathBase("/supplier")`):

### Suppliers
- `GET /supplier/v1/suppliers` - List suppliers (paginated, filterable)
- `POST /supplier/v1/suppliers` - Create new supplier
- `GET /supplier/v1/suppliers/{id}` - Get supplier details
- `PUT /supplier/v1/suppliers/{id}` - Update supplier information
- `DELETE /supplier/v1/suppliers/{id}` - Deactivate supplier
- `GET /supplier/v1/suppliers/{id}/performance` - Get performance summary
- `GET /supplier/v1/suppliers/search` - Search suppliers by name, tax ID

### Supplier Onboarding
- `GET /supplier/v1/supplier-onboarding` - List onboarding requests
- `POST /supplier/v1/supplier-onboarding/initiate` - Start onboarding process
- `GET /supplier/v1/supplier-onboarding/{id}` - Get onboarding status
- `POST /supplier/v1/supplier-onboarding/{id}/approve` - Approve supplier
- `POST /supplier/v1/supplier-onboarding/{id}/reject` - Reject supplier
- `POST /supplier/v1/supplier-onboarding/{id}/request-info` - Request additional information

### Supplier Evaluations
- `GET /supplier/v1/supplier-evaluations` - List all evaluations
- `POST /supplier/v1/supplier-evaluations` - Create new evaluation
- `GET /supplier/v1/supplier-evaluations/{id}` - Get evaluation details
- `GET /supplier/v1/supplier-evaluations/supplier/{supplierId}` - Get supplier's evaluations
- `PUT /supplier/v1/supplier-evaluations/{id}` - Update evaluation scores

### Supplier Certifications
- `GET /supplier/v1/supplier-certifications` - List certifications
- `POST /supplier/v1/supplier-certifications` - Add certification
- `GET /supplier/v1/supplier-certifications/{id}` - Get certification details
- `GET /supplier/v1/supplier-certifications/supplier/{supplierId}` - Get supplier certifications
- `DELETE /supplier/v1/supplier-certifications/{id}` - Remove certification
- `POST /supplier/v1/supplier-certifications/{id}/renew` - Renew certification

### Supplier Contacts
- `GET /supplier/v1/supplier-contacts/supplier/{supplierId}` - List supplier contacts
- `POST /supplier/v1/supplier-contacts` - Add new contact
- `GET /supplier/v1/supplier-contacts/{id}` - Get contact details
- `PUT /supplier/v1/supplier-contacts/{id}` - Update contact information
- `DELETE /supplier/v1/supplier-contacts/{id}` - Remove contact

### Supplier Audit
- `GET /supplier/v1/supplier-audit/supplier/{supplierId}` - Get audit trail for supplier
- `GET /supplier/v1/supplier-audit/changes` - Query all supplier changes (filtered)

---

## Health & Monitoring

### Health Endpoints
- **Liveness**: `GET /supplier/liveness` - Service is running
- **Readiness**: `GET /supplier/readiness` - Service is ready (DB + dependencies healthy)

### Observability
- **Metrics**: Prometheus metrics at `/supplier/metrics`
- **Tracing**: OpenTelemetry distributed tracing to configured OTLP endpoint
- **Logging**: Structured logging with correlation IDs via ServiceDefaults

### Health Check Components
- PostgreSQL connection
- Redis cache availability
- RabbitMQ connection
- External service connectivity (IAM, PurchaseOrder, Accounting)

---

## Configuration

### Required Secrets (Google Secret Manager)
```
ConnectionStrings__SupplierDbContext - PostgreSQL connection string
Jwt__PublicKey  - Base64-encoded RSA-2048 public key (PEM format)
```

### Environment Variables
```bash
ConnectionStrings__SupplierDbContext="Host=postgres;Port=5432;Database=supplier_app_db;Username=app;Password=..."
ConnectionStrings__redis="redis:6379"
ConnectionStrings__rabbitmq="amqp://guest:guest@rabbitmq:5672"
Jwt__Issuer="https://dev.api.maliev.com/auth"
Jwt__Audience="https://dev.api.maliev.com"
ExternalServices__IAMService__BaseUrl="http://iam-service:8080"
ExternalServices__PurchaseOrderService__BaseUrl="http://purchase-order-service:8080"
ExternalServices__AccountingService__BaseUrl="http://accounting-service:8080"
```

### Configuration Files
- `appsettings.json` - Production settings (no secrets)
- `appsettings.Development.json` - Local development overrides
- `appsettings.Testing.json` - Test configuration with test keys

---

## IAM Integration

### Required Permissions
- `suppliers.read` - View supplier information
- `suppliers.write` - Create and update suppliers
- `suppliers.delete` - Deactivate suppliers
- `suppliers.onboard` - Manage supplier onboarding workflow
- `suppliers.approve` - Approve new suppliers
- `suppliers.evaluate` - Create and manage supplier evaluations
- `suppliers.certifications.read` - View supplier certifications
- `suppliers.certifications.write` - Manage supplier certifications
- `suppliers.contacts.read` - View supplier contacts
- `suppliers.contacts.write` - Manage supplier contacts
- `suppliers.audit.read` - View supplier audit trails

### Predefined Roles
- **Procurement Manager**: Full supplier management access (`suppliers.*`)
- **Buyer**: Create/update suppliers and contacts (`suppliers.write`, `suppliers.read`, `suppliers.contacts.*`)
- **Quality Manager**: Evaluation and certification management (`suppliers.evaluate`, `suppliers.certifications.*`, `suppliers.read`)
- **Onboarding Specialist**: Manage onboarding workflow (`suppliers.onboard`, `suppliers.approve`, `suppliers.read`)
- **Auditor**: Read-only access to all supplier data and audit trails (`suppliers.read`, `suppliers.audit.read`)

---

## Testing

### Test Coverage
**36/36 integration tests passing (100%)**

Test suites:
- **SuppliersController Tests** (12 tests)
  - CRUD operations (create, read, update, delete)
  - Search and filtering
  - Authorization checks (permission-based)
- **SupplierOnboardingController Tests** (8 tests)
  - Onboarding workflow (initiate, approve, reject)
  - Status transitions
- **SupplierEvaluationsController Tests** (7 tests)
  - Evaluation creation and scoring
  - Performance metrics calculations
- **SupplierCertificationsController Tests** (5 tests)
  - Certification management and expiration tracking
- **SupplierContactsController Tests** (3 tests)
  - Contact CRUD operations
- **IAM Registration Tests** (1 test)
  - Permission registration with IAM service on startup

### Running Tests

```bash
# Run all tests
dotnet test Maliev.SupplierService.sln --verbosity normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~SuppliersControllerTests"
```

### Test Infrastructure
- **Testcontainers**: Real PostgreSQL 18, Redis, RabbitMQ containers
- **WireMock.Net**: Mock external services (IAM, PurchaseOrder, Accounting)
- **MassTransit Test Harness**: Verify event publishing and consumption
- **xUnit**: Test framework with `Assert.*` assertions

---

## Database

### Database Schema

**PostgreSQL 18** with Entity Framework Core migrations.

**Main Tables:**
- `Suppliers` - Supplier master data (name, tax ID, address, status, risk category)
- `SupplierContacts` - Contact persons (name, role, email, phone)
- `SupplierEvaluations` - Performance evaluations (scores, comments, evaluator)
- `SupplierCertifications` - Quality/compliance certifications (type, issuer, expiry)
- `SupplierOnboarding` - Onboarding workflow tracking (status, approver, timestamps)
- `SupplierAuditTrail` - Complete audit history (action, user, timestamp, changes)

**Supplier Status Values:**
- `Application`, `UnderReview`, `BackgroundCheck`, `FinancialReview`, `Approved`, `Rejected`, `Active`, `Blocked`

**Evaluation Criteria:**
- Delivery Performance (on-time delivery rate)
- Quality (defect rate, compliance)
- Pricing Competitiveness (cost vs market)
- Service Responsiveness (communication, issue resolution)

### Database Migrations

```bash
# Port forward to PostgreSQL pod (MUST use pod, not service)
kubectl port-forward -n maliev-dev postgres-cluster-1 5432:5432

# Set connection string environment variable
export ConnectionStrings__SupplierDbContext="Host=localhost;Port=5432;Database=supplier_app_db;Username=postgres;Password=<password>;"

# Create migration
dotnet ef migrations add MigrationName --project Maliev.SupplierService.Data

# Apply migration
dotnet ef database update --project Maliev.SupplierService.Data

# Rollback migration
dotnet ef database update PreviousMigrationName --project Maliev.SupplierService.Data
```

---

## Deployment

### Kubernetes Deployment

Service uses GitHub Actions workflows:
- `ci-develop.yml` - Deploy to `maliev-dev` namespace
- `ci-staging.yml` - Deploy to `maliev-staging` namespace
- `ci-main.yml` - Deploy to `maliev-prod` namespace

Deployments are managed via GitOps (ArgoCD) in the `maliev-gitops` repository.

### Port Forwarding

```bash
# Forward to service
kubectl port-forward -n maliev-dev svc/maliev-supplier-service 8080:8080

# Forward to PostgreSQL (for migrations)
kubectl port-forward -n maliev-dev postgres-cluster-1 5432:5432

# Forward to Redis
kubectl port-forward -n maliev-dev svc/redis 6379:6379
```

### Logs

```bash
# Tail logs
kubectl logs -f deployment/maliev-supplier-service -n maliev-dev

# Get pod status
kubectl get pods -n maliev-dev | grep supplier-service

# Describe pod
kubectl describe pod <pod-name> -n maliev-dev
```

---

## Common Issues

### Issue: Tests fail with "Database connection string not configured"
**Solution**: Set `ConnectionStrings__SupplierDbContext` environment variable before running tests, or configure via User Secrets:
```bash
cd Maliev.SupplierService.Tests
dotnet user-secrets set "ConnectionStrings:SupplierDbContext" "Host=localhost;Port=5432;..."
```

### Issue: Migration fails with "Cannot connect to database"
**Solution**: Ensure PostgreSQL is accessible. If using Kubernetes, port-forward to the pod (NOT service):
```bash
kubectl port-forward -n maliev-dev postgres-cluster-1 5432:5432
```

### Issue: Scalar UI returns 404
**Solution**: Scalar is disabled in production. Check environment is Development or Staging.

### Issue: All JWT validations fail
**Solution**: Ensure `Jwt:PublicKey` is correctly configured in Google Secret Manager and matches the AuthService private key.

### Issue: Events not publishing
**Solution**: Verify RabbitMQ connection string is configured and MessagingContracts package is up-to-date.

### Issue: Supplier evaluation scores not calculating
**Solution**: Ensure evaluation criteria weights are configured in `appsettings.json` and sum to 100.

---

## Development Guidelines

### Adding New Endpoints
1. Create request/response models in `Models/`
2. Add validators using Data Annotations
3. Implement service logic in `Services/`
4. Add controller action in `Controllers/`
5. Write integration tests in `Tests/Integration/`
6. Update API documentation in README.md

### Adding New Database Entities
1. Create entity class in `Data/Entities/`
2. Add EF Core configuration in `Data/Configurations/`
3. Update `SupplierDbContext.cs` with DbSet
4. Create migration: `dotnet ef migrations add EntityName --project Maliev.SupplierService.Data`
5. Apply migration: `dotnet ef database update --project Maliev.SupplierService.Data`

### Code Style
- Follow .NET naming conventions
- Use async/await for all I/O operations
- Implement repository pattern for data access
- Use dependency injection for all services
- Add XML documentation comments for public APIs
- Include structured logging with correlation IDs

---

## Support

- **CLAUDE.md**: Service-specific development guidelines
- **ServiceDefaults Documentation**: `B:\maliev\Maliev.Aspire\Maliev.Aspire.ServiceDefaults\README.md`
- **MessagingContracts**: `B:\maliev\Maliev.MessagingContracts\README.md`
- **Test Summary**: `B:\maliev\all-services-test-summary.txt`

---

## License

**Proprietary** - Copyright © 2025 MALIEV Co., Ltd. All rights reserved.
