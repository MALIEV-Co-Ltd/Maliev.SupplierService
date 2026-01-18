using Maliev.SupplierService.Api.DTOs.Responses;
using Xunit;

namespace Maliev.SupplierService.Tests.Unit;

public class DtoModelTests
{
    [Fact]
    public void ResponseModels_Properties_AreAccessible()
    {
        var capability = new CapabilityResponse(Guid.NewGuid(), "Cap", "Desc", true);
        Assert.Equal("Cap", capability.Name);

        var contact = new ContactResponse(Guid.NewGuid(), "Name", "Role", "Email", "Phone", true, DateTime.UtcNow);
        Assert.Equal("Name", contact.Name);

        var error = new DependencyErrorResponse("Msg", new List<DependencyInfo> { new DependencyInfo("Svc", 1, "Err") });
        Assert.Equal("Msg", error.Message);
        Assert.Equal("Svc", error.Dependencies[0].ServiceName);
    }
}
