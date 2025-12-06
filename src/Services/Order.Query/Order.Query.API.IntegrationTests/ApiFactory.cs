using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Order.Query.Api;
using Order.Query.API.IntegrationTests.Given.DbGiven;
using Order.Query.Events;
using Order.Query.Features.OrderView;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Order.Query.API.IntegrationTests;

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "TestCollection";
}

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    private readonly WebApiContainerFactory _webApiContainerFactory = new();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresConnectionString = _webApiContainerFactory.PostgresConnectionString;
        Environment.SetEnvironmentVariable("Logger__elasticsearch", _webApiContainerFactory.ElasticSearchUri);
        
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
            
            services
                .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer();
        });
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
    
    private DbGiven? _sqlGiven;
    private IDocumentStore? _store;
    
    internal IDocumentStore GetDocumentStore
    {
        get
        {
            _store ??= Services.GetRequiredService<IDocumentStore>();
            return _store;
        }
    }
    
    internal DbGiven DbGiven => _sqlGiven ??= new DbGiven(GetDocumentStore);
    
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;
    public async Task InitializeAsync()
    {
        await _webApiContainerFactory.InitializeAsync();
    }

    public new async Task DisposeAsync()
    { 
        await _webApiContainerFactory.DisposeAsync(); 
        HttpClient.Dispose();
    }
}