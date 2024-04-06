namespace Catalog.API.Features.Products.GetProductByCategory;

public record GetProductByCategoryResponse(
    PaginatedItems<ProductModule> ProductModule);