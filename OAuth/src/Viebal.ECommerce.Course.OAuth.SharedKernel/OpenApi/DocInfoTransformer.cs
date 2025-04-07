using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Viebal.ECommerce.Course.OAuth.SharedKernel.OpenApi;

class DocInfoTransformer(IConfiguration config) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var info = config.GetRequiredSection("OpenApi:Info").Get<OpenApiInfo>() ?? new OpenApiInfo
        {
            Title = "Viebal ECommerce Course API",
            Version = "v1.0",
            License = new OpenApiLicense
            {
                Name = "Viebal ECommerce Course License",
                Url = new Uri("https://creativecommons.org/licenses/by-nc-sa/4.0")
            },
            TermsOfService = new Uri("https://en.wikipedia.org/wiki/Terms_of_service#Content"),
            Contact = new OpenApiContact
            {
                Url = new Uri("https://viebal.top")
            },
            Description = "Viebal ECommerce Course API"
        };

        document.Info = info;

        var servers = config.GetRequiredSection("OpenApi:Servers").Get<OpenApiServer[]>() ?? [];

        foreach (var server in servers)
        {
            server.Extensions["x-internal"] = new OpenApiBoolean(false);
        }

        document.Servers = servers;

        return Task.CompletedTask;
    }
}
