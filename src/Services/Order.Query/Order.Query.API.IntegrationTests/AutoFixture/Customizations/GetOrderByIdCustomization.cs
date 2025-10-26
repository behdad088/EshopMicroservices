using AutoFixture;
using Order.Query.API.Features.GetOrderById;

namespace Order.Query.API.IntegrationTests.AutoFixture.Customizations;

public class GetOrderByIdCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request>(x => x
            .With(r => r.OrderId, Ulid.NewUlid().ToString )
            .With(r => r.CustomerId, Guid.NewGuid().ToString()));
    }
}