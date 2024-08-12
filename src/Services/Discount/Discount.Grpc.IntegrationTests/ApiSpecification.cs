using Discount.Grpc.IntegrationTests.Database;
using Microsoft.Extensions.Logging;

namespace Discount.Grpc.IntegrationTests;

public class ApiSpecification(ITestOutputHelper outputHelper) : IAsyncLifetime
{
    private IDisposable? _testContext;
    private GrpcChannel? _channel;
    private HttpClient? _httpClient;
    private ApiFactory? ApiFactory { get; set; }
    private DatabaseSeeder? _databaseSeeder;

    public DatabaseSeeder DatabaseSeeder => _databaseSeeder!;
    
    private ILoggerFactory LoggerFactory => ApiFactory!.LoggerFactory;
    internal GrpcChannel Channel => _channel ??= CreateChannel();
    private HttpClient HttpClient => _httpClient ??= ApiFactory!.CreateDefaultClient();
    
    internal CancellationToken CancellationToken => new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token; 
    
    private GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress(HttpClient.BaseAddress!, new GrpcChannelOptions
        {
            LoggerFactory = LoggerFactory,
            HttpClient = HttpClient
        });
    }
    
    public async Task InitializeAsync()
    {
        ApiFactory = new ApiFactory();
        _testContext = ApiFactory.GetTestContext(outputHelper);
        _httpClient = ApiFactory.CreateDefaultClient();
        _databaseSeeder = new DatabaseSeeder(ApiFactory!.Services);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _testContext?.Dispose();
        _databaseSeeder?.Dispose();
        
        _channel = null;
        
        if (ApiFactory is not null) 
            await ApiFactory.DisposeAsync().ConfigureAwait(false);

    }
}