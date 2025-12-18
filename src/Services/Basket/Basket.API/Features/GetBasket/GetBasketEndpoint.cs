using Basket.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Basket.API.Features.GetBasket;

public static class GetBasketEndpoint
{
    public static IEndpointRouteBuilder MapGetBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/customers/{username}", GetBasketAsync)
            .WithName("GetBasket")
            .Produces<GetBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Basket by username")
            .WithDescription("Get Basket by username")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetBasketAsync(
        string username,
        ISender sender,
        HttpContext httpContext,
        IAuthorizationService authorizationService)
    {
        var isAuthorized =
            await authorizationService.CanGetBasketAsync(httpContext, username);

        if (!isAuthorized)
        {
            return TypedResults.Forbid();
        }
        
        var result = await sender.Send(new GetBasketQuery(username)).ConfigureAwait(false);

        return result switch
        {
            Result.NotFound => TypedResults.NotFound(),
            Result.Success success => TypedResults.Ok(Map(success.ShoppingCart)),
            _ => TypedResults.InternalServerError()
        };
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