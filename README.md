# Maliev Supplier Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.SupplierService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Comprehensive supplier lifecycle and relationship management system for the Maliev ecosystem.

**Role in MALIEV Architecture**: The authoritative engine for vendor management. It orchestrates the complete journey from initial onboarding and qualification to continuous performance evaluation and certification tracking, ensuring a high-quality supply chain for manufacturing excellence.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (High-frequency profile resolution)
- **Messaging**: RabbitMQ via MassTransit
- **Evaluation Engine**: Automated performance scorecard and risk assessment logic
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

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
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **End-to-End Onboarding**: Structured multi-stage qualification workflow covering initial application, due diligence, and final approval.
- **Precision Performance Scorecards**: Automated evaluation of delivery metrics, quality standards, and responsiveness based on real transaction data.
- **Active Certification Tracking**: Vigilant management of ISO standards, business licenses, and compliance documents with automated expiration alerting.
- **Granular Relationship Management**: Integrated contact directory and interaction logging for sales, technical, and finance representatives.
- **Immutable Audit History**: Complete, high-fidelity logging of all supplier status transitions and profile modifications for enterprise compliance.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.SupplierService.git
cd Maliev.SupplierService
```

2. **Spin up Infrastructure**
```bash
docker run --name supplier-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name supplier-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__SupplierDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.SupplierService.Data
dotnet run --project Maliev.SupplierService.Api
```

The service will be available at `http://localhost:5000/supplier`. Access the interactive documentation at `http://localhost:5000/supplier/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/supplier/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/suppliers` | Search and filter the qualified supplier registry |
| POST | `/supplier-onboarding/initiate` | Start a new vendor qualification process |
| GET | `/supplier-evaluations` | Access supplier performance and risk metrics |
| POST | `/supplier-certifications` | Register or renew a quality/compliance certificate |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /supplier/liveness`
- **Readiness**: `GET /supplier/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /supplier/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-supplier-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
