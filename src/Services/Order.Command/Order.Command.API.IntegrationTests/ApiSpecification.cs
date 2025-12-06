using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace Order.Command.API.IntegrationTests;

public class ApiSpecification(WebApiContainerFactory containerFactory) : IAsyncLifetime
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    private HttpClient? _httpClient;
    private ApiFactory? _apiFactory;
    
    internal HttpClient HttpClient()
    {
        var client = _httpClient ??= _apiFactory!.CreateClient();
        client.ClearDefaultHeaders();
        return client;
    }
    
    internal ITestHarness TestHarness => _apiFactory!.Services.GetTestHarness();
    
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;

    internal SqlDbGiven SqlDbGiven => new(_apiFactory!.Services.GetRequiredService<IApplicationDbContext>() ??
        throw new Exception("IApplicationDbContext is not initialized."));

    private IApplicationDbContext? _dbContext;

    internal IApplicationDbContext DbContext => _dbContext ??=
        _apiFactory!.Services.GetRequiredService<IApplicationDbContext>() ??
        throw new Exception("IApplicationDbContext is not initialized.");
    
    public async Task ClearDatabaseAsync()
    {
        await using var connection = new SqlConnection(containerFactory.MssqlConnectionString);
        await connection.OpenAsync(CancellationToken.None);
        
        await _respawner.ResetAsync(connection);
        await Task.CompletedTask;
    }
    
    private Respawner _respawner = default!;
    
    public async Task InitializeAsync()
    {
        _apiFactory = new ApiFactory(
            containerFactory.MssqlConnectionString,
            containerFactory.RmqConfiguration,
            containerFactory.ElasticSearchUri);
        _ = _apiFactory.CreateClient();

        await using var connection = new SqlConnection(containerFactory.MssqlConnectionString);
        await connection.OpenAsync(CancellationToken);
        
        _respawner = Respawner.CreateAsync(connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = ["dbo"],
                TablesToIgnore = ["__EFMigrationsHistory"]
            }).GetAwaiter().GetResult();
        
    }

    public async Task DisposeAsync()
    {
        if (_apiFactory is not null)
            await _apiFactory.DisposeAsync();
        
        _httpClient?.Dispose();
    }
}