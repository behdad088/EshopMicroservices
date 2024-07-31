using Basket.API.Features.DeleteBasket;
using Basket.API.Features.GetBasket;
using Basket.API.Features.StoreBasket;

namespace Basket.API.Common;

public static class RoutingRegistrar
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetBasketEndpoint();
        app.MapStoreBasketEndpoint();
        app.MapDeleteBasketEndpoint();
    }
}