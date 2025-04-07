namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Email;

public class EmailOptions
{
    public string? From { get; set; }

    public string? FromName { get; set; }

    public string? Smtp { get; set; }

    public int? Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }
}
