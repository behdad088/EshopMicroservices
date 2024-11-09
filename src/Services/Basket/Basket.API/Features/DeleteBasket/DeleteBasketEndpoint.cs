using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Basket.API.Features.DeleteBasket;

public static class DeleteBasketEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBasketEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{username}", DeleteBasketAsync)
            .WithName("DeleteBasket")
            .Produces<DeleteBasketResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Basket by username")
            .WithDescription("Delete Product by username");

        return app;
    }

    private static async Task<Ok<DeleteBasketResponse>> DeleteBasketAsync(
        string username,
        ISender sender)
    {
        var queryResult = await sender.Send(new DeleteBasketCommand(username)).ConfigureAwait(false);
        return TypedResults.Ok(Map(queryResult));
    }

    private static DeleteBasketResponse Map(DeleteBasketResult response) => new(response.IsSuccess);
}