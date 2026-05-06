using Duende.IdentityServer.Models;

namespace Identity.API.Tests.UnitTests;

public class ConfigTests
{
    [Fact]
    public void IdentityResources_contains_openid_and_profile()
    {
        var names = Config.IdentityResources.Select(r => r.Name).ToList();
        names.ShouldContain("openid");
        names.ShouldContain("profile");
    }

    [Fact]
    public void ApiScopes_contains_all_four_microservices()
    {
        var names = Config.ApiScopes.Select(s => s.Name).ToList();
        names.ShouldContain("basket");
        names.ShouldContain("catalog");
        names.ShouldContain("orders_command");
        names.ShouldContain("orders_query");
    }

    [Fact]
    public void ApiResources_have_secrets_for_introspection()
    {
        foreach (var resource in Config.ApiResources)
        {
            resource.ApiSecrets.ShouldNotBeEmpty(
                $"ApiResource '{resource.Name}' must have a secret for introspection");
        }
    }

    [Fact]
    public void Clients_contains_all_four_configured_clients()
    {
        var ids = Config.Clients.Select(c => c.ClientId).ToList();
        ids.ShouldContain("postman-client");
        ids.ShouldContain("postman-client-password");
        ids.ShouldContain("device-client");
        ids.ShouldContain("ciba-client");
    }

    [Fact]
    public void PostmanClient_uses_authorization_code_with_pkce()
    {
        var client = Config.Clients.Single(c => c.ClientId == "postman-client");
        client.AllowedGrantTypes.ShouldContain(GrantType.AuthorizationCode);
        client.RequirePkce.ShouldBeTrue();
        client.RequireClientSecret.ShouldBeFalse();
    }

    [Fact]
    public void PasswordClient_uses_resource_owner_password_grant()
    {
        var client = Config.Clients.Single(c => c.ClientId == "postman-client-password");
        client.AllowedGrantTypes.ShouldContain(GrantType.ResourceOwnerPassword);
        client.RequirePkce.ShouldBeFalse();
    }

    [Fact]
    public void Customer_role_can_checkout_basket()
    {
        var rolesForCheckout = Config.RolePolicyDefinitions.PolicyToRoles
            .FirstOrDefault(kv => kv.Key == "basket:user-basket:checkout").Value;

        rolesForCheckout.ShouldNotBeNull();
        rolesForCheckout.ShouldContain(Config.Roles.Customer);
    }

    [Fact]
    public void Admin_role_cannot_checkout_basket()
    {
        var rolesForCheckout = Config.RolePolicyDefinitions.PolicyToRoles
            .FirstOrDefault(kv => kv.Key == "basket:user-basket:checkout").Value;

        rolesForCheckout.ShouldNotBeNull();
        rolesForCheckout.ShouldNotContain(Config.Roles.Admin);
    }

    [Fact]
    public void Admin_role_has_catalog_mutation_permissions()
    {
        var catalogMutationPolicies = new[]
        {
            "catalog:product:create",
            "catalog:product:delete",
            "catalog:product:update"
        };

        foreach (var policy in catalogMutationPolicies)
        {
            var roles = Config.RolePolicyDefinitions.PolicyToRoles[policy];
            roles.ShouldContain(Config.Roles.Admin,
                $"Admin should have policy '{policy}'");
            roles.ShouldNotContain(Config.Roles.Customer,
                $"Customer should NOT have policy '{policy}'");
        }
    }

    [Fact]
    public void PolicyToRoles_has_no_duplicate_assignments()
    {
        foreach (var (policy, roles) in Config.RolePolicyDefinitions.PolicyToRoles)
        {
            roles.Distinct().Count().ShouldBe(roles.Length,
                $"Policy '{policy}' has duplicate role assignments");
        }
    }
}
