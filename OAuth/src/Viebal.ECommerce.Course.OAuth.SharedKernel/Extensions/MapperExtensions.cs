using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

public static class MapperExtensions
{
    public static void AddAppMapper(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(config =>
        {
            config.AddMaps(assembly);
        });
    }
}
