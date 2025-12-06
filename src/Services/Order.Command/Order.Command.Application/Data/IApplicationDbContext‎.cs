using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Order.Command.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Domain.Models.Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Domain.Models.Outbox> Outboxes { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}