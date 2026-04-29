using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Identity.API;

public static class Config
{
    // -------------------------------------------------------------------------
    // POLICIES
    // -------------------------------------------------------------------------
    // Policies are fine-grained permission strings that get embedded into the
    // access token as "permissions" claims via SeedData.cs > AddRoles().
    // Each downstream service (basket, catalog, etc.) reads these claims to
    // decide whether the caller is allowed to perform a specific action.
    // Format convention: "<service>:<resource>:<action>"
    // -------------------------------------------------------------------------
    private static class Policies
    {
        // Basket service — controls who can read, write, and checkout a basket
        public const string BasketCanDeleteBasket   = "basket:user-basket:delete";
        public const string BasketCanGetBasket      = "basket:user-basket:get";
        public const string BasketCanStoreBasket    = "basket:user-basket:store";
        public const string BasketCanCheckoutBasket = "basket:user-basket:checkout";

        // Catalog service — write operations are admin-only; reads are open
        public const string CatalogCanCreateCatalog = "catalog:product:create";
        public const string CatalogCanDeleteCatalog = "catalog:product:delete";
        public const string CatalogCanUpdateCatalog = "catalog:product:update";

        // Orders Command service — handles writes (create, update, delete)
        public const string OrdersCommandCanCreateOrder                  = "orders_command:order:create";
        public const string OrdersCommandCanDeleteOrder                  = "orders_command:order:delete";
        public const string OrdersCommandCanGetOrder                     = "orders_command:order:get";
        public const string OrdersCommandCanGetAllOrders                 = "orders_command:order:all";
        public const string OrdersCommandCanGetOrdersListsByCustomerId   = "orders_command:order:all-by-customer-id";
        public const string OrdersCommandCanGetOrdersListsByOrderName    = "orders_command:order:all-by-order-name";
        public const string OrdersCommandCanUpdateOrder                  = "orders_command:order:update";

        // Orders Query service — handles reads only (CQRS read side)
        public const string OrdersQueryCanGetOrder                       = "orders_query:order:get";
        public const string OrdersQueryCanGetAllOrders                   = "orders_query:order:all";
        public const string OrdersQueryCanGetOrdersListsByCustomerId     = "orders_query:order:all-by-customer-id";
        public const string OrdersQueryCanGetOrdersListsByOrderName      = "orders_query:order:all-by-order-name";
    }

    // -------------------------------------------------------------------------
    // ROLE → POLICY MAPPING
    // -------------------------------------------------------------------------
    // This dictionary is read by SeedData.cs to attach permission claims to
    // each role in the database at startup. When a user logs in, ProfileService
    // reads their role's claims and injects them into the access token as an
    // array of "permissions" strings.
    //
    // Design decision: roles are coarse (admin / customer), policies are fine.
    // Downstream services authorize against policies, not roles directly, which
    // keeps authorization logic out of the identity server.
    // -------------------------------------------------------------------------
    public static class RolePolicyDefinitions
    {
        public static readonly Dictionary<string, string[]> PolicyToRoles = new()
        {
            // Both admin and customer can manage their own basket
            { Policies.BasketCanDeleteBasket,   [Roles.Admin, Roles.Customer] },
            { Policies.BasketCanStoreBasket,    [Roles.Admin, Roles.Customer] },
            { Policies.BasketCanGetBasket,      [Roles.Admin, Roles.Customer] },
            // Only customers can checkout — admins manage the system, not shop
            { Policies.BasketCanCheckoutBasket, [Roles.Customer] },

            // Catalog mutations are admin-only
            { Policies.CatalogCanCreateCatalog, [Roles.Admin] },
            { Policies.CatalogCanDeleteCatalog, [Roles.Admin] },
            { Policies.CatalogCanUpdateCatalog, [Roles.Admin] },

            // Only customers create orders; admin can delete/view all
            { Policies.OrdersCommandCanCreateOrder,                [Roles.Customer] },
            { Policies.OrdersCommandCanDeleteOrder,                [Roles.Admin] },
            { Policies.OrdersCommandCanGetOrder,                   [Roles.Admin, Roles.Customer] },
            { Policies.OrdersCommandCanGetAllOrders,               [Roles.Admin] },
            { Policies.OrdersCommandCanGetOrdersListsByCustomerId, [Roles.Admin, Roles.Customer] },
            { Policies.OrdersCommandCanGetOrdersListsByOrderName,  [Roles.Admin] },
            { Policies.OrdersCommandCanUpdateOrder,                [Roles.Admin, Roles.Customer] },

            // Query side mirrors command read permissions
            { Policies.OrdersQueryCanGetOrder,                     [Roles.Admin, Roles.Customer] },
            { Policies.OrdersQueryCanGetAllOrders,                 [Roles.Admin] },
            { Policies.OrdersQueryCanGetOrdersListsByCustomerId,   [Roles.Admin, Roles.Customer] },
            { Policies.OrdersQueryCanGetOrdersListsByOrderName,    [Roles.Admin] },
        };
    }

