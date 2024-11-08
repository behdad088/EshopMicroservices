﻿using BuildingBlocks.Pagination;

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
            .WithDescription("Get Products");
        return app;
    }

    private static async Task<Ok<GetProductResponse>> GetProducts(
        [AsParameters] PaginationRequest paginationRequest,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductsQuery(paginationRequest)).ConfigureAwait(false);
        return TypedResults.Ok(new GetProductResponse(queryResult.Result));
    }
}