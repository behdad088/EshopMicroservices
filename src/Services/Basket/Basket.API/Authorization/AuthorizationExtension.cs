using Microsoft.AspNetCore.Authorization;

namespace Basket.API.Authorization;

public static class AuthorizationExtension
{
    public static async Task<bool> CanDeleteBasketAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string username)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, username, Policies.CanDeleteBasket)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanGetBasketAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string username)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, username, Policies.CanGetBasket)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanStoreBasketAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string username)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, username, Policies.CanStoreBasket)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
}