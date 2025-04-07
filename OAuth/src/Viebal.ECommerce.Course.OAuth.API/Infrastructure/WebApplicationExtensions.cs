using Asp.Versioning.Builder;
using System.Reflection;

namespace Viebal.ECommerce.Course.OAuth.API.Infrastructure;

static class WebApplicationExtensions
{
    public static ApiVersionSet apiVersionSet = default!;

    public static RouteGroupBuilder MapGroup(this WebApplication webApp, EndpointGroupBase group, string? groupName = default)
    {
        groupName ??= group.GetType().Name;

        return webApp.MapGroup($"/api/v{{apiVersion:apiVersion}}/{groupName.ToLowerInvariant()}")
            .WithApiVersionSet(apiVersionSet)
            .WithTags(groupName)
            .WithOpenApi();
    }

    public static WebApplication MapEndpoints(this WebApplication webApp)
    {
        var endpointGroupType = typeof(EndpointGroupBase);
        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(endpointGroupType));

        apiVersionSet = webApp.NewApiVersionSet()
            .HasApiVersion(new Asp.Versioning.ApiVersion(1))
            .HasApiVersion(new Asp.Versioning.ApiVersion(2))
            .ReportApiVersions()
            .Build();

        foreach (var endpointGroup in endpointGroupTypes)
        {
            if (Activator.CreateInstance(endpointGroup) is EndpointGroupBase instance)
                instance.Map(webApp);
        }

        return webApp;
    }
}
