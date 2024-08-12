using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Discount.Grpc.IntegrationTests.Helpers;

public class GrpcTestContext : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly ApiFactory _apiFactory;
    private readonly ITestOutputHelper _outputHelper;

    public GrpcTestContext(ApiFactory apiFactory, ITestOutputHelper outputHelper)
    {
        _stopwatch = Stopwatch.StartNew();
        _apiFactory = apiFactory;
        _outputHelper = outputHelper;
        _apiFactory.LoggedMessage += WriteMessage;
    }

    private void WriteMessage(LogLevel logLevel, string category, EventId eventId, string message, Exception? exception)
    {
        var log = $"{_stopwatch.Elapsed.TotalSeconds:N3}s {category} - {logLevel}: {message}";
        if (exception != null)
        {
            log += Environment.NewLine + exception;
        }
        _outputHelper.WriteLine(log);
    }

    public void Dispose()
    {
        _apiFactory.LoggedMessage -= WriteMessage;
    }
}