using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Viebal.ECommerce.Course.OAuth.SharedKernel.Enums;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

public static class OpenApiExtensions
{
    //private static readonly string BEARER_SCHEMA = OpenApiConstants.Bearer.ToUpper();

    public static IServiceCollection AddOpenApiDoc(this IServiceCollection services, ApiCategory apiCategories)
    {
        services.AddOpenApi(opts =>
        {
            // Doc inclusion predicate 
            //opts.DocIncludePredicate((documentName, apiDescriotion) = =>
            //{
            //    static ApiCategory GetApiCategory(ApiDescription apiDescription)
            //    {
            //        return apiDescription.ActionDescriptor.DisplayName.Contains(".Public", StringComparison.InvariantCultureIgnoreCase) ?? false ? ApiCategory.PublicApi : ApiCategory.InternalApi;
            //    }
            //    var apiCategory =
            //});

            opts.AddDocumentTransformer((doc, ctx, _) =>
            {
                ConfigureInternalApi(apiCategories, () =>
                {
                    doc.Info = new OpenApiInfo
                    {
                        Title = ApiCategory.InternalApi.GetDescription(),
                        Version = "v1.0"
                    };
                });

                return Task.CompletedTask;
            });

            opts.AddDocumentTransformer((doc, ctx, _) =>
            {
                ConfigurePublicApi(apiCategories, services.BuildServiceProvider(), version =>
                {
                    doc.Info = new OpenApiInfo
                    {
                        Title = ApiCategory.InternalApi.GetDescription(),
                        Version = version
                    };
                });

                return Task.CompletedTask;
            });

        });

        return services;
    }

    private static void ConfigureInternalApi(ApiCategory apiCategory, Action action)
    {
        if (!apiCategory.HasFlag(ApiCategory.InternalApi))
            return;

        action();
    }

    private static void ConfigurePublicApi(ApiCategory apiCategory, IServiceProvider svcProvider, Action<string> action)
    {
        if (!apiCategory.HasFlag(ApiCategory.PublicApi))
            return;

        var addedVersion = new HashSet<string>();
        var apiDescriptionProvider = svcProvider.GetRequiredService<IApiVersionDescriptionProvider>();
        var apiVersions = apiDescriptionProvider.ApiVersionDescriptions.Select(description => description.ApiVersion);

        foreach (var version in apiVersions)
        {
            var versionString = version != null ? $"v{version}" : string.Empty;
            if (addedVersion.Add(versionString))
                action(versionString);
        }
    }
}
