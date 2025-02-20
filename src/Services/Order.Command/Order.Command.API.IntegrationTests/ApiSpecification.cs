namespace Order.Command.API.IntegrationTests;

[Collection(GetWebApiContainerFactory.Name)]
public class ApiSpecification(WebApiContainerFactory containerFactory) : IAsyncLifetime
{
    private readonly CancellationTokenSource _timeoutCancellationTokenSource = new(TimeSpan.FromSeconds(30));
    private ApiFactory? _factory;
    private HttpClient? _httpClient;
    
    internal HttpClient HttpClient => _httpClient ??= _factory!.CreateClient();
    internal CancellationToken CancellationToken => _timeoutCancellationTokenSource.Token;

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