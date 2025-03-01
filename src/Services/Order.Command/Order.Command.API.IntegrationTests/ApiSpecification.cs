using Microsoft.Extensions.DependencyInjection;

namespace Order.Command.API.IntegrationTests;

public class ApiSpecification(WebApiContainerFactory containerFactory) : IAsyncLifetime
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    private ApiFactory? _factory;
    private HttpClient? _httpClient;

    internal HttpClient HttpClient()
    {
        _httpClient ??= _factory!.CreateClient();
        _httpClient.BaseAddress = new Uri($"{_httpClient.BaseAddress}api/v1/order/command/");
        
        return _httpClient;    
    }
    
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;

    internal SqlDbGiven SqlDbGiven => new(_factory!.Services.GetRequiredService<IApplicationDbContext>() ??
        throw new Exception("IApplicationDbContext is not initialized."));

    private IApplicationDbContext? _dbContext;

    internal IApplicationDbContext DbContext => _dbContext ??=
        _factory!.Services.GetRequiredService<IApplicationDbContext>() ??
        throw new Exception("IApplicationDbContext is not initialized.");
    
    public async Task InitializeAsync()
    {
        _factory = new ApiFactory(containerFactory.MssqlConnectionString, containerFactory.RmqConfiguration);

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        _httpClient?.Dispose();
    }
}