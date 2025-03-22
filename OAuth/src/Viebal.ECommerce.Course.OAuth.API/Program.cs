using Autofac.Extensions.DependencyInjection;
using Serilog;
using Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Config web host
builder.WebHost.ConfigureKestrel(opts =>
{
    opts.AddServerHeader = false;
    opts.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    opts.Limits.MaxConcurrentConnections = 100;
    opts.Limits.MaxConcurrentUpgradedConnections = 100;
    opts.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    opts.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);

    opts.ConfigureEndpointDefaults(listenOptions =>
    {
    });
});

// Configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.json", false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

// DI Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Mapper
builder.Services.AddAppMapper();

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

// Map health check endpoints
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync().ConfigureAwait(false);

internal static class EntryPoint { };
