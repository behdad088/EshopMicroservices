using BuildingBlocks.Pagination;

namespace Catalog.API.Features.Products.GetProducts;

public static class GetProductsEndpoint
{
    public static IEndpointRouteBuilder MapGetProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", GetProducts)
            .WithName("GetProducts")
            .Produces<GetProductResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Products")
            .WithDescription("Get Products")
            .AllowAnonymous();
        return app;
    }

    private static async Task<Ok<GetProductResponse>> GetProducts(
        [AsParameters] PaginationRequest paginationRequest,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductsQuery(paginationRequest)).ConfigureAwait(false);
        return TypedResults.Ok(MapResponse(queryResult.PaginatedResult));
    }

    private static GetProductResponse MapResponse(PaginatedItems<ProductModule> result)
    {
        return new GetProductResponse(
            new PaginatedItems<ProductResponse>(
                result.PageIndex,
                result.PageSize,
                result.Count,
                result.Data.Select(x => new ProductResponse(
                    x.Id,
                    x.Name,
                    x.Category,
                    x.Description,
                    x.ImageFile,
                    x.Price))));
    }
}