namespace Viebal.ECommerce.Course.OAuth.API.ConfigurationOptions;

class JwtBearerAppOptions
{
    public string? SecretKey { get; set; }

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public long ExpiryMinutes { get; set; }
}
