using Discount.Grpc.Data;
using Discount.Grpc.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Discount.Grpc.IntegrationTests;

public delegate void LogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", "Data Source=TestDiscountDb");
        builder.ConfigureTestServices(services =>
        {
            services.Remove(services.Single(x => x.ImplementationType == typeof(DiscountContext)));
            
            services.AddSingleton<ILoggerFactory>(LoggerFactory);
            services.AddDbContext<DiscountContext>(options =>
                options.UseSqlite("Data Source=TestDiscountDb"));
        });
    }
    
    public event LogMessage? LoggedMessage;

    public ApiFactory()
    {
        LoggerFactory = new LoggerFactory();
        LoggerFactory.AddProvider(new ForwardingLoggerProvider((logLevel, category, eventId, message, exception) =>
        {
            LoggedMessage?.Invoke(logLevel, category, eventId, message, exception);
        }));
    }

    public LoggerFactory LoggerFactory { get; }

    public IDisposable GetTestContext(ITestOutputHelper outputHelper)
    {
        return new GrpcTestContext(this, outputHelper);
    }
}