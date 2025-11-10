using eshop.Shared.Pagination;

namespace Catalog.API.Features.Products.GetProducts;

public record GetProductResponse(PaginatedItems<ProductResponse> Result);

public record ProductResponse(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")]
    List<string> Category,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("image_file")]
    string ImageFile,
    [property: JsonPropertyName("price")] decimal? Price);