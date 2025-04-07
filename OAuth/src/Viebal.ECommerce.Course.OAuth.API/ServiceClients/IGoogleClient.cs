using Refit;
using System.Text.Json.Serialization;

namespace Viebal.ECommerce.Course.OAuth.API.ServiceClients;

interface IGoogleAuthClient
{

    [Post("/token")]
    Task<GoogleTokenResponse> ExchangeCodeForTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> request);
}

interface IGoogleUserClient
{

    [Get("/oauth2/v1/userinfo")]
    Task<GoogleUserProfile> GetUserProfileAsync([Header("Authorization")] string authorization);
}

record GoogleTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] string ExpriresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken);

record GoogleUserProfile(
    string Id,
    string Email,
    [property: JsonPropertyName("verified_email")] bool VerifiedEmail,
    string Name,
    [property: JsonPropertyName("given_name")] string GivenName,
    [property: JsonPropertyName("family_name")] string FamilyName,
    string Picture,
    string Locale);
