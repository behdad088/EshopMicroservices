namespace Catalog.API.Features.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/products", UpdateProduct)
            .WithName("UpdateProduct")
            .Produces<UpdateProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update Product")
            .WithDescription("Update Product");
        return app;
    }

    private static async Task<IResult> UpdateProduct(UpdateProductRequest request, ISender sender)
    {
        var command = request.ToCommand();
        if (command is null)
            return TypedResults.BadRequest("Request is null");

        var result = await sender.Send(command).ConfigureAwait(false);
        return TypedResults.Ok(new UpdateProductResponse(result.Product));
    }

    private static UpdateProductCommand? ToCommand(this UpdateProductRequest? request)
    {
        return request is null
            ? null
            : new UpdateProductCommand(
                Id: request.Id,
                Name: request.Name,
                Category: request.Category,
                Description: request.Description,
                ImageFile: request.ImageFile,
                Price: request.Price);
    }
}