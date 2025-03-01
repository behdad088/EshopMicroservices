using System.Reflection;
using BuildingBlocks.CQRS.Extensions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order.Command.Application.Configurations;
using Order.Command.Application.Outbox;
using Order.Command.Application.Rmq;
using Order.Command.Application.Rmq.CloudEvent;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application;

public static class DependencyInjection
{
    private static readonly Dictionary<string, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>>
        Configurations = new();

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        RabbitMqConfigurations rabbitMqConfigurations
        )
    {
        services
            .AddEventPublisher<OrderCreatedEvent, OrderCreatedCloudEventFactory>()
            .AddEventPublisher<OrderUpdatedEvent, OrderUpdatedCloudEventFactory>()
            .AddEventPublisher<OrderDeletedEvent, OrderDeletedCloudEventFactory>()
            .AddRmq(rabbitMqConfigurations);
        services.RegisterMediateR(Assembly.GetExecutingAssembly());
        services.AddHostedService<OutboxService>();
        return services;
    }

    private static IServiceCollection AddEventPublisher<TEvent, TCloudEventFactory>(
        this IServiceCollection services)
        where TEvent : class, IDomainEvent
        where TCloudEventFactory : class, ICloudEventFactory<TEvent>
    {
        var exchange = GetExchange<TEvent>();
        
        Configurations[typeof(TEvent).Name] = (_, cfg) =>
        {
            cfg.Message<CloudEvent<TEvent>>(config =>
            {
                config.SetEntityName($"{exchange}_exchange");
            });
            cfg.Publish<CloudEvent<TEvent>>(publishConfig =>
            {
                publishConfig.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
            });
        };
        
        services.AddTransient<IEventPublisher<TEvent>, EventPublisher<TEvent>>();
        services.AddTransient<ICloudEventFactory<TEvent>, TCloudEventFactory>();
        return services;
    }


    private static IServiceCollection AddRmq(
        this IServiceCollection services,
        RabbitMqConfigurations rabbitMqConfigurations)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfigurations.Uri, "/", h =>
                {
                    h.Username(rabbitMqConfigurations.Username);
                    h.Password(rabbitMqConfigurations.Password);
                });

                foreach (var configuration in Configurations)
                {
                    configuration.Value.Invoke(context, cfg);
                }
            });
        });

        return services;
    }
    
    private static string GetExchange<TEvent>() where TEvent : class, IDomainEvent
    {
        var eventName = typeof(TEvent).Name;
        var eventNameInSnakeCase =
            string.Concat(eventName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
                .ToLower();
        return eventNameInSnakeCase;
    }
}