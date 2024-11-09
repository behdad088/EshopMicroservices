using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Features.GetBasket;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder MapGetBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{username}", GetBasketAsync)
            .WithName("GetBasket")
            .Produces<GetBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Basket by username")
            .WithDescription("Get Basket by username");

        return app;
    }

    private static async Task<Ok<GetBasketResponse>> GetBasketAsync(
        string username,
        ISender sender)
    {
        var queryResult = await sender.Send(new GetBasketQuery(username)).ConfigureAwait(false);
        return TypedResults.Ok(Map(queryResult.ShoppingCart));
    }

    private static GetBasketResponse Map(ShoppingCart result)
    {
        return new GetBasketResponse(
            result.Username,
            MapItems(result.Items),
            result.TotalPrice
        );
    }

    private static List<BasketItem> MapItems(List<ShoppingCartItem> items)
    {
        return items.Select(x => new BasketItem(x.Quantity, x.Color, x.Price, x.ProductId, x.ProductName)).ToList();
    }
}