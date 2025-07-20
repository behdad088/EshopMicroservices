using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.API;

public static class Config
{
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Customer = "customer";
    }
    
    private static class ScopeNames
    {
        public const string Basket = "basket";
        public const string OrdersCommand = "orders_command";
    }
    
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(ScopeNames.Basket, "Basket Service"),
            new ApiScope(ScopeNames.OrdersCommand, "Orders Command Service")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "postman-client",
                RequirePkce = true,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://oauth.pstmn.io/v1/browser-callback" },
                AllowedScopes = {  
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    ScopeNames.Basket,
                    ScopeNames.OrdersCommand
                },
                AllowAccessTokensViaBrowser = true
            }

        };
}