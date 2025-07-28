using Basket.API.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Features.DeleteBasket;

public static class DeleteBasketEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBasketEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{username}", DeleteBasketAsync)
            .WithName("DeleteBasket")
            .Produces<DeleteBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Delete Basket by username")
            .WithDescription("Delete Product by username")
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> DeleteBasketAsync(
        string username,
        ISender sender,
        HttpContext httpContext,
        IAuthorizationService authorizationService)
    {
        var isAuthorized =
            await authorizationService.CanDeleteBasketAsync(httpContext, username);

        if (!isAuthorized)
        {
            return TypedResults.Forbid();
        }
        
        var queryResult = await sender.Send(new DeleteBasketCommand(username)).ConfigureAwait(false);
        return TypedResults.Ok(Map(queryResult));
    }

    private static DeleteBasketResponse Map(DeleteBasketResult response) => new(response.IsSuccess);
}