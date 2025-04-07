using Viebal.ECommerce.Course.OAuth.Domain.Entities;

namespace Viebal.ECommerce.Course.OAuth.API.Services;

interface IAppTokenProvider
{
    public string GenerateAccessToken(User email);

    public string GenerateRefreshToken();
}
