using System.Data.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor(IMediator mediator) : DbTransactionInterceptor
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
        var eventToDispatch = GetDomainEvents(eventData.Context);
        DispatchDomainEvents(eventToDispatch).GetAwaiter().GetResult();
        
        return base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
    }

    private async Task DispatchDomainEvents(List<IDomainEvent>? domainEvents)
    {
        if (domainEvents is null) return;

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent).ConfigureAwait(false);
    }

    private static List<IDomainEvent>? GetDomainEvents(DbContext? context)
    {
        if (context is null) return null;

        var entities = context.ChangeTracker
            .Entries<IAggregate>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity).ToArray();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ToList().ForEach(e => e.ClearDomainEvents());
        return domainEvents;
    }
}