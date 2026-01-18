using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.Services.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class ExternalServiceClientTests
{
    private readonly ILogger<InvoiceServiceClient> _logger = Substitute.For<ILogger<InvoiceServiceClient>>();

    [Fact]
    public async Task CheckReferencesAsync_ReturnsTrue_WhenReferencesExist()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { referenceCount = 5 })
            };
            return Task.FromResult(resp);
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://invoice-service") };
        var serviceClient = new InvoiceServiceClient(client, _logger);

        // Act
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.HasReferences);
        Assert.Equal(5, result.ReferenceCount);
    }

    [Fact]
    public async Task CheckReferencesAsync_ReturnsServiceUnavailable_WhenServiceIsDown()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            return Task.FromResult(resp);
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://invoice-service") };
        var serviceClient = new InvoiceServiceClient(client, _logger);

        // Act
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.HasReferences);
        Assert.True(result.ServiceUnavailable);
    }

    [Fact]
    public async Task MaterialServiceClient_CheckReferencesAsync_ReturnsTrue_WhenReferencesExist()
    {
        // Arrange
        var logger = Substitute.For<ILogger<MaterialServiceClient>>();
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { referenceCount = 3 })
            };
            return Task.FromResult(resp);
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://material-service") };
        var serviceClient = new MaterialServiceClient(client, logger);

        // Act
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.HasReferences);
        Assert.Equal(3, result.ReferenceCount);
        Assert.Equal("MaterialService", result.ServiceName);
    }

    [Fact]
    public async Task PurchaseOrderServiceClient_CheckReferencesAsync_ReturnsTrue_WhenReferencesExist()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PurchaseOrderServiceClient>>();
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { referenceCount = 7 })
            };
            return Task.FromResult(resp);
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://po-service") };
        var serviceClient = new PurchaseOrderServiceClient(client, logger);

        // Act
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid());

        // Assert
        Assert.True(result.HasReferences);
        Assert.Equal(7, result.ReferenceCount);
        Assert.Equal("PurchaseOrderService", result.ServiceName);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;
        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            => _sendAsync = sendAsync;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _sendAsync(request, cancellationToken);
    }
}
