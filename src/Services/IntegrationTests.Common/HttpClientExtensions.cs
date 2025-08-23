namespace IntegrationTests.Common;

public static class HttpClientExtensions
{
    public static void ClearDefaultHeaders(this HttpClient client)
    {
        foreach (var header in client.DefaultRequestHeaders.ToList())
        {
            client.DefaultRequestHeaders.Remove(header.Key);
        }
    }
}