using AutoFixture;
using AutoFixture.Xunit2;

namespace Catalog.API.IntegrationTests.AutoFixture
{
    public class CatalogRequestAutoDataAttribute() : AutoDataAttribute(CatalogRequestFixtureFactory.Create)
    {
        private static class CatalogRequestFixtureFactory
        {
            public static IFixture Create()
            {
                var fixture = new Fixture();
                fixture.Customize(new CreateProductRequestCustomization());
                fixture.Customize(new UpdateProductRequestCustomization());
                return fixture;
            }
        }
    }
}
