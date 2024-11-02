namespace Catalog.API.Features.Products.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(x => Ulid.TryParse(x, out _)).WithMessage("Product Id is not a valid UUID");
    }
}