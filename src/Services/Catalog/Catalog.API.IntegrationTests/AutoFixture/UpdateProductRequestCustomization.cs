﻿using AutoFixture;
using Catalog.API.Features.Products.UpdateProduct;

namespace Catalog.API.IntegrationTests.AutoFixture
{
    public class UpdateProductRequestCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<UpdateProductRequest>(composer => composer
                    .With(r => r.Id, Ulid.NewUlid().ToString())
                    .With(r => r.Name, "Product Test Name - Updated")
                    .With(r => r.Category, ["product test category - Updated"])
                    .With(r => r.Description, "Product Test Description - Updated")
                    .With(r => r.ImageFile, "Test image file - Updated")
                    .With(r => r.Price, 1000)
                    );
        }
    }
}
