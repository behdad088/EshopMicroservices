using Mapster;
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
        var command = request.Adapt<StoreBasketCommand>();
        var result = await sender.Send(command).ConfigureAwait(false);
        return TypedResults.Ok(MapResult(result));
    }

    private static StoreBasketCommand MapCommand(StoreBasketRequest request)
    {
        return new StoreBasketCommand(
            new ShoppingCart
            {
                Username = request.ShoppingCart!.Username,
                Items = request.ShoppingCart!.Items!.Select(x => new ShoppingCartItem
                {
                    ProductId = x.ProductId,
                    Color = x.Color,
                    Price = x.Price,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity
                }).ToList().ToList()
            });
    }

    private static StoreBasketResponse MapResult(StoreBasketResult result)
    {
        return new StoreBasketResponse(
            new BasketDtoResponse(
                result.ShoppingCart.Username,
                result.ShoppingCart.Items.Select(x =>
                    new BasketItem(
                        x.Quantity,
                        x.Color,
                        x.Price,
                        x.ProductId,
                        x.ProductName)).ToList(),
                result.ShoppingCart.TotalPrice));
    }
}