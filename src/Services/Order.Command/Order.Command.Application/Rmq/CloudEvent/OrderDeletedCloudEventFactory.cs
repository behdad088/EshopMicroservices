using System.Net.Mime;
using Order.Command.Application.Rmq.CloudEvent.Models;

namespace Order.Command.Application.Rmq.CloudEvent;

internal sealed class OrderDeletedCloudEventFactory : ICloudEventFactory<OrderDeletedEvent>
{
    public CloudEvent<OrderDeletedEvent> Create(OrderDeletedEvent @event)
    {
        return new CloudEvent<OrderDeletedEvent>
        {
            Id = Ulid.NewUlid().ToString(),
            Type = "com.eshop.order-command.order-deleted",
            Source = "urn:com:eshop:order-command-api",
            SpecVersion = "1.0",
            DataContentType = MediaTypeNames.Application.Json,
            Time = DateTimeOffset.UtcNow,
            Data = @event
        };
    }
}