using Viebal.ECommerce.Course.OAuth.API.ConfigurationOptions;

namespace Viebal.ECommerce.Course.OAuth.API.Extensions;

static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtBearerAppOptions>().Bind(configuration.GetSection("Auth:Jwt"));
        services.AddOptions<GoogleAppOptions>().Bind(configuration.GetSection("Auth:Google"));
        services.AddOptions<ServerSettings>().Bind(configuration.GetSection("ServerSettings"));

        return services;
    }
}
