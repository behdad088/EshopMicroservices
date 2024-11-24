namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdResponse(
    [property: JsonPropertyName("id")] Ulid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")]
    List<string> Category,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("image_file")]
    string ImageFile,
    [property: JsonPropertyName("price")] decimal? Price);

public record GetProductById(
    Ulid Id,
    string Name,
    List<string> Category,
    string? Description,
    string ImageFile,
    decimal? Price,
    int Version);