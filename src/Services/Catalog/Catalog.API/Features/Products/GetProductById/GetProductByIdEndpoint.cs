namespace Catalog.API.Features.Products.GetProductById;

public static class GetProductByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", GetProductById)
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Product by id")
            .WithDescription("Get Product by id");

        return app;
    }

    private static async Task<Ok<GetProductByIdResponse>> GetProductById(
        string id,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductByIdQuery(id)).ConfigureAwait(false);
        var result = ToResponse(queryResult);
        return TypedResults.Ok(result);
    }

    private static GetProductByIdResponse? ToResponse(GetProductByIdResult? result)
    {
        return result is null
            ? null
            : new GetProductByIdResponse(
                Id: result.Product.Id,
                Name: result.Product.Name,
                Category: result.Product.Category,
                Description: result.Product.Description,
                ImageFile: result.Product.ImageFile,
                Price: result.Product.Price);
    }
}