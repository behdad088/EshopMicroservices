using Basket.API.Features.GetBasket;

namespace Basket.API.Common;

public static class RoutingRegistrar
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetBasketEndpoint();
    }
}