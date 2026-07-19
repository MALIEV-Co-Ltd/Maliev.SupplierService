using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class ExternalServiceClientTests
{
    private readonly ILogger<InvoiceServiceClient> _logger = Substitute.For<ILogger<InvoiceServiceClient>>();

    [Fact]
    public async Task InvoiceServiceClient_CheckReferencesAsync_ReturnsTrue_WhenReferencesExist()
    {
        // Arrange
        var handler = new MockHttpMessageHandler((req, ct) =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("true")
            };
            return Task.FromResult(resp);
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://invoice-service") };
        var serviceClient = new InvoiceServiceClient(client, _logger);

        // Act
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result.HasReferences);
        Assert.False(result.ServiceUnavailable);
    }

    [Fact]
    public async Task InvoiceServiceClient_CheckReferencesAsync_ReturnsServiceUnavailable_WhenServiceIsDown()
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
        var result = await serviceClient.CheckReferencesAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.False(result.HasReferences);
        Assert.True(result.ServiceUnavailable);
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
