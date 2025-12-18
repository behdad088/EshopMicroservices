using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Basket.API.Authorization;

public class UserIdRequirement : IAuthorizationRequirement
{
    public UserIdRequirement(IEnumerable<string> requirements) =>
        Requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
    public IEnumerable<string> Requirements { get; }
}

public class UserAuthorizationHandler : AuthorizationHandler<UserIdRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserIdRequirement requirement,
        string username)
    {
        var userRole = GetUserRole(context.User);

        if (userRole != "admin")
        {
            var usernameInClaim = GetUsername(context.User);
            if (string.IsNullOrEmpty(usernameInClaim) || username != usernameInClaim)
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }

        if (!ValidateUserPermissions(
                context.User.Claims.ToArray(),
                requirement.Requirements.ToArray()))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }

    private static bool ValidateUserPermissions(
        IReadOnlyList<Claim> claims,
        IReadOnlyList<string> requirements
    )
    {
        var invalidClaims =
            requirements.Where(requirement => !claims.Any(claim => claim.Value.StartsWith(requirement)));

        if (invalidClaims.Any())
            return false;
        
        return true;
    }
    
    private static string? GetUsername(ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst("preferred_username")?.Value;
        return userId;
    }
    
    private static string GetUserRole(ClaimsPrincipal claimsPrincipal)
    {
        var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        return role ?? string.Empty;
    }
}