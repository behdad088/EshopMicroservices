using AutoFixture;
using AutoFixture.Xunit2;

namespace Order.Command.API.IntegrationTests.AutoFixture;

public class OrderRequestAutoDataAttribute() : AutoDataAttribute(OrderRequestFixtureFactory.Create)
{
    private static class OrderRequestFixtureFactory
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            // fixture.Customize(new GetOrderByCustomerRequestCustomization());
            return fixture;
        }
    }
}