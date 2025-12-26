using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Command.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialisedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await SeedDbAsync(context);
    }

    private static async Task SeedDbAsync(ApplicationDbContext context)
    {
        await SeedOrderWithItemsAsync(context);
    }

    private static async Task SeedOrderWithItemsAsync(ApplicationDbContext context)
    {
        if (!await context.Orders.AnyAsync())
        {
            await context.Orders.AddRangeAsync(InitialData.OrdersWithItems());
            await context.SaveChangesAsync();
        }
    }
}