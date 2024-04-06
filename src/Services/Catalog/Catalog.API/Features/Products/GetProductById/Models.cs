namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdResponse(
    [property: JsonPropertyName("result")] ProductModule ProductModule)
{
}