using Catalog.API.Authorization;

namespace Catalog.API.Features.Products.DeleteProduct;

public static class DeleteProductEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/{id}", UpdateProduct)
            .WithName("DeleteProduct")
            .Produces<DeleteProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Delete Product")
            .WithDescription("Delete Product")
            .RequireAuthorization(Policies.CanDeleteProduct);
        return app;
    }

    private static async Task<Ok<DeleteProductResponse>> UpdateProduct(string? id, ISender sender)
    {
        var result = await sender.Send(new DeleteProductCommand(id)).ConfigureAwait(false);
        return TypedResults.Ok(new DeleteProductResponse(result.IsSuccess));
    }
}