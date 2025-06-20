﻿using Microsoft.EntityFrameworkCore;
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

        services.AddDbContextPool<AppDbContext>(opts =>
        {
            var test = configuration.GetConnectionString("DefaultConnection");
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? string.Empty);

            opts.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            opts.UseSeeding((context, _) =>
            {
                var userSet = context.Set<User>();

                var hasUsers = userSet.Any();
                if (!hasUsers)
                {
                    userSet.AddRange(users);
                    context.SaveChanges();
                }
            });

            opts.UseAsyncSeeding(async (context, _, cancellation) =>
            {
                var userSet = context.Set<User>();

                var hasUsers = await userSet.AnyAsync(cancellation);
                if (!hasUsers)
                {
                    await userSet.AddRangeAsync(users, cancellation);
                    await context.SaveChangesAsync(cancellation);
                }
            });
        });

        services.AddDbContextPool<CourseDbContext>(opts =>
        {
            var test = configuration.GetConnectionString("CourseDb");
            opts.UseSqlServer(configuration.GetConnectionString("CourseDb") ?? string.Empty);

            opts.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        return services;
    }
}
