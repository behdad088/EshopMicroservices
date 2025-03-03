using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Order.Command.Application.Data;
using Order.Command.Infrastructure.Data;
using Order.Command.Infrastructure.Data.Interceptors;

namespace Order.Command.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<IDbTransactionInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, option) =>
        {
            option.AddInterceptors(sp.GetRequiredService<IDbTransactionInterceptor>(), new AuditableEntityInterceptor());
            option.UseSqlServer(connectionString);
        });

        services.AddTransient<IApplicationDbContext, ApplicationDbContext>();

        return services;
    }
}