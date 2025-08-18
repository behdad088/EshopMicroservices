using Microsoft.AspNetCore.Authorization;

namespace Order.Command.API.Authorization;

public static class AuthorizationExtension
{
    public static async Task<bool> CanCreateOrderAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, customerId, Policies.CanCreateOrder)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanDeleteOrderAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, customerId, Policies.CanDeleteOrder)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanGetOrderByIdAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, customerId, Policies.CanGetOrder)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanGetOrdersByCustomerIdAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, customerId, Policies.CanGetListOfOrdersByCustomerId)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanGetUpdateOrderAsync(this IAuthorizationService authorizationService,
        HttpContext httpContext,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(httpContext.User, customerId, Policies.CanUpdateOrder)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
}