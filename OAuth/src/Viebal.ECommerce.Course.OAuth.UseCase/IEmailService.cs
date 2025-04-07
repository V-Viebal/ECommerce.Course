namespace Viebal.ECommerce.Course.OAuth.UseCase;

public interface IEmailService
{
    Task SendOtpByEmailAsync(string email, CancellationToken cancellation = default);
}
