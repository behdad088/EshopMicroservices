using System.Reflection;
using eshop.Shared.CQRS.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eshop.Shared.CQRS.Extensions;

public static class CqrsExtensions
{
    public static void RegisterMediateR(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(CommandValidationBehaviors<,>));
            config.AddOpenBehavior(typeof(QueryValidationBehaviors<,>));
            config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
    }
}