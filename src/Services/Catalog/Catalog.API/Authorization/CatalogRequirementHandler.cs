using Microsoft.AspNetCore.Authorization;

namespace Catalog.API.Authorization;

public class CatalogRequirement : IAuthorizationRequirement
{
    public CatalogRequirement(IEnumerable<string> requirements) =>
        Requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
    public IEnumerable<string> Requirements { get; }
}

public class CatalogRequirementHandler : AuthorizationHandler<CatalogRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CatalogRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions").Select(c => c.Value);

        if (requirement.Requirements.All(p => permissions.Contains(p)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}