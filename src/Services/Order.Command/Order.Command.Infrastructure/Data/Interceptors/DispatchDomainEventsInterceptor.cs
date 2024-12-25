using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Order.Command.Domain.Abstractions;

namespace Order.Command.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var eventToDispatch = GetDomainEvents(eventData.Context);
        var interceptionResult = base.SavingChanges(eventData, result);
        DispatchDomainEvents(eventToDispatch).GetAwaiter().GetResult();
        return interceptionResult;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var eventToDispatch = GetDomainEvents(eventData.Context);
        var interceptionResult = base.SavingChangesAsync(eventData, result, cancellationToken);
        DispatchDomainEvents(eventToDispatch).GetAwaiter().GetResult();
        return interceptionResult;
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