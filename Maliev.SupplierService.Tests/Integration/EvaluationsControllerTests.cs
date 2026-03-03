using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Domain.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class EvaluationsControllerTests : BaseIntegrationTest
{
    public EvaluationsControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddEvaluation_WithValidData_Returns201()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Quality,
            Score: 5,
            Comments: "Excellent quality",
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var evaluation = await GetResponseAsync<EvaluationResponse>(response);
        Assert.NotNull(evaluation);
        Assert.Equal(5, evaluation!.Score);
        Assert.Equal(PerformanceRatingCategory.Quality, evaluation.Category);
    }

    [Fact]
    public async Task GetEvaluations_ReturnsList()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Delivery,
            Score: 4,
            Comments: "Good delivery",
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );
        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/evaluations", request);

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}/evaluations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await GetResponseAsync<EvaluationListResponse>(response);
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(4, result.AverageScore);
    }
}
