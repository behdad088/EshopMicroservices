using System.Net.Mime;
using FluentValidation;
using MassTransit;
using Order.Query.Events;
using Order.Query.Projections;
using Order.Query.EventProcessor.Configurations;
using Order.Query.EventProcessor.Consumer;
using Order.Query.EventProcessor.MassTransitConfiguration;
using Order.Query.Features.OrderView;
using Event = Order.Query.Events.Event;

namespace Order.Query.EventProcessor;

public static class DependencyInjection
{
    private static readonly Dictionary<string, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>>
        EndpointConfigurations = new();
    private static readonly Dictionary<string, Action<IBusRegistrationConfigurator>> ConsumerConfiguration = new();
    
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        RabbitMqConfigurations rabbitMqConfigurations,
        QueueConfigurations queueConfigurations
    )
    {
        var queues = queueConfigurations.OrderViewQueues;
        return services
            .AddConsumer<OrderCreatedEvent, OrderView>(queues.OrderCreatedEvent)
            .AddConsumer<OrderUpdatedEvent, OrderView>(queues.OrderUpdatedEvent)
            .AddConsumer<OrderDeletedEvent, OrderView>(queues.OrderDeletedEvent)
            .AddRmq(rabbitMqConfigurations);
    }

    private static IServiceCollection AddConsumer<TEvent, TView>(
        this IServiceCollection services,
        Queue queue)
        where TEvent : Event
        where TView : class, IProjection<TView, TEvent>
    {
        EndpointConfigurations[typeof(TEvent).Name] = (context, cfg) =>
        {
            cfg.ReceiveEndpoint(queue.Name, e =>
            {
                e.ConfigureConsumeTopology = false;
                e.DefaultContentType = new ContentType("application/json");
                e.UseRawJsonDeserializer(RawSerializerOptions.All, isDefault: true);

                e.Bind(queue.Exchange, bind =>
                {
                    bind.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
                });

                e.ConfigureConsumer<EventConsumer<TView, TEvent>>(context);
            });
        };

        ConsumerConfiguration[typeof(TEvent).Name] = consumerRegister =>
        {
            consumerRegister.AddConsumer<EventConsumer<TView, TEvent>>();
        };

        return services;
    }

    private static IServiceCollection AddRmq(
        this IServiceCollection services,
        RabbitMqConfigurations rabbitMqConfigurations)
    {
        services.AddMassTransit(x =>
        {
            foreach (var consumer in ConsumerConfiguration)
            {
                consumer.Value.Invoke(x);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfigurations.Uri, "/", h =>
                {
                    h.Username(rabbitMqConfigurations.Username);
                    h.Password(rabbitMqConfigurations.Password);
                });

                cfg.SendTopology.ErrorQueueNameFormatter = new CustomErrorQueueNameFormatter();
                
                foreach (var configuration in EndpointConfigurations)
                {
                    configuration.Value.Invoke(context, cfg);
                }
            });
        });
        return services;
    }
}