using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Data.Enums;
using Maliev.SupplierService.Tests.Integration.Infrastructure;

namespace Maliev.SupplierService.Tests.Integration;

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
            Score: 4,
            Comments: "Good quality products",
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await GetResponseAsync<EvaluationResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(4, result!.Score);
        Assert.Equal(PerformanceRatingCategory.Quality, result.Category);
    }

    [Fact]
    public async Task AddEvaluation_WithInvalidScore_Returns400()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Quality,
            Score: 6, // Invalid - should be 1-5
            Comments: null,
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddEvaluation_WithFutureDate_Returns400()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Delivery,
            Score: 3,
            Comments: null,
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvaluations_ReturnsListWithAverage()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();

        // Add multiple evaluations
        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations",
            new CreateEvaluationRequest(PerformanceRatingCategory.Quality, 5, null, DateOnly.FromDateTime(DateTime.UtcNow)));

        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations",
            new CreateEvaluationRequest(PerformanceRatingCategory.Delivery, 3, null, DateOnly.FromDateTime(DateTime.UtcNow)));

        await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations",
            new CreateEvaluationRequest(PerformanceRatingCategory.Communication, 4, null, DateOnly.FromDateTime(DateTime.UtcNow)));

        // Act
        var response = await Client.GetAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await GetResponseAsync<EvaluationListResponse>(response);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(4m, result.AverageScore); // (5 + 3 + 4) / 3 = 4
    }

    [Fact]
    public async Task AddEvaluation_NonExistingSupplier_Returns404()
    {
        // Arrange
        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Pricing,
            Score: 3,
            Comments: null,
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{Guid.NewGuid()}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task AddEvaluation_ValidScores_Accepted(int score)
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync(taxId: $"TAX{score}");

        var request = new CreateEvaluationRequest(
            Category: PerformanceRatingCategory.Overall,
            Score: score,
            Comments: $"Score {score} test",
            EvaluationDate: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        // Act
        var response = await Client.PostAsJsonAsync(
            $"/suppliers/v1/suppliers/{supplier.Id}/evaluations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
