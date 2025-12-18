using Basket.API.Features.CheckoutBasket.Commands.CreateOrder;

namespace Basket.API.Features.CheckoutBasket.Commands;

public static class DependencyInjectionExtensions
{
    public static void AddApiClientHandlers(this IServiceCollection services)
    {
        services.AddTransient<CreateOrderCommand>();
    }
}