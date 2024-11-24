namespace Catalog.API.Features.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/products", UpdateProduct)
            .WithName("UpdateProduct")
            .Produces<UpdateProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .WithSummary("Update Product")
            .WithDescription("Update Product");
        return app;
    }

    private static async Task<IResult> UpdateProduct(
        HttpContext context,
        UpdateProductRequest request,
        ISender sender)
    {
        var eTag = context.Request.Headers["If-Match"];
        var command = request.ToCommand(eTag);

        if (command is null)
            return TypedResults.BadRequest("Request is null");

        await sender.Send(command).ConfigureAwait(false);
        return TypedResults.NoContent();
    }

    private static UpdateProductCommand? ToCommand(this UpdateProductRequest? request, string? etag)
    {
        return request is null
            ? null
            : new UpdateProductCommand(
                Id: request.Id,
                Name: request.Name,
                Category: request.Category,
                Description: request.Description,
                ImageFile: request.ImageFile,
                Price: request.Price,
                Etag: etag);
    }
}