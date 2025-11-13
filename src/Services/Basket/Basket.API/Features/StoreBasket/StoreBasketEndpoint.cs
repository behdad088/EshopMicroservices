using Basket.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Basket.API.Features.StoreBasket;

public static class StoreBasketEndpoint
{
    public static IEndpointRouteBuilder MapStoreBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/customers", StoreBasketAsync)
            .WithName("StoreBasket")
            .Produces<StoreBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Store Basket")
            .WithDescription("Store Basket")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> StoreBasketAsync(
        StoreBasketRequest request,
        ISender sender,
        HttpContext httpContext,
        IAuthorizationService authorizationService)
    {
        var isAuthorized =
            await authorizationService.CanStoreBasketAsync(httpContext, request.ShoppingCart?.Username ?? string.Empty);

        if (!isAuthorized)
        {
            return TypedResults.Forbid();
        }
        
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