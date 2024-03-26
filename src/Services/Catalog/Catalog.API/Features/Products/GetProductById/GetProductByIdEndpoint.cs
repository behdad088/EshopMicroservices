﻿namespace Catalog.API.Features.Products.GetProductById;

public static class GetProductByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", GetProductById)
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Product")
            .WithDescription("Get Product");

        return app;
    }

    public static async Task<Ok<GetProductByIdResponse>> GetProductById(
        Guid id,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetProductByIdQuery(id));
        return TypedResults.Ok(new GetProductByIdResponse(queryResult.Product));
    }
}