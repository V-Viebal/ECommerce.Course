using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Viebal.ECommerce.Course.OAuth.API.ConfigurationOptions;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;

namespace Viebal.ECommerce.Course.OAuth.API.Services;

class JwtBearerAppProvider(IOptions<JwtBearerAppOptions> jwtBearerOpts) : IAppTokenProvider
{
    public string GenerateAccessToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var privateKey = Encoding.UTF8.GetBytes(jwtBearerOpts.Value.SecretKey ?? string.Empty);

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = jwtBearerOpts.Value.Audience,
            Issuer = jwtBearerOpts.Value.Issuer,
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(jwtBearerOpts.Value.ExpiryMinutes),
            Subject = GenerateClaims(user),
            SigningCredentials = signingCredentials
        };

        var token = handler.CreateJwtSecurityToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private static ClaimsIdentity GenerateClaims(User user)
    {
        var ci = new ClaimsIdentity();

        ci.AddClaims(new[]
        {
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        });

        return ci;
    }
}
