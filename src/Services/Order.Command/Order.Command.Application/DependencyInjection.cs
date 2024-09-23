using System.Reflection;
using BuildingBlocks.CQRS.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Command.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.RegisterMediateR(Assembly.GetExecutingAssembly());
        return services;
    }
}