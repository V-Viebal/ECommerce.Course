using Refit;
using System.Text.Json.Serialization;

namespace Viebal.ECommerce.Course.OAuth.API.ServiceClients;

interface IGoogleUserClient
{

    [Get("/oauth2/v1/userinfo")]
    Task<GoogleUserProfile> GetUserProfileAsync([Header("Authorization")] string authorization);
}

record GoogleUserProfile(
    string Id,
    string Email,
    [property: JsonPropertyName("verified_email")] bool VerifiedEmail,
    string Name,
    [property: JsonPropertyName("given_name")] string GivenName,
    [property: JsonPropertyName("family_name")] string FamilyName,
    string Picture,
    string Locale);
