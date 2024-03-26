using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.API.Features.Products.GetProduct;

public static class GetProductEndpoint
{
    public static IEndpointRouteBuilder MapGetProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", GetProducts)
            .WithName("GetProduct")
            .Produces<GetProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product")
            .WithDescription("Get Product");
        return app;
    }

    public static async Task<Ok<GetProductResponse>> GetProducts(
        [AsParameters] PaginationRequest paginationRequest, 
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductsQuery(paginationRequest));
        return TypedResults.Ok(new GetProductResponse(queryResult.Result));
    }
}