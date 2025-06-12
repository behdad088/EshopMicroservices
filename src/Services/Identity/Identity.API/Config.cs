using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.API;

public static class Config
{
    private class ScopeNames
    {
        public const string Basket = "basket";
        public const string OrdersCommand = "orders_command";
        public const string Discount = "discount";
    }
    
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(ScopeNames.Basket, "Basket Service"),
            new ApiScope(ScopeNames.OrdersCommand, "Orders Command Service"),
            new ApiScope(ScopeNames.Discount, "Discount Service"),
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
                    ScopeNames.OrdersCommand,
                    ScopeNames.Discount },
                AllowAccessTokensViaBrowser = true
            }

        };
}