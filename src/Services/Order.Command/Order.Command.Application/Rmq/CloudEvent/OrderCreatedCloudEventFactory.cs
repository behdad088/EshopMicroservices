using System.Net.Mime;

namespace Order.Command.Application.Rmq.CloudEvent;

internal sealed class OrderCreatedCloudEventFactory : ICloudEventFactory<OrderCreatedEvent>
{
    public CloudEvent<OrderCreatedEvent> Create(OrderCreatedEvent @event)
    {
        return new CloudEvent<OrderCreatedEvent>
        {
            Id = Ulid.NewUlid().ToString(),
            Type = "com.eshop.order-command.order-created",
            Source = "urn:com:eshop:order-command-api",
            SpecVersion = "1.0",
            DataContentType = MediaTypeNames.Application.Json,
            Time = DateTimeOffset.UtcNow,
            Data = @event
        };
    }
}