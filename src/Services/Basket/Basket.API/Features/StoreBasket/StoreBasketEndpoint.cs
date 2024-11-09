using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Features.StoreBasket;

public static class StoreBasketEndpoint
{
    public static IEndpointRouteBuilder MapStoreBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", StoreBasketAsync)
            .WithName("StoreBasket")
            .Produces<StoreBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Store Basket")
            .WithDescription("Store Basket");

        return app;
    }

    private static async Task<Ok<StoreBasketResponse>> StoreBasketAsync(StoreBasketRequest request, ISender sender)
    {
        var command = MapCommand(request);
        var result = await sender.Send(command).ConfigureAwait(false);
        return TypedResults.Ok(MapResult(result));
    }

    private static StoreBasketCommand MapCommand(StoreBasketRequest? request)
    {
        return new StoreBasketCommand(request?.ShoppingCart);
    }

    private static StoreBasketResponse MapResult(StoreBasketResult result)
    {
        return new StoreBasketResponse(result.ShoppingCart);
    }
}