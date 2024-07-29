namespace Catalog.API.Features.Products.GetProducts;

public record GetProductResponse(PaginatedItems<ProductModule> Result);