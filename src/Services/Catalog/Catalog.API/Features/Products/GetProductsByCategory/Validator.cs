﻿using Catalog.API.Features.Products.GetProductByCategory;

namespace Catalog.API.Features.Products.GetProductsByCategory;

public class GetProductByCategoryQueryValidator : AbstractValidator<GetProductByCategoryQuery>
{
    public GetProductByCategoryQueryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category cannot be null");
    }
}
