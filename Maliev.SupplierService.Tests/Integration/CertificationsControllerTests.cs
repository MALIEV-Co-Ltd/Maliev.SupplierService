using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
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
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            ExternalFileRef: "https://example.com/license.pdf",
            Notes: "Verified"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var certification = await GetResponseAsync<CertificationResponse>(response);
        Assert.NotNull(certification);
        Assert.Equal("Business License 2024", certification!.DocumentName);
    }

    [Fact]
    public async Task GetCertifications_ReturnsList()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateCertificationRequest(
            DocumentType: CertificationType.TaxForm,
            DocumentName: "Tax Form 2023",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            ExternalFileRef: null,
            Notes: null
        );
        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications", request);

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<List<CertificationResponse>>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result!);
    }

    [Fact]
    public async Task DeleteCertification_Returns204()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateCertificationRequest(
            DocumentType: CertificationType.BusinessLicense,
            DocumentName: "To Delete",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate: null,
            ExternalFileRef: null,
            Notes: null
        );
        var createResponse = await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications", request);
        var certification = await GetResponseAsync<CertificationResponse>(createResponse);

        // Act
        var response = await Client.DeleteAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications/{certification!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetExpiringCertifications_ReturnsExpectedItems()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        
        // One expiring soon (10 days)
        var expiringRequest = new CreateCertificationRequest(
            DocumentType: CertificationType.InsuranceCertificate,
            DocumentName: "Expiring Soon",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            ExternalFileRef: null,
            Notes: null
        );
        
        // One not expiring soon (60 days)
        var stableRequest = new CreateCertificationRequest(
            DocumentType: CertificationType.QualityCertification,
            DocumentName: "Stable",
            IssueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)),
            ExternalFileRef: null,
            Notes: null
        );

        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications", expiringRequest);
        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/certifications", stableRequest);

        // Act
        var response = await Client.GetAsync("/supplier/v1/suppliers/certifications/expiring?daysThreshold=30");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<ExpiringCertificationsListResponse>(response);
        Assert.NotNull(result);
        Assert.Contains(result!.Items, c => c.DocumentName == "Expiring Soon");
        Assert.DoesNotContain(result.Items, c => c.DocumentName == "Stable");
    }
}
