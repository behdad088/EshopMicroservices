namespace Basket.API.IntegrationTests;

public static class FakePermission
{
    public static object GetPermissions(
        string[] permissions,
        string? sub = null,
        string? username = null,
        string[]? roles = null,
        string[]? scopes = null
    )
    {
        var claims = new Dictionary<string, object>
        {
            { "sub", sub ?? Guid.NewGuid().ToString() },
            { "name", username ?? Guid.NewGuid().ToString() },
            { "role", roles ?? ["customer"] },
            { "scope", scopes ?? ["short"] },
            { "permissions", permissions },
        };

        return claims;
    }
}