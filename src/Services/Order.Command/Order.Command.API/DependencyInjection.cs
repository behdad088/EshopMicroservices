namespace Order.Command.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication UseApiServices(this WebApplication services)
    {
        return services;
    }
}