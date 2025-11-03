using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Order.Query.Api;

namespace Order.Query.API.IntegrationTests;

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "TestCollection";
}

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {

        });
    }
    
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;
}