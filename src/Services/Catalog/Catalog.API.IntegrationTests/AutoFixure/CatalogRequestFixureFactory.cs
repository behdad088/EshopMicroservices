using AutoFixture;
using AutoFixture.Xunit2;

namespace Catalog.API.IntegrationTests.AutoFixure
{
    public class CatalogRequestAutoDataAttribute : AutoDataAttribute
    {
        public CatalogRequestAutoDataAttribute() : base(CatalogRequestFixureFactory.Create)
        {
        }

        private static class CatalogRequestFixureFactory
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
