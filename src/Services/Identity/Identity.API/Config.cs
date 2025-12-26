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
        public const string BasketCanCheckoutBasket = "basket:user-basket:checkout";
        
        // Define policies for catalog operations
        public const string CatalogCanCreateCatalog = "catalog:product:create";
        public const string CatalogCanDeleteCatalog = "catalog:product:delete";
        public const string CatalogCanUpdateCatalog = "catalog:product:update";
        
        // Define policies for orders command operations
        public const string OrdersCommandCanCreateOrder = "orders_command:order:create";
        public const string OrdersCommandCanDeleteOrder = "orders_command:order:delete";
        public const string OrdersCommandCanGetOrder = "orders_command:order:get";
        public const string OrdersCommandCanGetAllOrders = "orders_command:order:all";
        public const string OrdersCommandCanGetOrdersListsByCustomerId = "orders_command:order:all-by-customer-id";
        public const string OrdersCommandCanGetOrdersListsByOrderName = "orders_command:order:all-by-order-name";
        public const string OrdersCommandCanUpdateOrder = "orders_command:order:update";
        
        // Define policies for orders query operations
        public const string OrdersQueryCanGetOrder = "orders_query:order:get";
        public const string OrdersQueryCanGetAllOrders = "orders_query:order:all";
        public const string OrdersQueryCanGetOrdersListsByCustomerId = "orders_query:order:all-by-customer-id";
        public const string OrdersQueryCanGetOrdersListsByOrderName = "orders_query:order:all-by-order-name";
    }
    public static class RolePolicyDefinitions
    {
        public static readonly Dictionary<string, string[]> PolicyToRoles = new()
        {
            { Policies.BasketCanDeleteBasket, [Roles.Admin , Roles.Customer] },
            { Policies.BasketCanStoreBasket, [Roles.Admin, Roles.Customer] },
            { Policies.BasketCanGetBasket, [Roles.Admin , Roles.Customer] },
            { Policies.BasketCanCheckoutBasket, [ Roles.Customer] },
            
            { Policies.CatalogCanCreateCatalog, [Roles.Admin] },
            { Policies.CatalogCanDeleteCatalog, [Roles.Admin] },
            { Policies.CatalogCanUpdateCatalog, [Roles.Admin] },
            
            { Policies.OrdersCommandCanCreateOrder, [ Roles.Customer] },
            { Policies.OrdersCommandCanDeleteOrder, [Roles.Admin] },
            { Policies.OrdersCommandCanGetOrder, [Roles.Admin, Roles.Customer] },
            { Policies.OrdersCommandCanGetAllOrders, [Roles.Admin] },
            { Policies.OrdersCommandCanGetOrdersListsByCustomerId, [Roles.Admin, Roles.Customer] },
            { Policies.OrdersCommandCanGetOrdersListsByOrderName, [Roles.Admin] },
            { Policies.OrdersCommandCanUpdateOrder, [Roles.Admin, Roles.Customer] },
            
            { Policies.OrdersQueryCanGetOrder, [Roles.Admin, Roles.Customer] },
            { Policies.OrdersQueryCanGetAllOrders, [Roles.Admin] },
            { Policies.OrdersQueryCanGetOrdersListsByCustomerId, [Roles.Admin, Roles.Customer] },
            { Policies.OrdersQueryCanGetOrdersListsByOrderName, [Roles.Admin] },
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
        public const string OrdersQuery = "orders_query";
    }
    
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(ScopeNames.Basket, "Basket Service"),
            new ApiScope(ScopeNames.Catalog, "Catalog Service"),
            new ApiScope(ScopeNames.OrdersCommand, "Orders Command Service"),
            new ApiScope(ScopeNames.OrdersQuery, "Orders Query Service")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("basket", "Basket API")
        {
            Scopes = { ScopeNames.Basket }
        },
        new ApiResource("catalog", "Catalog API")
        {
            Scopes = { ScopeNames.Catalog }
        },
        new ApiResource("order.command", "Order Command API")
        {
            Scopes = { ScopeNames.OrdersCommand }
        },
        new ApiResource("order.query", "Order Query API")
        {
            Scopes = { ScopeNames.OrdersQuery }
        }
    ];
    
    public static IEnumerable<Client> Clients =>
    [
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
                    ScopeNames.OrdersCommand,
                    ScopeNames.OrdersQuery
                },
                AllowAccessTokensViaBrowser = true
            }
    ];
}