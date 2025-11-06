using AutoFixture;
using Order.Query.API.Features.GetOrdersByName;

namespace Order.Query.API.IntegrationTests.AutoFixture.Customizations;

public class GetOrdersByNameCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Request>(x => x
            .With(r => r.OrderName, "Test Name")
            .With(r => r.PageIndex, 0 )
            .With(r => r.PageSize, 10));
    }
}