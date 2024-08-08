using AutoFixture;
using AutoFixture.Xunit2;

namespace Basket.API.IntegrationTests.AutoFixture;

public class BasketRequestAutoDataAttribute() : AutoDataAttribute(BasketRequestFixtureFactory.Create)
{
    private static class BasketRequestFixtureFactory 
    {
        public static IFixture Create()
        {
            var fixture = new Fixture();
            fixture.Customize(new StoreBasketRequestCustomization());
            return fixture;
        }
    }
}