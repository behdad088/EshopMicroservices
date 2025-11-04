using AutoFixture;
using Order.Query.API.Features.GetOrdersByCustomer;

namespace Order.Query.API.IntegrationTests.AutoFixture.Customizations;

public class GetOrdersByCustomerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request>(x => x
            .With(r => r.CustomerId, Guid.NewGuid().ToString())
            .With(r => r.PageIndex, 0 )
            .With(r => r.PageSize, 10));
    }
}