    // -------------------------------------------------------------------------
    // ROLES
    // -------------------------------------------------------------------------
    // Two coarse roles exist in the system. Role membership is stored in
    // AspNetUserRoles (via ASP.NET Identity) and seeded in SeedData.cs.
    //   admin    → Bob Smith  (BobSmith@email.com  / Pass123$)
    //   customer → Alice Smith (AliceSmith@email.com / Pass123$)
    // -------------------------------------------------------------------------
    public static class Roles
    {
        public const string Admin    = "admin";
        public const string Customer = "customer";
    }

    // -------------------------------------------------------------------------
    // SCOPE NAME CONSTANTS
    // -------------------------------------------------------------------------
    // Scopes map 1-to-1 with downstream microservices. A client must request
    // the relevant scope to receive an access token the service will accept.
    // Keeping them in a private class avoids magic strings scattered across
    // ApiScopes, ApiResources, and Client definitions below.
    // -------------------------------------------------------------------------
    private static class ScopeNames
    {
        public const string Basket        = "basket";
        public const string Catalog       = "catalog";
        public const string OrdersCommand = "orders_command";
        public const string OrdersQuery   = "orders_query";
    }

    // -------------------------------------------------------------------------
    // IDENTITY RESOURCES
    // -------------------------------------------------------------------------
    // Identity resources represent claims about the user that can be requested
    // via the "openid" and "profile" scopes. These are returned via the
    // /connect/userinfo endpoint and embedded in the ID token.
    //   openid  → mandatory for all OIDC flows; provides the "sub" claim
    //   profile → standard profile claims: name, given_name, family_name, etc.
    // -------------------------------------------------------------------------
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };

    // -------------------------------------------------------------------------
    // API SCOPES
    // -------------------------------------------------------------------------
    // API scopes are the permission labels a client requests in order to get an
    // access token that downstream services will accept. Each scope corresponds
    // to one microservice. Clients list which scopes they are allowed to request
    // in their AllowedScopes property below.
    // -------------------------------------------------------------------------
    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(ScopeNames.Basket,        "Basket Service"),
        new ApiScope(ScopeNames.Catalog,       "Catalog Service"),
        new ApiScope(ScopeNames.OrdersCommand, "Orders Command Service"),
        new ApiScope(ScopeNames.OrdersQuery,   "Orders Query Service")
    ];

    // -------------------------------------------------------------------------
    // API RESOURCES
    // -------------------------------------------------------------------------
    // API resources represent the actual protected services. Each resource:
    //   1. Declares which scopes grant access to it (Scopes list).
    //   2. Has an ApiSecret so it can call POST /connect/introspect with HTTP
    //      Basic Auth (resource name + secret) to validate incoming tokens.
    //
    // WHY ApiSecrets were added:
    //   Previously ApiResources had no secrets, which meant the downstream
    //   services had no way to authenticate themselves when calling the
    //   introspection endpoint. Without a secret, /connect/introspect returns
    //   401. Each service should store its own secret in its own appsettings
    //   or environment variables and never share it.
    //
    // Introspection curl example for the basket service:
    //   curl -u 'basket:basket-secret' \
    //        --data 'token=<access_token>' \
    //        https://localhost:5007/connect/introspect
    //
    // NOTE: In production, replace these plain-text secrets with values loaded
    // from a secrets manager (e.g. Azure Key Vault, AWS Secrets Manager) and
    // hash them with .Sha256() as shown below.
    // -------------------------------------------------------------------------
    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("basket", "Basket API")
        {
            Scopes = { ScopeNames.Basket },
            // Secret used by the Basket service to authenticate at /connect/introspect
            ApiSecrets = { new Secret("basket-secret".Sha256()) }
        },
        new ApiResource("catalog", "Catalog API")
        {
            Scopes = { ScopeNames.Catalog },
            // Secret used by the Catalog service to authenticate at /connect/introspect
            ApiSecrets = { new Secret("catalog-secret".Sha256()) }
        },
        new ApiResource("order.command", "Order Command API")
        {
            Scopes = { ScopeNames.OrdersCommand },
            // Secret used by the Orders Command service at /connect/introspect
            ApiSecrets = { new Secret("order-command-secret".Sha256()) }
        },
        new ApiResource("order.query", "Order Query API")
        {
            Scopes = { ScopeNames.OrdersQuery },
            // Secret used by the Orders Query service at /connect/introspect
            ApiSecrets = { new Secret("order-query-secret".Sha256()) }
        }
    ];

    // -------------------------------------------------------------------------
    // CLIENTS
    // -------------------------------------------------------------------------
    // A client is any application that requests tokens from IdentityServer.
    // Each client has a specific grant type that determines HOW it obtains
    // tokens. Choosing the wrong grant type is the most common source of
    // invalid_scope, invalid_request, and 403 errors.
    // -------------------------------------------------------------------------
    public static IEnumerable<Client> Clients =>
    [
        // ---------------------------------------------------------------------
        // CLIENT 1: postman-client — Authorization Code + PKCE
        // ---------------------------------------------------------------------
        // USE CASE: Interactive browser-based login from Postman's "Get New
        // Access Token" UI. The user is redirected to the IdentityServer login
        // page, enters credentials, and Postman handles the code exchange
        // automatically behind the scenes.
        //
        // HOW IT WORKS (2 steps):
        //   Step 1 → Browser: GET /connect/authorize → login page → redirect
        //            back to Postman with ?code=ABC
        //   Step 2 → Postman: POST /connect/token (grant_type=authorization_code,
        //            code=ABC, code_verifier=...) → receives access_token + id_token
        //
        // WHY Code over Implicit:
        //   Implicit delivers tokens directly in the browser URL fragment, which
        //   exposes them to browser history, referrer headers, and JavaScript.
        //   Code flow keeps tokens off the URL entirely — only a short-lived,
        //   single-use code is in the URL.
        //
        // WHY RequirePkce = true:
        //   PKCE (Proof Key for Code Exchange) prevents authorization code
        //   interception attacks. The client generates a random code_verifier,
        //   hashes it to a code_challenge sent in step 1, then proves it owns
        //   the original verifier in step 2. Without PKCE a stolen code can be
        //   exchanged for tokens by an attacker.
        //
        // WHY RequireClientSecret = false:
        //   Postman is a public client — it cannot securely store a secret
        //   (anyone can read Postman's environment variables). PKCE replaces
        //   the secret for public clients.
        //
        // Postman setup:
        //   Grant Type:           Authorization Code (With PKCE)
        //   Auth URL:             https://localhost:5007/connect/authorize
        //   Access Token URL:     https://localhost:5007/connect/token
        //   Client ID:            postman-client
        //   Scope:                openid profile basket catalog orders_command orders_query
        //   Code Challenge Method: SHA-256
        // ---------------------------------------------------------------------
        new Client
        {
            ClientId             = "postman-client",
            AllowedGrantTypes    = GrantTypes.Code,
            RequirePkce          = true,
            RequireClientSecret  = false,
            RedirectUris         = { "https://oauth.pstmn.io/v1/browser-callback" },
            PostLogoutRedirectUris = { "https://oauth.pstmn.io/v1/browser-callback" },
            AllowedScopes        =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                ScopeNames.Basket,
                ScopeNames.Catalog,
                ScopeNames.OrdersCommand,
                ScopeNames.OrdersQuery
            },
            // Needed so the access token can be returned to a browser-based
            // client like Postman without being blocked by IdentityServer's
            // default restriction on returning tokens via the front channel.
            AllowAccessTokensViaBrowser = true,
            // Enables the offline_access scope so a refresh token is issued,
            // allowing Postman to get new access tokens without re-logging in.
            AllowOfflineAccess   = true
        },

        // ---------------------------------------------------------------------
        // CLIENT 2: postman-client-password — Resource Owner Password + Client Credentials
        // ---------------------------------------------------------------------
        // USE CASE: Direct curl / automated script token requests where there
        // is no browser available. The username and password are sent directly
        // to POST /connect/token without any redirect.
        //
        // HOW IT WORKS (1 step):
        //   POST /connect/token
        //     grant_type=password
        //     client_id=postman-client-password
        //     username=AliceSmith@email.com
        //     password=Pass123$
        //     scope=openid profile basket ...
        //   → receives access_token immediately
        //
        // WHY this grant type:
        //   ResourceOwnerPasswordAndClientCredentials combines two grants:
        //     - password          → user logs in with username/password directly
        //     - client_credentials → machine-to-machine with no user context
        //   This is convenient for testing both scenarios with one client.
        //
        // WHY RequirePkce = false:
        //   PKCE only applies to the authorization_code flow. There is no
        //   redirect or code exchange in the password grant, so PKCE is not
        //   applicable and must be disabled or IdentityServer returns
        //   invalid_request.
        //
        // SECURITY NOTE: The password grant is considered legacy by OAuth 2.1
        // and should only be used for testing/development. Never use it in
        // production — use authorization_code + PKCE instead.
        //
        // Seed users:
        //   AliceSmith@email.com / Pass123$ → role: customer
        //   BobSmith@email.com   / Pass123$ → role: admin
        // ---------------------------------------------------------------------
        new Client
        {
            ClientId            = "postman-client-password",
            AllowedGrantTypes   = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
            RequirePkce         = false,
            RequireClientSecret = false,
            AllowedScopes       =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                ScopeNames.Basket,
                ScopeNames.Catalog,
                ScopeNames.OrdersCommand,
                ScopeNames.OrdersQuery
            },
            AllowOfflineAccess  = true
        },

        // ---------------------------------------------------------------------
        // CLIENT 3: device-client — Device Authorization Flow
        // ---------------------------------------------------------------------
        // USE CASE: Input-constrained devices (smart TVs, CLIs, IoT) that
        // cannot open a browser or accept a redirect URI. The device displays
        // a short user code and URL; the user goes to that URL on a separate
        // device (phone/PC), enters the code, and logs in. The device polls
        // /connect/token until the user approves.
        //
        // HOW IT WORKS (3 steps):
        //   Step 1 → Device: POST /connect/deviceauthorization
        //            (client_id=device-client, scope=...) 
        //            → receives device_code, user_code, verification_uri
        //   Step 2 → User: opens verification_uri on another device, enters
        //            user_code, and logs in interactively
        //   Step 3 → Device: polls POST /connect/token
        //            (grant_type=urn:ietf:params:oauth:grant-type:device_code,
        //             device_code=...) until authorization_pending clears
        //            → receives access_token
        //
        // WHY no RedirectUris:
        //   Device flow has no redirect. The device polls for the token
        //   instead of receiving it via a callback URL.
        //
        // WHY AllowOfflineAccess = true:
        //   Devices typically run unattended for long periods. Refresh tokens
        //   let them renew access without requiring the user to re-authenticate.
        // ---------------------------------------------------------------------
        new Client
        {
            ClientId            = "device-client",
            AllowedGrantTypes   = GrantTypes.DeviceFlow,
            RequireClientSecret = false,
            AllowOfflineAccess  = true,
            AllowedScopes       =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                ScopeNames.Basket,
                ScopeNames.Catalog,
                ScopeNames.OrdersCommand,
                ScopeNames.OrdersQuery
            }
        },

        // ---------------------------------------------------------------------
        // CLIENT 4: ciba-client — Client Initiated Backchannel Authentication
        // ---------------------------------------------------------------------
        // USE CASE: The client application (e.g. a call-centre agent app or a
        // payment terminal) initiates authentication for a user on their phone
        // without requiring the user to be at the same device. A push
        // notification is sent to the user's phone; they approve it there and
        // the client receives a token.
        //
        // HOW IT WORKS (3 steps):
        //   Step 1 → Client: POST /connect/ciba
        //            (client_id, client_secret, scope, login_hint=user@email)
        //            → receives auth_req_id
        //   Step 2 → IdentityServer notifies user's device (push/ping/poll)
        //            User approves on their phone
        //   Step 3 → Client: polls POST /connect/token
        //            (grant_type=urn:openid:params:grant-type:ciba,
        //             auth_req_id=...) until approved
        //            → receives access_token
        //
        // WHY RequireClientSecret = true here:
        //   Unlike postman-client, CIBA clients are confidential (server-side
        //   apps) and can securely store a secret. The secret authenticates the
        //   client application itself at the backchannel endpoint, which is
        //   important because there is no user-facing redirect to verify intent.
        //
        // WHY BackChannelTokenDeliveryMode = "poll":
        //   Three modes exist — poll (client polls), ping (server pings the
        //   client's notification endpoint then client fetches), and push
        //   (server pushes the token directly). "poll" is simplest to set up
        //   for testing with no extra infrastructure needed.
        //
        // NOTE: In production, move the client secret to a secrets manager and
        // set BackChannelUserCodeParameter if your use case requires it.
        // ---------------------------------------------------------------------
        new Client
        {
            ClientId            = "ciba-client",
            AllowedGrantTypes   = GrantTypes.Ciba,
            RequireClientSecret = true,
            ClientSecrets       = { new Secret("ciba-secret".Sha256()) },
            // CibaLifetime: how long (seconds) the auth_req_id remains valid.
            // Default is 300s. Increase if your approval flow takes longer.
            CibaLifetime        = 300,
            // PollingInterval: minimum seconds the client must wait between
            // polling POST /connect/token with auth_req_id. Prevents hammering.
            PollingInterval     = 5,
            AllowOfflineAccess  = true,
            AllowedScopes       =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                ScopeNames.Basket,
                ScopeNames.Catalog,
                ScopeNames.OrdersCommand,
                ScopeNames.OrdersQuery
            }
        }
    ];
}