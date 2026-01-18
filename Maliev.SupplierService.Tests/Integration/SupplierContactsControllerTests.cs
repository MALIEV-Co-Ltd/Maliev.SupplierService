using System.Net;
using System.Net.Http.Json;
using Maliev.SupplierService.Api.DTOs.Requests;
using Maliev.SupplierService.Api.DTOs.Responses;
using Maliev.SupplierService.Tests.Integration.Infrastructure;
using Xunit;

namespace Maliev.SupplierService.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class SupplierContactsControllerTests : BaseIntegrationTest
{
    public SupplierContactsControllerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddContact_WithValidData_Returns201()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateContactRequest(
            Name: "Jane Smith",
            Email: "jane@example.com",
            Role: "Sales Manager",
            Phone: "+0987654321"
        );

        // Act
        var response = await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/contacts", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with {response.StatusCode}: {content}");
        }
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var contact = await GetResponseAsync<ContactResponse>(response);
        Assert.NotNull(contact);
        Assert.Equal("Jane Smith", contact!.Name);
    }

    [Fact]
    public async Task GetContacts_ReturnsList()
    {
        // Arrange
        var supplier = await CreateTestSupplierAsync();
        var request = new CreateContactRequest(
            Name: "Jane Smith",
            Email: "jane@example.com",
            Role: "Sales Manager",
            Phone: "+0987654321"
        );
        await Client.PostAsJsonAsync($"/supplier/v1/suppliers/{supplier.Id}/contacts", request);

        // Act
        var response = await Client.GetAsync($"/supplier/v1/suppliers/{supplier.Id}/contacts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contacts = await GetResponseAsync<List<ContactResponse>>(response);
        Assert.NotNull(contacts);
        Assert.NotEmpty(contacts!);
    }
}
