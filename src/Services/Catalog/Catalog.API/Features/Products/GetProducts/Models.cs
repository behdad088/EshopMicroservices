namespace Catalog.API.Features.Products.GetProduct;

public record GetProductResponse(PaginatedItems<ProductModule> Result);

public record ProductModule(
    [property: JsonPropertyName("id")]
    Guid Id,
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("category")]
    List<string> Category,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("image_file")]
    string ImageFile,
    [property: JsonPropertyName("price")]
    decimal? Price);
