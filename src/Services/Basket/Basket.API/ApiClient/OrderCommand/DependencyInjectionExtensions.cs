using Basket.API.ApiClient.AccessToken;
using eshop.Shared;
using Refit;

namespace Basket.API.ApiClient.OrderCommand;

public static class DependencyInjectionExtensions
{
    public static void AddOrderCommandClient(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddRefitClient<IOrderCommandClient>()
            .ConfigureHttpClient((_, c) =>
            {
                var config = configuration.TryGetValidatedOptions<OrderCommandClientConfigurations>();
                c.BaseAddress = new Uri(config.BaseUrl);
            }).AddHttpMessageHandler<AuthenticatedHttpClientHandler>();
    } 
}