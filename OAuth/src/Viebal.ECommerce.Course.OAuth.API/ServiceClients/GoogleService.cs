using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Refit;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Viebal.ECommerce.Course.OAuth.API.ServiceClients;


interface IGoogleService
{
    public Task<GoogleUserProfile> GetUserInfoFromCodeAsync(string code);
}

sealed class GoogleService : IGoogleService
{
    private readonly IGoogleAuthClient _authClient;
    private readonly IGoogleUserClient _userClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleService> _logger;

    public GoogleService(
        IGoogleAuthClient authClient,
        IGoogleUserClient userClient,
        IConfiguration config,
        ILogger<GoogleService> logger)
    {
        _authClient = authClient;
        _userClient = userClient;
        _config = config;
        _logger = logger;
    }

    public async Task<GoogleUserProfile> GetUserInfoFromCodeAsync(string code)
    {
        try
        {
            var googleConfig = _config.GetSection("OAuth:Google");

            // Exchange code for token
            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", googleConfig.GetValue<string>("ClientId") ?? string.Empty },
                { "client_secret", googleConfig.GetValue<string>("ClientSecret") ?? string.Empty },
                { "code", code },
                { "grant_type", "authorization_code" }
            };

            var tokenResponse = await _authClient.ExchangeCodeForTokenAsync(tokenRequest).ConfigureAwait(false);

            // Get user info using the access token
            var userInfo = await _userClient.GetUserProfileAsync($"Bearer {tokenResponse.AccessToken}").ConfigureAwait(false);
            return userInfo;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Error getting user info from Google");
            throw new AuthenticationException("Failed to authenticate with Google", ex);
        }
    }
}

class TestHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TestHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory)
        : base(options, logger, encoder)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the user is already authenticated using a cookie or token
        // Check if the user is already authenticated using a cookie or token
        var identity = new ClaimsIdentity([], "Bearer");
        var principle = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principle, "Bearer");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
