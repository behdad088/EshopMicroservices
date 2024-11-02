using BuildingBlocks.Pagination;

namespace Catalog.API.Features.Products.GetProductsByCategory;

public static class GetProductsByCategoryEndpoint
{
    public static IEndpointRouteBuilder MapGetProductByCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{category}", GetProductsByCategory)
            .WithName("GetProductByCategory")
            .Produces<GetProductByCategoryResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product By Category")
            .WithDescription("Get Product By Category");
        return app;
    }

    private static async Task<Ok<GetProductByCategoryResponse>> GetProductsByCategory(
        [AsParameters] PaginationRequest paginationRequest,
        string? category,
        ISender sender)
    {
        var query = new GetProductByCategoryQuery(paginationRequest, category!);
        var queryResult = await sender.Send(query).ConfigureAwait(false);
        return TypedResults.Ok(new GetProductByCategoryResponse(queryResult.Product));
    }
}