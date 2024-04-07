using Catalog.API.Features.Products.GetProduct;

namespace Catalog.API.Features.Products.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PaginationRequest.PageIndex).GreaterThanOrEqualTo(0).WithMessage("PageIndex must be greater or equal to 0");
        RuleFor(x => x.PaginationRequest.PageSize).InclusiveBetween(2, 20).WithMessage("Page size must be between 2 to 20");
    }
}