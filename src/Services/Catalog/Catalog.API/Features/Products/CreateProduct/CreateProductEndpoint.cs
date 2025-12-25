using Catalog.API.Authorization;
using Microsoft.Net.Http.Headers;

namespace Catalog.API.Features.Products.CreateProduct;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products", CreateProduct)
            .WithName("CreateProduct")
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create Product")
            .WithDescription("Create Product")
            .RequireAuthorization(Policies.CanCreateProduct);
        return app;
    }

    private static async Task<Created> CreateProduct(
        HttpContext context,
        CreateProductRequest request,
        ISender sender)
    {
        var command = request.ToCommand();
        var result = await sender.Send(command).ConfigureAwait(false);
        context.Response.Headers.Append(HeaderNames.AccessControlExposeHeaders, new[] { HeaderNames.Location });

        return TypedResults.Created($"/api/v1/catalog/products/{result.Id}");
    }

    private static CreateProductCommand ToCommand(this CreateProductRequest request)
    {
        return new CreateProductCommand(Id: request.Id,
            Description: request.Description,
            ImageFile: request.ImageFile,
            Name: request.Name,
            Price: request.Price,
            Category: request.Category);
    }
}