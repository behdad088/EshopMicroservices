using BuildingBlocks.CQRS.Behaviours;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.CQRS.Extensions;

public static class CqrsExtensions
{
    public static void RegisterMediateR(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            config.AddOpenBehavior(typeof(QueryValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
    }
}