namespace Basket.API.ApiClient.AccessToken;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GetAccessTokenAsync()
    {
        var token = _httpContextAccessor.HttpContext
            ?.Request.Headers.Authorization.ToString()
            .Replace("Bearer ", string.Empty) ?? string.Empty;

        return Task.FromResult(token);
    }
}