# Maliev Supplier Service

Comprehensive supplier lifecycle management system for the MALIEV platform, handling supplier onboarding, evaluation, certification, and relationship management with full IAM integration.

## Service Description

The Supplier Service manages all aspects of supplier relationships including registration, qualification, performance evaluation, certifications, contacts, and audit trails. It integrates with Procurement and Accounting services to provide a complete supplier management solution.

## Architecture Overview

### Project Structure
```
Maliev.SupplierService/
├── Maliev.SupplierService.Api/          # Presentation layer
│   ├── Controllers/                     # REST API endpoints
│   ├── Services/                        # Business logic
│   └── Models/                          # DTOs
├── Maliev.SupplierService.Data/         # Data access layer
│   ├── Entities/                        # EF Core entities
│   ├── Repositories/                    # Repository implementations
│   └── Migrations/                      # Database migrations
└── Maliev.SupplierService.Tests/        # Integration tests
```

## Technologies Used

- **.NET 10.0** - Runtime and framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL 18** - Relational database
- **Redis** - Distributed caching
- **RabbitMQ** - Message queue via MassTransit
- **OpenTelemetry** - Observability (metrics, logs, traces)
- **xUnit** - Testing framework

## Dependencies

### Databases
- **PostgreSQL**: Supplier master data, evaluations, certifications, contacts
- **Redis**: Caching for frequently accessed supplier data

### Messaging
- **RabbitMQ**: Event publishing for supplier changes and evaluations

### External Services
- **IAM Service**: Authentication and authorization
- **Purchase Order Service**: Supplier order history
- **Accounting Service**: Financial records and payment history

## IAM Integration

### Required Permissions
- `suppliers.read` - View supplier information
- `suppliers.write` - Create and update suppliers
- `suppliers.delete` - Delete/deactivate suppliers
- `suppliers.onboard` - Manage supplier onboarding workflow
- `suppliers.evaluate` - Create and manage supplier evaluations
- `suppliers.certifications.read` - View supplier certifications
- `suppliers.certifications.write` - Manage supplier certifications
- `suppliers.contacts.read` - View supplier contacts
- `suppliers.contacts.write` - Manage supplier contacts
- `suppliers.audit.read` - View supplier audit trails

### Predefined Roles
- **Procurement Manager**: Full supplier management access
- **Buyer**: Read/write access to suppliers and contacts
- **Quality Manager**: Evaluation and certification management
- **Auditor**: Read-only access to all supplier data and audit trails

## API Endpoints

### Core Controllers
- **SuppliersController** (`/v1/suppliers`) - Supplier CRUD operations
- **SupplierOnboardingController** (`/v1/supplier-onboarding`) - Onboarding workflow
- **SupplierEvaluationsController** (`/v1/supplier-evaluations`) - Performance evaluations
- **SupplierCertificationsController** (`/v1/supplier-certifications`) - Quality certifications
- **SupplierContactsController** (`/v1/supplier-contacts`) - Contact management
- **SupplierAuditController** (`/v1/supplier-audit`) - Audit trail queries

### Key Operations
- `GET /v1/suppliers` - List suppliers with filtering
- `POST /v1/suppliers` - Create new supplier
- `GET /v1/suppliers/{id}` - Get supplier details
- `PUT /v1/suppliers/{id}` - Update supplier
- `DELETE /v1/suppliers/{id}` - Deactivate supplier
- `POST /v1/supplier-onboarding/initiate` - Start onboarding process
- `POST /v1/supplier-evaluations` - Create supplier evaluation
- `GET /v1/supplier-evaluations/supplier/{supplierId}` - Get supplier evaluations

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "SupplierDatabase": "Host=postgres;Port=5432;Database=maliev_suppliers;Username=app;Password=secret",
    "Redis": "redis:6379"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "maliev-supplier-service",
    "Audience": "maliev-services"
  },
  "ExternalServices": {
    "IAM": {
      "BaseUrl": "http://iam-service:8080"
    }
  }
}
```

## Database

**PostgreSQL 18** with Entity Framework Core migrations.

**Main Tables:**
- `Suppliers` - Supplier master data (name, tax ID, address, status)
- `SupplierContacts` - Contact persons
- `SupplierEvaluations` - Performance evaluations and scores
- `SupplierCertifications` - Quality/compliance certifications
- `SupplierOnboarding` - Onboarding workflow tracking
- `SupplierAuditTrail` - Complete audit history

## Running the Service

### Development
```bash
# Run the service
cd Maliev.SupplierService.Api
dotnet run
```

**Access:**
- API: http://localhost:5000
- Health: http://localhost:5000/suppliers/liveness
- Metrics: http://localhost:5000/suppliers/metrics

### Docker
```bash
docker build -t maliev/supplier-service:latest .
docker run -p 8080:8080 maliev/supplier-service:latest
```

### Tests
```bash
# Run all tests
dotnet test
```

## Test Status

**From Test Summary (2025-12-24):**
- **Status**: FAILED (30 tests)
- **Critical Issues**:
  - Authorization checks not properly enforced (expected Forbidden, got Created/NotFound)
  - Data validation returning Forbidden instead of BadRequest

**Issues to Fix:**
1. Implement proper authorization middleware
2. Separate authorization checks from data validation
3. Return correct HTTP status codes (400 for bad data, 403 for auth failures)

## Key Features

- **Supplier Lifecycle Management**: Complete onboarding workflow from application to approval
- **Performance Evaluation**: Scorecards for delivery, quality, pricing, service
- **Certification Tracking**: ISO, quality standards, compliance certifications
- **Contact Management**: Multiple contacts per supplier with roles
- **Audit Trail**: Complete history of all supplier changes
- **Integration**: Events published for supplier changes, evaluations, approvals

## Events Published

- `SupplierCreatedEvent` - New supplier registered
- `SupplierApprovedEvent` - Supplier approved for procurement
- `SupplierEvaluatedEvent` - Evaluation completed
- `SupplierDeactivatedEvent` - Supplier deactivated/blocked

## Support

- Test Summary: `B:\maliev\all-services-test-summary.txt`
- ServiceDefaults: `B:\maliev\Maliev.Aspire\Maliev.Aspire.ServiceDefaults\README.md`

## License

Proprietary - Copyright 2025 MALIEV Co., Ltd. All rights reserved.
