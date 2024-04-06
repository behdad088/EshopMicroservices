namespace Catalog.API.Features.Products.GetProduct;

public record GetProductResponse(PaginatedItems<ProductModule> Result);