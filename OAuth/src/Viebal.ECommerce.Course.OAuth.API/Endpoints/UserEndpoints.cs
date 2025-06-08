
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Viebal.ECommerce.Course.OAuth.API.Infrastructure;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;

namespace Viebal.ECommerce.Course.OAuth.API.Endpoints;

class UserEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication webApp)
    {
        var routes = webApp.MapGroup(this, "Users");

        routes.Map(GetUserProfile);
        routes.Map(ResetPassword);
        routes.Map(SignOut);
    }

    [HttpGet("me")]
    async Task<IResult> GetUserProfile(HttpContext context, AppDbContext dbContext, CancellationToken cancellation)
    {
        var userEmail = context.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == userEmail, cancellation);

        if (user is null)
            return Results.NotFound();

        return Results.Ok(new
        {
            user.Email,
            user.FirstName,
            user.LastName,
            user.Photo
        });
    }

    [HttpPost("me/password/reset")]
    async Task<IResult> ResetPassword([FromBody] ResetPassword request, HttpContext context, AppDbContext dbContext, CancellationToken cancellation)
    {
        var userEmail = context.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == userEmail, cancellation);

        if (user is null)
            return Results.NotFound();

        user.Password = request.NewPassword;

        await dbContext.SaveChangesAsync(cancellation);

        return Results.Ok();
    }

    [HttpPost("me/sign-out")]
    async Task<IResult> SignOut(HttpContext context, AppDbContext dbContext, CancellationToken cancellation)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        if (!long.TryParse(userIdClaim, out var userId))
            return Results.NotFound();

        var tokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellation);

        dbContext.RemoveRange(tokens);
        await dbContext.SaveChangesAsync(cancellation);
        //await dbContext.RefreshTokens
        //    .Where(x => x.UserId == userId)
        //    .ExecuteDeleteAsync(cancellation);

        return Results.Ok();
    }
}

record ResetPassword(string NewPassword);
