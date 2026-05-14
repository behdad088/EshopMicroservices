using E2ETests.Helpers;

namespace E2ETests.Fixtures;

public class E2EFixture : IAsyncLifetime
{
    // Service base URLs. Defaults match docker-compose.e2e.yml host port mappings.
    public string IdentityBaseUrl => Env("E2E_IDENTITY_URL", "https://localhost:7063");
    public string CatalogBaseUrl => Env("E2E_CATALOG_URL", "http://localhost:6000");
    public string BasketBaseUrl => Env("E2E_BASKET_URL", "http://localhost:6001");
    public string OrderQueryBaseUrl => Env("E2E_ORDER_QUERY_URL", "http://localhost:6005");

    // Alice's credentials — seeded in Identity.API/SeedData.cs
    public string Username { get; } = "AliceSmith@email.com";

    // Set during InitializeAsync from the JWT sub claim
    public string CustomerId { get; private set; } = null!;

    private IPlaywright Playwright { get; set; } = null!;

    // Single shared API context. All requests carry Alice's Bearer token.
    public IAPIRequestContext Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var token = await TokenHelper.GetTokenAsync(IdentityBaseUrl);
        CustomerId = TokenHelper.GetSubClaim(token.AccessToken);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Api = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token.AccessToken}"
            }
        });
    }

    public async Task DisposeAsync()
    {
        await Api.DisposeAsync();
        Playwright.Dispose();
    }

    private static string Env(string key, string defaultValue) =>
        Environment.GetEnvironmentVariable(key) ?? defaultValue;
}
