using System.Data.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor(
    IMediator mediator,
    ILogger<DispatchDomainEventsInterceptor> _logger) : DbTransactionInterceptor
{
    public override void TransactionCommitted(
        DbTransaction transaction,
        TransactionEndEventData eventData)
    {
        var eventToDispatch = GetDomainEvents(eventData.Context);
        DispatchDomainEvents(eventToDispatch).GetAwaiter().GetResult();
        
        base.TransactionCommitted(transaction, eventData);
    }

    public override Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = new())
    {
        _logger.LogInformation("Transaction committed");
        var eventToDispatch = GetDomainEvents(eventData.Context);
        DispatchDomainEvents(eventToDispatch).GetAwaiter().GetResult();
        
        return base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
    }

    private async Task DispatchDomainEvents(List<IDomainEvent>? domainEvents)
    {
        if (domainEvents is null) return;

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Dispatching domain event {EventType}", domainEvent.GetType().Name);
            await mediator.Publish(domainEvent).ConfigureAwait(false);
        }
    }

    private List<IDomainEvent>? GetDomainEvents(DbContext? context)
    {
        _logger.LogInformation("Getting domain events");
        if (context is null)
        {
            _logger.LogInformation("Domain events are null");
            return null;
        }

        var entities = context.ChangeTracker
            .Entries<IAggregate>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity).ToArray();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();
        
        entities.ToList().ForEach(e => e.ClearDomainEvents());
        
        _logger.LogInformation("Domain events were successfully retrieved. returning domain events.");
        return domainEvents;
    }
}