using AutoFixture;
using AutoFixture.Xunit2;
using Order.Query.API.IntegrationTests.AutoFixture.Customizations;

namespace Order.Query.API.IntegrationTests.AutoFixture;

public class DomainDataAutoAttribute() : AutoDataAttribute(FixtureFactory.Create)
{
    private static class FixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            
            fixture.Customize(new GetOrderByIdCustomization());
            fixture.Customize(new GetOrdersCustomization());
            fixture.Customize(new GetOrdersByCustomerCustomization());
            
            return fixture;
        }
    }
}