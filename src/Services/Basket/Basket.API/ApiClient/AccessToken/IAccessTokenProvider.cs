namespace Basket.API.ApiClient.AccessToken;

public interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
}