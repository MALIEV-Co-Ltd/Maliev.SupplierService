# Multi-stage build for Supplier Service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy nuget.config and project files
COPY ["nuget.config", "."]
COPY ["Maliev.SupplierService.slnx", "."]
COPY ["Maliev.SupplierService.Api/Maliev.SupplierService.Api.csproj", "Maliev.SupplierService.Api/"]
COPY ["Maliev.SupplierService.Application/Maliev.SupplierService.Application.csproj", "Maliev.SupplierService.Application/"]
COPY ["Maliev.SupplierService.Domain/Maliev.SupplierService.Domain.csproj", "Maliev.SupplierService.Domain/"]
COPY ["Maliev.SupplierService.Infrastructure/Maliev.SupplierService.Infrastructure.csproj", "Maliev.SupplierService.Infrastructure/"]

# Restore with GitHub Packages authentication using BuildKit secrets
RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    NUGET_USERNAME=$(cat /run/secrets/nuget_username) \
    NUGET_PASSWORD=$(cat /run/secrets/nuget_password) \
    dotnet restore "./Maliev.SupplierService.Api/Maliev.SupplierService.Api.csproj"

# Copy source code
COPY ["Maliev.SupplierService.Api/", "Maliev.SupplierService.Api/"]
COPY ["Maliev.SupplierService.Application/", "Maliev.SupplierService.Application/"]
COPY ["Maliev.SupplierService.Domain/", "Maliev.SupplierService.Domain/"]
COPY ["Maliev.SupplierService.Infrastructure/", "Maliev.SupplierService.Infrastructure/"]

# Build the application
WORKDIR /src/Maliev.SupplierService.Api
RUN dotnet build "Maliev.SupplierService.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Maliev.SupplierService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application (now owned by app)
COPY --chown=app:app --from=publish /app/publish .

# Change ownership to the built-in 'app' user
RUN chown -R app:app /app

# Switch to the non-root 'app' user (already exists in ASP.NET runtime image)
USER app

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/supplier/liveness || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Maliev.SupplierService.Api.dll"]
