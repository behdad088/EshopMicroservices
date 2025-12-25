namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductRequest(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")]
    List<string> Category,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("image_file")]
    string ImageFile,
    [property: JsonPropertyName("price")] decimal? Price);

public record UpdateProductResponse(
    [property: JsonPropertyName("result")] ProductModule Product);