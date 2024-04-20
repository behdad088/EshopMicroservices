using AutoFixture;
using Catalog.API.Features.Products.CreateProduct;
using Catalog.API.Features.Products.UpdateProduct;

namespace Catalog.API.IntegrationTests.AutoFixure
{
    public class UpdateProductRequestCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<UpdateProductRequest>(composer => composer
                    .With(r => r.Id, () => Guid.NewGuid())
                    .With(r => r.Name, "Product Test Name - Updated")
                    .With(r => r.Category, ["product test category - Updated"])
                    .With(r => r.Description, "Product Test Description - Updated")
                    .With(r => r.ImageFile, "Test image file - Updated")
                    .With(r => r.Price, 1000)
                    );
        }
    }
}
