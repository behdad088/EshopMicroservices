using AutoFixture;
using Catalog.API.Features.Products.CreateProduct;

namespace Catalog.API.IntegrationTests.AutoFixture
{
    public class CreateProductRequestCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<CreateProductRequest>(composer => composer
                    .With(r => r.Id, Ulid.NewUlid().ToString())
                    .With(r => r.Name, "Product Test Name")
                    .With(r => r.Category, ["product test category"])
                    .With(r => r.Description, "Product Test Description")
                    .With(r => r.ImageFile, "Test image file")
                    .With(r => r.Price, 1000)
                    );
        }
    }
}
