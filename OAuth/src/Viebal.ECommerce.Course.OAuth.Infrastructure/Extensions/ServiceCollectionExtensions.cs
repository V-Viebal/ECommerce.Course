using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Cache;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Email;
using Viebal.ECommerce.Course.OAuth.UseCase;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrasOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection("Database"));
        services.AddOptions<EmailOptions>().Bind(configuration.GetSection("Email"));

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAppMemoryCache, AppMemoryCache>();

        return services;
    }
}
