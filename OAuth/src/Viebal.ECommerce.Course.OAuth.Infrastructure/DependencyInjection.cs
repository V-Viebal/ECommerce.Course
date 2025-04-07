using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viebal.ECommerce.Course.OAuth.Domain.Entities;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Extensions;

namespace Viebal.ECommerce.Course.OAuth.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrasOptions(configuration);
        services.AddServices();

        var users = configuration.GetSection("Database:Seeding:Users").Get<User[]>() ?? [];

        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("InMemoryDb").UseAsyncSeeding(async (context, _, cancellation) =>
        {
            foreach (var user in users)
                await context.Set<User>().AddAsync(user, cancellation);

            await context.SaveChangesAsync(cancellation);
        }));

        return services;
    }
}
