using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Order.Query.API.Authorization;

public static class AuthorizationExtension
{
    public static async Task<bool> CanGetOrderByIdAsync(this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(user, customerId, Policies.CanGetOrder)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
    
    public static async Task<bool> CanCanGetListOfOrdersByCustomerIdAsync(this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        string? customerId)
    {
        var result = await authorizationService.AuthorizeAsync(user, customerId, Policies.CanGetListOfOrdersByCustomerId)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
}