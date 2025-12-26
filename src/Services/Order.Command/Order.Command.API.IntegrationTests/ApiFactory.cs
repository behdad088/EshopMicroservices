using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Order.Command.Infrastructure.Data;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Order.Command.API.IntegrationTests;

public class ApiFactory(
    string postgresConnectionString,
    RmqConfiguration rmqConfiguration,
    string elasticSearchString
    ) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", postgresConnectionString);
        Environment.SetEnvironmentVariable("RabbitMQ__URI", rmqConfiguration.Uri);
        Environment.SetEnvironmentVariable("RabbitMQ__Username", rmqConfiguration.Username);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", rmqConfiguration.Password);
        Environment.SetEnvironmentVariable("Logger__elasticsearch", elasticSearchString);
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
            sp.Database.EnsureDeletedAsync().GetAwaiter().GetResult();
            sp.Database.MigrateAsync().GetAwaiter().GetResult();
            
            services
                .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer();
        }).UseEnvironment("Test");
    }
    
    
}