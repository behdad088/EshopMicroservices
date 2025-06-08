using Order.Command.Application.Rmq.CloudEvent.Models;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Application.Rmq.CloudEvent;

internal interface ICloudEventFactory<TEvent> where TEvent : IDomainEvent
{
    CloudEvent<TEvent> Create(TEvent @event);
}