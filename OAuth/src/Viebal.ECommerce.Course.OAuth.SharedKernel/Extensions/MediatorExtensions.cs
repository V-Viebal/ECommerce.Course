using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Viebal.ECommerce.Course.OAuth.SharedKernel.Cqrs.Behaviors;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

public static class MediatorExtensions
{
    public static void AddAppMediaR(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Add behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
    }
}
