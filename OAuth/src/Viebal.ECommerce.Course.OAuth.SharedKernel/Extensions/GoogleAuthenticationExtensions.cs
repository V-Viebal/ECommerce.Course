using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

public static class GoogleAuthenticationExtensions
{
    public static AuthenticationBuilder AddAppGoogle(
        this AuthenticationBuilder authBuilder,
        IConfiguration configuration,
        string cookieScheme = CookieAuthenticationDefaults.AuthenticationScheme)
    {
        var googleConfig = configuration.GetSection("Auth:Google");

        // This extenstion needs cookie as sign in the user after challenging
        authBuilder.AddCookie(cookieScheme, opts =>
        {
            opts.Cookie.HttpOnly = true;
            opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            opts.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        });

        authBuilder.AddGoogle(opts =>
        {
            opts.ClientId = googleConfig.GetValue<string>("ClientId") ?? string.Empty;
            opts.ClientSecret = googleConfig.GetValue<string>("ClientSecret") ?? string.Empty;
            opts.CallbackPath = "/signin-google";
            opts.SignInScheme = cookieScheme;
            opts.UsePkce = true;
            opts.Events.OnCreatingTicket = context =>
            {
                var accessToken = context.TokenResponse.AccessToken;
                return Task.CompletedTask;
            };
        });

        return authBuilder;
    }
}
