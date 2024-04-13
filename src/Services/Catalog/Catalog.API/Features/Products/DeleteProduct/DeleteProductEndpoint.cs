namespace Catalog.API.Features.Products.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", UpdataeProduct)
            .WithName("DeleteProduct")
            .Produces<DeleteProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Delete Product")
            .WithDescription("Delete Product");
        return app;
    }

    public static async Task<Ok<DeleteProductResponse>> UpdataeProduct(string? id, ISender sender)
    {
        var result = await sender.Send(new DeleteProductCommand(id));
        return TypedResults.Ok(new DeleteProductResponse(result.IsSuccess));
    }
}