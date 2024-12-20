﻿using System.Text.RegularExpressions;

namespace Catalog.API.Features.Products.UpdateProduct;

public partial class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().Must(x => Ulid.TryParse(x, out _)).WithMessage("Product Id is required");
        RuleFor(x => x.Etag).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x) && EtagRegex().IsMatch(x));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");


        When(x => x.Category is not null,
            () => RuleForEach(x => x.Category)
                .NotEmpty()
                .WithMessage("Category item cannot be null"));

        RuleFor(x => x.ImageFile)
            .NotEmpty()
            .WithMessage("ImageFile cannot be null")
            .When(x => x.ImageFile is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .When(x => x.Price is not null);
    }

    [GeneratedRegex("""^W\/"\d+"$""")]
    private static partial Regex EtagRegex();
}