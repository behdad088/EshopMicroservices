using System.Net.Mime;
using Order.Command.Application.Rmq.CloudEvent.Models;

namespace Order.Command.Application.Rmq.CloudEvent;

public sealed class OrderUpdatedCloudEventFactory : ICloudEventFactory<OrderUpdatedEvent>
{
    public CloudEvent<OrderUpdatedEvent> Create(OrderUpdatedEvent @event)
    {
        return new CloudEvent<OrderUpdatedEvent>
        {
            Id = Ulid.NewUlid().ToString(),
            Type = "com.eshop.order-command.order-updated",
            Source = "urn:com:eshop:order-command-api",
            SpecVersion = "1.0",
            DataContentType = MediaTypeNames.Application.Json,
            Time = DateTimeOffset.UtcNow,
            Data = @event
        };
    }
}