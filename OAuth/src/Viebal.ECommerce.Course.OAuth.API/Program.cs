using Asp.Versioning;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// Config Options
builder.Services.AddAppConfigurations(builder.Configuration);

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

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

builder.AddServiceDefaults();

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
    .AddRefitClient<IGoogleAuthClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oauth2.googleapis.com"));

builder.Services
    .AddRefitClient<IGoogleUserClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://www.googleapis.com"));

builder.Services.AddScoped<IGoogleService, GoogleService>();
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

    app.MapOpenApi("/openapi/{documentName}.json").AllowAnonymous();
    app.MapScalarApiReference(opts =>
    {
        opts.AddDocument("v1");
    }).AllowAnonymous();

    app.MapGet("/", () => Results.Redirect("/scalar")).AllowAnonymous().ExcludeFromDescription();
}

app.UseCors();
app.UseRouting();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoints
app.MapDefaultEndpoints();
app.MapEndpoints();


//app.MapGet("/accessToken", async (string accessToken, IGoogleUserClient googleClient) =>
//{
//    var userInfo = await googleClient.GetUserProfileAsync($"Bearer {accessToken}");
//    var photoBytes = await new HttpClient().GetAsync(new Uri(userInfo.Picture)).ConfigureAwait(false);

//    using var responseStream = await photoBytes.Content.ReadAsStreamAsync();
//    var memoryStream = new MemoryStream();

//    await responseStream.CopyToAsync(memoryStream);
//    memoryStream.Seek(0, SeekOrigin.Begin);

//    // Save the image to the local file system
//    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "downloaded_image5.png");
//    var directory = Path.GetDirectoryName(filePath);
//    if (!Directory.Exists(directory))
//    {
//        Directory.CreateDirectory(directory!);
//    }

//    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
//    {
//        await memoryStream.CopyToAsync(fileStream);
//    }

//    memoryStream.Seek(0, SeekOrigin.Begin);
//    return Results.Stream(memoryStream, "image/png", enableRangeProcessing: true);
//}).AllowAnonymous();

//app.MapGet("/test", async (string code, IGoogleService googleService) =>
//{
//    var user = await googleService.GetUserInfoFromCodeAsync(code).ConfigureAwait(false);
//    return Results.Ok(user);
//}).AllowAnonymous();

//app.MapGet("/auth/signin-google", async (string redirectUrl, HttpContext context) =>
//{
//    var properties = new AuthenticationProperties
//    {
//        RedirectUri = $"/auth/callback-google?redirectUrl={redirectUrl}"
//    };

//    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties).ConfigureAwait(false);
//}).AllowAnonymous();


//app.MapGet("/auth/callback-google", async (string redirectUrl, HttpContext context, [FromServices] IAppTokenProvider appTokenService) =>
//{
//    if (string.IsNullOrWhiteSpace(redirectUrl))
//        return Results.BadRequest("No provided redirect url for callback");

//    var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme).ConfigureAwait(false);
//    if (!result.Succeeded)
//        return Results.Unauthorized();

//    var email = result.Principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;
//    var token = appTokenService.GenerateToken(email);

//    var appendTokenRedirectUrl = $"{redirectUrl}?accessToken={token}";
//    return Results.Redirect(appendTokenRedirectUrl);
//}).AllowAnonymous();

//app.MapGet("/tess", async (
//    string code,
//    LoginType socialType,
//    [FromServices] IOptions<GoogleAppOptions> googleOpts,
//    [FromServices] IOptions<ServerSettings> serverSettingOpts,
//    CancellationToken cancellationToken) =>
//{
//    if (socialType == LoginType.None) return Results.BadRequest("Invalid social login type");

//    var authorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//    {
//        ClientSecrets = new()
//        {
//            ClientId = googleOpts.Value.ClientId,
//            ClientSecret = googleOpts.Value.ClientSecret
//        },
//        DataStore = default // Don't need to store the token
//    });

//    var userId = string.Empty; // Since we don't wanna store the token, so we leave the user id empty
//    var tokenResponse = await authorizationCodeFlow.ExchangeCodeForTokenAsync(userId, code, serverSettingOpts.Value.Domain, cancellationToken);
//    if (tokenResponse == null)
//        return Results.BadRequest("Invalid authorization code");

//    using var httpClient = new HttpClient();
//    var uri = new Uri($"{GoogleDefaults.UserInformationEndpoint}?access_token={tokenResponse.AccessToken}");
//    var response = await httpClient.GetAsync(uri, cancellationToken);

//    return Results.Ok(response);
//}).AllowAnonymous();

//app.MapGet("/secure", (HttpContext context) =>
//{
//    if (!context.User.Identity!.IsAuthenticated)
//        return Results.Unauthorized();

//    return Results.Ok("This is secure data");
//}).RequireAuthorization();

await app.RunAsync().ConfigureAwait(false);