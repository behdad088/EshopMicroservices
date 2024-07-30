using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Features.GetBasket;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder MapGetBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/Basket/{username}", GetBasketAsync)
            .WithName("GetBasket")
            .Produces<GetBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Product by id")
            .WithDescription("Get Product by id");

        return app;
    }
    
    private static async Task<Ok<GetBasketResponse>> GetBasketAsync(
        string username,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetBasketQuery(username));
        var result = queryResult.ShoppingCart.Adapt<GetBasketResponse>();
        return TypedResults.Ok(result);
    }
}