using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;

namespace Viebal.ECommerce.Course.OAuth.API.Infrastructure;

static class RouteBuilderExtensions
{
    public static IEndpointRouteBuilder Map(this IEndpointRouteBuilder routeBuilder, Delegate handler, IEnumerable<Action<RouteHandlerBuilder>>? actions = default, int version = 1)
    {
        var methodInfo = handler.Method;
        var routeAttribute = methodInfo.GetCustomAttribute<HttpGetAttribute>() as HttpMethodAttribute ??
            methodInfo.GetCustomAttribute<HttpPostAttribute>() as HttpMethodAttribute ??
            methodInfo.GetCustomAttribute<HttpPutAttribute>() as HttpMethodAttribute ??
            methodInfo.GetCustomAttribute<HttpDeleteAttribute>() as HttpMethodAttribute;

        if (routeAttribute is null)
            throw new InvalidOperationException("No HTTP method attribute found on the handler method.");

        var pattern = routeAttribute.Template ?? string.Empty;
        var httpMethod = routeAttribute.HttpMethods.FirstOrDefault();

        if (httpMethod is null)
            throw new InvalidOperationException("No HTTP method found in the attribute.");

        var route = routeBuilder.MapMethods(pattern, handler, httpMethod)
            .WithName(handler.Method.Name)
            .MapToApiVersion(version);

        route.MapActions(actions);

        return routeBuilder;
    }

    private static void MapActions(this RouteHandlerBuilder routeHandlerBuilder, IEnumerable<Action<RouteHandlerBuilder>>? actions)
    {
        if (actions is null) return;

        foreach (var action in actions)
            action(routeHandlerBuilder);
    }

    private static RouteHandlerBuilder MapMethods(this IEndpointRouteBuilder routeBuilder, string pattern, Delegate handler, string httpMethod)
    {
        return httpMethod switch
        {
            "GET" => routeBuilder.MapGet(pattern, handler),
            "POST" => routeBuilder.MapPost(pattern, handler),
            "PUT" => routeBuilder.MapPut(pattern, handler),
            "DELETE" => routeBuilder.MapDelete(pattern, handler),
            "PATCH" => routeBuilder.MapPatch(pattern, handler),
            _ => throw new InvalidOperationException($"Unsupported HTTP method: {httpMethod}")
        };
    }
}
