namespace Catalog.API.Features.Products.GetProductByCategory;

public record GetProductByCategoryResponse(
    PaginatedItems<ProductModule> ProductModule);

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