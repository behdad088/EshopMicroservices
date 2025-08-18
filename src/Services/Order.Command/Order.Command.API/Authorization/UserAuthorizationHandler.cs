using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Order.Command.API.Authorization;

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
        string customerId)
    {
        var userRole = GetUserRole(context.User);
        if (userRole == string.Empty)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        if (userRole == "admin")
        {
            if (!ValidateUserPermissions(
                    context.User.Claims.ToList(),
                    requirement.Requirements.ToList()))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        var userIdInClaim = GetUserId(context.User);
        if (customerId != userIdInClaim)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        if (!ValidateUserPermissions(
                context.User.Claims.ToList(),
                requirement.Requirements.ToList()))
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
    
    private static string? GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }
    
    private static string GetUserRole(ClaimsPrincipal claimsPrincipal)
    {
        var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
        return role ?? string.Empty;
    }
}
