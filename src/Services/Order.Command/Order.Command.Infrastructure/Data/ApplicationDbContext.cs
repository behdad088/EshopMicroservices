using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Order.Command.Application.Data;
using Order.Command.Domain.Models;

namespace Order.Command.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Domain.Models.Order> Orders => Set<Domain.Models.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Outbox> Outboxes => Set<Outbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}