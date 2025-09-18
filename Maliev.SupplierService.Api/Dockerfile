FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Maliev.SupplierService.Api/Maliev.SupplierService.Api.csproj", "Maliev.SupplierService.Api/"]
COPY ["Maliev.SupplierService.Data/Maliev.SupplierService.Data.csproj", "Maliev.SupplierService.Data/"]
RUN dotnet restore "./Maliev.SupplierService.Api/Maliev.SupplierService.Api.csproj"
COPY . .
WORKDIR "/src/Maliev.SupplierService.Api"
RUN dotnet build "./Maliev.SupplierService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Maliev.SupplierService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Maliev.SupplierService.Api.dll"]