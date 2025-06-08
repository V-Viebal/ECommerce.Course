using Asp.Versioning;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Refit;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using Viebal.ECommerce.Course.OAuth.API.ConfigurationOptions;
using Viebal.ECommerce.Course.OAuth.API.Extensions;
using Viebal.ECommerce.Course.OAuth.API.Infrastructure;
using Viebal.ECommerce.Course.OAuth.API.ServiceClients;
using Viebal.ECommerce.Course.OAuth.API.Services;
using Viebal.ECommerce.Course.OAuth.Infrastructure;
using Viebal.ECommerce.Course.OAuth.Infrastructure.Data;
using Viebal.ECommerce.Course.OAuth.SharedKernel.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Config web host
builder.WebHost.UseKestrel(opts =>
{
    opts.AddServerHeader = false;
    opts.Limits.MaxRequestBodySize = 10_000_000; // 10MB
    opts.Limits.MaxConcurrentConnections = 100;
    opts.Limits.MaxConcurrentUpgradedConnections = 100;
    opts.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    opts.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    opts.Configure(builder.Configuration.GetSection("Kestrel"));
});

// Configuration
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.json", false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var appConfigPath = Environment.GetEnvironmentVariable("APP_CONFIG_PATH");
if (!string.IsNullOrWhiteSpace(appConfigPath))
{
    builder.Configuration.AddJsonFile(Path.Combine(appConfigPath, $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true);
}

// Config Options
builder.Services.AddAppConfigurations(builder.Configuration);

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

// DI Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// OAuth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        var jwtBearerOpts = builder.Configuration.GetRequiredSection("Auth:Jwt").Get<JwtBearerAppOptions>();
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = jwtBearerOpts?.Audience ?? string.Empty,
            ValidIssuer = jwtBearerOpts?.Issuer ?? string.Empty,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerOpts?.SecretKey ?? string.Empty)),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddAppGoogle(builder.Configuration);

builder.Services.AddAuthorizationBuilder()
    .AddDefaultPolicy("Default", builder => builder.RequireAuthenticatedUser())
    .AddFallbackPolicy("Default", builder => builder.RequireAuthenticatedUser());

#if !EXCLUDE_SERVICE_DEFAULTS
builder.AddServiceDefaults();
#else
builder.Services.AddHealthChecks();
#endif

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new(1);
    opts.ApiVersionReader = new UrlSegmentApiVersionReader();
    opts.ReportApiVersions = true; // Includes API versions in response headers
    opts.AssumeDefaultVersionWhenUnspecified = false; // Throws 400 if version is missing
}).AddApiExplorer(opts =>
{
    opts.GroupNameFormat = "'v'V";
    opts.SubstituteApiVersionInUrl = true;
});

builder.Services.AddOpenApi("v1");
//builder.Services.AddOpenApiDoc(Viebal.ECommerce.Course.OAuth.SharedKernel.Enums.ApiCategory.InternalApi | Viebal.ECommerce.Course.OAuth.SharedKernel.Enums.ApiCategory.PublicApi);

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services
    .AddRefitClient<IGoogleUserClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.googleapis.com"));

builder.Services.AddScoped<IAppTokenProvider, JwtBearerAppProvider>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy(opts =>
    {
        opts.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

if (!(app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local")))
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    var context = app.Services.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
    await context.Database.MigrateAsync().ConfigureAwait(false);

    var courseContext = app.Services.GetRequiredService<CourseDbContext>();
    await courseContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
    await courseContext.Database.MigrateAsync().ConfigureAwait(false);


    app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
    app.MapScalarApiReference(opts =>
    {
        opts.AddDocument("v1");
    }).AllowAnonymous();

    app.MapGet("/", () => Results.Redirect("/scalar")).AllowAnonymous().ExcludeFromDescription();
}

app.UseCors();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoints
#if !EXCLUDE_SERVICE_DEFAULTS
app.MapDefaultEndpoints();
#else
app.MapHealthChecks("/health").AllowAnonymous();
#endif

app.MapEndpoints();

try
{
    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}