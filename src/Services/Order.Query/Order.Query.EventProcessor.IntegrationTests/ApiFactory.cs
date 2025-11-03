using Marten;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Order.Query.EventProcessor.IntegrationTests.Given.DbGiven;
using Order.Query.EventProcessor.IntegrationTests.Masstransit;
using Order.Query.Events;
using Order.Query.Features.OrderView;

namespace Order.Query.EventProcessor.IntegrationTests;

public class ApiFactory(WebApiContainerFactory webApiContainerFactory) : WebApplicationFactory<Program>
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));

    private DbGiven? _sqlGiven;
    private TestConsumeObserver? _consumeObserver = null;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresConnectionString = webApiContainerFactory.PostgresConnectionString;
        var rmqConfiguration = webApiContainerFactory.RmqConfiguration;
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", postgresConnectionString);
        Environment.SetEnvironmentVariable("RabbitMQ__URI", rmqConfiguration.Uri);
        Environment.SetEnvironmentVariable("RabbitMQ__Username", rmqConfiguration.Username);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", rmqConfiguration.Password);
        
        builder.ConfigureTestServices(services =>
        {
            services.AddMarten(options =>
            {
                options.Connection(postgresConnectionString);
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
        });
    }

    private IDocumentStore? _store;
    internal IDocumentStore GetDocumentStore
    {
        get
        {
            _store ??= Services.GetRequiredService<IDocumentStore>();
            return _store;
        }
    }
    
    private HttpClient? _httpClient;
    internal HttpClient HttpClient
    {
        get
        {
            _httpClient ??= CreateClient();
            return _httpClient;
        }
    }
    
    internal ITestHarness TestHarness => Services.GetTestHarness();
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;

    internal DbGiven DbGiven => _sqlGiven ??= new DbGiven(GetDocumentStore);
    
    internal TestConsumeObserver ConsumeObserver => _consumeObserver ??= new TestConsumeObserver(TestHarness);
}