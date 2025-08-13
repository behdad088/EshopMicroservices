using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.API;

public static class Config
{
    private static class Policies
    {
        // Define policies for basket operations
        public const string BasketCanDeleteBasket = "basket:user-basket:delete";
        public const string BasketCanGetBasket = "basket:user-basket:get";
        public const string BasketCanStoreBasket = "basket:user-basket:store";
        
        // Define policies for catalog operations
        public const string CatalogCanCreateCatalog = "catalog:product:create";
        public const string CatalogCanDeleteCatalog = "catalog:product:delete";
        public const string CatalogCanUpdateCatalog = "catalog:product:update";
        
    }
    public static class RolePolicyDefinitions
    {
        public static readonly Dictionary<string, string[]> PolicyToRoles = new()
        {
            { Policies.BasketCanDeleteBasket, [Roles.Admin , Roles.Customer] },
            { Policies.BasketCanStoreBasket, [Roles.Admin, Roles.Customer] },
            { Policies.BasketCanGetBasket, [Roles.Admin , Roles.Customer] },
            { Policies.CatalogCanCreateCatalog, [Roles.Admin] },
            { Policies.CatalogCanDeleteCatalog, [Roles.Admin] },
            { Policies.CatalogCanUpdateCatalog, [Roles.Admin] }
        };
    }
    
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Customer = "customer";
    }
    
    private static class ScopeNames
    {
        public const string Basket = "basket";
        public const string Catalog = "catalog";
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
            new ApiScope(ScopeNames.Catalog, "Catalog Service"),
            new ApiScope(ScopeNames.OrdersCommand, "Orders Command Service")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new[]
        {
            new ApiResource("basket", "Basket API")
            {
                Scopes = { ScopeNames.Basket }
            },
            new ApiResource("catalog", "Catalog API")
            {
                Scopes = { ScopeNames.Catalog }
            }
        };
    
    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "postman-client",
                AllowedGrantTypes = GrantTypes.Implicit,
                RedirectUris = { "https://oauth.pstmn.io/v1/browser-callback" },
                AllowedScopes = {  
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    ScopeNames.Basket,
                    ScopeNames.Catalog,
                    ScopeNames.OrdersCommand
                },
                AllowAccessTokensViaBrowser = true
            }
        };
}