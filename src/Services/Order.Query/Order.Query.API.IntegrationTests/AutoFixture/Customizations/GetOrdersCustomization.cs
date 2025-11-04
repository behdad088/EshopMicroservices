using AutoFixture;
using Order.Query.Api.Features.GetOrders;

namespace Order.Query.API.IntegrationTests.AutoFixture.Customizations;

public class GetOrdersCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request>(x => x
            .With(r => r.PageIndex, 0 )
            .With(r => r.PageSize, 10));
    }
}