using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Data.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;

namespace Maliev.SupplierService.Tests.Integration;

public class CertificationsControllerTests : BaseIntegrationTest
{
    public CertificationsControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddCertification_WithValidData_Returns201()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        var request = new CreateCertificationRequest(
            DocumentType: CertificationType.BusinessLicense,
            DocumentName: "Business License 2024",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            ExternalFileRef: "https://storage.example.com/docs/license.pdf",
            Notes: "Annual business license"
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await GetResponseAsync<CertificationResponse>(response);
        Assert.NotNull(result);
        Assert.Equal("Business License 2024", result!.DocumentName);
        Assert.False(result.IsExpired);
        Assert.False(result.IsExpiringSoon);
    }

    [Fact]
    public async Task AddCertification_WithExpiredDate_ShowsExpired()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        var request = new CreateCertificationRequest(
            DocumentType: CertificationType.TaxForm,
            DocumentName: "Expired Tax Form",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            ExternalFileRef: null,
            Notes: null
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await GetResponseAsync<CertificationResponse>(response);
        Assert.True(result!.IsExpired);
    }

    [Fact]
    public async Task AddCertification_NonExistingSupplier_Returns404()
    {
        // Arrange
        var request = new CreateCertificationRequest(
            DocumentType: CertificationType.BusinessLicense,
            DocumentName: "Test",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate: null,
            ExternalFileRef: null,
            Notes: null
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{Guid.NewGuid()}/certifications", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExpiringCertifications_ReturnsCertificationsWithinThreshold()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Add certification expiring in 15 days
        var expiringRequest = new CreateCertificationRequest(
            DocumentType: CertificationType.InsuranceCertificate,
            DocumentName: "Expiring Insurance",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            ExternalFileRef: null,
            Notes: null
        );
        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications", expiringRequest);

        // Add certification expiring in 60 days (outside 30-day threshold)
        var notExpiringRequest = new CreateCertificationRequest(
            DocumentType: CertificationType.QualityCertification,
            DocumentName: "Valid Certification",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
            ExternalFileRef: null,
            Notes: null
        );
        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications", notExpiringRequest);

        // Act
        var response = await Client.GetAsync("/suppliers/v1/certifications/expiring?days=30");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<ExpiringCertificationsListResponse>(response);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal("Expiring Insurance", result.Items[0].DocumentName);
        Assert.True(result.Items[0].DaysUntilExpiration <= 30);
    }

    [Fact]
    public async Task DeleteCertification_ExistingId_Returns204()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        var createRequest = new CreateCertificationRequest(
            DocumentType: CertificationType.TaxForm,
            DocumentName: "To Delete",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate: null,
            ExternalFileRef: null,
            Notes: null
        );

        var createResponse = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications", createRequest);
        var certification = await GetResponseAsync<CertificationResponse>(createResponse);

        // Act
        var response = await Client.DeleteAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/certifications/{certification!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
