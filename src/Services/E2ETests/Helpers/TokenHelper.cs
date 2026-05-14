using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace E2ETests.Helpers;

public static class TokenHelper
{
    public static async Task<TokenResponse> GetTokenAsync(
        string identityBaseUrl,
        CancellationToken ct = default)
    {
        using var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        using var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"]  = "postman-client-password",
            ["username"]   = "AliceSmith@email.com",
            ["password"]   = "Pass123$",
            ["scope"]      = "openid profile catalog basket orders_command orders_query"
        });

        var response = await client.PostAsync(
            $"{identityBaseUrl}/connect/token",
            formContent,
            ct);

        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);

        if (tokenResponse == null)
            throw new Exception("Could not get token response");

        return tokenResponse;
    }

    // Extracts the 'sub' claim from the JWT payload without signature validation.
    // The token was just issued by IdentityServer so trust is implicit.
    public static string GetSubClaim(string accessToken)
    {
        var payload = accessToken.Split('.')[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("sub").GetString()!;
    }
}

public record TokenResponse(
    [property: JsonPropertyName("access_token")]
    string AccessToken,
    [property: JsonPropertyName("token_type")]
    string TokenType,
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn);
