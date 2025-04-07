
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Viebal.ECommerce.Course.OAuth.API.ConfigurationOptions;
using Viebal.ECommerce.Course.OAuth.API.Infrastructure;
using Viebal.ECommerce.Course.OAuth.API.ServiceClients;
using Viebal.ECommerce.Course.OAuth.API.Services;
using Viebal.ECommerce.Course.OAuth.Contract.Enums;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;
using Viebal.ECommerce.Course.OAuth.UseCase;

namespace Viebal.ECommerce.Course.OAuth.API.Endpoints;

class AuthenticationEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication webApp)
    {
        var routes = webApp.MapGroup(this, "Auth");

        routes.Map(RefreshToken);
        routes.Map(GetAccessTokenByPassword);
        routes.Map(GetAccessTokenByLoginType);

        routes.Map(SignUpSendEmailOtp);
        routes.Map(SignUpConfirmEmailOtp);
    }

    [AllowAnonymous]
    [HttpGet("refresh-token")]
    async Task<IResult> RefreshToken([FromQuery] string refreshToken, IAppTokenProvider tokenProvider, AppDbContext dbContext, CancellationToken cancellation)
    {
        var token = await dbContext.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellation);

        if (token is null || token.ExpriesOnUtc < DateTime.UtcNow)
            return Results.UnprocessableEntity("Refresh token is expired");

        if (token.User is null)
            return Results.UnprocessableEntity("User not found for this refresh token");

        string accessToken = tokenProvider.GenerateAccessToken(token.User);

        token.Token = tokenProvider.GenerateRefreshToken();
        token.ExpriesOnUtc = DateTime.UtcNow.AddDays(7);

        await dbContext.SaveChangesAsync(cancellation);

        return Results.Ok(
            new
            {
                accessToken,
                refreshToken = token.Token,
            }
        );
    }

    #region Sign in

    [AllowAnonymous]
    [HttpPost("sign-in/email/by-password")]
    async Task<IResult> GetAccessTokenByPassword(
        [FromBody] Request request,
        AppDbContext dbContext,
        IAppTokenProvider tokenProvider,
        IOptions<ServerSettings> serverSettings,
        CancellationToken cancellation)
    {
        var user = await dbContext.Users
            .Where(x => x.Email == request.Email)
            .Where(x => x.Password == request.Password)
            .FirstOrDefaultAsync(cancellation);

        if (user is null)
            return Results.NotFound("Not found the user");

        var refreshToken = new RefreshToken
        {
            Token = tokenProvider.GenerateRefreshToken(),
            UserId = user.Id,
            ExpriesOnUtc = DateTime.UtcNow.AddDays(serverSettings.Value.RefreshExpiryDays)
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellation);

        await dbContext.SaveChangesAsync(cancellation);

        return Results.Ok(
            new
            {
                accessToken = tokenProvider.GenerateAccessToken(user),
                refreshToken = refreshToken.Token,
            }
        );
    }

    [AllowAnonymous]
    [HttpPost("sign-in/social/by-acess-token")]
    async Task<IResult> GetAccessTokenByLoginType(
        [FromBody] SocialRequest request,
        IGoogleUserClient googleClient,
        AppDbContext dbContext,
        IAppTokenProvider tokenProvider,
        IOptions<ServerSettings> serverSettings,
        CancellationToken cancellation)
    {
        var userProfile = await googleClient.GetUserProfileAsync($"Bearer {request.AccessToken}");

        if (userProfile is null)
            return Results.BadRequest($"Invalid {request.LoginType} access token.");

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == userProfile.Email, cancellation);

        user ??= new User
        {
            Email = userProfile.Email,
        };

        user.Locale = userProfile.Locale;
        user.FirstName = userProfile.GivenName;
        user.LastName = userProfile.FamilyName;
        user.Photo = userProfile.Picture;

        if (user.Id == 0)
            await dbContext.Users.AddAsync(user, cancellation);

        await dbContext.SaveChangesAsync(cancellation);

        var refreshToken = new RefreshToken
        {
            Token = tokenProvider.GenerateRefreshToken(),
            UserId = user.Id,
            ExpriesOnUtc = DateTime.UtcNow.AddDays(serverSettings.Value.RefreshExpiryDays)
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellation);

        await dbContext.SaveChangesAsync(cancellation);

        return Results.Ok(
            new
            {
                accessToken = tokenProvider.GenerateAccessToken(user),
                refreshToken = refreshToken.Token,
            }
        );
    }

    #endregion

    [AllowAnonymous]
    [HttpPost("sign-up/email/send-otp")]
    async Task<IResult> SignUpSendEmailOtp([FromBody] SendEmailOtp request, IEmailService emailService, CancellationToken cancellation)
    {
        await emailService.SendOtpByEmailAsync(request.Email, cancellation);
        return Results.Ok();
    }

    [AllowAnonymous]
    [HttpPost("sign-up/email/confirm-otp")]
    async Task<IResult> SignUpConfirmEmailOtp(
        [FromBody] ConfirmEmailOtp request,
        IAppMemoryCache memoryCache,
        AppDbContext dbContext,
        IAppTokenProvider tokenProvider,
        IOptions<ServerSettings> serverSettings,
        CancellationToken cancellation)
    {
        var accessToken = string.Empty;
        var refreshTokenResponse = string.Empty;

        try
        {
            memoryCache.GetOtpCodeForEmail(request.Email, out string? code);
            if (code != request.Otp)
                return Results.BadRequest("Invalid OTP code");

            var user = new User
            {
                Email = request.Email,
            };

            await dbContext.Users.AddAsync(user, cancellation);
            await dbContext.SaveChangesAsync(cancellation);

            var refreshToken = new RefreshToken
            {
                Token = tokenProvider.GenerateRefreshToken(),
                UserId = user.Id,
                ExpriesOnUtc = DateTime.UtcNow.AddDays(serverSettings.Value.RefreshExpiryDays)
            };

            await dbContext.RefreshTokens.AddAsync(refreshToken, cancellation);
            await dbContext.SaveChangesAsync(cancellation);

            accessToken = tokenProvider.GenerateAccessToken(user);
            refreshTokenResponse = refreshToken.Token;
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        return Results.Ok(new { AccessToken = accessToken, RefreshToken = refreshTokenResponse });
    }
}

class SocialRequest
{
    public LoginType LoginType { get; set; }

    public required string AccessToken { get; set; }
}

class Request
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}

record SendEmailOtp(string Email);
record ConfirmEmailOtp(string Email, string Otp);