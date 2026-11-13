using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.API.Configurations;
using Noxy.NET.MediaPlayer.API.Handlers;
using Noxy.NET.MediaPlayer.Application.Configurations;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Each appsettings section the API reads is bound to a config class and validated at startup.
builder.Services.AddOptions<ConfigConnectionString>().BindConfiguration(ConfigConnectionString.SectionName).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<ConfigCors>().BindConfiguration(ConfigCors.SectionName).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<ConfigRateLimitAuth>().BindConfiguration(ConfigRateLimitAuth.SectionName).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>()
    .Configure<IOptions<ConfigCors>>((cors, settings) => cors.AddPolicy(Constants.CorsPolicyName, policy => policy.WithOrigins(settings.Value.Origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddApplication();
builder.Services.AddPersistence((services, options) => options.UseSqlite(services.GetRequiredService<IOptions<ConfigConnectionString>>().Value.Database));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(Constants.RateLimitPolicyAuth, context =>
    {
        ConfigRateLimitAuth settings = context.RequestServices.GetRequiredService<IOptions<ConfigRateLimitAuth>>().Value;

        return RateLimitPartition.GetSlidingWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new()
        {
            PermitLimit = settings.PermitLimit,
            Window = settings.Window,
            SegmentsPerWindow = 6,
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        HttpResponse response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        // The sliding window can't say when a permit frees up, so give the full window: always safe, sometimes pessimistic.
        ConfigRateLimitAuth settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<ConfigRateLimitAuth>>().Value;
        response.Headers.RetryAfter = settings.WindowSeconds.ToString(CultureInfo.InvariantCulture);

        await response.WriteAsJsonAsync(new ProblemDetails { Status = response.StatusCode, Title = "Too many requests." }, cancellationToken);
    };
});

builder.Services.AddControllers();

// Validation uses the same ConfigJwt that TokenService signs with, so the two can't drift apart.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<ConfigJwt>>((bearer, jwt) =>
    {
        bearer.MapInboundClaims = false; // keep claim names as issued ("sub", "email")
        bearer.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidAudience = jwt.Value.Audience,
            IssuerSigningKey = jwt.Value.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Secure by default: any endpoint without its own authorization metadata requires a signed-in user.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseCors(Constants.CorsPolicyName);
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
