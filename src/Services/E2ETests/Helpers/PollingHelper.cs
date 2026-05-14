namespace E2ETests.Helpers;

public static class PollingHelper
{
    public static async Task<T> WaitForAsync<T>(
        Func<Task<T?>> action,
        Func<T, bool> predicate,
        int timeoutSeconds  = 30,
        int intervalSeconds = 2)
        where T : class
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var result = await action();
            if (result is not null && predicate(result))
                return result;

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
        }

        throw new TimeoutException(
            $"Condition was not met within {timeoutSeconds} seconds.");
    }
}
