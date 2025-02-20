using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Command.Infrastructure.Data;

namespace Order.Command.API.IntegrationTests;

public class ApiFactory(string mssqlConnectionString, RmqConfiguration rmqConfiguration) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", mssqlConnectionString);
        Environment.SetEnvironmentVariable("RabbitMQ__URI", rmqConfiguration.Uri);
        Environment.SetEnvironmentVariable("RabbitMQ__Username", rmqConfiguration.Username);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", rmqConfiguration.Password);
        builder.ConfigureTestServices(services =>
        {
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.UsingRabbitMq((context, rabbitMqCfg) =>
                {
                    rabbitMqCfg.Host(rmqConfiguration.Uri, "/", h =>
                    {
                        h.Username(rmqConfiguration.Username);
                        h.Password(rmqConfiguration.Password);
                    });
            
                    rabbitMqCfg.ConfigureEndpoints(context);
                });
            });
            
            var sp = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            sp.Database.MigrateAsync().GetAwaiter().GetResult();
        }).UseEnvironment("Test");
    }
}