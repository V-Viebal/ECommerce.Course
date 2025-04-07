using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;
using Viebal.ECommerce.Course.OAuth.UseCase;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Cache;

class AppMemoryCache(IMemoryCache memoryCache) : IAppMemoryCache
{
    [StringSyntax("CompositeFormat")]
    private readonly string _optCodeForEmailKeyFormat = "otp_email_{0}";

    public void GetOtpCodeForEmail(string email, out string? otpCode)
    {
        var key = string.Format(_optCodeForEmailKeyFormat, email);

        if (!memoryCache.TryGetValue(key, out otpCode))
            throw new InvalidDataException($"OTP code for email {email} not found in cache.");
    }

    public void SetOtpCodeForEmail(string email, string otpCode)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };


        var key = string.Format(_optCodeForEmailKeyFormat, email);
        memoryCache.Set(key, otpCode, cacheEntryOptions);
    }
}
