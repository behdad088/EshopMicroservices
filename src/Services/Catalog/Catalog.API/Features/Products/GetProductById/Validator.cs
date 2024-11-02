namespace Catalog.API.Features.Products.GetProductById;

public class Validator : AbstractValidator<GetProductByIdQuery>
{
    public Validator()
    {
        RuleFor(p => p.Id).NotEmpty().Must(x => Ulid.TryParse(x, out _)).WithMessage("Please provide a valid Ulid");
    }
}