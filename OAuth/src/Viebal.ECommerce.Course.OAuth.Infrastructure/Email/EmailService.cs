using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Security.Cryptography;
using Viebal.ECommerce.Course.OAuth.UseCase;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Email;

public class EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger, IAppMemoryCache memoryCache) : IEmailService
{

    public async Task SendOtpByEmailAsync(string email, CancellationToken cancellation = default)
    {
        var code = RandomNumberGenerator.GetInt32(999999).ToString("D6");
        await SendEmailAsync(new[] { email }, "Your OTP code", $"<h1>Your OTP code is: {code}</h1>", cancellation);
        memoryCache.SetOtpCodeForEmail(email, code);
    }

    private async Task SendEmailAsync(IEnumerable<string> to, string subject, string content, CancellationToken cancellation)
    {
        var message = new Message(to, subject, content);
        var readyMessage = CreateEmailMessage(message);
        await Send(cancellation, readyMessage);
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(emailOptions.Value.FromName, emailOptions.Value.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message.Content
        };

        return emailMessage;
    }

    private async Task Send(CancellationToken cancellation, params MimeMessage[] messages)
    {
        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            try
            {
                await client.ConnectAsync(emailOptions.Value.Smtp, emailOptions.Value.Port ?? 465, true, cancellation).ConfigureAwait(false);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(emailOptions.Value.From, emailOptions.Value.Password, cancellation).ConfigureAwait(false);

                foreach (var message in messages)
                    await client.SendAsync(message, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true, cancellation);
                client.Dispose();
            }
        }
    }
}
