namespace Viebal.ECommerce.Course.OAuth.UseCase;

public interface IAppMemoryCache
{
    void SetOtpCodeForEmail(string email, string otpCode);

    void GetOtpCodeForEmail(string email, out string? otpCode);
}
