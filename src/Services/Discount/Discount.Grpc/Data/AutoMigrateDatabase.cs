using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Data;

public static class AutoMigrateDatabase
{
    public static IServiceCollection MigrateDatabase(this IServiceCollection serviceCollection)
    {
        using var scope = serviceCollection.BuildServiceProvider().CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
        dbContext.Database.MigrateAsync();
        return serviceCollection;
    }

}