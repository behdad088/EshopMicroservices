using Microsoft.Extensions.DependencyInjection;
using Order.Query.Events;
using Order.Query.Features.OrderView;

namespace Order.Query.PostgresConfig;

public static class PostgresDbExtension
{
    public static IServiceCollection AddPostgresDb(this IServiceCollection services, string connectionString)
    {
        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.UseSystemTextJsonForSerialization(); 
            options.Schema.For<OrderView>()
                .Index(x => x.CustomerId)
                .Index(x => x.OrderStatus)
                .Index(x => x.TotalPrice)
                .Index(x => x.CreatedAt)
                .FullTextIndex(x => x.OrderName!)
                .UniqueIndex(x => x.Id);
    
            options.Schema.For<EventStream>()
                .Index(x => x.Id)
                .Index(x => x.ViewId)
                .Index(x => x.EventType)
                .Index(x => x.CreatedAt)
                .UniqueIndex(x => x.Id);
        }).UseLightweightSessions();
        return services;
    }
}